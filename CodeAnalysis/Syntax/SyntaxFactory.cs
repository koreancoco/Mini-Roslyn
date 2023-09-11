namespace CodeAnalysis.Syntax
{
    public class SyntaxFactory
    {
        public static ExpressionSyntax ParseExpression(string text)
        {
            var lexer = new InternalSyntax.Lexer(SourceText.From(text));
            var parser = new InternalSyntax.LanguageParser(lexer);
            var node = parser.ParseExpression();
            return (ExpressionSyntax)node.CreateRed();
        }
    }
}