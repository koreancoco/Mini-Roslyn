using CodeAnalysis.Syntax.InternalSyntax;

namespace CodeAnalysis.Syntax
{
    public abstract class LSharpSyntaxNode : SyntaxNode
    {
        internal LSharpSyntaxNode(GreenNode green, SyntaxNode parent, int position)
            : base(green, parent, position)
        {
        }
        
        /// <summary>
        /// Returns the <see cref="SyntaxKind"/> of the node.
        /// </summary>
        public SyntaxKind Kind()
        {
            return (SyntaxKind)this.Green.RawKind;
        }

        protected override string KindText
        {
            get
            {
                return Kind().ToString();
            }
        }

        public override string Language
        {
            get { return LanguageNames.LSharp; }
        }
        
        
    }
}