using System.Collections.Immutable;
using MiniRoslyn.CodeAnalysis.Symbols;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal class BoundCallExpression : BoundExpression
    {
        public BoundCallExpression(FunctionSymbol function, ImmutableArray<BoundExpression> arguments)
        {
            Function = function;
            Arguments = arguments;
        }

        public override BoundNodeKind Kind => BoundNodeKind.CallExpression;
        public FunctionSymbol Function { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }
        public override TypeSymbol Type => Function.Type;
        public override TResult Accept<TResult>(IBoundExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitBoundCallExpression(this);
        }
    }
}