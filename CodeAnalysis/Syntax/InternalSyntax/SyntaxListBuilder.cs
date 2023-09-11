using System;
using System.Diagnostics;
using CodeAnalysis.Utilities;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    public class SyntaxListBuilder
    {
        private ArrayElement<LSharpSyntaxNode>[] _nodes;

        public int Count { get; private set; }

        public int Length
        {
            get { return _nodes.Length; }
        }

        public SyntaxListBuilder(int size)
        {
            _nodes = new ArrayElement<LSharpSyntaxNode>[size];
        }
        
        public void Add(LSharpSyntaxNode item)
        {
            if (item == null) return;

            EnsureAdditionalCapacity(1);

            _nodes[Count++].Value = item;
        }

        public void Clear()
        {
            this.Count = 0;
        }
        
        public LSharpSyntaxNode this[int index]
        {
            get
            {
                return _nodes[index];
            }

            set
            {
                _nodes[index].Value = value;
            }
        }
        
        // 实现ToListNode
        public LSharpSyntaxNode ToListNode()
        {
            switch (this.Count)
            {
                case 0:
                    return null;
                case 1:
                    return _nodes[0];
                case 2:
                    return SyntaxList.List(_nodes[0], _nodes[1]);
                case 3:
                    return SyntaxList.List(_nodes[0], _nodes[1], _nodes[2]);
                default:
                    var tmp = new ArrayElement<LSharpSyntaxNode>[this.Count];
                    Array.Copy(_nodes, tmp, this.Count);
                    return SyntaxList.List(tmp);
            }
        }
        
        private void EnsureAdditionalCapacity(int additionalCount)
        {
            int currentSize = _nodes.Length;
            int requiredSize = this.Count + additionalCount;

            if (requiredSize <= currentSize) return;

            int newSize =
                requiredSize < 8 ? 8 :
                requiredSize >= (int.MaxValue / 2) ? int.MaxValue :
                Math.Max(requiredSize, currentSize * 2); // NB: Size will *at least* double.
            Debug.Assert(newSize >= requiredSize);

            Array.Resize(ref _nodes, newSize);
        }
    }
}