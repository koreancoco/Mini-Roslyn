using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MiniRoslyn.CodeAnalysis.Syntax;

namespace MiniRoslyn.CodeAnalysis.Binding.Visitor
{
    internal class Lowerer : BoundTreeRewriter
    {
        private int _labelIndex;
        
        private Lowerer()
        {
        }

        private LabelSymbol GenerateLabel()
        {
            return new LabelSymbol($"label{++_labelIndex}");
        }

        public static BoundBlockStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            var stmt = lowerer.VisitBoundStatement(statement);
            return Flatten(stmt);
        }

        // 展开所有block语句
        private static BoundBlockStatement Flatten(BoundStatement statement)
        {
            var stmtBuilder = ImmutableArray.CreateBuilder<BoundStatement>();
            var stack = new Stack<BoundStatement>();
            stack.Push(statement);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current is BoundBlockStatement block)
                {
                    foreach (var s in block.Statements.Reverse())
                    {
                        stack.Push(s);
                    }
                }
                else
                {
                    stmtBuilder.Add(current);
                }
            }

            return new BoundBlockStatement(stmtBuilder.ToImmutable());
        }

        public override BoundStatement VisitBoundIfStatement(BoundIfStatement syntax)
        {
            if (syntax.ElseStatement == null)
            {
                // if <condition>
                //      <then>
                //
                // ---->
                //
                // gotoFalse <condition> end
                // <then>  
                // end:

                var endLabel = GenerateLabel();
                var endLabelStatement = new BoundLabelStatement(endLabel);
                var gotoFalse = new BoundConditionalGotoStatement(syntax.Condition, endLabel, true);
                var result = new BoundBlockStatement(ImmutableArray.Create(gotoFalse, syntax.ThenStatement, endLabelStatement));
                return VisitBoundBlockStatement(result);
            }
            else
            {
                // if <condition>
                //      <then>
                // else
                //      <else>
                //
                // ---->
                //
                // gotoFalse <condition> else
                // <then>
                // goto end
                // else:
                // <else>
                // end:
                
                var elseLabel = GenerateLabel();
                var endLabel = GenerateLabel();
                var elseLabelStatement = new BoundLabelStatement(elseLabel);
                var endLabelStatement = new BoundLabelStatement(endLabel);
                var gotoElse = new BoundConditionalGotoStatement(syntax.Condition, elseLabel, true);
                var gotoEnd = new BoundGotoStatement(endLabel);
                var result = new BoundBlockStatement(ImmutableArray.Create(
                    gotoElse, 
                    syntax.ThenStatement, 
                    gotoEnd,
                    elseLabelStatement,
                    syntax.ElseStatement,
                    endLabelStatement
                    ));
                return VisitBoundBlockStatement(result);
            }
        }

        public override BoundStatement VisitBoundWhileStatement(BoundWhileStatement syntax)
        {
            // while <condition>
            //      <body>
            //
            // ---->
            //
            // check:
            // gotoFalse <condition> end
            // <body>
            // goto check
            // end

            var checkLabel = GenerateLabel();
            var endLabel = GenerateLabel();
            var checkLabelStatement = new BoundLabelStatement(checkLabel);
            var endLabelStatement = new BoundLabelStatement(endLabel);

            var gotoEnd = new BoundConditionalGotoStatement(syntax.Condition, endLabel, true);
            var gotoCheck = new BoundGotoStatement(checkLabel);
            var result = new BoundBlockStatement(ImmutableArray.Create(
                checkLabelStatement, 
                gotoEnd, 
                syntax.Body,
                gotoCheck,
                endLabelStatement
            ));
            
            return VisitBoundBlockStatement(result);
        }

        public override BoundStatement VisitBoundForStatement(BoundForStatement syntax)
        {
            // for <var> = <lower> to <upper>
            //      <body>
            //
            // ---->
            //
            // {
            //      var <var> = <lower>
            //      while (<var> <= <upper>)
            //      {
            //          <body>
            //          <var> = <var> + 1
            //      }   
            // }

            var variableDeclaration = new BoundVariableDeclaration(syntax.Variable, syntax.LowerBound);
            var variableExpression = new BoundVariableExpression(syntax.Variable);
            var condition = new BoundBinaryExpression(
                variableExpression,
                BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken, typeof(int), typeof(int)),
                syntax.UpperBound
            );            
            var increment = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    syntax.Variable,
                    new BoundBinaryExpression(
                        variableExpression,
                        BoundBinaryOperator.Bind(SyntaxKind.PlusToken, typeof(int), typeof(int)),
                        new BoundLiteralExpression(1)
                    )
                )
            );
            var whileBody = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(syntax.Body, increment));
            var whileStatement = new BoundWhileStatement(condition, whileBody);
            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(variableDeclaration, whileStatement));
     
            return VisitBoundBlockStatement(result);
        }
    }
}