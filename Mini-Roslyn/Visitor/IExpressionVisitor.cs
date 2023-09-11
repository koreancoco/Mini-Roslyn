namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public interface IExpressionVisitor<TResult>
    {
        TResult VisitLiteralExpression(LiteralExpressionSyntax node);
        TResult VisitBinaryExpression(BinaryExpressionSyntax node);
        TResult VisitParenthesizedExpression(ParenthesizedExpressionSyntax node);
        TResult VisitUnaryExpressionSyntax(UnaryExpressionSyntax node);
        TResult VisitSyntaxToken(SyntaxToken token);
        
    }
}