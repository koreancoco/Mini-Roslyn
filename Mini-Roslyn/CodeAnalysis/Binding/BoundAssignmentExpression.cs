using System;
using MiniRoslyn.CodeAnalysis.Symbols;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal class BoundAssignmentExpression : BoundExpression
    {
        public BoundAssignmentExpression(VariableSymbol variable, BoundExpression expression)
        {
            Variable = variable;
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
        public override TypeSymbol Type => Expression.Type;
        public override TResult Accept<TResult>(IBoundExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitBoundAssignmentExpression(this);
        }
    }
}