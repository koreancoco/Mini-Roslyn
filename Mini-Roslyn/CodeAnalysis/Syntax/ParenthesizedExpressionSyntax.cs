using System.Collections.Generic;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ParenthesizedExpression; }
        }

        public ParenthesizedExpressionSyntax(SyntaxToken openParenToken, ExpressionSyntax expression, SyntaxToken closeParenToken)
        {
            OpenParenToken = openParenToken;
            Expression = expression;
            CloseParenToken = closeParenToken;
        }

        public SyntaxToken OpenParenToken { get; set; }
        public ExpressionSyntax Expression { get; set; }
        public SyntaxToken CloseParenToken { get; set; }

        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitParenthesizedExpression(this);
        }
    }
}