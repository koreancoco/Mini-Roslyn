using System;
using System.Diagnostics;
using CodeAnalysis.Utilities;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    public abstract class GreenNode
    {
        internal const int ListKind = 1;
        
        private readonly ushort _kind;

        private int _fullWidth;

        public abstract string Language { get; }

        #region Kind

        public int RawKind
        {
            get { return _kind; }
        }

        public bool IsList
        {
            get
            {
                return RawKind == ListKind;
            }
        }

        public abstract string KindText { get; }

        public virtual bool IsToken
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Slots

        private int _slotCount;

        public int SlotCount
        {
            get
            {
                return _slotCount;
            }
            protected set
            {
                _slotCount = value;
            }
        }


        #endregion
        
        #region Spans

        public int FullWidth
        {
            get
            {
                return _fullWidth;
            }

            protected set
            {
                _fullWidth = value;
            }
        }

        public virtual int Width
        {
            get
            {
                return _fullWidth - GetLeadingTriviaWidth() - GetTrailingTriviaWidth();
            }
        }
        
        public virtual int GetLeadingTriviaWidth()
        {
            return this.FullWidth != 0 ?
                this.GetFirstTerminal().GetLeadingTriviaWidth() :
                0;
        }

        public virtual int GetTrailingTriviaWidth()
        {
            return this.FullWidth != 0 ?
                this.GetLastTerminal().GetTrailingTriviaWidth() :
                0;
        }
        
        public bool HasLeadingTrivia
        {
            get
            {
                return this.GetLeadingTriviaWidth() != 0;
            }
        }

        public bool HasTrailingTrivia
        {
            get
            {
                return this.GetTrailingTriviaWidth() != 0;
            }
        }
        

        #endregion

        #region Caching

        internal const int MaxCachedChildNum = 3;
        
        internal bool IsCacheable
        {
            get
            {
                return this.SlotCount <= MaxCachedChildNum;
            }
        }
        
        internal abstract GreenNode GetSlot(int index);
        
        public abstract int GetSlotOffset(int index);


        internal bool IsCacheEquivalent(int kind, GreenNode child1)
        {
            return this.RawKind == kind
                && this.GetSlot(0) == child1;
        }


        internal bool IsCacheEquivalent(int kind, GreenNode child1, GreenNode child2)
        {
            return this.RawKind == kind
                && this.GetSlot(0) == child1
                && this.GetSlot(1) == child2;
        }
        
        internal bool IsCacheEquivalent(int kind, GreenNode child1, GreenNode child2, GreenNode child3)
        {
            return this.RawKind == kind
                   && this.GetSlot(0) == child1
                   && this.GetSlot(1) == child2
                   && this.GetSlot(2) == child3;
        }

        internal int GetCacheHash()
        {
            Debug.Assert(this.IsCacheable);

            int code = this.RawKind;
            int cnt = this.SlotCount;
            for (int i = 0; i < cnt; i++)
            {
                var child = GetSlot(i);
                if (child != null)
                {
                    code = Hash.Combine(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(child), code);
                }
            }

            return code & Int32.MaxValue;
        }
        
        #endregion

        #region Tokens

        public virtual object GetValue() { return null; }
        public virtual string GetValueText() { return string.Empty; }
        
        public virtual GreenNode GetLeadingTriviaCore() { return null; }

        internal GreenNode GetFirstTerminal()
        {
            GreenNode node = this;

            do
            {
                GreenNode firstChild = null;
                for (int i = 0, n = node.SlotCount; i < n; i++)
                {
                    var child = node.GetSlot(i);
                    if (child != null)
                    {
                        firstChild = child;
                        break;
                    }
                }
                node = firstChild;
            } while (node?._slotCount > 0);

            return node;
        }
        
        internal GreenNode GetLastTerminal()
        {
            GreenNode node = this;

            do
            {
                GreenNode lastChild = null;
                for (int i = node.SlotCount - 1; i >= 0; i--)
                {
                    var child = node.GetSlot(i);
                    if (child != null)
                    {
                        lastChild = child;
                        break;
                    }
                }
                node = lastChild;
            } while (node?._slotCount > 0);

            return node;
        }

        #endregion

        #region Factories

        public SyntaxNode CreateRed()
        {
            return CreateRed(null, 0);
        }
        
        internal abstract SyntaxNode CreateRed(SyntaxNode parent, int position);

        #endregion

        #region Text

        public abstract string ToFullString();
        

        public virtual void WriteTo(System.IO.TextWriter writer)
        {
            this.WriteTo(writer, true, true);
        }

        protected internal virtual void WriteTo(System.IO.TextWriter writer, bool leading, bool trailing)
        {
            bool first = true;
            int n = this.SlotCount;
            int lastIndex = n - 1;
            for (; lastIndex >= 0; lastIndex--)
            {
                var child = this.GetSlot(lastIndex);
                if (child != null)
                {
                    break;
                }
            }

            for (var i = 0; i <= lastIndex; i++)
            {
                var child = this.GetSlot(i);
                if (child != null)
                {
                    child.WriteTo(writer, leading | !first, trailing | (i < lastIndex));
                    first = false;
                }
            }
        }

        #endregion
        
        protected GreenNode(ushort kind)
        {
            _kind = kind;
        }
        
        protected GreenNode(ushort kind,int fullWidth)
        {
            _kind = kind;
            _fullWidth = fullWidth;
        }
        
        protected void AdjustFlagsAndWidth(GreenNode node)
        {
            _fullWidth += node._fullWidth;
        }
        
    }
}