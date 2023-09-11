using System;
using System.Collections.Generic;
using MiniRoslyn.CodeAnalysis.Binding;
using MiniRoslyn.CodeAnalysis.Symbols;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Scripting
{
    internal class Evaluator : IBoundExpressionVisitor<object>, IBoundStatementVisitor<object>
    {
        private int _ip = 0;
        private readonly BoundProgram _program;
        private Dictionary<VariableSymbol, object> _globals;
        private readonly Stack<Dictionary<VariableSymbol, object>> _locals = new Stack<Dictionary<VariableSymbol, object>>();
        private Dictionary<BoundLabel,int> _labelToNextIp = new Dictionary<BoundLabel, int>();
        private Random _random;

        public Evaluator(BoundProgram program,Dictionary<VariableSymbol, object> globals)
        {
            _program = program;
            _globals = globals;
            _locals.Push(new Dictionary<VariableSymbol, object>());
        }

        public object Evaluate()
        {
            return VisitBoundBlockStatement(_program.Statement);
        }

        private object VisitBoundStatement(BoundStatement syntax)
        {
            return syntax.Accept(this);
        }
        
        private object VisitBoundExpression(BoundExpression syntax)
        {
            return syntax.Accept(this);
        }
        
        public object VisitBoundLiteralExpression(BoundLiteralExpression syntax)
        {
            return syntax.Value;
        }

        public object VisitBoundBinaryExpression(BoundBinaryExpression syntax)
        {
            var left = VisitBoundExpression(syntax.Left);
            var right = VisitBoundExpression(syntax.Right);
            switch (syntax.Operator.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    if (syntax.Type == TypeSymbol.Int)
                        return (int)left + (int)right;
                    else
                        return (string)left + (string)right;
                case BoundBinaryOperatorKind.Subtraction:
                    return (int)left - (int)right;
                case BoundBinaryOperatorKind.Multiplication:
                    return (int)left * (int)right;
                case BoundBinaryOperatorKind.Division:
                    return (int)left / (int)right;
                case BoundBinaryOperatorKind.LogicalAnd:
                    return (bool)left && (bool)right;
                case BoundBinaryOperatorKind.LogicalOr:
                    return (bool)left || (bool)right;
                case BoundBinaryOperatorKind.Equals:
                    return Equals(left,right);
                case BoundBinaryOperatorKind.NotEquals:
                    return !Equals(left,right);
                case BoundBinaryOperatorKind.Less:
                    return (int)left < (int)right;
                case BoundBinaryOperatorKind.LessOrEquals:
                    return (int)left <= (int)right;
                case BoundBinaryOperatorKind.Greater:
                    return (int)left > (int)right;
                case BoundBinaryOperatorKind.GreaterOrEquals:
                    return (int)left >= (int)right;
                case BoundBinaryOperatorKind.BitwiseAnd:
                    if(syntax.Type == TypeSymbol.Int)
                        return (int)left & (int)right;
                    return (bool)left & (bool)right;
                case BoundBinaryOperatorKind.BitwiseOr:
                    if(syntax.Type == TypeSymbol.Int)
                        return (int)left | (int)right;
                    return (bool)left | (bool)right;
                case BoundBinaryOperatorKind.BitwiseXor:
                    if(syntax.Type == TypeSymbol.Int)
                        return (int)left ^ (int)right;
                    return (bool)left ^ (bool)right;
                default:
                    throw new Exception($"Unexpected binary operator {syntax.Operator.Kind}");
            }
        }

        public object VisitBoundUnaryExpression(BoundUnaryExpression syntax)
        {
            var operand = VisitBoundExpression(syntax.Operand);
            switch (syntax.Operator.Kind)
            {
                case BoundUnaryOperatorKind.Identity:
                    return operand;
                case BoundUnaryOperatorKind.Negation:
                    return -(int)operand;
                case BoundUnaryOperatorKind.LogicalNegation:
                    return !(bool)operand;
                case BoundUnaryOperatorKind.OnesComplement:
                    return ~(int)operand;
                default:
                    throw new Exception($"Unexpected unary operator {syntax.Operator.Kind}");
            }
        }

        public object VisitBoundVariableExpression(BoundVariableExpression syntax)
        {
            if (syntax.Variable.Kind == SymbolKind.GlobalVariable)
            {
                return _globals[syntax.Variable];
            }
            else
            {
                var local = _locals.Peek();
                return local[syntax.Variable];
            }
        }

        public object VisitBoundAssignmentExpression(BoundAssignmentExpression syntax)
        {
            var value = VisitBoundExpression(syntax.Expression);
            Assign(syntax.Variable,value);
            return value;
        }

        public object VisitBoundErrorExpression(BoundErrorExpression syntax)
        {
            throw new NotImplementedException();
        }

        public object VisitBoundCallExpression(BoundCallExpression syntax)
        {
            if (syntax.Function == BuiltinFunctions.Input)
            {
                return Console.ReadLine();
            }
            else if (syntax.Function == BuiltinFunctions.Print)
            {
                var value = (string) VisitBoundExpression(syntax.Arguments[0]);
                Console.WriteLine(value);
                return null;
            }
            else if (syntax.Function == BuiltinFunctions.Random)
            {
                var max = (int)VisitBoundExpression(syntax.Arguments[0]);
                if (_random == null)
                    _random = new Random();
                return _random.Next(max);
            }
            else
            {
                var locals = new Dictionary<VariableSymbol, object>();
                for (int i = 0; i < syntax.Arguments.Length; i++)
                {
                    var parameter = syntax.Function.Parameters[i];
                    var value = VisitBoundExpression(syntax.Arguments[i]);
                    locals.Add(parameter, value);
                }
                
                _locals.Push(locals);

                var statement = _program.Functions[syntax.Function];
                var result = VisitBoundStatement(statement);

                _locals.Pop();

                return result;
            }
        }

        public object VisitBoundConversionExpression(BoundConversionExpression syntax)
        {
            var value = VisitBoundExpression(syntax.Expression);
            if (syntax.Type == TypeSymbol.Int)
            {
                return Convert.ToInt32(value);
            }
            else if (syntax.Type == TypeSymbol.Bool)
            {
                return Convert.ToBoolean(value);
            }
            else if (syntax.Type == TypeSymbol.String)
            {
                return Convert.ToString(value);
            }
            else
            {
                throw new Exception($"Unexpected type {syntax.Type}");
            }
        }

        public object VisitBoundBlockStatement(BoundBlockStatement syntax)
        {
            // 告诉每一个label自己的下一个语句是什么
            for (int i = 0; i < syntax.Statements.Length; i++)
            {
                var statement = syntax.Statements[i];
                if (statement is BoundLabelStatement labelStatement)
                {
                    _labelToNextIp.Add(labelStatement.BoundLabel,i+1);
                }
            }

            object value = null;
            while (_ip < syntax.Statements.Length)
            {
                value = VisitBoundStatement(syntax.Statements[_ip]);
            }
            return value;
        }

        public object VisitBoundExpressionStatement(BoundExpressionStatement syntax)
        {
            var value = VisitBoundExpression(syntax.Expression);
            _ip++;
            return value;
        }

        public object VisitBoundVariableDeclaration(BoundVariableDeclaration syntax)
        {
            var value = VisitBoundExpression(syntax.Initializer);
            Assign(syntax.Variable, value);
            _ip++;
            return value;
        }

        public object VisitBoundIfStatement(BoundIfStatement syntax)
        {
            var condition = (bool)VisitBoundExpression(syntax.Condition);
            
            if (condition)
            {
                return VisitBoundStatement(syntax.ThenStatement);
            }
            else if(syntax.ElseStatement != null)
            {
                return VisitBoundStatement(syntax.ElseStatement);
            }

            return null;
        }

        public object VisitBoundWhileStatement(BoundWhileStatement syntax)
        {
            object value = null;
            while ((bool) VisitBoundExpression(syntax.Condition))
            {
                value = VisitBoundStatement(syntax.Body);
            }
            
            return value;
        }

        public object VisitBoundForStatement(BoundForStatement syntax)
        {
            throw new NotImplementedException();
        }

        public object VisitBoundLabelStatement(BoundLabelStatement syntax)
        {
            _ip++;
            
            return null;
        }

        public object VisitBoundConditionalGotoStatement(BoundConditionalGotoStatement syntax)
        {
            var condition = (bool)VisitBoundExpression(syntax.Condition);
            if (condition == syntax.JumpIfTrue)
            {
                _ip = _labelToNextIp[syntax.BoundLabel];
            }
            else
            {
                _ip++;
            }

            return null;
        }

        public object VisitBoundGotoStatement(BoundGotoStatement syntax)
        {
            _ip = _labelToNextIp[syntax.BoundLabel];

            return null;
        }

        public object VisitBoundDoWhileStatement(BoundDoWhileStatement syntax)
        {
            throw new NotImplementedException();
        }

        public object VisitBoundFunctionDeclaration(BoundFunctionDeclaration syntax)
        {
            throw new NotImplementedException();
        }
        private void Assign(VariableSymbol variable, object value)
        {
            if (variable.Kind == SymbolKind.GlobalVariable)
            {
                _globals[variable] = value;
            }
            else
            {
                var locals = _locals.Peek();
                locals[variable] = value;
            }
        }
    }
}