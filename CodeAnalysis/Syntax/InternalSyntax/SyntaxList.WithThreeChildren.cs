namespace CodeAnalysis.Syntax.InternalSyntax
{
    public class WithThreeChildren : SyntaxList
    {
        private readonly LSharpSyntaxNode _child0;
        private readonly LSharpSyntaxNode _child1;
        private readonly LSharpSyntaxNode _child2;

        public WithThreeChildren(LSharpSyntaxNode child0, LSharpSyntaxNode child1, LSharpSyntaxNode child2)
        {
            this.SlotCount = 3;
            _child0 = child0;
            this.AdjustFlagsAndWidth(child0);
            _child1 = child1;
            this.AdjustFlagsAndWidth(child1);
            _child2 = child2;
            this.AdjustFlagsAndWidth(child2);
        }
        
        internal override GreenNode GetSlot(int index)
        {
            switch (index)
            {
                case 0:
                    return _child0;
                case 1:
                    return _child1;
                case 2:
                    return _child2;
                default:
                    return null;
            }
        }

        internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
        {
            throw new System.NotImplementedException();
        }
    }
}