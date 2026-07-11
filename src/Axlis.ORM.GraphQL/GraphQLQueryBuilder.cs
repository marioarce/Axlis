using System.Text;
using Axlis.ORM.GraphQL.Queries;

namespace Axlis.ORM.GraphQL;

/// <summary>
/// Builds batch GraphQL queries that fetch multiple Sitecore items in a single HTTP request.
/// Uses GraphQL field aliases (<c>item0</c>, <c>item1</c>, …) so the server returns
/// all results in one <c>data</c> envelope.
/// </summary>
public static class GraphQLQueryBuilder
{
    /// <summary>
    /// Builds a batch GraphQL query to fetch multiple items by their paths or IDs.
    /// </summary>
    /// <param name="paths">Collection of paths or IDs to fetch.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    ///   <item><description><c>Query</c> — the complete GraphQL query string, or <see cref="string.Empty"/> when <paramref name="paths"/> is empty.</description></item>
    ///   <item><description><c>AliasToPath</c> — dictionary mapping each alias (e.g. <c>item0</c>) back to its original path.</description></item>
    /// </list>
    /// </returns>
    public static (string Query, Dictionary<string, string> AliasToPath) BuildBatchQuery(IEnumerable<string> paths)
    {
        var pathList = paths?.ToList() ?? new List<string>();

        if (pathList.Count == 0)
        {
            return (string.Empty, new Dictionary<string, string>());
        }

        var aliasToPath = new Dictionary<string, string>(pathList.Count, StringComparer.OrdinalIgnoreCase);
        var queryBuilder = new StringBuilder();

        queryBuilder.AppendLine("query GetItemsByPaths {");

        for (var i = 0; i < pathList.Count; i++)
        {
            var alias = $"item{i}";
            var path = pathList[i];
            aliasToPath[alias] = path;

            var escapedPath = EscapeGraphQLString(path);
            queryBuilder.AppendLine($"  {alias}: item(path: \"{escapedPath}\", language: \"en\") {{");
            queryBuilder.AppendLine("    ...ItemData");
            queryBuilder.AppendLine("  }");
        }

        queryBuilder.AppendLine("}");
        queryBuilder.AppendLine();
        queryBuilder.Append(GetItemDataFragments());

        return (queryBuilder.ToString(), aliasToPath);
    }

    /// <summary>
    /// Escapes characters that are not allowed inside a GraphQL string literal.
    /// </summary>
    private static string EscapeGraphQLString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    /// <summary>
    /// Extracts the reusable fragment definitions from <see cref="GetItemByPathQuery.Query"/>
    /// so the batch query can share the same field selection sets.
    /// </summary>
    private static string GetItemDataFragments()
    {
        var fullQuery = GetItemByPathQuery.Query;
        var fragmentStart = fullQuery.IndexOf("fragment ItemData", StringComparison.Ordinal);
        return fragmentStart >= 0 ? fullQuery[fragmentStart..] : string.Empty;
    }
}
