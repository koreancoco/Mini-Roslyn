using System;
using System.Collections.Generic;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.BangToken:
                case SyntaxKind.TildeToken:
                    return 10;

                default:
                    return 0;
            }
        }
        
        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 9;

                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 8;
                
                case SyntaxKind.LessToken:
                case SyntaxKind.LessOrEqualsToken:
                case SyntaxKind.GreaterToken:
                case SyntaxKind.GreaterOrEqualsToken:
                    return 7;
                
                case SyntaxKind.EqualsEqualsToken:
                case SyntaxKind.BangEqualsToken:
                    return 6;
                case SyntaxKind.AmpersandToken:
                    return 5;
                case SyntaxKind.CaretToken:
                    return 4;
                case SyntaxKind.BarToken:
                    return 3;
                
                case SyntaxKind.AmpersandAmpersandToken:
                    return 2;

                case SyntaxKind.BarBarToken:
                    return 1;

                default:
                    return 0;
            }
        }

        public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
        {
            var kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in kinds)
            {
                if (kind.GetUnaryOperatorPrecedence() > 0)
                    yield return kind;
            }
        }

        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
        {
            var kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in kinds)
            {
                if (kind.GetBinaryOperatorPrecedence() > 0)
                    yield return kind;
            }
        }

        public static SyntaxKind GetKeywordKind(string text)
        {
            switch (text)
            {
                case "true":
                    return SyntaxKind.TrueKeyword;
                case "false":
                    return SyntaxKind.FalseKeyword;
                case "var":
                    return SyntaxKind.VarKeyword;
                case "let":
                    return SyntaxKind.LetKeyword;
                case "if":
                    return SyntaxKind.IfKeyword;
                case "else":
                    return SyntaxKind.ElseKeyword;
                case "while":
                    return SyntaxKind.WhileKeyword;
                case "for":
                    return SyntaxKind.ForKeyword;
                case "to":
                    return SyntaxKind.ToKeyword;
                case "do":
                    return SyntaxKind.DoKeyword;
                case "function":
                    return SyntaxKind.FunctionKeyword;
                case "break":
                    return SyntaxKind.BreakKeyword;
                case "continue":
                    return SyntaxKind.ContinueKeyword;
                default:
                    return SyntaxKind.IdentifierToken;
            }
        }

        public static string GetText(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    return "+";
                case SyntaxKind.MinusToken:
                    return "-";
                case SyntaxKind.StarToken:
                    return "*";
                case SyntaxKind.SlashToken:
                    return "/";
                case SyntaxKind.BangToken:
                    return "!";
                case SyntaxKind.EqualsToken:
                    return "=";
                case SyntaxKind.AmpersandAmpersandToken:
                    return "&&";
                case SyntaxKind.BarBarToken:
                    return "||";
                case SyntaxKind.TildeToken:
                    return "~";
                case SyntaxKind.AmpersandToken:
                    return "&";
                case SyntaxKind.CaretToken:
                    return "^";
                case SyntaxKind.BarToken:
                    return "|";
                case SyntaxKind.EqualsEqualsToken:
                    return "==";
                case SyntaxKind.BangEqualsToken:
                    return "!=";
                case SyntaxKind.OpenParenthesisToken:
                    return "(";
                case SyntaxKind.CloseParenthesisToken:
                    return ")";
                case SyntaxKind.OpenBraceToken:
                    return "{";
                case SyntaxKind.CloseBraceToken:
                    return "}";
                case SyntaxKind.LessToken:
                    return "<";
                case SyntaxKind.CommaToken:
                    return ",";
                case SyntaxKind.LessOrEqualsToken:
                    return "<=";
                case SyntaxKind.GreaterToken:
                    return ">";
                case SyntaxKind.GreaterOrEqualsToken:
                    return ">=";
                case SyntaxKind.ColonToken:
                    return ":";
                case SyntaxKind.FalseKeyword:
                    return "false";
                case SyntaxKind.TrueKeyword:
                    return "true";
                case SyntaxKind.VarKeyword:
                    return "var";
                case SyntaxKind.LetKeyword:
                    return "let";
                case SyntaxKind.IfKeyword:
                    return "if";
                case SyntaxKind.ElseKeyword:
                    return "else";
                case SyntaxKind.WhileKeyword:
                    return "while";
                case SyntaxKind.ForKeyword:
                    return "for";
                case SyntaxKind.ToKeyword:
                    return "to";
                case SyntaxKind.DoKeyword:
                    return "do";
                case SyntaxKind.FunctionKeyword:
                    return "function";
                case SyntaxKind.BreakKeyword:
                    return "break";
                case SyntaxKind.ContinueKeyword:
                    return "continue";
                default:
                    return null;
            }
        }
    }
}