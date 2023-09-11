using MiniRoslyn.CodeAnalysis.Syntax;

namespace MiniRoslyn.CodeAnalysis.Visitor
{
    public interface IExpressionVisitor<TResult>
    {
        TResult VisitLiteralExpression(LiteralExpressionSyntax syntax);
        TResult VisitBinaryExpression(BinaryExpressionSyntax syntax);
        TResult VisitParenthesizedExpression(ParenthesizedExpressionSyntax syntax);
        TResult VisitUnaryExpression(UnaryExpressionSyntax syntax);
        TResult VisitNameExpression(NameExpressionSyntax syntax);
        TResult VisitAssignmentExpression(AssignmentExpressionSyntax syntax);
        TResult VisitCallExpression(CallExpressionSyntax syntax);
        TResult VisitConversionExpression(ConversionExpressionSyntax syntax);
    }
}