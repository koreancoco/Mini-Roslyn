using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public abstract class ExpressionSyntax : SyntaxNode
    {
        public abstract TResult Accept<TResult>(IExpressionVisitor<TResult> visitor);
    }
}