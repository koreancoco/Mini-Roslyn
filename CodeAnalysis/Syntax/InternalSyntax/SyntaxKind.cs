namespace CodeAnalysis.Syntax.InternalSyntax
{
    public enum SyntaxKind
    {
        None,
        List = GreenNode.ListKind,
        // punctuation
        TildeToken,
        PlusToken,
        MinusToken,
        AsteriskToken,
        SlashToken,
        
        
        // trivia
        EndOfLineTrivia,
        WhitespaceTrivia,
        // token
        NumericLiteralToken,
        
        // unary expressions
        UnaryMinusExpression,
        UnaryPlusExpression,
        MultiplyExpression,
        DivideExpression,
        

        // binary expressions
        AddExpression,
        SubtractExpression,

        // primary expression
        NumericLiteralExpression,

        EndOfFileToken,
    }
}