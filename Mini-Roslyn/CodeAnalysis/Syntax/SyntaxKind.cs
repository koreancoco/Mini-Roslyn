namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        BadToken,

        EndOfFileToken,
        
        WhitespaceToken,
        
        StringToken,
        
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        NumberToken,
        IdentifierToken,
        BangToken,
        EqualsToken,
        OpenBraceToken,
        CloseBraceToken,
        LessToken,
        GreaterToken,
        CaretToken,
        BarToken,
        AmpersandToken,
        TildeToken,
        CommaToken,
        ColonToken,

        // compound punctuation
        BarBarToken,
        AmpersandAmpersandToken,
        EqualsEqualsToken,
        BangEqualsToken,
        LessOrEqualsToken,
        GreaterOrEqualsToken,

        // Keywords
        BreakKeyword,
        ContinueKeyword,
        FalseKeyword,
        TrueKeyword,
        VarKeyword,
        LetKeyword,
        IfKeyword,
        ElseKeyword,
        WhileKeyword,
        ForKeyword,
        ToKeyword,
        DoKeyword,
        FunctionKeyword,

        // Nodes
        CompilationUnit,
        ElseClause,
        TypeClause,
        Parameter,
        
        // expressions
        BinaryExpression,
        LiteralExpression,
        ParenthesizedExpression,
        UnaryExpression,
        NameExpression,
        AssignmentExpression,
        CallExpression,
        ConversionExpression,
        
        // statements
        BreakStatement,
        ContinueStatement,
        BlockStatement,
        ExpressionStatement,
        VariableDeclaration,
        FunctionDeclaration,
        IfStatement,
        WhileStatement,
        ForStatement,
        DoWhileStatement,
        GlobalStatement,

    }
}