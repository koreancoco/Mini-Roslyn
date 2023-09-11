using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal class BoundConditionalGotoStatement : BoundStatement
    {
        public BoundConditionalGotoStatement(BoundExpression condition, BoundLabel boundLabel, bool jumpIfTrue = true)
        {
            Condition = condition;
            BoundLabel = boundLabel;
            JumpIfTrue = jumpIfTrue;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
        public BoundExpression Condition { get; }
        public BoundLabel BoundLabel { get; }
        public bool JumpIfTrue { get; }
        public override TResult Accept<TResult>(IBoundStatementVisitor<TResult> visitor)
        {
            return visitor.VisitBoundConditionalGotoStatement(this);
        }
    }
}