using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public class BreakStatementSyntax : StatementSyntax
    {
        public BreakStatementSyntax(SyntaxToken breakKeyword)
        {
            BreakKeyword = breakKeyword;
        }

        public override SyntaxKind Kind => SyntaxKind.BreakKeyword;
        public SyntaxToken BreakKeyword { get; }

        public override TResult Accept<TResult>(IStatementVisitor<TResult> visitor)
        {
            return visitor.VisitBreakStatement(this);
        }
    }
}