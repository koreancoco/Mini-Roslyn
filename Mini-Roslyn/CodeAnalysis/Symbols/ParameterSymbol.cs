namespace MiniRoslyn.CodeAnalysis.Symbols
{
    public class ParameterSymbol : VariableSymbol
    {
        internal ParameterSymbol(string name, TypeSymbol type) 
            : base(name, true, type)
        {
        }

        public override SymbolKind Kind => SymbolKind.Parameter;
    }
}