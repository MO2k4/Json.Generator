using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Json.Generator;

public static class StringExtensions
{
    public static string EnsureEndsWith(this NameSyntax source, string suffix) =>
        source.ToString().EnsureEndsWith(suffix);

    private static string EnsureEndsWith(this string source, string suffix)
    {
        if (source.EndsWith(suffix)) return source;
        return source + suffix;
    }

    public static string ToLowerCase(this string argument) => $"{char.ToLower(argument[0])}{argument.Substring(1)}";
}