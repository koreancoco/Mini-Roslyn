using CodeAnalysis.Syntax.InternalSyntax;

namespace CodeAnalysis.Parselet
{
    internal interface IPrefixParselet
    {
        ExpressionSyntax Parse(LanguageParser parser, SyntaxToken token);
    }
}