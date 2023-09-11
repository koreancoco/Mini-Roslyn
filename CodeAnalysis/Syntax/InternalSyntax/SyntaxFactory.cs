namespace CodeAnalysis.Syntax.InternalSyntax
{
    public static class SyntaxFactory
    {
        private const string CrLf = "\r\n";
        public static readonly SyntaxTrivia CarriageReturnLineFeed = EndOfLine(CrLf);
        public static readonly SyntaxTrivia LineFeed = EndOfLine("\n");
        public static readonly SyntaxTrivia CarriageReturn = EndOfLine("\r");
        public static readonly SyntaxTrivia Space = Whitespace(" ");

        public static SyntaxToken Literal(string text, double value)
        {
            return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, text, value);
        }
        
        public static SyntaxToken Token(LSharpSyntaxNode leading, SyntaxKind kind, LSharpSyntaxNode trailing)
        {
            return SyntaxToken.Create(kind, leading, trailing);
        }
        
        public static SyntaxToken Literal(LSharpSyntaxNode leading, string text, double value, LSharpSyntaxNode trailing)
        {
            return SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);
        }

        
        public static SyntaxTrivia Whitespace(string text, bool elastic = false)
        {
            var trivia = SyntaxTrivia.Create(SyntaxKind.WhitespaceTrivia, text);

            return trivia;
        }

        public static SyntaxTrivia EndOfLine(string text, bool elastic = false)
        {
            SyntaxTrivia trivia = null;

            // use predefined trivia
            switch (text)
            {
                case "\r":
                    trivia = SyntaxFactory.CarriageReturn;
                    break;
                case "\n":
                    trivia = SyntaxFactory.LineFeed;
                    break;
                case "\r\n":
                    trivia = SyntaxFactory.CarriageReturnLineFeed;
                    break;
            }
            
            // note: predefined trivia might not yet be defined during initialization
            if (trivia != null)
            {
                return trivia;
            }
            
            trivia = SyntaxTrivia.Create(SyntaxKind.EndOfLineTrivia, text);
            if (!elastic)
            {
                return trivia;
            }

            return trivia;
        }
    }
}