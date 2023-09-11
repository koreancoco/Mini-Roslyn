using System;
using System.Diagnostics;
using CodeAnalysis.Syntax.InternalSyntax;

namespace CodeAnalysis.Syntax
{
    public partial struct SyntaxTriviaList
    {
        public struct Enumerator
        {
            private SyntaxToken _token;
            private GreenNode _singleNodeOrList;
            private int _baseIndex;
            private int _count;

            private int _index;
            private GreenNode _current;
            private int _position;
            public Enumerator(ref SyntaxTriviaList list)
            {
                _token = list.Token;
                _singleNodeOrList = list.Node;
                _baseIndex = list.Index;
                _count = list.Count;

                _index = -1;
                _current = null;
                _position = list.Position;
            }

            public bool MoveNext()
            {
                int newIndex = _index + 1;
                if (newIndex >= _count)
                {
                    // invalidate iterator
                    _current = null;
                    return false;
                }

                _index = newIndex;

                if (_current != null)
                {
                    _position += _current.FullWidth;
                }

                _current = GetGreenNodeAt(_singleNodeOrList, newIndex);
                return true;
            }



            public SyntaxTrivia Current
            {
                get
                {
                    if (_current == null)
                    {
                        throw new InvalidOperationException();
                    }

                    return new SyntaxTrivia(_token, _current, _position, _baseIndex + _index);
                }
            }
        }
    }
}