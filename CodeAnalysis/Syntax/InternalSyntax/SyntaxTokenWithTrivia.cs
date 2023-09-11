namespace CodeAnalysis.Syntax.InternalSyntax
{
    public class SyntaxTokenWithTrivia : SyntaxToken
    {
        private readonly LSharpSyntaxNode _leading;
        private readonly LSharpSyntaxNode _trailing;
        internal SyntaxTokenWithTrivia(SyntaxKind kind,LSharpSyntaxNode leading, LSharpSyntaxNode trailing) 
            : base(kind)
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