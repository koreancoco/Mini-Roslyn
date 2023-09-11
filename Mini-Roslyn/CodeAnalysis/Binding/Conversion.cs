using MiniRoslyn.CodeAnalysis.Symbols;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    public class Conversion
    {
        public static readonly Conversion None = new Conversion(false, true, false);

        public static readonly Conversion Identity = new Conversion(true, true, true);
        public static readonly Conversion Implicit = new Conversion(true, false, true);
        public static readonly Conversion Explicit = new Conversion(true, false, false);
        public Conversion(bool exists, bool isIdentity, bool isImplicit)
        {
            Exists = exists;
            IsIdentity = isIdentity;
            IsImplicit = isImplicit;
        }

        // 能否进行类型转换
        public bool Exists { get; }
        public bool IsIdentity { get; }
        public bool IsImplicit { get; }
        public bool IsExplicit => !IsImplicit;

        public static Conversion Classify(TypeSymbol from, TypeSymbol to)
        {
            if (from == to)
            {
                return Identity;
            }

            if (from == TypeSymbol.String)
            {
                if (to == TypeSymbol.Int || to == TypeSymbol.Bool)
                    return Explicit;
            }

            if (from == TypeSymbol.Int || from == TypeSymbol.Bool)
            {
                if (to == TypeSymbol.String)
                    return Explicit;
            }

            return None;
        }
    }
}