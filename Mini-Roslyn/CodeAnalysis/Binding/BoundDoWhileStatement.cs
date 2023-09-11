using MiniRoslyn.CodeAnalysis.Syntax;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal class BoundDoWhileStatement : BoundLoopStatement
    {
        public BoundDoWhileStatement(BoundStatement body, BoundExpression condition, BoundLabel breakLabel, BoundLabel continueLabel)
            : base(breakLabel, continueLabel)
        {
            Body = body;
            Condition = condition;
        }

        public override BoundNodeKind Kind => BoundNodeKind.DoWhileStatement;
        public BoundStatement Body { get; }
        public BoundExpression Condition { get; }
        public override TResult Accept<TResult>(IBoundStatementVisitor<TResult> visitor)
        {
            return visitor.VisitBoundDoWhileStatement(this);
        }
    }
}