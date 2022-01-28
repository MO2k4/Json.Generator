using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Json.Generator;

[Generator]
public class CodeGenerator : ISourceGenerator
{
    private const string ClassNameIdentifier = "{{ClassName}}";
    private const string NamespaceIdentifier = "{{Namespace}}";
    private const string PropertyIdentifier = "{{Property}}";

    private List<string> Log { get; } = new();

    public void Initialize(GeneratorInitializationContext context) =>
        context.RegisterForSyntaxNotifications(() => new AttributeSyntaxReceiver<JsonFieldExistsAttribute>());

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not AttributeSyntaxReceiver<JsonFieldExistsAttribute> syntaxReceiver) return;

        var template = GetEmbeddedResource("Json.Generator.Templates.JsonFieldSetCheck.txt");
        var propertyTemplate = GetEmbeddedResource("Json.Generator.Templates.Property.txt");
        foreach (var @class in syntaxReceiver.Classes)
        {
            var model = context.Compilation.GetSemanticModel(@class.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(@class);
            if (symbol == null) continue;

            var sourceCode = GetSourceCodeFor(symbol, template, propertyTemplate);
            Log.Add($"Build model for class: {symbol.Name}");
            Log.Add(sourceCode);

            context.AddSource($"{symbol.Name}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        }

        context.AddSource("Logs",
            SourceText.From($@"/*{Environment.NewLine + string.Join(Environment.NewLine, Log) + Environment.NewLine}*/",
                Encoding.UTF8));
    }

    private static string GetSourceCodeFor(INamespaceOrTypeSymbol symbol, string template, string propertyTemplate) =>
        template
            .Replace(ClassNameIdentifier, symbol.Name)
            .Replace(NamespaceIdentifier, GetNamespaceRecursively(symbol.ContainingNamespace))
            .Replace(PropertyIdentifier, GetPropertyPlaceholderContent(symbol, propertyTemplate));

    private static string GetPropertyPlaceholderContent(INamespaceOrTypeSymbol symbol, string propertyTemplate) =>
        GetProperties(symbol)
            .Select(property => propertyTemplate
                .Replace("{{CamelPropertyName}}", property.Name.ToLowerCase())
                .Replace("{{PropertyName}}", property.Name)
                .Replace("{{Type}}", property.Type.Name))
            .Aggregate((current, next) => current + Environment.NewLine + next);

    private string GetEmbeddedResource(string path)
    {
        using var stream = GetType().Assembly.GetManifestResourceStream(path);
        using StreamReader streamReader = new(stream);

        return streamReader.ReadToEnd();
    }

    private static string GetNamespaceRecursively(ISymbol symbol) =>
        symbol.ContainingNamespace == null
            ? symbol.Name
            : (GetNamespaceRecursively(symbol.ContainingNamespace) + "." + symbol.Name).Trim('.');

    private static IEnumerable<IPropertySymbol> GetProperties(INamespaceOrTypeSymbol symbol) =>
        symbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.GetMethod != null && p.SetMethod != null && !p.Parameters.Any());
}