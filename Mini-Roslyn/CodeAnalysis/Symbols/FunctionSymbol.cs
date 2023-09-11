using System.Collections.Immutable;
using MiniRoslyn.CodeAnalysis.Binding;
using MiniRoslyn.CodeAnalysis.Syntax;

namespace MiniRoslyn.CodeAnalysis.Symbols
{
    internal class FunctionSymbol : Symbol
    {
        internal FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol type, BoundStatement body = null) 
            : base(name)
        {
            Parameters = parameters;
            Type = type;
            Body = body;
        }

        public override SymbolKind Kind => SymbolKind.Function;
        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol Type { get; }
        public BoundStatement Body { get; }
    }
}