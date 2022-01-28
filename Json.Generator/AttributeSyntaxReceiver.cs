using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Json.Generator;

public class AttributeSyntaxReceiver<TAttribute> : ISyntaxReceiver where TAttribute : Attribute
{
    public List<ClassDeclarationSyntax> Classes { get; } = new();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        bool IsCurrentAttributeSet(AttributeListSyntax attributeList) =>
            attributeList.Attributes.Any(attribute =>
                attribute.Name.EnsureEndsWith("Attribute") == typeof(TAttribute).Name);

        if (syntaxNode is ClassDeclarationSyntax { AttributeLists: { Count: > 0 } } classDeclarationSyntax &&
            classDeclarationSyntax.AttributeLists.Any(IsCurrentAttributeSet))
        {
            Classes.Add(classDeclarationSyntax);
        }
    }
}