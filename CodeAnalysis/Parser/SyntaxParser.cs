using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CodeAnalysis.Syntax.InternalSyntax;
using CodeAnalysis.Utilities;

namespace CodeAnalysis
{
    public class SyntaxParser
    {
        private readonly Lexer lexer;
        private ArrayElement<SyntaxToken>[] _lexedTokens;
        private int _tokenOffset;
        private int _tokenCount;
        
        private SyntaxToken _currentToken;

        protected SyntaxToken CurrentToken
        {
            get
            {
                if (_currentToken == null)
                {
                    _currentToken = FetchCurrentToken();
                }

                return _currentToken;
            }
        }

        private SyntaxToken FetchCurrentToken()
        {
            if (_tokenOffset >= _tokenCount)
            {
                // add new token
            }

            return _lexedTokens[_tokenOffset];
        }

        public SyntaxParser(Lexer lexer)
        {
            this.lexer = lexer;
            
            Prelex();
        }

        public SyntaxToken[] GetTokens()
        {
            var tokens = new SyntaxToken[_tokenCount];
            for (int i = 0; i < _tokenCount; i++)
            {
                tokens[i] = _lexedTokens[i].Value;
            }
            return tokens;
        }

        private void Prelex()
        {
            var size = Math.Min(4096, Math.Max(32, this.lexer.TextWindow.Text.Length / 2));
            _lexedTokens = new ArrayElement<SyntaxToken>[size];
            var lexer = this.lexer;
            
            for (int i = 0; i < size; i++)
            {
                var token = lexer.Lex();
                
                this.AddLexedToken(token);
                if (token.Kind == SyntaxKind.EndOfFileToken)
                {
                    break;
                }
            }
        }

        private void AddLexedToken(SyntaxToken token)
        {
            Debug.Assert(token != null);
            if (_tokenCount >= _lexedTokens.Length)
            {
                this.AddLexedTokenSlot();
            }
            
            _lexedTokens[_tokenCount].Value = token;
            _tokenCount++;
        }

        private void AddLexedTokenSlot()
        {
            var newLexedTokens = new ArrayElement<SyntaxToken>[_lexedTokens.Length * 2];
            Array.Copy(_lexedTokens, newLexedTokens, _lexedTokens.Length);
            _lexedTokens = newLexedTokens;
        }
        
        protected SyntaxToken PeekToken(int n)
        {
            Debug.Assert(n >= 0);
            while (_tokenOffset + n >= _tokenCount)
            {
                // this.AddNewToken();
            }

            return _lexedTokens[_tokenOffset + n];
        }
        
        
        protected SyntaxToken EatToken()
        {
            var ct = this.CurrentToken;
            MoveToNextToken();
            return ct;
        }

        private void MoveToNextToken()
        {
            _currentToken = default(SyntaxToken);

            _tokenOffset++;
        }
    }
}