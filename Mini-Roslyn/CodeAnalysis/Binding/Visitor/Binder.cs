using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MiniRoslyn.CodeAnalysis.Syntax;
using MiniRoslyn.CodeAnalysis.Syntax.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding.Visitor
{
    internal class Binder : IExpressionVisitor<BoundExpression>, IStatementVisitor<BoundStatement>
    {
        private BoundScope _scope;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        
        public Binder(BoundScope parent)
        {
            _scope = new BoundScope(parent);
        }
        public DiagnosticBag Diagnostics => _diagnostics;
        
        public BoundStatement BindStatement(StatementSyntax syntax)
        {
            return this.VisitStatement(syntax);
        }

        private BoundStatement VisitStatement(StatementSyntax syntax)
        {
            return syntax.Accept(this);
        }
        
        private BoundExpression VisitExpression(ExpressionSyntax syntax)
        {
            return syntax.Accept(this);
        }

        public BoundExpression VisitLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        public BoundExpression VisitBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = VisitExpression(syntax.Left);
            var boundRight = VisitExpression(syntax.Right);
            var boundBinaryOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);
            if (boundBinaryOperator == null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return boundLeft;
            }
            
            return new BoundBinaryExpression(boundLeft, boundBinaryOperator, boundRight);
        }
        
        public BoundExpression VisitUnaryExpressionSyntax(UnaryExpressionSyntax syntax)
        {
            var boundOperand = VisitExpression(syntax.Operand);
            var boundUnaryOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);

            if (boundUnaryOperator == null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
                return boundOperand;
            }

            return new BoundUnaryExpression(boundUnaryOperator, boundOperand);
        }

        public BoundExpression VisitNameExpressionSyntax(NameExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            if (string.IsNullOrEmpty(name))
            {
                // This means the token was inserted by the parser. We already
                // reported error so we can just return an error expression.
                return new BoundLiteralExpression(0);
            }
            // 访问变量时需要检查变量是否声明
            if(!_scope.TryLookup(name,out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundLiteralExpression(0);// 没有的话，默认值是0
            }
            return new BoundVariableExpression(variable);
        }

        public BoundExpression VisitAssignmentExpressionSyntax(AssignmentExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            var boundExpression = VisitExpression(syntax.Expression);
            // 无法访问，说明该变量未定义
            if (!_scope.TryLookup(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return boundExpression;
            }

            if (variable.IsReadonly)
            {
                _diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, name);
            }
            
            // 类型校验: 不允许a = 1 再进行 a = false 
            if (boundExpression.Type != variable.Type)
            {
                _diagnostics.ReportCannotConvert(syntax.Expression.Span, boundExpression.Type, variable.Type);
                return boundExpression;
            }
            
            return new BoundAssignmentExpression(variable, boundExpression);
        }

        public BoundStatement VisitBlockStatement(BlockStatementSyntax syntax)
        {
            // 进入语句块需要创建新的作用域
            _scope = new BoundScope(_scope);
            
            var stmtBuilder = ImmutableArray.CreateBuilder<BoundStatement>();
            foreach (var stmt in syntax.Statements)
            {
                stmtBuilder.Add(VisitStatement(stmt));
            }

            // 退出语句块，恢复作用域
            _scope = _scope.Parent;
            
            return new BoundBlockStatement(stmtBuilder.ToImmutable());
        }

        public BoundStatement VisitExpressionStatement(ExpressionStatementSyntax syntax)
        {
            var boundExpression = VisitExpression(syntax.Expression);
            return new BoundExpressionStatement(boundExpression);
        }

        public BoundStatement VisitVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            var name = syntax.Identifier.Text;
            var initializer = VisitExpression(syntax.Initializer);
            var isReadonly = syntax.Declarator.Kind == SyntaxKind.LetKeyword;
            var variableType = initializer.Type;
            var variable = new VariableSymbol(name, isReadonly, variableType);
            // 检查变量是否在当前作用域重复声明
            if (!_scope.TryDeclare(variable))
            {
                _diagnostics.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);
            }

            return new BoundVariableDeclaration(variable, initializer);
        }

        public BoundStatement VisitIfStatementSyntax(IfStatementSyntax syntax)
        {
            var condition = VisitExpression(syntax.Condition, typeof(bool));
            var thenStatement = VisitStatement(syntax.ThenStatement);
            var elseStatement = syntax.ElseStatement == null ? null : VisitStatement(syntax.ElseStatement);

            return new BoundIfStatement(condition, thenStatement,elseStatement);
        }
        
        private BoundExpression VisitExpression(ExpressionSyntax syntax, Type targetType)
        {
            var result = VisitExpression(syntax);
            if (result.Type != targetType)
                _diagnostics.ReportCannotConvert(syntax.Span, result.Type, targetType);

            return result;
        }

        public BoundStatement VisitElseClauseSyntax(ElseClauseSyntax syntax)
        {
            return VisitStatement(syntax.ElseStatement);
        }

        public BoundStatement VisitWhileStatementSyntax(WhileStatementSyntax syntax)
        {
            var condition = VisitExpression(syntax.Condition, typeof(bool));
            var body = VisitStatement(syntax.Body);
            return new BoundWhileStatement(condition, body);
        }

        public BoundStatement VisitForStatementSyntax(ForStatementSyntax syntax)
        {
            var name = syntax.Identifier.Text;
            // for循环的increment变量不可变
            var variable = new VariableSymbol(name, true, typeof(int));
            var lowerBound = VisitExpression(syntax.LowerBound, typeof(int));
            var upperBound = VisitExpression(syntax.UpperBound, typeof(int));
            _scope = new BoundScope(_scope);
            if (!_scope.TryDeclare(variable))
                _diagnostics.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);
            var body = VisitStatement(syntax.Body);
            _scope = _scope.Parent;
            return new BoundForStatement(variable, lowerBound, upperBound, body);
        }

        public BoundExpression VisitParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
        {
            return VisitExpression(syntax.Expression);
        }

        public static BoundGlobalScope BoundGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax)
        {
            var parentScope = CreateParentScope(previous);
            var binder = new Binder(parentScope);
            var boundStatement = binder.BindStatement(syntax.Statement);
            var variables = binder._scope.GetDeclaredVariables();
            var diagnostics = binder.Diagnostics.ToImmutableArray();

            if (previous != null)
            {
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);
            }
            
            return new BoundGlobalScope(previous,diagnostics,variables,boundStatement);
        }

        private static BoundScope CreateParentScope(BoundGlobalScope previous)
        {
            var stack = new Stack<BoundGlobalScope>();
            // submission1 -> submission2 -> submission3
            // 按提交顺序恢复变量声明
            while (previous!=null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            BoundScope parent = null;

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);
                foreach (var v in previous.Variables)
                {
                    scope.TryDeclare(v);
                }
                
                parent = scope;
            }

            return parent;
        }
    }
}