using System.Collections.Immutable;
using MiniRoslyn.CodeAnalysis.Symbols;
using MiniRoslyn.CodeAnalysis.Syntax;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal class BoundFunctionDeclaration : BoundStatement
    {
        public BoundFunctionDeclaration(FunctionSymbol function)
        {
            Function = function;
        }

        public override BoundNodeKind Kind => BoundNodeKind.FunctionDeclaration;
        public FunctionSymbol Function { get; }
        public override TResult Accept<TResult>(IBoundStatementVisitor<TResult> visitor)
        {
            return visitor.VisitBoundFunctionDeclaration(this);
        }
    }
}