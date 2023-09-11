using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public class GlobalStatementSyntax : MemberSyntax
    {
        public GlobalStatementSyntax(StatementSyntax statement)
        {
            Statement = statement;
        }

        public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
        public StatementSyntax Statement { get; }
        public override TResult Accept<TResult>(IStatementVisitor<TResult> visitor)
        {
            return visitor.VisitGlobalStatement(this);
        }
    }
}