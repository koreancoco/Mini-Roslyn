namespace CodeAnalysis.Syntax
{
    public abstract partial class ExpressionSyntax : LSharpSyntaxNode
    {
        internal ExpressionSyntax(CodeAnalysis.Syntax.InternalSyntax.GreenNode green, SyntaxNode parent, int position)
            : base(green, parent, position)
        {
        }
    }
    
    public sealed partial class LiteralExpressionSyntax : ExpressionSyntax
    {
        internal LiteralExpressionSyntax(CodeAnalysis.Syntax.InternalSyntax.LSharpSyntaxNode green, SyntaxNode parent, int position) : 
            base(green, parent, position)
        {
        }

        /// <summary>SyntaxToken representing the keyword corresponding to the kind of the literal expression.</summary>
        public SyntaxToken Token
        {
            get
            {
                return new SyntaxToken(this, ((CodeAnalysis.Syntax.InternalSyntax.LiteralExpressionSyntax)this.Green).token, this.Position, 0);
            }
        }
        
        internal override SyntaxNode GetCachedSlot(int index)
        {
            switch (index)
            {
                default: return null;
            }
        }
    }

    public sealed partial class BinaryExpressionSyntax : ExpressionSyntax
    {
        private ExpressionSyntax left;
        private ExpressionSyntax right;
        internal BinaryExpressionSyntax(Syntax.InternalSyntax.LSharpSyntaxNode green, SyntaxNode parent, int position)
            : base(green, parent, position)
        {
        }
        
        
        public ExpressionSyntax Left
        {
            get { return this.GetRedAtZero(ref left); }
        }
        
        public ExpressionSyntax Right
        {
            get { return this.GetRed(ref right,2); }
        }
        
        internal override SyntaxNode GetCachedSlot(int index)
        {
            switch (index)
            {
                case 0: return this.left;
                case 2: return this.right;
                default: return null;
            }
        }
        
    }

}