using System;
using System.IO;
using System.Linq;
using System.Text;
using MiniRoslyn.CodeAnalysis.Syntax;

namespace MiniRoslyn.CodeAnalysis
{
    public class AstPrinter
    {
        private SyntaxNode _root;
        public AstPrinter(SyntaxNode root)
        {
            _root = root;
        }

        public void PrintAst(StringBuilder sb)
        {
            this.PrettyPrint(new StringWriter(sb),_root);
            Console.WriteLine(sb.ToString());
        }
        private void PrettyPrint(TextWriter textWriter, SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";

            textWriter.Write(indent);
            textWriter.Write(marker);
            textWriter.Write(node.Kind);

            if (node is SyntaxToken t && t.Value != null)
            {
                textWriter.Write(" ");
                textWriter.Write(t.Value);
            }

            textWriter.WriteLine();
            
            indent += isLast ? "   " : "│   ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())            
                PrettyPrint(textWriter, child, indent, child == lastChild);
        }
    }
}