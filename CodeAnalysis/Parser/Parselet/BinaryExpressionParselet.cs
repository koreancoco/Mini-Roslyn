using CodeAnalysis.Syntax.InternalSyntax;

namespace CodeAnalysis.Parselet
{
    public class BinaryExpressionParselet : IInfixParselet
    {
        public ExpressionSyntax Parse(LanguageParser parser, ExpressionSyntax left, SyntaxToken token)
        {
            var tk = SyntaxFacts.GetBinaryExpression(token.Kind);
            var right = parser.ParseSubExpressionCore(SyntaxFacts.GetPrecedence(tk));
            return parser.Factory.BinaryExpression(tk, left, token, right);
        }
    }
}