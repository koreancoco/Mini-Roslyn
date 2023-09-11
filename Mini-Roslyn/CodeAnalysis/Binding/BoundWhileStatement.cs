using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal class BoundWhileStatement : BoundLoopStatement
    {
        public BoundWhileStatement(BoundExpression condition, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel) 
            : base(breakLabel, continueLabel)
        {
            Condition = condition;
            Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
        public override TResult Accept<TResult>(IBoundStatementVisitor<TResult> visitor)
        {
            return visitor.VisitBoundWhileStatement(this);
        }
    }
}