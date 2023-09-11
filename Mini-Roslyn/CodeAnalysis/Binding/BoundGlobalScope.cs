using System.Collections.Immutable;
using MiniRoslyn.CodeAnalysis.Diagnostics;
using MiniRoslyn.CodeAnalysis.Symbols;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal class BoundGlobalScope
    {
        public BoundGlobalScope(BoundGlobalScope previous, ImmutableArray<Diagnostic> diagnostics, ImmutableArray<VariableSymbol> variables, ImmutableArray<FunctionSymbol> functions, ImmutableArray<BoundStatement> statements)
        {
            Previous = previous;
            Diagnostics = diagnostics;
            Variables = variables;
            Functions = functions;
            Statements = statements;
        }

        public BoundGlobalScope Previous { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public ImmutableArray<FunctionSymbol> Functions { get; }
        public ImmutableArray<BoundStatement> Statements { get; }
    }
}