using System;
using MiniRoslyn.CodeAnalysis.Symbols;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal class BoundLiteralExpression : BoundExpression
    {
        public BoundLiteralExpression(object value)
        {
            Value = value;

            if (value is bool)
                Type = TypeSymbol.Bool;
            else if (value is int)
                Type = TypeSymbol.Int;
            else if (value is string)
                Type = TypeSymbol.String;
            else
                throw new Exception($"Unexpected literal '{value}' of type {value.GetType()}");
        }

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public override TypeSymbol Type { get; }
        public object Value { get; }
        public override TResult Accept<TResult>(IBoundExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitBoundLiteralExpression(this);
        }
    }
}