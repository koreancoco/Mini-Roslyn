using CodeAnalysis.Syntax.InternalSyntax;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    /// <summary>
    /// 词法分析过程中的一些信息
    /// </summary>
    public struct TokenInfo
    {
        public SyntaxKind Kind;
        internal SpecialType ValueKind;
        public string Text; 
        public double DoubleValue; 

    }
}