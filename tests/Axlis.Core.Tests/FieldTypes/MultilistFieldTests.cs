using Axlis.Core;
using Axlis.Core.FieldTypes;

namespace Axlis.Core.Tests.FieldTypes;

public class MultilistFieldTests
{
    private static ItemTemplateField Field(
        IReadOnlyList<string?>? ids = null,
        IReadOnlyList<Item>? items = null)
        => new("id", "Tags", null, null, "MultilistField",
               targetIds: ids, targetItems: items);

    [Fact]
    public void Ids_FiltersNulls()
    {
        var field = new MultilistField(Field(ids: new List<string?> { "A", null, "B" }));
        Assert.Equal(new[] { "A", "B" }, field.Ids);
    }

    [Fact]
    public void Ids_EmptyWhenNoTargetIds()
    {
        var field = new MultilistField(Field());
        Assert.Empty(field.Ids);
    }

    [Fact]
    public void IsEmpty_NoIds_ReturnsTrue()
    {
        var field = new MultilistField(Field());
        Assert.True(field.IsEmpty);
    }

    [Fact]
    public void IsEmpty_WithIds_ReturnsFalse()
    {
        var field = new MultilistField(Field(ids: new List<string?> { "A" }));
        Assert.False(field.IsEmpty);
    }

    [Fact]
    public void As_EmptyItems_ReturnsEmpty()
    {
        var field = new MultilistField(Field());
        var result = field.As<ExtendedItem>();
        Assert.Empty(result);
    }

    [Fact]
    public void As_PopulatesInnerItemOnEachInstance()
    {
        var child = new Item("C1", "/sitecore/content/c1", "C1", null, 1, false, null, null, null, null);
        var field = new MultilistField(Field(items: new List<Item> { child }));

        var result = field.As<ExtendedItem>();

        Assert.Single(result);
        Assert.Equal("C1", result[0].Id);
    }
}
