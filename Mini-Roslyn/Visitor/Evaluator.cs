using System;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public class Evaluator : IExpressionVisitor<int>
    {
        private ExpressionSyntax _root;

        public Evaluator(ExpressionSyntax root)
        {
            _root = root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }
        
        private int EvaluateExpression(ExpressionSyntax node)
        {
            return Visit(node);
        }
        public int Visit(SyntaxNode node)
        {
            return node.Accept(this);
        }

        public int VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            return this.VisitSyntaxToken(node.LiteralToken);
        }

        public int VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            var left = this.Visit(node.Left);
            var right = this.Visit(node.Right);
            switch (node.OperatorToken.Kind)
            {
                case SyntaxKind.PlusToken:
                    return left + right;
                case SyntaxKind.MinusToken:
                    return left - right;
                case SyntaxKind.StarToken:
                    return left * right;
                case SyntaxKind.SlashToken:
                    return left / right;
                default:
                    throw new Exception($"Unexpected binary operator {node.OperatorToken.Kind}");
            }
        }

        public int VisitParenthesizedExpression(ParenthesizedExpressionSyntax node)
        {
            return this.Visit(node.Expression);
        }

        public int VisitUnaryExpressionSyntax(UnaryExpressionSyntax node)
        {
            var operand = this.Visit(node.Operand);
            switch (node.OperatorToken.Kind)
            {
                case SyntaxKind.PlusToken:
                    return operand;
                case SyntaxKind.MinusToken:
                    return -operand;
                default:
                    throw new Exception($"Unexpected unary operator {node.OperatorToken.Kind}");
            }
        }

        public int VisitSyntaxToken(SyntaxToken token)
        {
            return (int) token.Value;
        }
    }
}