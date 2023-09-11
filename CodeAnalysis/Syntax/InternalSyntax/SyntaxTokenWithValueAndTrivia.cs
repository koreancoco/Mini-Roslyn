namespace CodeAnalysis.Syntax.InternalSyntax
{
    public class SyntaxTokenWithValueAndTrivia<T> : SyntaxTokenWithValue<T>
    {
        private readonly LSharpSyntaxNode _leading;
        private readonly LSharpSyntaxNode _trailing;
        public SyntaxTokenWithValueAndTrivia(SyntaxKind kind, string text, T value,LSharpSyntaxNode leading, LSharpSyntaxNode trailing) 
            : base(kind, text, value)
        {
            if (leading != null)
            {
                this.AdjustFlagsAndWidth(leading);
                _leading = leading;
            }
            if (trailing != null)
            {
                this.AdjustFlagsAndWidth(trailing);
                _trailing = trailing;
            }
        }

        public override LSharpSyntaxNode GetLeadingTrivia()
        {
            return _leading;
        }

        public override LSharpSyntaxNode GetTrailingTrivia()
        {
            return _trailing;
        }
    }
}