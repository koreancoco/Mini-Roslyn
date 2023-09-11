using System;
using System.Collections.Generic;
using CodeAnalysis.Syntax.InternalSyntax;
using CodeAnalysis.Parselet;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    public class LanguageParser : SyntaxParser
    {
        private readonly ContextAwareSyntax _syntaxFactory;
        private readonly SyntaxFactoryContext _syntaxFactoryContext;
        public LanguageParser(Lexer lexer) : base(lexer)
        {
            _syntaxFactoryContext = new SyntaxFactoryContext();
            _syntaxFactory = new ContextAwareSyntax(_syntaxFactoryContext);
        }

        internal ContextAwareSyntax Factory
        {
            get
            {
                return _syntaxFactory;
            }
        }

        public ExpressionSyntax ParseExpression()
        {
            return ParseSubExpressionCore(0);
        }

        internal ExpressionSyntax ParseSubExpressionCore(uint precedence)
        {
            ExpressionSyntax leftOperand;
            var token = this.EatToken();
            IPrefixParselet prefixParselet;
            if (!s_prefixParselets.TryGetValue(token.Kind, out prefixParselet))
            {
                throw new Exception("No prefix parselet found");
            }
            
            leftOperand = prefixParselet.Parse(this, token);
            uint newPrecedence;
            while (precedence < (newPrecedence = GePrecedence(this.CurrentToken.Kind)))
            {
                if (precedence == newPrecedence && IsRightAssociative(this.CurrentToken.Kind)) // 同一优先级且右结合，break之后处理
                {
                    break;
                }
                token = this.EatToken();
                IInfixParselet infixParselet;
                if (!s_infixParselets.TryGetValue(token.Kind, out infixParselet))
                {
                    throw new Exception("No infix parselet found");
                }
                
                leftOperand = infixParselet.Parse(this, leftOperand, token);
            }
            
            // 同一优先级且右结合

            return leftOperand;
        }
        
        private uint GePrecedence(SyntaxKind kind)
        {
            SyntaxKind opKind = SyntaxKind.None;
            if (IsExpectedBinaryOperator(kind))
            {
                opKind = SyntaxFacts.GetBinaryExpression(kind);
            }
            
            return SyntaxFacts.GetPrecedence(opKind);
        }
        
        private static bool IsExpectedBinaryOperator(SyntaxKind kind)
        {
            return SyntaxFacts.IsBinaryExpression(kind);
        }
        
        private static bool IsExpectedAssignmentOperator(SyntaxKind kind)
        {
            return SyntaxFacts.IsAssignmentExpressionOperatorToken(kind);
        }
        
        
        internal static bool IsRightAssociative(SyntaxKind op)
        {
            switch (op)
            {
                // case SyntaxKind.SimpleAssignmentExpression:
                // case SyntaxKind.AddAssignmentExpression:
                // case SyntaxKind.SubtractAssignmentExpression:
                // case SyntaxKind.MultiplyAssignmentExpression:
                // case SyntaxKind.DivideAssignmentExpression:
                // case SyntaxKind.ModuloAssignmentExpression:
                // case SyntaxKind.AndAssignmentExpression:
                // case SyntaxKind.ExclusiveOrAssignmentExpression:
                // case SyntaxKind.OrAssignmentExpression:
                // case SyntaxKind.LeftShiftAssignmentExpression:
                // case SyntaxKind.RightShiftAssignmentExpression:
                // case SyntaxKind.CoalesceExpression:
                //     return true;
                default:
                    return false;
            }
        }
        
        private static readonly Dictionary<SyntaxKind,IPrefixParselet> s_prefixParselets = new Dictionary<SyntaxKind, IPrefixParselet>
        {
            {SyntaxKind.NumericLiteralToken, new LiteralExpressionParselet()},
            {SyntaxKind.MinusToken, new PrefixUnaryExpressionParselet()},
            {SyntaxKind.PlusToken, new PrefixUnaryExpressionParselet()},
        };
        
        private static readonly Dictionary<SyntaxKind,IInfixParselet> s_infixParselets = new Dictionary<SyntaxKind, IInfixParselet>
        {
            {SyntaxKind.MinusToken, new BinaryExpressionParselet()},
            {SyntaxKind.PlusToken, new BinaryExpressionParselet()},
            {SyntaxKind.AsteriskToken, new BinaryExpressionParselet()},
            {SyntaxKind.SlashToken, new BinaryExpressionParselet()},
        };
        
    }
}