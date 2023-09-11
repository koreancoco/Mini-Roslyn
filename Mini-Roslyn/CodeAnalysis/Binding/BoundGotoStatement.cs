using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal class BoundGotoStatement : BoundStatement
    {
        public BoundGotoStatement(BoundLabel boundLabel)
        {
            BoundLabel = boundLabel;
        }

        public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;
        public BoundLabel BoundLabel;
        public override TResult Accept<TResult>(IBoundStatementVisitor<TResult> visitor)
        {
            return visitor.VisitBoundGotoStatement(this);
        }
    }
}