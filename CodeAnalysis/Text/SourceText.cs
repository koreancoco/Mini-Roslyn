using System;

namespace CodeAnalysis
{
    public abstract class SourceText
    {
        /// <summary>
        /// The length of the text in characters.
        /// </summary>
        public abstract int Length { get; }
        
        /// <summary>
        /// Copy a range of characters from this SourceText to a destination array.
        /// </summary>
        public abstract void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count);

        public static SourceText From(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            return new StringText(text);
        }
    }
}