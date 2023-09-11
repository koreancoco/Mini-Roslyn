namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public class TypeClauseSyntax : SyntaxNode
    {
        public TypeClauseSyntax(SyntaxToken colonToken, SyntaxToken identifier)
        {
            ColonToken = colonToken;
            Identifier = identifier;
        }

        public override SyntaxKind Kind => SyntaxKind.TypeClause;
        public SyntaxToken ColonToken { get; }
        public SyntaxToken Identifier { get; }
    }
}