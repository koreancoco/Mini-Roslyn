using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniRoslyn.CodeAnalysis.Visitor;
using MiniRoslyn.CodeAnalysis.Text;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public class SyntaxToken : SyntaxNode
    {
        private SyntaxKind _kind;
        private int _position;
        private string _text;
        private object _value;

        public SyntaxToken(SyntaxKind kind, int position, string text, object value)
        {
            _kind = kind;
            _position = position;
            _text = text;
            _value = value;
        }


        public override SyntaxKind Kind => _kind;
        
        public int Position => _position;

        public string Text => _text;

        public object Value => _value;

        public override TextSpan Span => new TextSpan(Position, Text?.Length ?? 0);
        /// <summary>
        /// A token is missing if it was inserted by the parser and doesn't appear in source.
        /// </summary>
        public bool IsMissing => Text == null;
    }
}