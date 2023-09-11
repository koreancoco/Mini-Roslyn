using MiniRoslyn.CodeAnalysis.Syntax;

namespace MiniRoslyn.CodeAnalysis.Visitor
{
    public interface IStatementVisitor<TResult>
    {
        TResult VisitBlockStatement(BlockStatementSyntax syntax);
        TResult VisitExpressionStatement(ExpressionStatementSyntax syntax);
        TResult VisitVariableDeclaration(VariableDeclarationSyntax syntax);
        TResult VisitIfStatement(IfStatementSyntax syntax);
        TResult VisitElseClause(ElseClauseSyntax syntax);
        TResult VisitWhileStatement(WhileStatementSyntax syntax);
        TResult VisitForStatement(ForStatementSyntax syntax);
        TResult VisitDoWhileStatement(DoWhileStatementSyntax syntax);
        TResult VisitGlobalStatement(GlobalStatementSyntax syntax);
        TResult VisitFunctionDeclaration(FunctionDeclarationSyntax syntax);
        TResult VisitBreakStatement(BreakStatementSyntax syntax);
        TResult VisitContinueStatement(ContinueStatementSyntax syntax);
    }
}