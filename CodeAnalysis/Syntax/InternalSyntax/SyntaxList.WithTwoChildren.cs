namespace CodeAnalysis.Syntax.InternalSyntax
{
    public class WithTwoChildren : SyntaxList
    {
        private readonly LSharpSyntaxNode _child0;
        private readonly LSharpSyntaxNode _child1;
        
        public WithTwoChildren(LSharpSyntaxNode child0, LSharpSyntaxNode child1)
        {
            this.SlotCount = 2;
            _child0 = child0;
            this.AdjustFlagsAndWidth(child0);
            _child1 = child1;
            this.AdjustFlagsAndWidth(child1);
        }


        internal override GreenNode GetSlot(int index)
        {
            switch (index)
            {
                case 0:
                    return _child0;
                case 1:
                    return _child1;
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