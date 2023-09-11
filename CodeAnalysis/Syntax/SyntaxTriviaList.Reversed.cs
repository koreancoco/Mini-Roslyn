namespace CodeAnalysis.Syntax
{
    public struct Reversed
    {
        private SyntaxTriviaList _list;

        public Reversed(SyntaxTriviaList list)
        {
            _list = list;
        }
    }
}