using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CodeAnalysis.Syntax.InternalSyntax;

namespace CodeAnalysis.Syntax
{
    [DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
    public struct SyntaxTrivia : IEquatable<SyntaxTrivia>
    {
        internal SyntaxTrivia(SyntaxToken token, GreenNode triviaNode, int position, int index)
        {
            Token = token;
            UnderlyingNode = triviaNode;
            Position = position;
            Index = index;

            Debug.Assert(this.RawKind != 0 || this.Equals(default(SyntaxTrivia)));
        }
        
        /// <summary>
        /// An integer representing the language specific kind of this trivia.
        /// </summary>
        public int RawKind => UnderlyingNode?.RawKind ?? 0;
        
        private string GetDebuggerDisplay()
        {
            return GetType().Name + " " + (UnderlyingNode?.KindText ?? "None") + " " + ToString();
        }
        
        /// <summary>
        /// The parent token that contains this token in its LeadingTrivia or TrailingTrivia collection.
        /// </summary>
        public SyntaxToken Token { get; }

        internal GreenNode UnderlyingNode { get; }

        internal int Position { get; }

        internal int Index { get; }
        
        
        /// <summary>
        /// Determines whether two <see cref="SyntaxTrivia"/>s are equal.
        /// </summary>
        public static bool operator ==(SyntaxTrivia left, SyntaxTrivia right)
        {
            return left.Equals(right);
        }
        
        /// <summary>
        /// Determines whether two <see cref="SyntaxTrivia"/>s are unequal.
        /// </summary>
        public static bool operator !=(SyntaxTrivia left, SyntaxTrivia right)
        {
            return !left.Equals(right);
        }
        
        /// <summary>
        /// Determines whether the supplied <see cref="SyntaxTrivia"/> is equal to this
        /// <see cref="SyntaxTrivia"/>.
        /// </summary>
        public bool Equals(SyntaxTrivia other)
        {
            return Token == other.Token && UnderlyingNode == other.UnderlyingNode && Position == other.Position && Index == other.Index;
        }
    }
}