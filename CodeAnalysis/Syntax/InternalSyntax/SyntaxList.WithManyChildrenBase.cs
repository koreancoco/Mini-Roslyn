using CodeAnalysis.Utilities;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    public abstract class WithManyChildrenBase : SyntaxList
    {
        internal readonly ArrayElement<LSharpSyntaxNode>[] children;

        protected WithManyChildrenBase(ArrayElement<LSharpSyntaxNode>[] children)
        {
            this.children = children;
            this.InitializeChildren();
        }

        private void InitializeChildren()
        {
            int n = children.Length;
            if (n < byte.MaxValue)
            {
                this.SlotCount = (byte)n;
            }
            else
            {
                this.SlotCount = byte.MaxValue;
            }
            for (int i = 0; i < children.Length; i++)
            {
                this.AdjustFlagsAndWidth(children[i]);
            }
        }
        
        internal override GreenNode GetSlot(int index)
        {
            return this.children[index];
        }
    }
}