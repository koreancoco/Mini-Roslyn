using System.Collections.Generic;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public class UnaryExpressionSyntax : ExpressionSyntax
    {
        public override SyntaxKind Kind
        {
            get { return SyntaxKind.UnaryExpression; }
        }

        public UnaryExpressionSyntax(SyntaxToken operatorToken, ExpressionSyntax operand)
        {
            OperatorToken = operatorToken;
            Operand = operand;
        }

        public SyntaxToken OperatorToken { get; set; }
        public ExpressionSyntax Operand { get; set; }
        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }
    }
}