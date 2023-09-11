namespace CodeAnalysis.Syntax.InternalSyntax
{
    public abstract class AbstractLexer
    {
        internal readonly SlidingTextWindow TextWindow;

        
        protected AbstractLexer(SourceText text)
        {
            this.TextWindow = new SlidingTextWindow(text);
        }
    }
}