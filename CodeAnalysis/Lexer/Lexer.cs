using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using CodeAnalysis.Syntax.InternalSyntax;
using CodeAnalysis.Utilities;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    public class Lexer : AbstractLexer
    {
        private readonly StringBuilder _builder;
        private SyntaxListBuilder _leadingTriviaCache = new SyntaxListBuilder(10);
        private SyntaxListBuilder _trailingTriviaCache = new SyntaxListBuilder(10);

        private const int MaxCachedTokenSize = 42;
        private readonly LexerCache _cache;

        public Lexer(SourceText text) : base(text)
        {
            _builder = new StringBuilder();
            _cache = new LexerCache();
        }
        public SyntaxToken Lex()
        {
            _leadingTriviaCache.Clear();
            this.LexSyntaxTrivia(afterFirstToken: TextWindow.Position > 0, isTrailing: false, triviaList: ref _leadingTriviaCache);
            var leading = _leadingTriviaCache;

            var tokenInfo = default(TokenInfo);
            
            this.Start();
            this.ScanSyntaxToken(ref tokenInfo);
            
            _trailingTriviaCache.Clear();
            this.LexSyntaxTrivia(afterFirstToken: true, isTrailing: true, triviaList: ref _trailingTriviaCache);
            var trailing = _trailingTriviaCache;
            
            return Create(ref tokenInfo,leading, trailing);
        }

        public void Start()
        {
            TextWindow.Start();
        }
        
        public void LexSyntaxTrivia(bool afterFirstToken, bool isTrailing, ref SyntaxListBuilder triviaList)
        {
            while (true)
            {
                this.Start();
                char ch = TextWindow.PeekChar();

                switch (ch)
                {
                    case ' ':
                    case '\t':       // Horizontal tab
                        AddTrivia(this.ScanWhitespace(), ref triviaList);
                        break;
                    case '\r':
                    case '\n':
                        AddTrivia(this.ScanEndOfLine(), ref triviaList);
                        // Trailing trivia 到 endLine 为止
                        if (isTrailing)
                        {
                            return;
                        }
                        break;
                    default:
                        return;
                }
            }
        }

        private void AddTrivia(SyntaxTrivia trivia, ref SyntaxListBuilder list)
        {
            list.Add(trivia);
        }

        private SyntaxTrivia ScanEndOfLine()
        {
            char ch;
            switch (ch = TextWindow.PeekChar())
            {
                case '\r':
                    TextWindow.AdvanceChar();
                    if (TextWindow.PeekChar() == '\n')
                    {
                        TextWindow.AdvanceChar();
                        return SyntaxFactory.CarriageReturnLineFeed;
                    }

                    return SyntaxFactory.CarriageReturn;
                case '\n':
                    TextWindow.AdvanceChar();
                    return SyntaxFactory.LineFeed;
            }

            return null;
        }

        /// <summary>
        /// todo 性能优化
        /// </summary>
        /// <returns></returns>
        private SyntaxTrivia ScanWhitespace()
        {
            if (_createWhitespaceTriviaFunction == null)
            {
                _createWhitespaceTriviaFunction = this.CreateWhitespaceTrivia;
            }

            int hashcode = Hash.FnvOffsetBias;
            bool onlySpaces = true;
            top:
            char ch = TextWindow.PeekChar();
            switch (ch)
            {
                case '\t':       // Horizontal tab
                    onlySpaces = false;
                    goto case ' ';
                case ' ':
                    TextWindow.AdvanceChar();
                    hashcode = Hash.CombineFNVHash(hashcode, ch);
                    goto top;
                case '\r':      // Carriage Return
                case '\n':      // Line-feed
                    break;
                default:
                    break;
            }

            if (TextWindow.Width == 1 && onlySpaces)
            {
                // 直接取space
                return SyntaxFactory.Space;
            }
            else
            {
                var width = TextWindow.Width;
                if (width < MaxCachedTokenSize) // 超过42个字符的token罕见，所以不缓存
                {
                    return _cache.LookupTrivia(
                        TextWindow.CharacterWindow,
                        TextWindow.LexemeRelativeStart,
                        width,
                        hashcode,
                        _createWhitespaceTriviaFunction
                    );
                }
                else
                {
                    return CreateWhitespaceTrivia();// 比委托调用更快
                }
            }
        }

        private Func<SyntaxTrivia> _createWhitespaceTriviaFunction;

        private SyntaxTrivia CreateWhitespaceTrivia()
        {
            return SyntaxFactory.Whitespace(TextWindow.GetText(intern: true));
        }

        public void ScanSyntaxToken(ref TokenInfo info)
        {
            info.Kind = SyntaxKind.None;
            info.Text = null;
            char ch = TextWindow.PeekChar();
            switch (ch)
            {
                case '+':
                    TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.PlusToken;
                    break;
                case '-':
                    TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.MinusToken;
                    break;
                case '*':
                    TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.AsteriskToken;
                    break;
                case '/':
                    TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.SlashToken;
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    ScanNumericLiteral(ref info);
                    break;
                case SlidingTextWindow.InvalidCharacter:
                    info.Kind = SyntaxKind.EndOfFileToken;
                    break;
                default:
                    break;
            }

        }

        private SyntaxToken Create(ref TokenInfo info, SyntaxListBuilder leading, SyntaxListBuilder trailing)
        {
            SyntaxToken token = null;
            LSharpSyntaxNode leadingNode = SyntaxList.List(leading);
            LSharpSyntaxNode trailingNode = SyntaxList.List(trailing);
            switch (info.Kind)
            {
                case SyntaxKind.NumericLiteralToken:
                    token = SyntaxFactory.Literal(leadingNode,info.Text, info.DoubleValue,trailingNode);
                    break;
                default:
                    token = SyntaxFactory.Token(leadingNode,info.Kind,trailingNode); 
                    break;
            }
            return token;
        }

        
        private bool ScanNumericLiteral(ref TokenInfo info)
        {
            bool isHex = false;
            info.ValueKind = SpecialType.None;
            _builder.Clear();// store raw number without prefix like 0x,0X

            char ch = TextWindow.PeekChar();
            if (ch == '0' && ((ch = TextWindow.PeekChar(1)) == 'x' || ch == 'X'))
            {
                TextWindow.AdvanceChar(2);
                isHex = true;
            }

            if (isHex)
            {
                // It's OK if it has no digits after the '0x' -- we'll catch it in ScanNumericLiteral
                // and give a proper error then.
                while (SyntaxFacts.IsHexDigit(ch = TextWindow.PeekChar()))
                {
                    _builder.Append(ch);
                    TextWindow.AdvanceChar();
                }
                info.ValueKind = SpecialType.System_UInt64;
            }
            else
            {
                while ((ch = TextWindow.PeekChar()) >= '0' && ch <= '9')
                {
                    _builder.Append(ch);
                    TextWindow.AdvanceChar();
                }
                info.ValueKind = SpecialType.System_UInt64;
                
                // todo 支持浮点数
            }
            
            info.Kind = SyntaxKind.NumericLiteralToken;
            info.Text = TextWindow.GetText(true);
            Debug.Assert(info.Text != null);
            var valueText = _builder.ToString();
            if (info.ValueKind == SpecialType.System_UInt64)
            {
                info.DoubleValue = this.GetValueUInt64(valueText,isHex);
            }
            else
            {
                info.DoubleValue = this.GetValueDouble(valueText);
            }

            return true;
        }
        
        private double GetValueDouble(string text)
        {
            double result;
            if (!Double.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out result))
            {
                //we've already lexed the literal, so the error must be from overflow
                // this.AddError(MakeError(ErrorCode.ERR_FloatOverflow, "double"));
            }

            return result;
        }
        
        //used for all non-directive integer literals (cast to desired type afterward)
        private ulong GetValueUInt64(string text, bool isHex)
        {
            ulong result;
            if (!UInt64.TryParse(text, isHex ? NumberStyles.AllowHexSpecifier : NumberStyles.None, CultureInfo.InvariantCulture, out result))
            {
                //we've already lexed the literal, so the error must be from overflow
                // this.AddError(MakeError(ErrorCode.ERR_IntOverflow));
            }

            return result;
        }
    }
}