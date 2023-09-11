using System;
using System.Diagnostics;
using CodeAnalysis.Syntax.InternalSyntax;

namespace CodeAnalysis.Syntax
{
    public partial struct SyntaxTriviaList
    {
        internal SyntaxTriviaList(SyntaxToken token, GreenNode node, int position, int index = 0)
        {
            Token = token;
            Node = node;
            Position = position;
            Index = index;
        }
        
        internal SyntaxToken Token { get; }

        internal GreenNode Node { get; }

        internal int Position { get; }

        internal int Index { get; }

        public int Count
        {
            get { return Node == null ? 0 : (Node.IsList ? Node.SlotCount : 1); }
        }
        
        public SyntaxTrivia ElementAt(int index)
        {
            return this[index];
        }
        
        /// <summary>
        /// Gets the trivia at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the trivia to get.</param>
        /// <returns>The token at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="Count" />. </exception>
        public SyntaxTrivia this[int index]
        {
            get
            {
                if (Node != null)
                {
                    if (Node.IsList)
                    {
                        if (unchecked((uint) index < (uint) Node.SlotCount))
                        {
                            return new SyntaxTrivia(Token, Node.GetSlot(index), Position + Node.GetSlotOffset(index), Index + index);
                        }
                    }
                    else if(index == 0)
                    {
                        return new SyntaxTrivia(Token, Node, Position, Index);
                    }
                }
                
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
        
        /// <summary>
        /// The absolute span of the list elements in characters, including the leading and trailing trivia of the first and last elements.
        /// </summary>
        public TextSpan FullSpan
        {
            get
            {
                if (Node == null)
                {
                    return default(TextSpan);
                }

                return new TextSpan(this.Position, Node.FullWidth);
            }
        }
        
        /// <summary>
        /// The absolute span of the list elements in characters, not including the leading and trailing trivia of the first and last elements.
        /// </summary>
        public TextSpan Span
        {
            get
            {
                if (Node == null)
                {
                    return default(TextSpan);
                }

                return TextSpan.FromBounds(Position + Node.GetLeadingTriviaWidth(),
                    Position + Node.FullWidth - Node.GetTrailingTriviaWidth());
            }
        }
        
        /// <summary>
        /// Returns the first trivia in the list.
        /// </summary>
        /// <returns>The first trivia in the list.</returns>
        /// <exception cref="InvalidOperationException">The list is empty.</exception>        
        public SyntaxTrivia First()
        {
            if (Any())
            {
                return this[0];
            }

            throw new InvalidOperationException();
        }
        
        /// <summary>
        /// Returns the last trivia in the list.
        /// </summary>
        /// <returns>The last trivia in the list.</returns>
        /// <exception cref="InvalidOperationException">The list is empty.</exception>        
        public SyntaxTrivia Last()
        {
            if (Any())
            {
                return this[this.Count - 1];
            }

            throw new InvalidOperationException();
        }
        
        /// <summary>
        /// Does this list have any items.
        /// </summary>
        public bool Any()
        {
            return Node != null;
        }
        
        /// <summary>
        /// Returns a list which contains all elements of <see cref="SyntaxTriviaList"/> in reversed order.
        /// </summary>
        /// <returns><see cref="Reversed"/> which contains all elements of <see cref="SyntaxTriviaList"/> in reversed order</returns>
        public Reversed Reverse()
        {
            return new Reversed(this);
        }
        
        public Enumerator GetEnumerator()
        {
            return new Enumerator(ref this);
        }
        
        private static GreenNode GetGreenNodeAt(GreenNode node, int i)
        {
            Debug.Assert(node.IsList || (i == 0 && !node.IsList));
            return node.IsList ? node.GetSlot(i) : node;
        }
        
        public int IndexOf(SyntaxTrivia triviaInList)
        {
            for (int i = 0, n = this.Count; i < n; i++)
            {
                var trivia = this[i];
                if (trivia == triviaInList)
                {
                    return i;
                }
            }

            return -1;
        }
        
        public int IndexOf(int rawKind)
        {
            for (int i = 0, n = this.Count; i < n; i++)
            {
                if (this[i].RawKind == rawKind)
                {
                    return i;
                }
            }

            return -1;
        }
        
        public override string ToString()
        {
            return Node != null ? Node.ToString() : string.Empty;
        }

        public string ToFullString()
        {
            return Node != null ? Node.ToFullString() : string.Empty;
        }
        
    }
}