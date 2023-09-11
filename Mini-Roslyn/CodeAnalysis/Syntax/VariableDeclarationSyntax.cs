using MiniRoslyn.CodeAnalysis.Visitor;

namespace MiniRoslyn.CodeAnalysis.Syntax
{
    public class VariableDeclarationSyntax : MemberSyntax
    {
        public VariableDeclarationSyntax(SyntaxToken declarator, SyntaxToken identifier, TypeClauseSyntax typeClause, SyntaxToken equalsToken, ExpressionSyntax initializer)
        {
            Declarator = declarator;
            Identifier = identifier;
            TypeClause = typeClause;
            EqualsToken = equalsToken;
            Initializer = initializer;
        }

        public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
        public SyntaxToken Declarator { get; }
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax TypeClause { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Initializer { get; }
        public override TResult Accept<TResult>(IStatementVisitor<TResult> visitor)
        {
            return visitor.VisitVariableDeclaration(this);
        }
    }
}