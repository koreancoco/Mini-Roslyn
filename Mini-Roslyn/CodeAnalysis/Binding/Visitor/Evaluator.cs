using System;
using System.Collections.Generic;
using MiniRoslyn.CodeAnalysis.Syntax;

namespace MiniRoslyn.CodeAnalysis.Binding.Visitor
{
    internal class Evaluator : IBoundExpressionVisitor<object>, IBoundStatementVisitor<object>
    {
        private object _lastValue;
        private int _ip = 0;
        private BoundBlockStatement _root;
        private Dictionary<VariableSymbol, object> _variables;
        private Dictionary<LabelSymbol,int> _labelToNextIp = new Dictionary<LabelSymbol, int>();

        public Evaluator(BoundBlockStatement root,Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            _variables = variables;
        }

        public object Evaluate()
        {
            VisitBoundBlockStatement(_root);
            return _lastValue;
        }

        private void VisitBoundStatement(BoundStatement syntax)
        {
            syntax.Accept(this);
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
                    return (int)left + (int)right;
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
                    if(syntax.Type == typeof(int))
                        return (int)left & (int)right;
                    return (bool)left & (bool)right;
                case BoundBinaryOperatorKind.BitwiseOr:
                    if(syntax.Type == typeof(int))
                        return (int)left | (int)right;
                    return (bool)left | (bool)right;
                case BoundBinaryOperatorKind.BitwiseXor:
                    if(syntax.Type == typeof(int))
                        return (int)left ^ (int)right;
                    return (bool)left ^ (bool)right;
                default:
                    throw new Exception($"Unexpected binary operator {syntax.Operator.Kind}");
            }
        }

        public object VisitBoundUnaryExpressionSyntax(BoundUnaryExpression syntax)
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
            return _variables[syntax.Variable];
        }

        public object VisitBoundAssignmentExpression(BoundAssignmentExpression syntax)
        {
            var value = VisitBoundExpression(syntax.Expression);
            _variables[syntax.Variable] = value;
            return value;
        }

        public object VisitBoundBlockStatement(BoundBlockStatement syntax)
        {
            // 告诉每一个label自己的下一个语句是什么
            for (int i = 0; i < syntax.Statements.Length; i++)
            {
                var statement = syntax.Statements[i];
                if (statement is BoundLabelStatement labelStatement)
                {
                    _labelToNextIp.Add(labelStatement.Label,i+1);
                }
            }

            while (_ip < syntax.Statements.Length)
            {
                VisitBoundStatement(syntax.Statements[_ip]);
            }

            return _lastValue;
        }

        public object VisitBoundExpressionStatement(BoundExpressionStatement syntax)
        {
            _lastValue = VisitBoundExpression(syntax.Expression);
            _ip++;

            return _lastValue;
        }

        public object VisitBoundVariableDeclaration(BoundVariableDeclaration syntax)
        {
            var value = VisitBoundExpression(syntax.Initializer);
            _variables[syntax.Variable] = value;
            _lastValue = value;
            _ip++;
            
            return _lastValue;
        }

        public object VisitBoundIfStatement(BoundIfStatement syntax)
        {
            var condition = (bool)VisitBoundExpression(syntax.Condition);
            if (condition)
            {
                VisitBoundStatement(syntax.ThenStatement);
            }
            else if(syntax.ElseStatement != null)
            {
                VisitBoundStatement(syntax.ElseStatement);
            }
            
            return _lastValue;
        }

        public object VisitBoundWhileStatement(BoundWhileStatement syntax)
        {
            while ((bool) VisitBoundExpression(syntax.Condition))
            {
                VisitBoundStatement(syntax.Body);
            }
            
            return _lastValue;
        }

        public object VisitBoundForStatement(BoundForStatement syntax)
        {
            throw new NotImplementedException();
        }

        public object VisitBoundLabelStatement(BoundLabelStatement syntax)
        {
            _ip++;
            
            return _lastValue;
        }

        public object VisitBoundConditionalGotoStatement(BoundConditionalGotoStatement syntax)
        {
            var condition = (bool)VisitBoundExpression(syntax.Condition);
            if (condition && !syntax.JumpIfFalse
                || !condition && syntax.JumpIfFalse)
            {
                _ip = _labelToNextIp[syntax.Label];
            }
            else
            {
                _ip++;
            }

            return _lastValue;
        }

        public object VisitBoundGotoStatement(BoundGotoStatement syntax)
        {
            _ip = _labelToNextIp[syntax.Label];

            return _lastValue;
        }
    }
}