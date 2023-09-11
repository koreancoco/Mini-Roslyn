using System.IO;
using CodeAnalysis.Utilities;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    public abstract class LSharpSyntaxNode : GreenNode
    {
        public override string Language
        {
            get { return LanguageNames.LSharp; }
        }

        public SyntaxKind Kind
        {
            get { return (SyntaxKind)this.RawKind; }
        }

        public override string KindText
        {
            get
            {
                return this.Kind.ToString();
            }
        }
        
        internal LSharpSyntaxNode(SyntaxKind kind)
            : base((ushort)kind)
        {
            
        }
        
        internal LSharpSyntaxNode(SyntaxKind kind,int fullWidth)
            : base((ushort)kind,fullWidth)
        {
            
        }
        
        public virtual LSharpSyntaxNode GetLeadingTrivia()
        {
            return null;
        }
        
        public virtual LSharpSyntaxNode GetTrailingTrivia()
        {
            return null;
        }

        public override string ToFullString()
        {
            var sb = PooledStringBuilder.GetInstance();
            var writer = new System.IO.StringWriter(sb.Builder, System.Globalization.CultureInfo.InvariantCulture);
            this.WriteTo(writer, leading: true, trailing: true);
            return sb.ToStringAndFree();
        }

        public override GreenNode GetLeadingTriviaCore()
        {
            return GetLeadingTrivia();
        }

        public override int GetSlotOffset(int index)
        {
            // This implementation should not support arbitrary
            // length lists since the implementation is O(n).
            System.Diagnostics.Debug.Assert(index < 11); // Max. slots 11 (TypeDeclarationSyntax)

            int offset = 0;
            for (int i = 0; i < index; i++)
            {
                var child = this.GetSlot(i);
                if (child != null)
                {
                    offset += child.FullWidth;
                }
            }

            return offset;
        }
    }
}