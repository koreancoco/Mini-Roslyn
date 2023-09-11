using MiniRoslyn.CodeAnalysis.Binding;

namespace MiniRoslyn.CodeAnalysis.Visitor
{
    internal interface IBoundExpressionVisitor<TResult>
    {
        TResult VisitBoundLiteralExpression(BoundLiteralExpression syntax);
        TResult VisitBoundBinaryExpression(BoundBinaryExpression syntax);
        TResult VisitBoundUnaryExpression(BoundUnaryExpression syntax);
        TResult VisitBoundVariableExpression(BoundVariableExpression syntax);
        TResult VisitBoundAssignmentExpression(BoundAssignmentExpression syntax);
        TResult VisitBoundErrorExpression(BoundErrorExpression syntax);
        TResult VisitBoundCallExpression(BoundCallExpression syntax);
        TResult VisitBoundConversionExpression(BoundConversionExpression syntax);
    }
}