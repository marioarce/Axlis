using Axlis.ORM.GraphQL;

namespace Axlis.ORM.GraphQL.Tests;

public sealed class GraphQLQueryBuilderTests
{
    [Fact]
    public void BuildBatchQuery_EmptyPaths_ReturnsEmptyQueryAndDict()
    {
        var (query, aliasToPath) = GraphQLQueryBuilder.BuildBatchQuery(Array.Empty<string>());

        Assert.Equal(string.Empty, query);
        Assert.Empty(aliasToPath);
    }

    [Fact]
    public void BuildBatchQuery_NullPaths_ReturnsEmptyQueryAndDict()
    {
        var (query, aliasToPath) = GraphQLQueryBuilder.BuildBatchQuery(null!);

        Assert.Equal(string.Empty, query);
        Assert.Empty(aliasToPath);
    }

    [Fact]
    public void BuildBatchQuery_SinglePath_ContainsItem0Alias()
    {
        var paths = new[] { "/sitecore/content/home" };
        var (query, aliasToPath) = GraphQLQueryBuilder.BuildBatchQuery(paths);

        Assert.Contains("item0:", query);
        Assert.Single(aliasToPath);
        Assert.Equal("/sitecore/content/home", aliasToPath["item0"]);
    }

    [Fact]
    public void BuildBatchQuery_MultiplePaths_ProducesCorrectAliases()
    {
        var paths = new[] { "/path/a", "/path/b", "/path/c" };
        var (query, aliasToPath) = GraphQLQueryBuilder.BuildBatchQuery(paths);

        Assert.Equal(3, aliasToPath.Count);
        Assert.Equal("/path/a", aliasToPath["item0"]);
        Assert.Equal("/path/b", aliasToPath["item1"]);
        Assert.Equal("/path/c", aliasToPath["item2"]);
    }

    [Fact]
    public void BuildBatchQuery_Query_ContainsGetItemsByPathsOperation()
    {
        var paths = new[] { "/sitecore/content" };
        var (query, _) = GraphQLQueryBuilder.BuildBatchQuery(paths);

        Assert.Contains("query GetItemsByPaths", query);
    }

    [Fact]
    public void BuildBatchQuery_Query_ContainsItemDataFragment()
    {
        var paths = new[] { "/sitecore/content" };
        var (query, _) = GraphQLQueryBuilder.BuildBatchQuery(paths);

        Assert.Contains("fragment ItemData", query);
    }

    [Fact]
    public void BuildBatchQuery_PathWithSpecialChars_EscapesDoubleQuotes()
    {
        var paths = new[] { "/path/with\"quote" };
        var (query, _) = GraphQLQueryBuilder.BuildBatchQuery(paths);

        Assert.Contains("\\\"", query);
        Assert.DoesNotContain("path: \"/path/with\"quote\"", query);
    }

    [Fact]
    public void BuildBatchQuery_PathWithBackslash_EscapesBackslash()
    {
        var paths = new[] { "/path/with\\back" };
        var (query, _) = GraphQLQueryBuilder.BuildBatchQuery(paths);

        Assert.Contains("\\\\", query);
    }

    [Fact]
    public void BuildBatchQuery_Query_EachPathAppearsInLanguageEnCall()
    {
        var paths = new[] { "/sitecore/content/home" };
        var (query, _) = GraphQLQueryBuilder.BuildBatchQuery(paths);

        Assert.Contains("language: \"en\"", query);
    }
}
