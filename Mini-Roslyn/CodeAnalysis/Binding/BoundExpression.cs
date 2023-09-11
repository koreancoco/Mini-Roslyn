using System.IO;
using MiniRoslyn.CodeAnalysis.Symbols;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract TypeSymbol Type { get; }
        public abstract TResult Accept<TResult>(IBoundExpressionVisitor<TResult> visitor);
    }
}