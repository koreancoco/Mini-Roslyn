using MiniRoslyn.CodeAnalysis.Symbols;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression(VariableSymbol variable)
        {
            Variable = variable;
        }

        public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
        public VariableSymbol Variable { get; }
        public override TResult Accept<TResult>(IBoundExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitBoundVariableExpression(this);
        }

        public override TypeSymbol Type => Variable.Type;
    }
}