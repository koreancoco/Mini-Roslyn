using System.Collections.Generic;
using System.IO;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax(SyntaxToken literalToken)
            :this(literalToken,literalToken.Value)
        {
        }

        public LiteralExpressionSyntax(SyntaxToken literalToken, object value)
        {
            LiteralToken = literalToken;
            Value = value;
        }

        public SyntaxToken LiteralToken { get; }
        public object Value { get; }

        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;

        public override R Accept<R>(IExpressionVisitor<R> evaluator)
        {
            return evaluator.VisitLiteralExpression(this);
        }
        
    }
}