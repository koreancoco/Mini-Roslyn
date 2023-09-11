using System.IO;
using CodeAnalysis.Utilities;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    public class SyntaxToken : LSharpSyntaxNode
    {
        public virtual string Text
        {
            get { return SyntaxFacts.GetText(this.Kind); }
        }

        /// <summary>
        /// Returns the string representation of this token, not including its leading and trailing trivia.
        /// </summary>
        /// <returns>The string representation of this token, not including its leading and trailing trivia.</returns>
        /// <remarks>The length of the returned string is always the same as Span.Length</remarks>
        public override string ToString()
        {
            return this.Text;
        }
        
        public virtual object Value
        {
            get
            {
                switch (this.Kind)
                {
                    // case SyntaxKind.TrueKeyword:
                    //     return true;
                    // case SyntaxKind.FalseKeyword:
                    //     return false;
                    // case SyntaxKind.NullKeyword:
                    //     return null;
                    default:
                        return this.Text;
                }
            }
        }
        
        
        internal static readonly SyntaxKind FirstTokenWithWellKnownText = SyntaxKind.TildeToken;
        internal static readonly SyntaxKind LastTokenWithWellKnownText = SyntaxKind.EndOfFileToken;
        
        private static readonly ArrayElement<SyntaxToken>[] s_tokensWithNoTrivia = new ArrayElement<SyntaxToken>[(int)LastTokenWithWellKnownText + 1];
        private static readonly ArrayElement<SyntaxToken>[] s_tokensWithSingleTrailingSpace = new ArrayElement<SyntaxToken>[(int)LastTokenWithWellKnownText + 1];
        private static readonly ArrayElement<SyntaxToken>[] s_tokensWithSingleTrailingCRLF = new ArrayElement<SyntaxToken>[(int)LastTokenWithWellKnownText + 1];

        static SyntaxToken()
        {
            for (var kind = FirstTokenWithWellKnownText; kind <= LastTokenWithWellKnownText; kind++)
            {
                s_tokensWithNoTrivia[(int)kind].Value = new SyntaxToken(kind);
                s_tokensWithSingleTrailingSpace[(int)kind].Value = new SyntaxTokenWithTrivia(kind, null, SyntaxFactory.Space);
                s_tokensWithSingleTrailingCRLF[(int)kind].Value = new SyntaxTokenWithTrivia(kind, null, SyntaxFactory.CarriageReturnLineFeed);
            }
        }
        
        internal SyntaxToken(SyntaxKind kind)
            :base(kind)
        {
            FullWidth = this.Text.Length;
        }
        
        internal SyntaxToken(SyntaxKind kind,int fullWidth)
        :base(kind,fullWidth)
        {
        }

        internal static SyntaxToken WithValue<T>(SyntaxKind kind, string text, T value)
        {
            return new SyntaxTokenWithValue<T>(kind, text, value);
        }
        
        
        internal static SyntaxToken WithValue<T>(SyntaxKind kind, LSharpSyntaxNode leading, string text, T value, LSharpSyntaxNode trailing)
        {
            return new SyntaxTokenWithValueAndTrivia<T>(kind, text, value, leading, trailing);
        }

        public static SyntaxToken Create(SyntaxKind kind, LSharpSyntaxNode leading, LSharpSyntaxNode trailing)
        {
            if (leading == null)
            {
                if (trailing == null)
                {
                    return s_tokensWithNoTrivia[(int)kind].Value;
                }
                else if (trailing == SyntaxFactory.Space)
                {
                    return s_tokensWithSingleTrailingSpace[(int)kind].Value;
                }
                else if (trailing == SyntaxFactory.CarriageReturnLineFeed)
                {
                    return s_tokensWithSingleTrailingCRLF[(int)kind].Value;
                }
            }
            return new SyntaxTokenWithTrivia(kind,leading, trailing);
        }

        internal override GreenNode GetSlot(int index)
        {
            throw new System.NotImplementedException();
        }

        public override int GetLeadingTriviaWidth()
        {
            var leading = GetLeadingTrivia();
            return leading != null ? leading.FullWidth : 0;
        }

        public override int GetTrailingTriviaWidth()
        {
            var trailing = GetTrailingTrivia();
            return trailing != null ? trailing.FullWidth : 0;
        }
        
        public override object GetValue()
        {
            return this.Value;
        }

        public virtual string ValueText
        {
            get { return this.Text; }
        }

        public override string GetValueText()
        {
            return this.ValueText;
        }

        internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
        {
            throw new System.NotImplementedException();
        }

        public override int Width
        {
            get
            {
                return this.Text.Length;
            }
        }

        public override bool IsToken
        {
            get
            {
                return true;
            }
        }


        protected internal override void WriteTo(System.IO.TextWriter writer, bool leading, bool trailing)
        {
            if (leading)
            {
                var trivia = this.GetLeadingTrivia();
                if (trivia != null)
                {
                    trivia.WriteTo(writer, true, true);
                }
            }

            writer.Write(this.Text);

            if (trailing)
            {
                var trivia = this.GetTrailingTrivia();
                if (trivia != null)
                {
                    trivia.WriteTo(writer, true, true);
                }
            }
        }
    }
}