using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MiniRoslyn.CodeAnalysis.Binding;
using MiniRoslyn.CodeAnalysis.Symbols;
using MiniRoslyn.CodeAnalysis.Syntax;

namespace MiniRoslyn.CodeAnalysis.Lowering
{
    internal class Lowerer : BoundTreeRewriter
    {
        private int _labelIndex;
        
        private Lowerer()
        {
        }

        private BoundLabel GenerateLabel()
        {
            return new BoundLabel($"label{++_labelIndex}");
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
                var gotoFalse = new BoundConditionalGotoStatement(syntax.Condition, endLabel, false);
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
                var gotoElse = new BoundConditionalGotoStatement(syntax.Condition, elseLabel, false);
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
            // gotoFalse <condition> break
            // continue:
            // <body>
            // goto check
            // break

            var checkLabel = GenerateLabel();
            var checkLabelStatement = new BoundLabelStatement(checkLabel);
            var continueLabelStatement = new BoundLabelStatement(syntax.ContinueLabel);
            var breakLabelStatement = new BoundLabelStatement(syntax.BreakLabel);

            var gotoBreak = new BoundConditionalGotoStatement(syntax.Condition, syntax.BreakLabel, false);
            var gotoCheck = new BoundGotoStatement(checkLabel);
            var result = new BoundBlockStatement(ImmutableArray.Create(
                checkLabelStatement, 
                gotoBreak, 
                continueLabelStatement,
                syntax.Body,
                gotoCheck,
                breakLabelStatement
            ));
            
            return VisitBoundBlockStatement(result);
        }

        public override BoundStatement VisitBoundDoWhileStatement(BoundDoWhileStatement syntax)
        {
            // do 
            //   <body>
            // while <condition>
            //
            // ---->
            // 
            // continue:
            // <body>
            // gotoTrue <condition> continue:
            // break:
            //
            
            var continueLabelStatement = new BoundLabelStatement(syntax.ContinueLabel);
            var gotoLoop = new BoundConditionalGotoStatement(syntax.Condition, syntax.ContinueLabel, true);
            var breakLabelStatement = new BoundLabelStatement(syntax.BreakLabel);
            var result = new BoundBlockStatement(ImmutableArray.Create(
                    continueLabelStatement,
                    syntax.Body,
                    gotoLoop,
                    breakLabelStatement
                )
            );

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
            //      let upper = <upper>
            //      while (<var> <= upper)
            //      {
            //          <body>
            //          continue:
            //          <var> = <var> + 1
            //      }   
            // }

            var variableDeclaration = new BoundVariableDeclaration(syntax.Variable, syntax.LowerBound);
            var upperBoundSymbol = new LocalVariableSymbol("upperBound", true, TypeSymbol.Int);
            var upperBoundDeclaration = new BoundVariableDeclaration(upperBoundSymbol, syntax.UpperBound);
            var variableExpression = new BoundVariableExpression(syntax.Variable);
            var condition = new BoundBinaryExpression(
                variableExpression,
                BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken, TypeSymbol.Int, TypeSymbol.Int),
                new BoundVariableExpression(upperBoundSymbol)
            );            
            var continueLabelStatement = new BoundLabelStatement(syntax.ContinueLabel);
            var increment = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    syntax.Variable,
                    new BoundBinaryExpression(
                        variableExpression,
                        BoundBinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Int, TypeSymbol.Int),
                        new BoundLiteralExpression(1)
                    )
                )
            );
            var whileBody = new BoundBlockStatement(ImmutableArray.Create(syntax.Body, increment, continueLabelStatement));
            var whileStatement = new BoundWhileStatement(condition, whileBody, syntax.BreakLabel, GenerateLabel());
            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                variableDeclaration, 
                upperBoundDeclaration,
                whileStatement));
     
            return VisitBoundBlockStatement(result);
        }
    }
}