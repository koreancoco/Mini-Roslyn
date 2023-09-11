using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public class ConversionExpressionSyntax : ExpressionSyntax
    {
        public ConversionExpressionSyntax(SyntaxToken typeToken, SyntaxToken openParenthesis, ExpressionSyntax expression, SyntaxToken closeParenthesis)
        {
            TypeToken = typeToken;
            OpenParenthesis = openParenthesis;
            Expression = expression;
            CloseParenthesis = closeParenthesis;
        }

        public override SyntaxKind Kind => SyntaxKind.ConversionExpression;
        public SyntaxToken TypeToken { get; }
        public SyntaxToken OpenParenthesis { get; }
        public ExpressionSyntax Expression { get; }
        public SyntaxToken CloseParenthesis {get;}
        public override TResult Accept<TResult>(IExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitConversionExpression(this);
        }
    }
}