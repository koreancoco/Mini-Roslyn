using System;
using System.CodeDom.Compiler;
using System.IO;
using MiniRoslyn.CodeAnalysis.Symbols;
using MiniRoslyn.CodeAnalysis.Syntax;
using MiniRoslyn.CodeAnalysis.Visitor;
using MiniRoslyn.IO;

namespace MiniRoslyn.CodeAnalysis.Binding
{
    internal class BoundNodePrinter : IBoundExpressionVisitor<IndentedTextWriter>, IBoundStatementVisitor<IndentedTextWriter>
    {
        private IndentedTextWriter _writer;

        public BoundNodePrinter(IndentedTextWriter writer)
        {
            _writer = writer;
        }

        public static void WriteTo(BoundStatement syntax, TextWriter writer)
        {
            BoundNodePrinter printer;
            if (writer is IndentedTextWriter iw)
            {
                printer = new BoundNodePrinter(iw);
            }
            else
            {
                printer = new BoundNodePrinter(new IndentedTextWriter(writer));
            }

            printer.VisitBoundStatement(syntax);
        }
        

        private void WriteNestedStatement(BoundStatement syntax)
        {
            var needsIndentation = !(syntax is BoundBlockStatement);

            if (needsIndentation)
                _writer.Indent++;

            VisitBoundStatement(syntax);

            if (needsIndentation)
                _writer.Indent--;
        }

        private void WriteNestedExpression(int parentPrecedence, BoundExpression expression)
        {
            if (expression is BoundUnaryExpression unary)
                WriteNestedExpression(parentPrecedence, SyntaxFacts.GetUnaryOperatorPrecedence(unary.Operator.SyntaxKind), unary);
            else if (expression is BoundBinaryExpression binary)
                WriteNestedExpression(parentPrecedence, SyntaxFacts.GetBinaryOperatorPrecedence(binary.Operator.SyntaxKind), binary);
            else
                VisitBoundExpression(expression);
        }

        private void WriteNestedExpression(int parentPrecedence, int currentPrecedence, BoundExpression expression)
        {
            var needsParenthesis = parentPrecedence >= currentPrecedence;

            if (needsParenthesis)
                _writer.WritePunctuation("(");
            
            VisitBoundExpression(expression);
            
            if (needsParenthesis)
                _writer.WritePunctuation(")");
        }

        public IndentedTextWriter VisitBoundLiteralExpression(BoundLiteralExpression syntax)
        {
            var value = syntax.Value.ToString();

            if (syntax.Type == TypeSymbol.Bool)
            {
                _writer.WriteKeyword(value);
            }
            else if (syntax.Type == TypeSymbol.Int)
            {
                _writer.WriteNumber(value);
            }
            else if (syntax.Type == TypeSymbol.String)
            {
                value = "\"" + value.Replace("\"", "\"\"") + "\"";
                _writer.WriteString(value);
            }
            else
            {
                throw new Exception($"Unexpected type {syntax.Type}");
            }

            return _writer;
        }

        public IndentedTextWriter VisitBoundBinaryExpression(BoundBinaryExpression syntax)
        {
            var op = SyntaxFacts.GetText(syntax.Operator.SyntaxKind);
            var precedence = SyntaxFacts.GetBinaryOperatorPrecedence(syntax.Operator.SyntaxKind);

            WriteNestedExpression(precedence, syntax.Left);
            _writer.Write(" ");
            _writer.WritePunctuation(op);
            _writer.Write(" ");
            WriteNestedExpression(precedence, syntax.Right);
            return _writer;
        }

        public IndentedTextWriter VisitBoundUnaryExpression(BoundUnaryExpression syntax)
        {
            var op = SyntaxFacts.GetText(syntax.Operator.SyntaxKind);
            var precedence = SyntaxFacts.GetUnaryOperatorPrecedence(syntax.Operator.SyntaxKind);

            _writer.WritePunctuation(op);
            WriteNestedExpression(precedence, syntax.Operand);
            return _writer;
        }

        public IndentedTextWriter VisitBoundVariableExpression(BoundVariableExpression syntax)
        {
            _writer.WriteIdentifier(syntax.Variable.Name);
            return _writer;
        }

        public IndentedTextWriter VisitBoundAssignmentExpression(BoundAssignmentExpression syntax)
        {
            _writer.WriteIdentifier(syntax.Variable.Name);
            _writer.WritePunctuation(" = ");
            VisitBoundExpression(syntax.Expression);
            return _writer;
        }

        public IndentedTextWriter VisitBoundErrorExpression(BoundErrorExpression syntax)
        {
            _writer.WriteKeyword("?");
            return _writer;
        }

        public IndentedTextWriter VisitBoundCallExpression(BoundCallExpression syntax)
        {
            _writer.WriteIdentifier(syntax.Function.Name);
            _writer.WritePunctuation("(");

            var isFirst = true;
            foreach (var argument in syntax.Arguments)
            {
                if (isFirst)
                    isFirst = false;
                else
                    _writer.WritePunctuation(", ");

                VisitBoundExpression(argument);
            }

            _writer.WritePunctuation(")");
            return _writer;
        }

        public IndentedTextWriter VisitBoundConversionExpression(BoundConversionExpression syntax)
        {
            _writer.WriteIdentifier(syntax.Type.Name);
            _writer.WritePunctuation("(");
            VisitBoundExpression(syntax.Expression);
            _writer.WritePunctuation(")");
            return _writer;
        }

        public IndentedTextWriter VisitBoundBlockStatement(BoundBlockStatement syntax)
        {
            _writer.WritePunctuation("{");
            _writer.WriteLine();
            _writer.Indent++;

            foreach (var statement in syntax.Statements)
            {
                VisitBoundStatement(statement);
            }

            _writer.Indent--;
            _writer.WritePunctuation("}");
            _writer.WriteLine();

            return _writer;
        }

        public IndentedTextWriter VisitBoundExpressionStatement(BoundExpressionStatement syntax)
        {
            VisitBoundExpression(syntax.Expression);
            _writer.WriteLine();
            return _writer;
        }

        public IndentedTextWriter VisitBoundVariableDeclaration(BoundVariableDeclaration syntax)
        {
            _writer.WriteKeyword(syntax.Variable.IsReadOnly ? "let " : "var ");
            _writer.WriteIdentifier(syntax.Variable.Name);
            _writer.WritePunctuation(" = ");
            VisitBoundExpression(syntax.Initializer);
            _writer.WriteLine();
            return _writer;
        }

        public IndentedTextWriter VisitBoundIfStatement(BoundIfStatement syntax)
        {
            _writer.WriteKeyword("if ");
            VisitBoundExpression(syntax.Condition);
            _writer.WriteLine();
            WriteNestedStatement(syntax.ThenStatement);

            if (syntax.ElseStatement != null)
            {
                _writer.WriteKeyword("else");
                _writer.WriteLine();
                WriteNestedStatement(syntax.ElseStatement);
            }

            return _writer;
        }

        public IndentedTextWriter VisitBoundWhileStatement(BoundWhileStatement syntax)
        {
            _writer.WriteKeyword("while ");
            VisitBoundExpression(syntax.Condition);
            _writer.WriteLine();
            WriteNestedStatement(syntax.Body);
            return _writer;
        }

        public IndentedTextWriter VisitBoundForStatement(BoundForStatement syntax)
        {
            _writer.WriteKeyword("for ");
            _writer.WriteIdentifier(syntax.Variable.Name);
            _writer.WritePunctuation(" = ");
            VisitBoundExpression(syntax.LowerBound);
            _writer.WriteKeyword(" to ");
            VisitBoundExpression(syntax.UpperBound);
            _writer.WriteLine();
            WriteNestedStatement(syntax.Body);
            return _writer;
        }

        public IndentedTextWriter VisitBoundLabelStatement(BoundLabelStatement syntax)
        {
            var unindent = _writer.Indent > 0;
            if (unindent)
                _writer.Indent--;

            _writer.WritePunctuation(syntax.BoundLabel.Name);
            _writer.WritePunctuation(":");
            _writer.WriteLine();

            if (unindent)
                _writer.Indent++;
            return _writer;
        }

        public IndentedTextWriter VisitBoundConditionalGotoStatement(BoundConditionalGotoStatement syntax)
        {
            _writer.WriteKeyword("goto ");
            _writer.WriteIdentifier(syntax.BoundLabel.Name);
            _writer.WriteKeyword(syntax.JumpIfTrue ? " if " : " unless ");
            VisitBoundExpression(syntax.Condition);
            _writer.WriteLine();
            return _writer;
        }

        public IndentedTextWriter VisitBoundGotoStatement(BoundGotoStatement syntax)
        {
            _writer.WriteKeyword("goto ");
            _writer.WriteIdentifier(syntax.BoundLabel.Name);
            _writer.WriteLine();
            return _writer;
        }

        public IndentedTextWriter VisitBoundDoWhileStatement(BoundDoWhileStatement syntax)
        {
            _writer.WriteKeyword("do");
            _writer.WriteLine();
            WriteNestedStatement(syntax.Body);
            _writer.WriteKeyword("while ");
            VisitBoundExpression(syntax.Condition);
            _writer.WriteLine();
            return _writer;
        }

        public IndentedTextWriter VisitBoundFunctionDeclaration(BoundFunctionDeclaration syntax)
        {
            _writer.WriteKeyword("function ");
            _writer.WriteIdentifier(syntax.Function.Name);
            _writer.WritePunctuation("(");

            var isFirst = true;
            foreach (var parameter in syntax.Function.Parameters)
            {
                if (isFirst)
                    isFirst = false;
                else
                    _writer.WritePunctuation(", ");

                _writer.WriteIdentifier(parameter.Name);
            }

            _writer.WritePunctuation(")");
            _writer.WriteLine();
            WriteNestedStatement(syntax.Function.Body);
            return _writer;
        }
        
        private IndentedTextWriter VisitBoundStatement(BoundStatement syntax)
        {
            return syntax.Accept(this);
        }
        
        private IndentedTextWriter VisitBoundExpression(BoundExpression syntax)
        {
            return syntax.Accept(this);
        }
        
    }
}