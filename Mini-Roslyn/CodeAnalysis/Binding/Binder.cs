using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using MiniRoslyn.CodeAnalysis.Diagnostics;
using MiniRoslyn.CodeAnalysis.Lowering;
using MiniRoslyn.CodeAnalysis.Symbols;
using MiniRoslyn.CodeAnalysis.Syntax;
using MiniRoslyn.CodeAnalysis.Text;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal class Binder : IExpressionVisitor<BoundExpression>, IStatementVisitor<BoundStatement>
    {
        private BoundScope _scope;
        private int _scopeDepth;
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)> _loopStack = new Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)>();
        private int _labelCounter;

        public Binder(BoundScope parent)
        {
            _scope = new BoundScope(parent);
        }
        public DiagnosticBag Diagnostics => _diagnostics;
        
        public static BoundGlobalScope BoundGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax)
        {
            var parentScope = CreateParentScope(previous);
            var binder = new Binder(parentScope);
            var builder = ImmutableArray.CreateBuilder<BoundStatement>(syntax.Members.Length);
            foreach (var member in syntax.Members)
            {
                var boundStatement = binder.VisitStatement(member);
                if (member.Kind == SyntaxKind.GlobalStatement)
                {
                    builder.Add(boundStatement);
                }
            }
            var variables = binder._scope.GetDeclaredVariables();
            var functions = binder._scope.GetDeclaredFunctions();
            var diagnostics = binder.Diagnostics.ToImmutableArray();

            if (previous != null)
            {
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);
            }
            
            return new BoundGlobalScope(previous, diagnostics, variables, functions, builder.ToImmutable());
        }

        public static BoundProgram BindProgram(BoundGlobalScope globalScope)
        {
            var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();

            var scope = globalScope;

            while (scope != null)
            {
                foreach (var function in scope.Functions)
                {
                    var loweredBody = Lowerer.Lower(function.Body);
                    functionBodies.Add(function, loweredBody);
                }

                scope = scope.Previous;
            }
            
            var statement = Lowerer.Lower(new BoundBlockStatement(globalScope.Statements));
            return new BoundProgram(globalScope.Diagnostics, functionBodies.ToImmutable(), statement);
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

            BoundScope parent = CreateRootScope();

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);
                foreach (var v in previous.Variables)
                {
                    scope.TryDeclareVariable(v);
                }

                foreach (var f in previous.Functions)
                {
                    scope.TryDeclareFunction(f);
                }
                
                parent = scope;
            }

            return parent;
        }

        // 预定义内置函数
        private static BoundScope CreateRootScope()
        {
            var scope = new BoundScope(null);
            foreach (var function in BuiltinFunctions.GetAll())
            {
                scope.TryDeclareFunction(function);
            }

            return scope;
        }
        
        private BoundStatement VisitStatement(StatementSyntax syntax)
        {
            return syntax.Accept(this);
        }

        private BoundExpression VisitExpression(ExpressionSyntax syntax, bool canBeVoid = false)
        {
            var result = syntax.Accept(this);
            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                _diagnostics.ReportExpressionMustHaveValue(syntax.Span);
                return new BoundErrorExpression();
            }
            
            return result;
        }
        
        private BoundExpression VisitExpression(ExpressionSyntax syntax, TypeSymbol targetType)
        {
            return BindConversion(syntax, targetType);
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
            if (boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
                return new BoundErrorExpression();
            
            var boundBinaryOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);
            if (boundBinaryOperator == null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return new BoundErrorExpression();
            }
            
            return new BoundBinaryExpression(boundLeft, boundBinaryOperator, boundRight);
        }
        
        public BoundExpression VisitUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = VisitExpression(syntax.Operand);
            if (boundOperand.Type == TypeSymbol.Error)
                return new BoundErrorExpression();
            
            var boundUnaryOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);

            if (boundUnaryOperator == null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
                return new BoundErrorExpression();
            }

            return new BoundUnaryExpression(boundUnaryOperator, boundOperand);
        }

        public BoundExpression VisitNameExpression(NameExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            if (string.IsNullOrEmpty(name))
            {
                // This means the token was inserted by the parser. We already
                // reported error so we can just return an error expression.
                return new BoundErrorExpression();
            }
            // 访问变量时需要检查变量是否声明
            if(!_scope.TryLookupVariable(name,out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundErrorExpression();// 没有的话，默认值是0
            }
            return new BoundVariableExpression(variable);
        }

        public BoundExpression VisitAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            var boundExpression = VisitExpression(syntax.Expression);
            // 无法访问，说明该变量未定义
            if (!_scope.TryLookupVariable(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return boundExpression;
            }

            if (variable.IsReadOnly)
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

        public BoundExpression VisitCallExpression(CallExpressionSyntax syntax)
        {
            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>(syntax.Arguments.Count);

            foreach (var argument in syntax.Arguments)
            {
                boundArguments.Add(VisitExpression(argument));
            }

            if (!_scope.TryLookupFunction(syntax.Identifier.Text,out var function))
            {
                _diagnostics.ReportUndefinedFunction(syntax.Identifier.Span, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            if (syntax.Arguments.Count != function.Parameters.Length)
            {
                _diagnostics.ReportWrongArgumentCount(syntax.Span, function.Name, function.Parameters.Length, syntax.Arguments.Count);
                return new BoundErrorExpression();
            }

            for (var i = 0; i < syntax.Arguments.Count; i++)
            {
                var argument = boundArguments[i];
                var parameter = function.Parameters[i];

                if (argument.Type != parameter.Type)
                {
                    _diagnostics.ReportWrongArgumentType(syntax.Span, parameter.Name, parameter.Type, argument.Type);
                    return new BoundErrorExpression();
                }
            }

            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        public BoundExpression VisitConversionExpression(ConversionExpressionSyntax syntax)
        {
            TypeSymbol.TryGetSymbol(syntax.TypeToken.Text, out var type);
            Debug.Assert(type != null);
            return BindConversion(syntax.Expression, type, true);
        }
        
        private BoundExpression BindConversion(ExpressionSyntax syntax, TypeSymbol type, bool allowExplicit = false)
        {
            var expression = VisitExpression(syntax);
            return BindConversion(syntax.Span, expression, type, allowExplicit);
        }
        
        private BoundExpression BindConversion(TextSpan diagnosticSpan, BoundExpression expression, TypeSymbol type, bool allowExplicit = false)
        {
            var conversion = Conversion.Classify(expression.Type, type);

            if (!conversion.Exists)
            {
                if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                {
                    _diagnostics.ReportCannotConvert(diagnosticSpan, expression.Type, type);
                }

                return new BoundErrorExpression();
            }
            
            if (!allowExplicit && conversion.IsExplicit)
            {
                _diagnostics.ReportCannotConvertImplicitly(diagnosticSpan, expression.Type, type);
            }

            if (conversion.IsIdentity)
                return expression;

            return new BoundConversionExpression(type, expression);
        }

        public BoundStatement VisitBlockStatement(BlockStatementSyntax syntax)
        {
            // 进入语句块需要创建新的作用域
            BeginScope();
            
            var stmtBuilder = ImmutableArray.CreateBuilder<BoundStatement>();
            foreach (var stmt in syntax.Statements)
            {
                stmtBuilder.Add(VisitStatement(stmt));
            }

            // 退出语句块，恢复作用域
            EndScope();
            
            return new BoundBlockStatement(stmtBuilder.ToImmutable());
        }

        public BoundStatement VisitExpressionStatement(ExpressionStatementSyntax syntax)
        {
            var boundExpression = VisitExpression(syntax.Expression, true);
            return new BoundExpressionStatement(boundExpression);
        }

        public BoundStatement VisitVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            var isReadOnly = syntax.Declarator.Kind == SyntaxKind.LetKeyword;
            // if typeClause is specified, then variable declaration is equivalent to assign a converted value to variable
            // var a : int = 10
            // <=>
            // var a = int(10)
            var type = BindTypeClause(syntax.TypeClause);
            var initializer = VisitExpression(syntax.Initializer);
            var variableType = type ?? initializer.Type;
            var variable = BindVariable(syntax.Identifier, isReadOnly, variableType);
            // 除了Conversion表达式之外，只支持隐式转换
            var convertedInitializer = BindConversion(syntax.Initializer, variableType);

            return new BoundVariableDeclaration(variable, convertedInitializer);
        }

        private TypeSymbol BindTypeClause(TypeClauseSyntax syntax)
        {
            if (syntax == null)
                return null;
            if (!TypeSymbol.TryGetSymbol(syntax.Identifier.Text, out var type))
            {
                _diagnostics.ReportUndefinedType(syntax.Identifier.Span, syntax.Identifier.Text);
            }

            return type;
        }

        public BoundStatement VisitIfStatement(IfStatementSyntax syntax)
        {
            var condition = VisitExpression(syntax.Condition, TypeSymbol.Bool);
            var thenStatement = VisitStatement(syntax.ThenStatement);
            var elseStatement = syntax.ElseStatement == null ? null : VisitStatement(syntax.ElseStatement);

            return new BoundIfStatement(condition, thenStatement,elseStatement);
        }

        public BoundStatement VisitElseClause(ElseClauseSyntax syntax)
        {
            return VisitStatement(syntax.ElseStatement);
        }

        public BoundStatement VisitWhileStatement(WhileStatementSyntax syntax)
        {
            var condition = VisitExpression(syntax.Condition, TypeSymbol.Bool);
            var body = VisitLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            return new BoundWhileStatement(condition, body, breakLabel, continueLabel);
        }

        public BoundStatement VisitForStatement(ForStatementSyntax syntax)
        {
            var lowerBound = VisitExpression(syntax.LowerBound, TypeSymbol.Int);
            var upperBound = VisitExpression(syntax.UpperBound, TypeSymbol.Int);
            
            BeginScope();
            
            var variable = BindVariable(syntax.Identifier, isReadOnly: true, TypeSymbol.Int);
            var body = VisitLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            
            EndScope();
            
            return new BoundForStatement(variable, lowerBound, upperBound, body, breakLabel, continueLabel);
        }

        public BoundStatement VisitDoWhileStatement(DoWhileStatementSyntax syntax)
        {
            var condition = VisitExpression(syntax.Condition, TypeSymbol.Bool);
            var statement = VisitLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            return new BoundDoWhileStatement(statement, condition, breakLabel, continueLabel);
        }

        public BoundStatement VisitGlobalStatement(GlobalStatementSyntax syntax)
        {
            return VisitStatement(syntax.Statement);
        }

        private BoundStatement VisitLoopBody(StatementSyntax body, out BoundLabel breakLabel, out BoundLabel continueLabel)
        {
            _labelCounter++;
            breakLabel = new BoundLabel($"break{_labelCounter}");
            continueLabel = new BoundLabel($"continue{_labelCounter}");
            _loopStack.Push((breakLabel,continueLabel));
            var boundBody = VisitStatement(body);
            _loopStack.Pop();
            
            return boundBody;
        }
        
        private BoundStatement BindErrorStatement()
        {
            return new BoundExpressionStatement(new BoundErrorExpression());
        }
        
        public BoundStatement VisitBreakStatement(BreakStatementSyntax syntax)
        {
            if (_loopStack.Count == 0)
            {
                _diagnostics.ReportInvalidBreakOrContinue(syntax.BreakKeyword.Span, syntax.BreakKeyword.Text);
                return BindErrorStatement();
            }
            
            var breakLabel = _loopStack.Peek().BreakLabel;
            return new BoundGotoStatement(breakLabel);
        }

        public BoundStatement VisitContinueStatement(ContinueStatementSyntax syntax)
        {
            if (_loopStack.Count == 0)
            {
                _diagnostics.ReportInvalidBreakOrContinue(syntax.ContinueKeyword.Span, syntax.ContinueKeyword.Text);
                return BindErrorStatement();
            }
            
            var continueLabel = _loopStack.Peek().ContinueLabel;
            return new BoundGotoStatement(continueLabel);
        }

        public BoundStatement VisitFunctionDeclaration(FunctionDeclarationSyntax syntax)
        {
            var functionName = syntax.Identifier.Text;
            var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
            var seenParameterNames = new HashSet<string>();
            foreach (var parameter in syntax.Parameters)
            {
                var parameterName = parameter.Identifier.Text;
                var parameterType = BindTypeClause(parameter.Type);
                if (!seenParameterNames.Add(parameterName))
                {
                    _diagnostics.ReportParameterAlreadyDeclared(parameter.Span, parameterName);
                }
                else
                {
                    parameters.Add(new ParameterSymbol(parameterName, parameterType));
                }
            }

            var type = BindTypeClause(syntax.Type) ?? TypeSymbol.Void;
            
            // 暂时不支持返回值
            if (type != TypeSymbol.Void)
                _diagnostics.XXX_ReportFunctionsAreUnsupported(syntax.Type.Span);
            
            BeginScope();
            // 声明变量
            foreach (var parameter in parameters)
            {
                _scope.TryDeclareVariable(parameter);
            }
            var body = VisitBlockStatement(syntax.Body);
            EndScope();
            
            var functionSymbol = new FunctionSymbol(functionName, parameters.ToImmutable(), type, body);
            if (!_scope.TryDeclareFunction(functionSymbol))
                _diagnostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Span, functionSymbol.Name);
            
            return new BoundFunctionDeclaration(functionSymbol);
        }

        public BoundExpression VisitParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
        {
            return VisitExpression(syntax.Expression);
        }
        
        private VariableSymbol BindVariable(SyntaxToken identifier, bool isReadOnly, TypeSymbol type)
        {
            var name = identifier.Text ?? "?";
            var declare = !identifier.IsMissing;
            VariableSymbol variable;
            if (InGlobalScope())
            {
                variable = new GlobalVariableSymbol(name, isReadOnly, type);
            }
            else
            {
                variable = new LocalVariableSymbol(name, isReadOnly, type);
            }

            if (declare && !_scope.TryDeclareVariable(variable))
                _diagnostics.ReportSymbolAlreadyDeclared(identifier.Span, name);

            return variable;
        }

        private bool InGlobalScope()
        {
            return _scopeDepth == 0;
        }
        
        private void BeginScope()
        {
            _scopeDepth++;
            _scope = new BoundScope(_scope);
        }

        private void EndScope()
        {
            _scopeDepth--;
            _scope = _scope.Parent;
        }
    }
}