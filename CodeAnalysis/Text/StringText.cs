namespace CodeAnalysis
{
    public class StringText : SourceText
    {
        private readonly string _source;

        public override int Length
        {
            get
            {
                return _source.Length;
            }
        }

        public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            _source.CopyTo(sourceIndex, destination, destinationIndex, count);
        }

        public StringText(string source)
        {
            _source = source;
        }
    }
}