using System;
using System.Diagnostics;
using System.Threading;
using CodeAnalysis.Syntax.InternalSyntax;

namespace CodeAnalysis.Syntax
{
    public abstract class SyntaxNode
    {
        private readonly SyntaxNode _parent;

        internal SyntaxNode(GreenNode green, SyntaxNode parent, int position)
        {
            Debug.Assert(position >= 0, "position cannot be negative");
            Debug.Assert(parent?.Green.IsList != true, "list cannot be a parent");

            Position = position;
            Green = green;
            _parent = parent;
        }
        
        /// <summary>
        /// An integer representing the language specific kind of this node.
        /// </summary>
        public int RawKind => Green.RawKind;

        protected abstract string KindText { get; }
        
        public abstract string Language { get; }

        internal GreenNode Green { get; }

        internal int Position { get; }
        
        internal int EndPosition => this.Position + this.Green.FullWidth;
        
        internal bool IsList => this.Green.IsList;
        public TextSpan FullSpan => new TextSpan(this.Position, this.Green.FullWidth);
        
        internal int SlotCount => this.Green.SlotCount;

        public int SpanStart => this.Position + this.Green.GetLeadingTriviaWidth();
        
        public TextSpan Span
        {
            get
            {
                // Start with the full span.
                var start = Position;
                var width = this.Green.FullWidth;

                // adjust for preceding trivia (avoid calling this twice, do not call Green.Width)
                var precedingWidth = this.Green.GetLeadingTriviaWidth();
                start += precedingWidth;
                width -= precedingWidth;

                // adjust for following trivia width
                width -= this.Green.GetTrailingTriviaWidth();

                Debug.Assert(width >= 0);
                return new TextSpan(start, width);
            }
        }


        protected T GetRed<T>(ref T field, int slot) where T : SyntaxNode
        {
            var result = field;

            if (result == null)
            {
                var green = this.Green.GetSlot(slot);
                if (green != null)
                {
                    result = (T)green.CreateRed(this, this.GetChildPosition(slot));
                    result = Interlocked.CompareExchange(ref field, result, null) ?? result;
                }
            }

            return result;
        }

        protected T GetRedAtZero<T>(ref T field) where T : SyntaxNode
        {
            var result = field;

            if (result == null)
            {
                var green = this.Green.GetSlot(0);
                if (green != null)
                {
                    result = (T)green.CreateRed(this, this.Position);
                    result = Interlocked.CompareExchange(ref field, result, null) ?? result;
                }
            }

            return result;
        }
        
        
        internal virtual int GetChildPosition(int index)
        {
            int offset = 0;
            var green = this.Green;
            while (index > 0)
            {
                index--;
                var prevSibling = this.GetCachedSlot(index);
                if (prevSibling != null)
                {
                    return prevSibling.EndPosition + offset;
                }
                var greenChild = green.GetSlot(index);
                if (greenChild != null)
                {
                    offset += greenChild.FullWidth;
                }
            }

            return this.Position + offset;
        }
        
        /// <summary>
        /// Determines whether this node has any leading trivia.
        /// </summary>
        public bool HasLeadingTrivia
        {
            get
            {
                // todo Green.HasLeadingTrivia　只能说明对应的节点的直接子节点有没有trivia
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Determines whether this node has any trailing trivia.
        /// </summary>
        public bool HasTrailingTrivia
        {
            get
            {
                // todo Green.HasTrailingTrivia　只能说明对应的节点的直接子节点有没有trivia
                throw new NotImplementedException();
            }
        }
        
        /// <summary>
        /// Gets a node at given node index without forcing its creation.
        /// If node was not created it would return null.
        /// </summary>
        internal abstract SyntaxNode GetCachedSlot(int index);

        #region Node Lookup

        /// <summary>
        /// The node that contains this node in its <see cref="ChildNodes"/> collection.
        /// </summary>
        public SyntaxNode Parent
        {
            get
            {
                return _parent;
            }
        }

        #endregion
    }
}