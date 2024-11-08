using Newtonsoft.Json.Serialization;
using JasperFx.Core;
using Marten;

namespace marten_docs_pii;

internal static class CasingExtensionMethods
{
    private static readonly SnakeCaseNamingStrategy SnakeCaseNamingStrategy = new();

    private static string ToSnakeCase(this string s)
    {
        return SnakeCaseNamingStrategy.GetPropertyName(s, false);
    }

    public static string FormatCase(this string s, Casing casing) =>
        casing switch
        {
            Casing.CamelCase => s.ToCamelCase(),
            Casing.SnakeCase => s.ToSnakeCase(),
            _ => s
        };
}