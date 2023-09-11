namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal enum BoundBinaryOperatorKind
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,

        LogicalAnd,
        LogicalOr,
        
        Equals,
        NotEquals,
        
        Less,
        LessOrEquals,
        Greater,
        GreaterOrEquals
    }
}