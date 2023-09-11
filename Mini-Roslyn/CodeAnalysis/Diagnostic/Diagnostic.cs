using MiniRoslyn.CodeAnalysis;
using MiniRoslyn.CodeAnalysis.Text;

namespace MiniRoslyn.CodeAnalysis.Diagnostics
{
    public class Diagnostic
    {
        public TextSpan Span { get; }
        public string Message { get; }

        public Diagnostic(TextSpan span, string message)
        {
            Span = span;
            Message = message;
        }

        public override string ToString() => Message;
    }
}