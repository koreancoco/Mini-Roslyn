using System.Collections.Immutable;
using MiniRoslyn.CodeAnalysis.Binding;
using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Lowering
{
    internal abstract class BoundTreeRewriter : IBoundExpressionVisitor<BoundExpression>,
        IBoundStatementVisitor<BoundStatement>
    {
        public BoundStatement VisitBoundStatement(BoundStatement syntax)
        {
            return syntax.Accept(this);
        }

        private BoundExpression VisitBoundExpression(BoundExpression syntax)
        {
            return syntax.Accept(this);
        }

        public virtual BoundExpression VisitBoundLiteralExpression(BoundLiteralExpression syntax)
        {
            return syntax;
        }

        public virtual BoundExpression VisitBoundBinaryExpression(BoundBinaryExpression syntax)
        {
            var left = VisitBoundExpression(syntax.Left);
            var right = VisitBoundExpression(syntax.Right);
            if (left == syntax.Left && right == syntax.Right)
                return syntax;

            return new BoundBinaryExpression(left, syntax.Operator, right);
        }

        public virtual BoundExpression VisitBoundUnaryExpression(BoundUnaryExpression syntax)
        {
            var operand = VisitBoundExpression(syntax.Operand);
            if (operand == syntax.Operand)
                return syntax;

            return new BoundUnaryExpression(syntax.Operator, operand);
        }

        public virtual BoundExpression VisitBoundVariableExpression(BoundVariableExpression syntax)
        {
            return syntax;
        }

        public virtual BoundExpression VisitBoundAssignmentExpression(BoundAssignmentExpression syntax)
        {
            var expression = VisitBoundExpression(syntax.Expression);
            if (expression == syntax.Expression)
                return syntax;

            return new BoundAssignmentExpression(syntax.Variable, expression);
        }

        public BoundExpression VisitBoundErrorExpression(BoundErrorExpression syntax)
        {
            return syntax;
        }

        public BoundExpression VisitBoundCallExpression(BoundCallExpression syntax)
        {
            ImmutableArray<BoundExpression>.Builder builder = null;

            for (var i = 0; i < syntax.Arguments.Length; i++)
            {
                var oldArgument = syntax.Arguments[i];
                var newArgument = VisitBoundExpression(oldArgument);
                if (newArgument != oldArgument)
                {
                    if (builder == null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundExpression>(syntax.Arguments.Length);

                        for (var j = 0; j < i; j++)
                            builder.Add(syntax.Arguments[j]);
                    }
                }

                if (builder != null)
                    builder.Add(newArgument);
            }

            if (builder == null)
                return syntax;

            return new BoundCallExpression(syntax.Function,builder.MoveToImmutable());
        }

        public BoundExpression VisitBoundConversionExpression(BoundConversionExpression syntax)
        {
            var expression = VisitBoundExpression(syntax.Expression);
            if (expression == syntax.Expression)
                return syntax;

            return new BoundConversionExpression(syntax.Type, expression);
        }

        public virtual BoundStatement VisitBoundBlockStatement(BoundBlockStatement syntax)
        {
            ImmutableArray<BoundStatement>.Builder builder = null;

            for (var i = 0; i < syntax.Statements.Length; i++)
            {
                var oldStatement = syntax.Statements[i];
                var newStatement = VisitBoundStatement(oldStatement);
                if (newStatement != oldStatement)
                {
                    if (builder == null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundStatement>(syntax.Statements.Length);

                        for (var j = 0; j < i; j++)
                            builder.Add(syntax.Statements[j]);
                    }
                }

                if (builder != null)
                    builder.Add(newStatement);
            }

            if (builder == null)
                return syntax;

            return new BoundBlockStatement(builder.MoveToImmutable());
        }

        public virtual BoundStatement VisitBoundExpressionStatement(BoundExpressionStatement syntax)
        {
            var expression = VisitBoundExpression(syntax.Expression);
            if (expression == syntax.Expression)
                return syntax;

            return new BoundExpressionStatement(expression);
        }

        public virtual BoundStatement VisitBoundVariableDeclaration(BoundVariableDeclaration syntax)
        {
            var initializer = VisitBoundExpression(syntax.Initializer);
            if (initializer == syntax.Initializer)
                return syntax;

            return new BoundVariableDeclaration(syntax.Variable, initializer);
        }

        public virtual BoundStatement VisitBoundIfStatement(BoundIfStatement syntax)
        {
            var condition = VisitBoundExpression(syntax.Condition);
            var thenStatement = VisitBoundStatement(syntax.ThenStatement);
            var elseStatement = syntax.ElseStatement == null ? null : VisitBoundStatement(syntax.ElseStatement);
            if (condition == syntax.Condition && thenStatement == syntax.ThenStatement &&
                elseStatement == syntax.ElseStatement)
                return syntax;

            return new BoundIfStatement(condition, thenStatement, elseStatement);
        }

        public virtual BoundStatement VisitBoundWhileStatement(BoundWhileStatement syntax)
        {
            var condition = VisitBoundExpression(syntax.Condition);
            var body = VisitBoundStatement(syntax.Body);
            if (condition == syntax.Condition && body == syntax.Body)
                return syntax;

            return new BoundWhileStatement(condition, body, syntax.BreakLabel, syntax.ContinueLabel);
        }

        public virtual BoundStatement VisitBoundForStatement(BoundForStatement syntax)
        {
            var lowerBound = VisitBoundExpression(syntax.LowerBound);
            var upperBound = VisitBoundExpression(syntax.UpperBound);
            var body = VisitBoundStatement(syntax.Body);
            if (lowerBound == syntax.LowerBound && upperBound == syntax.UpperBound && body == syntax.Body)
                return syntax;

            return new BoundForStatement(syntax.Variable, lowerBound, upperBound, body, syntax.BreakLabel, syntax.ContinueLabel);
        }

        public BoundStatement VisitBoundLabelStatement(BoundLabelStatement syntax)
        {
            return syntax;
        }

        public BoundStatement VisitBoundConditionalGotoStatement(BoundConditionalGotoStatement syntax)
        {
            var condition = VisitBoundExpression(syntax.Condition);
            if (condition == syntax.Condition)
                return syntax;

            return new BoundConditionalGotoStatement(condition, syntax.BoundLabel, syntax.JumpIfTrue);
        }

        public BoundStatement VisitBoundGotoStatement(BoundGotoStatement syntax)
        {
            return syntax;
        }

        public virtual BoundStatement VisitBoundDoWhileStatement(BoundDoWhileStatement syntax)
        {
            var condition = VisitBoundExpression(syntax.Condition);
            var body = VisitBoundStatement(syntax.Body);
            if (condition == syntax.Condition && body == syntax.Body)
                return syntax;

            return new BoundDoWhileStatement(body, condition, syntax.BreakLabel, syntax.ContinueLabel);
        }

        public BoundStatement VisitBoundFunctionDeclaration(BoundFunctionDeclaration syntax)
        {
            throw new System.NotImplementedException();
        }
    }
}