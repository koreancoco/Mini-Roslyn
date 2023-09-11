using CodeAnalysis.Syntax.InternalSyntax;

namespace CodeAnalysis.Parselet
{
    internal class LiteralExpressionParselet : IPrefixParselet
    {
        public ExpressionSyntax Parse(LanguageParser parser, SyntaxToken token)
        {
            return parser.Factory.LiteralExpression(SyntaxFacts.GetLiteralExpression(token.Kind), token);
        }
    }
}