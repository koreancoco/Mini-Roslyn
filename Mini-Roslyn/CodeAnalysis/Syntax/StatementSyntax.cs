using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public abstract class StatementSyntax : SyntaxNode
    {
        public abstract TResult Accept<TResult>(IStatementVisitor<TResult> visitor);
    }
}