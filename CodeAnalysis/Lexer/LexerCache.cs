using System;
using CodeAnalysis.Syntax.InternalSyntax;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    public class LexerCache
    {
        private TextKeyedCache<SyntaxTrivia> _triviaMap;

        public LexerCache()
        {
            _triviaMap = TextKeyedCache<SyntaxTrivia>.GetInstance();
        }

        public SyntaxTrivia LookupTrivia(
            char[] textBuffer, 
            int keyStart, 
            int keyLength, 
            int hashCode,
            Func<SyntaxTrivia> createTriviaFunction)
        {
            var value = _triviaMap.FindItem(textBuffer, keyStart, keyLength, hashCode);

            if (value == null)
            {
                value = createTriviaFunction();
                _triviaMap.AddItem(textBuffer, keyStart, keyLength, hashCode, value);
            }

            return value;
        }
    }
}