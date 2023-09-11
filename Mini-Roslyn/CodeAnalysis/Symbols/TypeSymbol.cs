namespace MiniRoslyn.CodeAnalysis.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Error = new TypeSymbol("?");
        public static readonly TypeSymbol Bool = new TypeSymbol("bool");
        public static readonly TypeSymbol Int = new TypeSymbol("int");
        public static readonly TypeSymbol String = new TypeSymbol("string");
        public static readonly TypeSymbol Void = new TypeSymbol("void");

        internal TypeSymbol(string name) : base(name)
        {
        }

        public override SymbolKind Kind => SymbolKind.Type;

        public static bool TryGetSymbol(string name,out TypeSymbol type)
        {
            type = null;
            switch (name)
            {
                case "bool":
                    type = Bool;
                    break;
                case "int":
                    type = Int;
                    break;
                case "string":
                    type = String;
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}