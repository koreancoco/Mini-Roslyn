using System;
using System.Diagnostics;
using CodeAnalysis.Utilities;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    public class SyntaxNodeCache
    {
        private const int CacheSizeBits = 16;
        private const int CacheSize = 1 << CacheSizeBits;
        private const int CacheMask = CacheSize - 1;

        private struct Entry
        {
            public readonly int hash;
            public readonly GreenNode node;

            public Entry(int hash, GreenNode node)
            {
                this.hash = hash;
                this.node = node;
            }
        }

        private static readonly Entry[] s_cache = new Entry[CacheSize];

        private static bool CanBeCached(GreenNode child1)
        {
            return child1 == null || child1.IsCacheable;
        }

        private static bool CanBeCached(GreenNode child1, GreenNode child2)
        {
            return CanBeCached(child1) && CanBeCached(child2);
        }

        private static bool CanBeCached(GreenNode child1, GreenNode child2, GreenNode child3)
        {
            return CanBeCached(child1) && CanBeCached(child2) && CanBeCached(child3);
        }

        private static int GetCacheHash(int kind, GreenNode child1)
        {
            kind = Hash.Combine(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(child1), kind);

            // ensure nonnegative hash
            return kind & Int32.MaxValue;
        }

        private static int GetCacheHash(int kind, GreenNode child1, GreenNode child2)
        {
            if (child1 != null)
            {
                kind = Hash.Combine(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(child1), kind);
            }

            if (child2 != null)
            {
                kind = Hash.Combine(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(child2), kind);
            }

            // ensure nonnegative hash
            return kind & Int32.MaxValue;
        }

        private static int GetCacheHash(int kind, GreenNode child1, GreenNode child2, GreenNode child3)
        {
            
            if (child1 != null)
            {
                kind = Hash.Combine(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(child1), kind);
            }

            if (child2 != null)
            {
                kind = Hash.Combine(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(child2), kind);
            }

            if (child3 != null)
            {
                kind = Hash.Combine(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(child3), kind);
            }

            // ensure nonnegative hash
            return kind & Int32.MaxValue;
        }
        
        internal static GreenNode TryGetNode(int kind, GreenNode child1, SyntaxFactoryContext context, out int hash)
        {
            return TryGetNode(kind, child1, out hash);
        }
        
        public static GreenNode TryGetNode(int kind, GreenNode child0, out int hash)
        {
            return TryGetNodeCore(kind, child0, out hash);
        }
        
        public static GreenNode TryGetNode(int kind, GreenNode child0, GreenNode child1, out int hash)
        {
            return TryGetNodeCore(kind, child0, child1, out hash);
        }

        public static GreenNode TryGetNode(int kind, GreenNode child0, GreenNode child1, GreenNode child2, out int hash)
        {
            return TryGetNodeCore(kind, child0, child1, child2, out hash);
        }
        
        private static GreenNode TryGetNodeCore(int kind, GreenNode child1, out int hash)
        {
            if (CanBeCached(child1))
            {
                int h = hash = GetCacheHash(kind, child1);
                int idx = h & CacheMask;
                var e = s_cache[idx];
                if (e.hash == h && e.node != null && e.node.IsCacheEquivalent(kind, child1))
                {
                    return e.node;
                }
            }
            else
            {
                hash = -1;
            }

            return null;
        }

        // todo 增加NodeFlags
        // private static GreenNode TryGetNode(int kind, GreenNode child1, GreenNode child2, GreenNode.NodeFlags flags, out int hash)
        private static GreenNode TryGetNodeCore(int kind, GreenNode child1, GreenNode child2, out int hash)
        {
            if (CanBeCached(child1, child2))
            {
                int h = hash = GetCacheHash(kind, child1, child2);
                int idx = h & CacheMask;
                var e = s_cache[idx];
                if (e.hash == h && e.node != null && e.node.IsCacheEquivalent(kind, child1, child2))
                {
                    return e.node;
                }
            }
            else
            {
                hash = -1;
            }

            return null;
        }
        
        private static GreenNode TryGetNodeCore(int kind, GreenNode child1, GreenNode child2, GreenNode child3, out int hash)
        {
            if (CanBeCached(child1, child2, child3))
            {
                int h = hash = GetCacheHash(kind, child1, child2, child3);
                int idx = h & CacheMask;
                var e = s_cache[idx];
                if (e.hash == h && e.node != null && e.node.IsCacheEquivalent(kind, child1, child2, child3))
                {
                    return e.node;
                }
            }
            else
            {
                hash = -1;
            }

            return null;
        }


        public static void AddNode(GreenNode node, int hash)
        {
            Debug.Assert(node.GetCacheHash() == hash);

            int idx = hash & CacheMask;
            s_cache[idx] = new Entry(hash, node);
        }
    }
}