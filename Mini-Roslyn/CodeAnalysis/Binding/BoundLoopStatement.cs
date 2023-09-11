using MiniRoslyn.CodeAnalysis.Binding;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal abstract class BoundLoopStatement : BoundStatement
    {
        protected BoundLoopStatement(BoundLabel breakLabel, BoundLabel continueLabel)
        {
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
        }

        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
    }
}