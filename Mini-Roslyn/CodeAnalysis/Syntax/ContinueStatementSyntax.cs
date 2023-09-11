using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public class ContinueStatementSyntax : StatementSyntax
    {
        public ContinueStatementSyntax(SyntaxToken continueKeyword)
        {
            ContinueKeyword = continueKeyword;
        }

        public override SyntaxKind Kind => SyntaxKind.ContinueKeyword;
        
        public SyntaxToken ContinueKeyword { get; }

        public override TResult Accept<TResult>(IStatementVisitor<TResult> visitor)
        {
            return visitor.VisitContinueStatement(this);
        }
    }
}