using CodeAnalysis.Utilities;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    public class WithManyChildren : WithManyChildrenBase
    {
        public WithManyChildren(ArrayElement<LSharpSyntaxNode>[] children) : base(children)
        {
        }

        internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
        {
            throw new System.NotImplementedException();
        }
    }
}