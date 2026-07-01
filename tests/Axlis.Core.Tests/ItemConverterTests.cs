using Axlis.Core;
using Axlis.Core.GraphQL;

namespace Axlis.Core.Tests;

public class ItemConverterTests
{
    private static GraphQLItemData SimpleData(string id = "ITEM1", string name = "Home", string path = "/sitecore/content/home")
        => new() { Id = id, Name = name, Path = path, Version = 1, HasChildren = false };

    // ── Basic conversion ──────────────────────────────────────────────────────

    [Fact]
    public void ToItem_NullInput_ReturnsNull()
    {
        Assert.Null(ItemConverter.ToItem(null));
    }

    [Fact]
    public void ToItem_BasicData_MapsIdNamePath()
    {
        var data = SimpleData(id: "ABC", name: "About", path: "/sitecore/content/about");
        var item = ItemConverter.ToItem(data);

        Assert.NotNull(item);
        Assert.Equal("ABC", item.Id);
        Assert.Equal("About", item.Name);
        Assert.Equal("/sitecore/content/about", item.Path);
        Assert.Equal(1, item.Version);
    }

    [Fact]
    public void ToItem_WithTemplate_MapsTemplateId()
    {
        var data = SimpleData();
        data.Template = new GraphQLTemplateData { Id = "6D1CD897-1936-4A3A-A511-289A94C2A7B1", Name = "Page" };

        var item = ItemConverter.ToItem(data)!;

        Assert.NotNull(item.Template);
        Assert.Equal("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}", item.Template!.TemplateId);
        Assert.Equal("Page", item.Template.TemplateName);
    }

    [Fact]
    public void ToItem_WithParent_MapsParentId()
    {
        var data = SimpleData();
        data.Parent = SimpleData(id: "PARENT", name: "Root", path: "/sitecore");

        var item = ItemConverter.ToItem(data)!;

        Assert.NotNull(item.Parent);
        Assert.Equal("PARENT", item.Parent!.Id);
    }

    [Fact]
    public void ToItem_WithChildren_MapsChildrenCollection()
    {
        var data = SimpleData(id: "ROOT");
        data.HasChildren = true;
        data.Children = new GraphQLChildrenData
        {
            Total = 2,
            Results = new List<GraphQLItemData>
            {
                SimpleData(id: "CHILD1", name: "Child 1", path: "/sitecore/content/child1"),
                SimpleData(id: "CHILD2", name: "Child 2", path: "/sitecore/content/child2")
            }
        };

        var item = ItemConverter.ToItem(data)!;

        Assert.NotNull(item.Children);
        Assert.Equal(2, item.Children!.Count);
        Assert.Equal("CHILD1", item.Children[0].Id);
        Assert.Equal("CHILD2", item.Children[1].Id);
    }

    // ── Field conversion ──────────────────────────────────────────────────────

    [Fact]
    public void ToItem_WithTextField_MapsFieldValue()
    {
        var data = SimpleData();
        data.Fields = new List<GraphQLFieldData>
        {
            new() { Id = "F1", Name = "Heading", Value = "Welcome", Typename = "TextField" }
        };

        var item = ItemConverter.ToItem(data)!;

        Assert.NotNull(item.Fields);
        var field = item.Fields!.First();
        Assert.Equal("Heading", field.FieldName);
        Assert.Equal("Welcome", field.FieldValue);
        Assert.Equal("TextField", field.FieldType);
    }

    [Fact]
    public void ToItem_CircularReference_DoesNotThrowOrInfiniteLoop()
    {
        var data = SimpleData(id: "CIRC");
        data.Parent = SimpleData(id: "CIRC");

        // First occurrence of CIRC (as parent) is converted; no infinite loop occurs
        var item = ItemConverter.ToItem(data);
        Assert.NotNull(item);
        // The root CIRC item is returned; parent had same ID but was the first converted
        Assert.Equal("CIRC", item!.Id);
    }

    // ── Ancestor conversion ───────────────────────────────────────────────────

    [Fact]
    public void ToAncestorItems_NullInput_ReturnsNull()
    {
        Assert.Null(ItemConverter.ToAncestorItems(null));
    }

    [Fact]
    public void ToAncestorItems_EmptyList_ReturnsNull()
    {
        Assert.Null(ItemConverter.ToAncestorItems(new List<GraphQLItemData>()));
    }

    [Fact]
    public void ToAncestorItems_ValidList_MapsAllItems()
    {
        var ancestors = new List<GraphQLItemData>
        {
            SimpleData(id: "A1", name: "Level1"),
            SimpleData(id: "A2", name: "Level2")
        };

        var result = ItemConverter.ToAncestorItems(ancestors)!;

        Assert.Equal(2, result.Count);
        Assert.Equal("A1", result[0].Id);
        Assert.Equal("A2", result[1].Id);
    }
}
