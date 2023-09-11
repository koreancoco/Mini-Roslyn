namespace MiniRoslyn.CodeAnalysis.Binding.Visitor
{
    internal interface IBoundExpressionVisitor<TResult>
    {
        TResult VisitBoundLiteralExpression(BoundLiteralExpression syntax);
        TResult VisitBoundBinaryExpression(BoundBinaryExpression syntax);
        TResult VisitBoundUnaryExpressionSyntax(BoundUnaryExpression syntax);
        TResult VisitBoundVariableExpression(BoundVariableExpression syntax);
        TResult VisitBoundAssignmentExpression(BoundAssignmentExpression syntax);
    }
}