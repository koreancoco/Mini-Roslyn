using CodeAnalysis.Utilities;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    public class WithLotsOfChildren : WithManyChildrenBase
    {
        private readonly int[] _childOffsets;

        public WithLotsOfChildren(ArrayElement<LSharpSyntaxNode>[] children) : base(children)
        {
            _childOffsets = CalculateOffsets(children);

        }

        internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
        {
            throw new System.NotImplementedException();
        }

        public override int GetSlotOffset(int index)
        {
            return _childOffsets[index];
        }
        
        private static int[] CalculateOffsets(ArrayElement<LSharpSyntaxNode>[] children)
        {
            int n = children.Length;
            var childOffsets = new int[n];
            int offset = 0;
            for (int i = 0; i < n; i++)
            {
                childOffsets[i] = offset;
                offset += children[i].Value.FullWidth;
            }
            return childOffsets;
        }
    }
}