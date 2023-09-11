using System;

namespace CodeAnalysis.Syntax.InternalSyntax
{
    public abstract class ExpressionSyntax : LSharpSyntaxNode
    {
        internal ExpressionSyntax(SyntaxKind kind) : base(kind)
        {
        }
    }

    public sealed partial class LiteralExpressionSyntax : ExpressionSyntax
    {
        internal readonly SyntaxToken token;

        internal LiteralExpressionSyntax(SyntaxKind kind, SyntaxToken token, SyntaxFactoryContext context) : base(kind)
        {
            this.SlotCount = 1;
            this.AdjustFlagsAndWidth(token);
            this.token = token;
        }

        internal override GreenNode GetSlot(int index)
        {
            switch (index)
            {
                case 0:
                    return this.token;
                default:
                    return null;
            }
        }

        internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
        {
            return new CodeAnalysis.Syntax.LiteralExpressionSyntax(this, parent, position);
        }
    }

    public sealed partial class PrefixUnaryExpressionSyntax : ExpressionSyntax
    {
        internal readonly SyntaxToken operatorToken;
        internal readonly ExpressionSyntax operand;

        public PrefixUnaryExpressionSyntax(SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax operand) :
            base(kind)
        {
            this.SlotCount = 2;
            this.AdjustFlagsAndWidth(operatorToken);
            this.operatorToken = operatorToken;
            this.AdjustFlagsAndWidth(operand);
            this.operand = operand;
        }

        /// <summary>SyntaxToken representing the kind of the operator of the prefix unary expression.</summary>
        public SyntaxToken OperatorToken
        {
            get { return this.operatorToken; }
        }

        /// <summary>ExpressionSyntax representing the operand of the prefix unary expression.</summary>
        public ExpressionSyntax Operand
        {
            get { return this.operand; }
        }

        internal override GreenNode GetSlot(int index)
        {
            switch (index)
            {
                case 0:
                    return this.operatorToken;
                case 1:
                    return this.operand;
                default:
                    return null;
            }
        }

        internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class BinaryExpressionSyntax : ExpressionSyntax
    {
        internal readonly ExpressionSyntax left;
        internal readonly SyntaxToken operatorToken;
        internal readonly ExpressionSyntax right;

        public BinaryExpressionSyntax(SyntaxKind kind, ExpressionSyntax left, SyntaxToken operatorToken,
            ExpressionSyntax right) : base(kind)
        {
            this.SlotCount = 3;
            this.AdjustFlagsAndWidth(left);
            this.left = left;
            this.AdjustFlagsAndWidth(operatorToken);
            this.operatorToken = operatorToken;
            this.AdjustFlagsAndWidth(right);
            this.right = right;
        }

        /// <summary>ExpressionSyntax node representing the expression on the left of the binary operator.</summary>
        public ExpressionSyntax Left
        {
            get { return this.left; }
        }

        /// <summary>SyntaxToken representing the operator of the binary expression.</summary>
        public SyntaxToken OperatorToken
        {
            get { return this.operatorToken; }
        }

        /// <summary>ExpressionSyntax node representing the expression on the right of the binary operator.</summary>
        public ExpressionSyntax Right
        {
            get { return this.right; }
        }

        internal override GreenNode GetSlot(int index)
        {
            switch (index)
            {
                case 0:
                    return this.left;
                case 1:
                    return this.operatorToken;
                case 2:
                    return this.right;
                default:
                    return null;
            }
        }

        internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
        {
            return new CodeAnalysis.Syntax.BinaryExpressionSyntax(this, parent, position);
        }
    }


    internal class ContextAwareSyntax
    {
        private SyntaxFactoryContext context;


        public ContextAwareSyntax(SyntaxFactoryContext context)
        {
            this.context = context;
        }

        public LiteralExpressionSyntax LiteralExpression(SyntaxKind kind, SyntaxToken token)
        {
            switch (kind)
            {
                case SyntaxKind.NumericLiteralExpression:
                    break;
                default:
                    throw new ArgumentException("kind");
            }
#if DEBUG
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            switch (token.Kind)
            {
                case SyntaxKind.NumericLiteralToken:
                    break;
                default:
                    throw new ArgumentException("token");
            }
#endif

            int hash;
            var cached = SyntaxNodeCache.TryGetNode((int) kind, token, this.context, out hash);
            if (cached != null) return (LiteralExpressionSyntax) cached;

            var result = new LiteralExpressionSyntax(kind, token, this.context);
            if (hash >= 0)
            {
                SyntaxNodeCache.AddNode(result, hash);
            }

            return result;
        }

        public PrefixUnaryExpressionSyntax PrefixUnaryExpression(SyntaxKind kind, SyntaxToken operatorToken,
            ExpressionSyntax operand)
        {
            switch (kind)
            {
                case SyntaxKind.UnaryMinusExpression:
                    break;
                case SyntaxKind.UnaryPlusExpression:
                    break;
                default:
                    throw new ArgumentException("kind");
            }
#if DEBUG
            if (operatorToken == null)
                throw new ArgumentNullException(nameof(operatorToken));
            switch (operatorToken.Kind)
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    break;
                default:
                    throw new ArgumentException("operatorToken");
            }

            if (operand == null)
                throw new ArgumentNullException(nameof(operand));
#endif

            int hash;
            var cached = SyntaxNodeCache.TryGetNode((int) kind, operatorToken, operand, out hash);
            if (cached != null) return (PrefixUnaryExpressionSyntax) cached;

            var result = new PrefixUnaryExpressionSyntax(kind, operatorToken, operand);
            if (hash >= 0)
            {
                SyntaxNodeCache.AddNode(result, hash);
            }

            return result;
        }

        public BinaryExpressionSyntax BinaryExpression(SyntaxKind kind, ExpressionSyntax left,
            SyntaxToken operatorToken, ExpressionSyntax right)
        {
            switch (kind)
            {
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubtractExpression:
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                    break;
                default:
                    throw new ArgumentException("kind");
            }
#if DEBUG
            if (left == null)
                throw new ArgumentNullException(nameof(left));
            if (operatorToken == null)
                throw new ArgumentNullException(nameof(operatorToken));
            switch (operatorToken.Kind)
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.AsteriskToken:
                case SyntaxKind.SlashToken:
                    break;
                default:
                    throw new ArgumentException("operatorToken");
            }

            if (right == null)
                throw new ArgumentNullException(nameof(right));
#endif

            int hash;
            var cached = SyntaxNodeCache.TryGetNode((int) kind, left, operatorToken, right, out hash);
            if (cached != null) return (BinaryExpressionSyntax) cached;

            var result = new BinaryExpressionSyntax(kind, left, operatorToken, right);
            if (hash >= 0)
            {
                SyntaxNodeCache.AddNode(result, hash);
            }

            return result;
        }
    }
}