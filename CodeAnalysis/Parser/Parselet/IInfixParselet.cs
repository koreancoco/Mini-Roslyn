using System;
using CodeAnalysis.Syntax.InternalSyntax;

namespace CodeAnalysis.Parselet
{
    internal interface IInfixParselet
    {
        ExpressionSyntax Parse(LanguageParser parser, ExpressionSyntax left, SyntaxToken token);
    }
}