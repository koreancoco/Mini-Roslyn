using System.Collections.Generic;
using MiniRoslyn.CodeAnalysis.Syntax;
using Xunit;

namespace MiniRoslyn.Test
{
    public class ParserTests
    {
        [Theory]
        [MemberData(nameof(GetBinaryExpressionPairs))]
        public void Parser_BinaryExpression_HonorsPrecedences(SyntaxKind op1, SyntaxKind op2)
        {
            var op1Precedence = SyntaxFacts.GetBinaryOperatorPrecedence(op1);
            var op2Precedence = SyntaxFacts.GetBinaryOperatorPrecedence(op2);
            var op1Text = SyntaxFacts.GetText(op1);
            var op2Text = SyntaxFacts.GetText(op2);
            var text = $"a {op1Text} b {op2Text} c";
            var expression = ParseExpression(text);

            if (op1Precedence >= op2Precedence)
            {
                //                      BinaryExpression
                //                     /          |    \
                //              BinaryExpression op2 NameExpression
                //             /     |       \             \
                //   NameExpression op1 NameExpression      c
                //          |                 | 
                //         a                  b 
                
                using (var e = new AssertingEnumerator(expression))
                {
                    e.AssertNode(SyntaxKind.BinaryExpression);
                    e.AssertNode(SyntaxKind.BinaryExpression);
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "a");
                    e.AssertToken(op1, op1Text);
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "b");
                    e.AssertToken(op2, op2Text);
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "c");
                }
            }
            else
            {
                //            
                //                   BinaryExpression                 
                //                  /        |        \                   
                //          NameExpression op1 BinaryExpression  
                //                 |           /      |       \  
                //                a  NameExpression op2 NameExpression
                //                          |                 | 
                //                         b                  c
                using (var e = new AssertingEnumerator(expression))
                {
                    e.AssertNode(SyntaxKind.BinaryExpression);
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "a");
                    e.AssertToken(op1, op1Text);
                    e.AssertNode(SyntaxKind.BinaryExpression);
                    e.AssertNode(SyntaxKind.NameExpression);                    
                    e.AssertToken(SyntaxKind.IdentifierToken, "b");
                    e.AssertToken(op2, op2Text);
                    e.AssertNode(SyntaxKind.NameExpression);
                    e.AssertToken(SyntaxKind.IdentifierToken, "c");
                }
            }
        }

        [Theory]
        [MemberData(nameof(GetUnaryBinaryExpressionPairs))]
        public void Parser_UnaryExpression_HonorsPrecedences(SyntaxKind unaryKind, SyntaxKind binaryKind)
        {
            var unaryText = SyntaxFacts.GetText(unaryKind);
            var binaryText = SyntaxFacts.GetText(binaryKind);
            string text = $"{unaryText} a {binaryText} b";
            var expression = ParseExpression(text);
            
            //                      BinaryExpression
            //                     /          |    \
            //              UnaryExpression bop NameExpression
            //            /         |                  \
            //         uop   NameExpression             b
            //                    |
            //                   a
            using (var e = new AssertingEnumerator(expression))
            {
                e.AssertNode(SyntaxKind.BinaryExpression);
                e.AssertNode(SyntaxKind.UnaryExpression);
                e.AssertToken(unaryKind, unaryText);
                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "a");
                e.AssertToken(binaryKind, binaryText);
                e.AssertNode(SyntaxKind.NameExpression);
                e.AssertToken(SyntaxKind.IdentifierToken, "b");
            }
        }
        
        private static ExpressionSyntax ParseExpression(string text)
        {
            var syntaxTree = SyntaxTree.Parse(text);
            var root = syntaxTree.Root;
            var member = Assert.Single(root.Members);
            var globalStatement = Assert.IsType<GlobalStatementSyntax>(member);
            return Assert.IsType<ExpressionStatementSyntax>(globalStatement.Statement).Expression;
        }

        public static IEnumerable<object[]> GetBinaryExpressionPairs()
        {
            var kinds1 = SyntaxFacts.GetBinaryOperatorKinds();
            var kinds2 = SyntaxFacts.GetBinaryOperatorKinds();
            foreach (var kind1 in kinds1)
            {
                foreach (var kind2 in kinds2)
                {
                    yield return new object[]{ kind1, kind2 };
                }
            }
        }
        
        public static IEnumerable<object[]> GetUnaryBinaryExpressionPairs()
        {
            var kinds1 = SyntaxFacts.GetUnaryOperatorKinds();
            var kinds2 = SyntaxFacts.GetBinaryOperatorKinds();
            foreach (var kind1 in kinds1)
            {
                foreach (var kind2 in kinds2)
                {
                    yield return new object[]{ kind1, kind2 };
                }
            }
        }
        
    }
}