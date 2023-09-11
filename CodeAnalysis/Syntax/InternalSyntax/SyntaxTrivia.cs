using System.Diagnostics;
using CodeAnalysis.Utilities;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    public class SyntaxTrivia : LSharpSyntaxNode
    {
        public readonly string Text;

        internal SyntaxTrivia(SyntaxKind kind, string text)
            : base(kind,text.Length)
        {
            this.Text = text;
        }
        internal static SyntaxTrivia Create(SyntaxKind kind, string text)
        {
            return new SyntaxTrivia(kind, text);
        }

        // 没有子节点了
        internal override GreenNode GetSlot(int index)
        {
            throw ExceptionUtilities.Unreachable;
        }

        internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
        {
            throw new System.NotImplementedException();
        }

        public override int GetLeadingTriviaWidth()
        {
            return 0;
        }

        public override int GetTrailingTriviaWidth()
        {
            return 0;
        }

        public override int Width
        {
            get
            {
                Debug.Assert(this.FullWidth == this.Text.Length);
                
                return this.FullWidth;
            }
        }
        
        protected internal override void WriteTo(System.IO.TextWriter writer, bool leading, bool trailing)
        {
            writer.Write(Text);
        }
    }
}