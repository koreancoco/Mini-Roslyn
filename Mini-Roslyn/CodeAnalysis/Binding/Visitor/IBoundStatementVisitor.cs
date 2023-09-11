namespace MiniRoslyn.CodeAnalysis.Binding.Visitor
{
    internal interface IBoundStatementVisitor<TResult>
    {
        TResult VisitBoundBlockStatement(BoundBlockStatement syntax);
        TResult VisitBoundExpressionStatement(BoundExpressionStatement syntax);
        TResult VisitBoundVariableDeclaration(BoundVariableDeclaration syntax);
        TResult VisitBoundIfStatement(BoundIfStatement syntax);
        TResult VisitBoundWhileStatement(BoundWhileStatement syntax);
        TResult VisitBoundForStatement(BoundForStatement syntax);
        TResult VisitBoundLabelStatement(BoundLabelStatement syntax);
        TResult VisitBoundConditionalGotoStatement(BoundConditionalGotoStatement syntax);
        TResult VisitBoundGotoStatement(BoundGotoStatement syntax);

    }
}