using System;
using System.Collections.Generic;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public class BinaryExpressionSyntax : ExpressionSyntax
    {
        public ExpressionSyntax Left { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Right { get; }

        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }

        public override SyntaxKind Kind
        {
            get
            {
                return SyntaxKind.BinaryExpression;
            }
        }

        public override R Accept<R>(IExpressionVisitor<R> evaluator)
        {
            return evaluator.VisitBinaryExpression(this);
        }
    }
}