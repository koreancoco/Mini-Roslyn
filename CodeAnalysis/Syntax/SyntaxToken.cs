using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CodeAnalysis.Syntax.InternalSyntax;

namespace CodeAnalysis.Syntax
{
    [StructLayout(LayoutKind.Auto)]
    [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
    public struct SyntaxToken : IEquatable<SyntaxToken>
    {
        internal SyntaxToken(SyntaxNode parent, GreenNode token, int position, int index)
        {
            Debug.Assert(parent == null || !parent.Green.IsList, "list cannot be a parent");
            Debug.Assert(token == null || token.IsToken, "token must be a token");
            Parent = parent;
            Node = token;
            Position = position;
            Index = index;
        }

        private string GetDebuggerDisplay()
        {
            return GetType().Name + " " + (Node != null ? Node.KindText : "None") + " " + ToString();
        }
        
        /// <summary>
        /// An integer representing the language specific kind of this token.
        /// </summary>
        public int RawKind => Node?.RawKind ?? 0;

        /// <summary>
        /// The language name that this token is syntax of.
        /// </summary>
        public string Language => Node?.Language ?? string.Empty;


        /// <summary>
        /// The node that contains this token in its Children collection.
        /// </summary>
        public SyntaxNode Parent { get; }

        internal GreenNode Node { get; }

        internal int Index { get; }

        internal int Position { get; }
        
        /// <summary>
        /// The width of the token in characters, not including its leading and trailing trivia.
        /// </summary>
        internal int Width => Node?.Width ?? 0;

        /// <summary>
        /// The complete width of the token in characters including its leading and trailing trivia.
        /// </summary>
        internal int FullWidth => Node?.FullWidth ?? 0;

        public TextSpan Span
        {
            get
            {   
                return Node != null ? new TextSpan(Position + Node.GetLeadingTriviaWidth(), Node.Width) : default(TextSpan);
            }
        }
        
        internal int EndPosition
        {
            get { return Node != null ? Position + Node.FullWidth : 0; }
        }

        public int SpanStart
        {
            get
            {
                return Node != null ? Position + Node.GetLeadingTriviaWidth() : 0;
            }
        }

        public TextSpan FullSpan => new TextSpan(Position, FullWidth);

        public object Value => Node?.GetValue();
        
        public string ValueText => Node?.GetValueText();

        public string Text => ToString();
        
        /// <summary>
        /// Returns the string representation of this token, not including its leading and trailing trivia.
        /// </summary>
        /// <returns>The string representation of this token, not including its leading and trailing trivia.</returns>
        /// <remarks>The length of the returned string is always the same as Span.Length</remarks>
        public override string ToString()
        {
            return (Node != null ? Node.ToString() : string.Empty);
        }
        
        /// <summary>
        /// Returns the full string representation of this token including its leading and trailing trivia.
        /// </summary>
        /// <returns>The full string representation of this token including its leading and trailing trivia.</returns>
        /// <remarks>The length of the returned string is always the same as FullSpan.Length</remarks>
        public string ToFullString()
        {
            return Node != null ? Node.ToFullString() : string.Empty;
        }
        
        /// <summary>
        /// Writes the full text of this token to the specified <paramref name="writer"/>.
        /// </summary>
        public void WriteTo(System.IO.TextWriter writer)
        {
            Node?.WriteTo(writer);
        }
        
        /// <summary>
        /// Writes the text of this token to the specified TextWriter, optionally including trivia.
        /// </summary>
        internal void WriteTo(System.IO.TextWriter writer, bool leading, bool trailing)
        {
            Node?.WriteTo(writer, leading, trailing);
        }

        /// <summary>
        /// Determines whether this token has any leading trivia.
        /// </summary>
        public bool HasLeadingTrivia
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether this token has any trailing trivia.
        /// </summary>
        public bool HasTrailingTrivia => throw new NotImplementedException();
        
        /// <summary>
        /// Full width of the leading trivia of this token.
        /// </summary>
        internal int LeadingWidth => Node?.GetLeadingTriviaWidth() ?? 0;
        
        /// <summary>
        /// Full width of the trailing trivia of this token.
        /// </summary>
        internal int TrailingWidth => Node?.GetTrailingTriviaWidth() ?? 0;

        /// <summary>
        /// Determines whether two <see cref="SyntaxToken"/>s are equal.
        /// </summary>
        public static bool operator ==(SyntaxToken left, SyntaxToken right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="SyntaxToken"/>s are unequal.
        /// </summary>
        public static bool operator !=(SyntaxToken left, SyntaxToken right)
        {
            return !left.Equals(right);
        }
        
        /// <summary>
        /// Determines whether the supplied <see cref="SyntaxToken"/> is equal to this
        /// <see cref="SyntaxToken"/>.
        /// </summary>
        public bool Equals(SyntaxToken other)
        {
            return Parent == other.Parent &&
                   Node == other.Node &&
                   Position == other.Position &&
                   Index == other.Index;
        }
        
        /// <summary>
        /// The list of trivia that appear before this token in the source code.
        /// </summary>
        public SyntaxTriviaList LeadingTrivia
        {
            get
            {
                return Node != null
                    ? new SyntaxTriviaList(this, Node.GetLeadingTriviaCore(), this.Position)
                    : default(SyntaxTriviaList);
            }
        }

        
    }
}