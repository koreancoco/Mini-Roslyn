using MiniRoslyn.CodeAnalysis.Symbols;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression(TypeSymbol type, BoundExpression expression)
        {
            Type = type;
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
        public override TypeSymbol Type { get; }
        public BoundExpression Expression { get; } 
        public override TResult Accept<TResult>(IBoundExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitBoundConversionExpression(this);
        }
    }
}