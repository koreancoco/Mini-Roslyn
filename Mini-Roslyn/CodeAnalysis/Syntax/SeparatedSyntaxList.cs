using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public abstract class SeparatedSyntaxList
    {
        public abstract ImmutableArray<SyntaxNode> GetWithSeparators();
    }
    
    public class SeparatedSyntaxList<T> : SeparatedSyntaxList, IEnumerable<T>
        where T: SyntaxNode
    {
        private ImmutableArray<SyntaxNode> _nodesAndSeparators;

        public SeparatedSyntaxList(ImmutableArray<SyntaxNode> nodesAndSeparators)
        {
            _nodesAndSeparators = nodesAndSeparators;
        }

        // syntaxToken数量
        public int Count => (_nodesAndSeparators.Length + 1) / 2; 
        
        // 按序索引syntaxToken
        public T this[int index] => (T)_nodesAndSeparators[2 * index];

        public SyntaxToken GetSeparator(int index)
        {
            if (index >= Count - 1)
                return null;
            
            return (SyntaxToken)_nodesAndSeparators[2 * index + 1];
        }

        public override ImmutableArray<SyntaxNode> GetWithSeparators() => _nodesAndSeparators;

        // foreach遍历所有的syntaxToken

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}