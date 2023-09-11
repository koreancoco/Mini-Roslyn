namespace CodeAnalysis.Syntax.InternalSyntax
{
    public static class SyntaxFacts
    {
        public static bool IsHexDigit(char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'A' && c <= 'F') ||
                   (c >= 'a' && c <= 'f');
        }

        public static bool IsWhitespace(char ch)
        {
            return ch == ' ' || ch == '\t';
        }

        public static string GetText(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    return "+";
                case SyntaxKind.MinusToken:
                    return "-";
                case SyntaxKind.AsteriskToken:
                    return "*";
                case SyntaxKind.SlashToken:
                    return "/";
            }
            return string.Empty;
        }

        public static SyntaxKind GetLiteralExpression(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.NumericLiteralToken:
                    return SyntaxKind.NumericLiteralExpression;
                default:
                    return SyntaxKind.None; 
            }
        }

        public static SyntaxKind GetPrefixUnaryExpression(SyntaxKind tokenKind)
        {
            switch (tokenKind)
            {
                case SyntaxKind.MinusToken:
                    return SyntaxKind.UnaryMinusExpression;
                case SyntaxKind.PlusToken:
                    return SyntaxKind.UnaryPlusExpression;
                default:
                    return SyntaxKind.None; 
            }
        }

        public static SyntaxKind GetBinaryExpression(SyntaxKind tokenKind)
        {
            switch (tokenKind)
            {
                case SyntaxKind.PlusToken:
                    return SyntaxKind.AddExpression;
                case SyntaxKind.MinusToken:
                    return SyntaxKind.SubtractExpression;
                case SyntaxKind.AsteriskToken:
                    return SyntaxKind.MultiplyExpression;
                case SyntaxKind.SlashToken:
                    return SyntaxKind.DivideExpression;
                default:
                    return SyntaxKind.None; 
            }
        }
        
        // 数值越大优先级越高
        public static uint GetPrecedence(SyntaxKind op)
        {
            switch (op)
            {
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubtractExpression:
                    return 11;
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                    return 12;
                case SyntaxKind.UnaryPlusExpression:
                case SyntaxKind.UnaryMinusExpression:
                default:
                    return 0;
            }
        }

        public static bool IsBinaryExpression(SyntaxKind kind)
        {
            return GetBinaryExpression(kind) != SyntaxKind.None;
        }

        public static bool IsAssignmentExpressionOperatorToken(SyntaxKind kind)
        {
            throw new System.NotImplementedException();
        }
    }
}