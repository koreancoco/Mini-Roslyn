using System.Collections.Generic;
using System.Collections.Immutable;
using MiniRoslyn.CodeAnalysis.Diagnostics;
using MiniRoslyn.CodeAnalysis.Symbols;
using MiniRoslyn.CodeAnalysis.Text;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private readonly SourceText _text;
        private DiagnosticBag _diagnostics = new DiagnosticBag();
        private int _position;

        public Parser(SourceText text)
        {
            var tokens = new List<SyntaxToken>();

            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();

                if (token.Kind != SyntaxKind.WhitespaceToken &&
                    token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            _text = text;
            _tokens = tokens.ToImmutableArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }

        public DiagnosticBag Diagnostics => _diagnostics;

        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
                return _tokens[_tokens.Length - 1];

            return _tokens[index];
        }

        private SyntaxToken Current => Peek(0);

        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }

        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            if (Current.Kind == kind)
                return NextToken();

            _diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
            return new SyntaxToken(kind, Current.Position, null, null);
        }

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var members = ParseMemberList();
            var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new CompilationUnitSyntax(members, endOfFileToken);
        }

        private ImmutableArray<MemberSyntax> ParseMemberList()
        {
            var members = ImmutableArray.CreateBuilder<MemberSyntax>();
            while (Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var startToken = Current;
                members.Add(ParseMember());
                // If ParseMember() did not consume any tokens,
                // we need to skip the current token and continue
                // in order to avoid an infinite loop.
                //
                // We don't need to report an error, because we'll
                // already tried to parse an expression statement
                // and reported one.
                if (Current == startToken)
                    NextToken();
            }

            return members.ToImmutable();
        }

        private MemberSyntax ParseMember()
        {
            if (Current.Kind == SyntaxKind.FunctionKeyword)
                return ParseFunctionDeclaration();

            return ParseGlobalStatement();
        }

        private FunctionDeclarationSyntax ParseFunctionDeclaration()
        {
            var functionKeyword = MatchToken(SyntaxKind.FunctionKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
            var parameters = ParseParameterList();
            var closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);
            var typeClause = ParseOptionalTypeClause();
            var body = ParseBlockStatement();
            return new FunctionDeclarationSyntax(functionKeyword, identifier, openParenthesisToken, parameters, closeParenthesisToken, typeClause, body);    
        }

        private SeparatedSyntaxList<ParameterSyntax> ParseParameterList()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
            
            while (Current.Kind != SyntaxKind.CloseParenthesisToken &&
                   Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var parameter = ParseParameter();
                nodesAndSeparators.Add(parameter);

                if (Current.Kind != SyntaxKind.CommaToken)
                {
                    break;
                }
                
                var comma = MatchToken(SyntaxKind.CommaToken);
                nodesAndSeparators.Add(comma);
            }

            return new SeparatedSyntaxList<ParameterSyntax>(nodesAndSeparators.ToImmutable());
        }
        
        private ParameterSyntax ParseParameter()
        {
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var typeClause = ParseTypeClause();
            return new ParameterSyntax(identifier, typeClause);
        }


        private GlobalStatementSyntax ParseGlobalStatement()
        {
            var statement = ParseStatement();
            return new GlobalStatementSyntax(statement);
        }

        private StatementSyntax ParseStatement()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenBraceToken:
                    return ParseBlockStatement();
                case SyntaxKind.VarKeyword:
                case SyntaxKind.LetKeyword:
                    return ParseVariableDeclaration();
                case SyntaxKind.IfKeyword:
                    return ParseIfStatement();
                case SyntaxKind.WhileKeyword:
                    return ParseWhileStatement();
                case SyntaxKind.ForKeyword:
                    return ParseForStatement();
                case SyntaxKind.DoKeyword:
                    return ParseDoWhileStatement();
                case SyntaxKind.BreakKeyword:
                    return ParseBreakStatementSyntax();
                case SyntaxKind.ContinueKeyword:
                    return ParseContinueStatementSyntax();
            }
            
            return ParseExpressionStatement();
        }

        private BreakStatementSyntax ParseBreakStatementSyntax()
        {
            var breakKeyword = MatchToken(SyntaxKind.BreakKeyword);
            return new BreakStatementSyntax(breakKeyword);
        }

        private ContinueStatementSyntax ParseContinueStatementSyntax()
        {
            var continueKeyword = MatchToken(SyntaxKind.ContinueKeyword);
            return new ContinueStatementSyntax(continueKeyword);
        }
        
        private DoWhileStatementSyntax ParseDoWhileStatement()
        {
            var doKeyword = MatchToken(SyntaxKind.DoKeyword);
            var body = ParseStatement();
            var whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
            var condition = ParseExpression();
            return new DoWhileStatementSyntax(doKeyword, body, whileKeyword, condition);
        }
        
        private ForStatementSyntax ParseForStatement()
        {
            var forKeyword = MatchToken(SyntaxKind.ForKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var equalsToken = MatchToken(SyntaxKind.EqualsToken);
            var lowerBound = ParseExpression();
            var toKeyword = MatchToken(SyntaxKind.ToKeyword);
            var upperBound = ParseExpression();
            var body = ParseStatement();
            return new ForStatementSyntax(forKeyword, identifier, equalsToken, lowerBound, toKeyword, upperBound, body);
        }
        
        private WhileStatementSyntax ParseWhileStatement()
        {
            var whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
            var condition = ParseExpression();
            var body = ParseStatement();
            return new WhileStatementSyntax(whileKeyword, condition, body);
        }

        private ElseClauseSyntax ParseElseClause()
        {
            var elseKeyword = MatchToken(SyntaxKind.ElseKeyword);
            var statement = ParseStatement();
            return new ElseClauseSyntax(elseKeyword,statement);
        }
        
        private IfStatementSyntax ParseIfStatement()
        {
            var ifKeyword = MatchToken(SyntaxKind.IfKeyword);
            var condition = ParseExpression();
            var thenStatement = ParseStatement();
            ElseClauseSyntax elseStatement = null;
            if (Current.Kind == SyntaxKind.ElseKeyword)
            {
                elseStatement = ParseElseClause();
            }
            return new IfStatementSyntax(ifKeyword, condition, thenStatement, elseStatement);
        }

        private VariableDeclarationSyntax ParseVariableDeclaration()
        {
            var expected = Current.Kind == SyntaxKind.VarKeyword ? SyntaxKind.VarKeyword : SyntaxKind.LetKeyword;
            var declarator = MatchToken(expected);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var typeClause = ParseOptionalTypeClause();
            var equalsToken = MatchToken(SyntaxKind.EqualsToken);
            var initializer = ParseExpression();
            return new VariableDeclarationSyntax(declarator, identifier, typeClause, equalsToken, initializer);
        }

        private TypeClauseSyntax ParseOptionalTypeClause()
        {
            if (Peek(0).Kind != SyntaxKind.ColonToken)
                return null;
            
            return ParseTypeClause();
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            var colonToken = MatchToken(SyntaxKind.ColonToken);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            return new TypeClauseSyntax(colonToken, identifier);
        }

        private BlockStatementSyntax ParseBlockStatement()
        {
            var stmtBuilder = ImmutableArray.CreateBuilder<StatementSyntax>();
            SyntaxToken openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);
            while (Current.Kind != SyntaxKind.EndOfFileToken && 
                   Current.Kind != SyntaxKind.CloseBraceToken)
            {
                var startToken = Current;
                var stmt = ParseStatement();
                stmtBuilder.Add(stmt);
                // If ParseStatement() did not consume any tokens,
                // we need to skip the current token and continue
                // in order to avoid an infinite loop.
                //
                // We don't need to report an error, because we'll
                // already tried to parse an expression statement
                // and reported one.
                if (Current == startToken)
                    NextToken();
            }
            
            var closeBraceToken = MatchToken(SyntaxKind.CloseBraceToken);
            return new BlockStatementSyntax(openBraceToken, stmtBuilder.ToImmutableArray(), closeBraceToken);
        }
        
        private ExpressionStatementSyntax ParseExpressionStatement()
        {
            var expression = ParseExpression();
            return new ExpressionStatementSyntax(expression);
        }

        private ExpressionSyntax ParseExpression()
        {
            return ParseAssignmentExpression();
        }

        private ExpressionSyntax ParseAssignmentExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken &&
                Peek(1).Kind == SyntaxKind.EqualsToken)
            {
                var identifierToken = NextToken();
                var operatorToken = NextToken();
                var right = ParseAssignmentExpression();
                return new AssignmentExpressionSyntax(identifierToken, operatorToken, right);
            }

            return ParseBinaryExpression();
        }

        private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
        {
            ExpressionSyntax left;
            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseBinaryExpression(unaryOperatorPrecedence);
                left = new UnaryExpressionSyntax(operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }

            while (true)
            {
                var precedence = Current.Kind.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                    break;

                var operatorToken = NextToken();
                var right = ParseBinaryExpression(precedence);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        private ExpressionSyntax ParsePrimaryExpression()
        {
            switch (Current.Kind)
            {
                case SyntaxKind.OpenParenthesisToken:
                    return ParseParenthesizedExpression();

                case SyntaxKind.FalseKeyword:
                case SyntaxKind.TrueKeyword:
                    return ParseBooleanLiteral();

                case SyntaxKind.NumberToken:
                    return ParseNumberLiteral();
                
                case SyntaxKind.StringToken:
                    return ParseStringLiteral();

                case SyntaxKind.IdentifierToken:
                default:
                    return ParseNameOrCallExpression();
            }
        }

        private ExpressionSyntax ParseNameOrCallExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.OpenParenthesisToken)
            {
                return ParseCallExpression();
            }
            
            return ParseNameExpression();
        }

        private ExpressionSyntax ParseCallExpression()
        {
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var openParenthesis = MatchToken(SyntaxKind.OpenParenthesisToken);
            var arguments = ParseArgumentList();
            var closeParenthesis = MatchToken(SyntaxKind.CloseParenthesisToken);
            if (TypeSymbol.TryGetSymbol(identifier.Text, out _) && arguments.Count == 1)
            {
                return new ConversionExpressionSyntax(identifier, openParenthesis, arguments[0], closeParenthesis);
            }
            
            return new CallExpressionSyntax(identifier, openParenthesis, arguments, closeParenthesis);
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArgumentList()
        {
            var builder = ImmutableArray.CreateBuilder<SyntaxNode>();
            while (Current.Kind != SyntaxKind.CloseParenthesisToken &&
                   Current.Kind != SyntaxKind.EndOfFileToken)
            {
                var argument = ParseExpression();
                builder.Add(argument);
                if (Current.Kind != SyntaxKind.CommaToken)
                {
                    break;
                }
                
                var separator = MatchToken(SyntaxKind.CommaToken);
                builder.Add(separator);
            }

            return new SeparatedSyntaxList<ExpressionSyntax>(builder.ToImmutable());
        }

        private ExpressionSyntax ParseParenthesizedExpression()
        {
            var left = MatchToken(SyntaxKind.OpenParenthesisToken);
            var expression = ParseExpression();
            var right = MatchToken(SyntaxKind.CloseParenthesisToken);
            return new ParenthesizedExpressionSyntax(left, expression, right);
        }

        private ExpressionSyntax ParseBooleanLiteral()
        {
            var isTrue = Current.Kind == SyntaxKind.TrueKeyword;
            var keywordToken =isTrue ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
            return new LiteralExpressionSyntax(keywordToken, isTrue);
        }

        private ExpressionSyntax ParseNumberLiteral()
        {
            var numberToken = MatchToken(SyntaxKind.NumberToken);
            return new LiteralExpressionSyntax(numberToken);
        }
        
        private ExpressionSyntax ParseStringLiteral()
        {
            var stringToken = MatchToken(SyntaxKind.StringToken);
            return new LiteralExpressionSyntax(stringToken);
        }

        private ExpressionSyntax ParseNameExpression()
        {
            var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
            return new NameExpressionSyntax(identifierToken);
        }
    }
}