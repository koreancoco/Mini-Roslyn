using System.Diagnostics;
using CodeAnalysis.Utilities;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    public abstract class SyntaxList : LSharpSyntaxNode
    {
        internal static LSharpSyntaxNode List(SyntaxListBuilder builder)
        {
            if (builder != null)
            {
                return builder.ToListNode();
            }

            return null;
        }

        internal static LSharpSyntaxNode List(LSharpSyntaxNode child0, LSharpSyntaxNode child1)
        {
            Debug.Assert(child0 != null);
            Debug.Assert(child1 != null);

            int hash;
            GreenNode cached = SyntaxNodeCache.TryGetNode((short) SyntaxKind.List, child0, child1, out hash);
            if (cached != null)
            {
                return (WithTwoChildren) cached;
            }

            var result = new WithTwoChildren(child0, child1);

            if (hash >= 0) // hash >= 0 表示可以缓存
            {
                SyntaxNodeCache.AddNode(result, hash);
            }

            return result;
        }

        internal static LSharpSyntaxNode List(LSharpSyntaxNode child0, LSharpSyntaxNode child1, LSharpSyntaxNode child2)
        {
            Debug.Assert(child0 != null);
            Debug.Assert(child1 != null);
            Debug.Assert(child2 != null);


            int hash;
            GreenNode cached = SyntaxNodeCache.TryGetNode((short) SyntaxKind.List, child0, child1, child2, out hash);
            if (cached != null)
            {
                return (WithThreeChildren) cached;
            }

            var result = new WithThreeChildren(child0, child1, child2);

            if (hash >= 0) // hash >= 0 表示可以缓存
            {
                SyntaxNodeCache.AddNode(result, hash);
            }

            return result;
        }

        internal static LSharpSyntaxNode List(ArrayElement<LSharpSyntaxNode>[] children)
        {
            if (children.Length < 10)
            {
                return new WithManyChildren(children);
            }

            return new WithLotsOfChildren(children);
        }

        protected SyntaxList() : base(SyntaxKind.List)
        {
        }
    }
}