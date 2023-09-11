using System.Collections.Generic;
using MiniRoslyn.CodeAnalysis.Diagnostics;
using MiniRoslyn.CodeAnalysis.Symbols;
using MiniRoslyn.CodeAnalysis.Text;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
   internal sealed class Lexer
    {
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly SourceText _text;

        private int _position;

        private int _start;
        private SyntaxKind _kind;
        private object _value;

        public Lexer(SourceText text)
        {
            _text = text;
        }
        
        public IEnumerable<SyntaxToken> LexTokens()
        {
            while (true)
            {
                var token = Lex();
                if (token.Kind == SyntaxKind.EndOfFileToken)
                    break;

                yield return token;
            }
        }

        public DiagnosticBag Diagnostics => _diagnostics;

        private char Current => Peek(0);

        private char Lookahead => Peek(1);

        private bool IsAtEnd() => Current == '\0';

        private char Peek(int offset)
        {
            var index = _position + offset;

            if (index >= _text.Length)
                return '\0';

            return _text[index];
        }

        private void Advance()
        {
            _position++;
        }

        public SyntaxToken Lex()
        {
            _start = _position;
            _kind = SyntaxKind.BadToken;
            _value = null;

            switch (Current)
            {
                case '\0':
                    _kind = SyntaxKind.EndOfFileToken;
                    break;
                case '+':
                    _kind = SyntaxKind.PlusToken;
                    Advance();
                    break;
                case '-':
                    _kind = SyntaxKind.MinusToken;
                    Advance();
                    break;
                case '*':
                    _kind = SyntaxKind.StarToken;
                    Advance();
                    break;
                case '/':
                    _kind = SyntaxKind.SlashToken;
                    Advance();
                    break;
                case '(':
                    _kind = SyntaxKind.OpenParenthesisToken;
                    Advance();
                    break;
                case ')':
                    _kind = SyntaxKind.CloseParenthesisToken;
                    Advance();
                    break;
                case '{':
                    _kind = SyntaxKind.OpenBraceToken;
                    Advance();
                    break;
                case '}':
                    _kind = SyntaxKind.CloseBraceToken;
                    Advance();
                    break;
                case '~':
                    _kind = SyntaxKind.TildeToken;
                    Advance();
                    break;
                case '^':
                    _kind = SyntaxKind.CaretToken;
                    Advance();
                    break;
                case ',':
                    _kind = SyntaxKind.CommaToken;
                    Advance();
                    break;
                case ':':
                    _kind = SyntaxKind.ColonToken;
                    Advance();
                    break;
                case '&':
                    Advance();
                    if (Current != '&')
                    {
                        _kind = SyntaxKind.AmpersandToken;
                    }
                    else
                    {
                        Advance();
                        _kind = SyntaxKind.AmpersandAmpersandToken;
                    }
                    break;
                case '|':
                    Advance();
                    if (Current != '|')
                    {
                        _kind = SyntaxKind.BarToken;
                    }
                    else
                    {
                        Advance();
                        _kind = SyntaxKind.BarBarToken;
                    }
                    break;
                case '=':
                    Advance();
                    if (Current != '=')
                    {
                        _kind = SyntaxKind.EqualsToken;
                    }
                    else
                    {
                        Advance();
                        _kind = SyntaxKind.EqualsEqualsToken;
                    }
                    break;
                case '!':
                    Advance();
                    if (Current != '=')
                    {
                        _kind = SyntaxKind.BangToken;
                    }
                    else
                    {
                        Advance();
                        _kind = SyntaxKind.BangEqualsToken;
                    }
                    break;
                case '<':
                    Advance();
                    if (Current != '=')
                    {
                        _kind = SyntaxKind.LessToken;
                    }
                    else
                    {
                        Advance();
                        _kind = SyntaxKind.LessOrEqualsToken;
                    }
                    break;
                case '>':
                    Advance();
                    if (Current != '=')
                    {
                        _kind = SyntaxKind.GreaterToken;
                    }
                    else
                    {
                        Advance();
                        _kind = SyntaxKind.GreaterOrEqualsToken;
                    }
                    break;
                case '\"':
                    ReadString();
                    break;
                case '0': case '1': case '2': case '3': case '4':
                case '5': case '6': case '7': case '8': case '9':
                    ReadNumber();
                    break;
                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    ReadWhiteSpace();
                    break;
                default:
                    if (char.IsLetter(Current))
                    {
                        ReadIdentifierOrKeyword();
                    }
                    else if (char.IsWhiteSpace(Current))
                    {
                        ReadWhiteSpace();
                    }
                    else
                    {
                        _diagnostics.ReportBadCharacter(_position, Current);
                        Advance();
                    }
                    break;
            }
            
            return MakeToken();
        }

        private SyntaxToken MakeToken()
        {
            return new SyntaxToken(_kind, _start, GetText(), _value);
        }
        
        private string GetText()
        {
            return SyntaxFacts.GetText(_kind) ?? _text.ToString(_start, _position - _start);
        }

        private void ReadString()
        {
            Advance();
            while (Current != '\"' && !IsAtEnd())
            {
                Advance();
            }

            var length = _position - _start;
            // unterminated string literal
            if (IsAtEnd())
            {
                _diagnostics.ReportUnterminatedString(new TextSpan(_start, length));
            }
            else
            {
                // trailing "
                Advance();
                _value = _text.ToString(_start + 1, length - 1);
            }

            _kind = SyntaxKind.StringToken;
        }

        private void ReadWhiteSpace()
        {
            while (char.IsWhiteSpace(Current))
                Advance();

            _kind = SyntaxKind.WhitespaceToken;
        }

        private void ReadNumber()
        {
            while (char.IsDigit(Current))
                Advance();

            var length = _position - _start;
            var text = _text.ToString(_start, length);
            if (!int.TryParse(text, out var value))
                _diagnostics.ReportInvalidNumber(new TextSpan(_start, length), text, TypeSymbol.Int);

            _value = value;
            _kind = SyntaxKind.NumberToken;
        }

        private void ReadIdentifierOrKeyword()
        {
            while (char.IsLetter(Current))
                Advance();
            
            var length = _position - _start;
            var text = _text.ToString(_start, length);
            _kind = SyntaxFacts.GetKeywordKind(text);
        }

    }
}