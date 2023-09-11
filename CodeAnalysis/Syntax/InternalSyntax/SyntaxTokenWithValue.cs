namespace CodeAnalysis.Syntax.InternalSyntax
{
    public class SyntaxTokenWithValue<T> : SyntaxToken
    {
        protected readonly string TextField;
        protected readonly T ValueField;
        
        public SyntaxTokenWithValue(SyntaxKind kind, string text, T value)
            : base(kind,text.Length)
        {
            this.TextField = text;
            this.ValueField = value;
        }
        
        public override string Text
        {
            get
            {
                return this.TextField;
            }
        }
        
        public override object Value
        {
            get
            {
                return this.ValueField;
            }
        }
    }
}