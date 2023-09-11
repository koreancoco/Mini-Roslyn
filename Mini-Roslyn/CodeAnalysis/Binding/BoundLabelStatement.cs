using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal class BoundLabelStatement : BoundStatement
    {
        public BoundLabelStatement(BoundLabel boundLabel)
        {
            BoundLabel = boundLabel;
        }

        public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;
        public BoundLabel BoundLabel { get; }
        public override TResult Accept<TResult>(IBoundStatementVisitor<TResult> visitor)
        {
            return visitor.VisitBoundLabelStatement(this);
        }
    }
}