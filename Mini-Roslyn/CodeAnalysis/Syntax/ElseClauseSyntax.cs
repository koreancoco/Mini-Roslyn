using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public class ElseClauseSyntax : StatementSyntax
    {
        public ElseClauseSyntax(SyntaxToken elseKeyword, StatementSyntax elseStatement)
        {
            ElseKeyword = elseKeyword;
            ElseStatement = elseStatement;
        }

        public override SyntaxKind Kind => SyntaxKind.ElseClause;
        public SyntaxToken ElseKeyword { get; }
        public StatementSyntax ElseStatement { get; }
        public override TResult Accept<TResult>(IStatementVisitor<TResult> visitor)
        {
            return visitor.VisitElseClause(this);
        }
    }
}