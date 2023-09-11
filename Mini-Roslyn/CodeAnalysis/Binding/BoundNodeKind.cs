namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // expression
        ErrorExpression,
        LiteralExpression,
        BinaryExpression,
        UnaryExpression,
        VariableExpression,
        AssignmentExpression,
        CallExpression,
        ConversionExpression,
        
        // statement
        BlockStatement,
        ExpressionStatement,
        VariableDeclaration,
        FunctionDeclaration,
        IfStatement,
        WhileStatement,
        ForStatement,
        LabelStatement,
        GotoStatement,
        ConditionalGotoStatement,
        DoWhileStatement,
    }
}