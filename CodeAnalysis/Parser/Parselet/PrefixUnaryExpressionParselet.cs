using CodeAnalysis.Syntax.InternalSyntax;

namespace CodeAnalysis.Parselet
{
    internal class PrefixUnaryExpressionParselet : IPrefixParselet
    {
        public ExpressionSyntax Parse(LanguageParser parser, SyntaxToken token)
        {
            var tk = SyntaxFacts.GetPrefixUnaryExpression(token.Kind);
            var operand = parser.ParseSubExpressionCore(0);
            return parser.Factory.PrefixUnaryExpression(tk, token, operand);
        }
    }
}