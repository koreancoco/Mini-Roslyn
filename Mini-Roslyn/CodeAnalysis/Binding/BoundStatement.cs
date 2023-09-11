using MiniRoslyn.CodeAnalysis.Visitor;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal abstract class BoundStatement : BoundNode
    {
        public abstract TResult Accept<TResult>(IBoundStatementVisitor<TResult> visitor);
    }
}