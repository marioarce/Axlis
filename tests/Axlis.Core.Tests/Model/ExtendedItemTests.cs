using Axlis.Core;

namespace Axlis.Core.Tests.Model;

public class ExtendedItemTests
{
    private static Item MakeItem(string id = "AAAA", string name = "Home", int version = 1)
        => new(id, "/sitecore/content/home", name, null, version, false, null, null, null, null);

    [Fact]
    public void GetCacheKeyValue_ContainsTypeName_IdNormalized_Name_Version()
    {
        var item = MakeItem(id: "{6D1CD897-1936-4A3A-A511-289A94C2A7B1}", name: "Home", version: 2);
        var extended = new ExtendedItem(item);

        var key = extended.GetCacheKeyValue();

        Assert.Contains("ExtendedItem", key);
        Assert.Contains("6d1cd897-1936-4a3a-a511-289a94c2a7b1", key);
        Assert.Contains("home", key);
        Assert.Contains("v2", key);
    }

    [Fact]
    public void GetCacheKeyValue_NullId_UsesNullPlaceholder()
    {
        // Item with empty id -> NormalizeGuid returns ""
        var item = new Item(string.Empty, "/path", "Test", null, 1, false, null, null, null, null);
        var extended = new ExtendedItem(item);
        var key = extended.GetCacheKeyValue();
        Assert.Contains("null", key);
    }

    [Fact]
    public void SetInnerItem_UpdatesId()
    {
        var extended = new ExtendedItem();
        var item = MakeItem(id: "BBBB", name: "About");
        extended.SetInnerItem(item);

        Assert.Equal("BBBB", extended.Id);
        Assert.Equal("About", extended.Name);
    }

    [Fact]
    public void Initialize_And_Reset_ChangeLoader()
    {
        ExtendedItem.Reset();

        var extended = new ExtendedItem();
        Assert.Null(extended.RawInnerItem);

        ExtendedItem.Reset();
    }

    [Fact]
    public void ExtendedItemIdComparer_TreatsEqualGuidsAsEqual()
    {
        var a = new ExtendedItem(MakeItem("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}"));
        var b = new ExtendedItem(MakeItem("{6d1cd897-1936-4a3a-a511-289a94c2a7b1}"));

        Assert.True(ExtendedItemIdComparer.Instance.Equals(a, b));
        Assert.Equal(ExtendedItemIdComparer.Instance.GetHashCode(a),
                     ExtendedItemIdComparer.Instance.GetHashCode(b));
    }

    [Fact]
    public void ExtendedItemIdComparer_TreatsDifferentGuidsAsNotEqual()
    {
        var a = new ExtendedItem(MakeItem("AAAA"));
        var b = new ExtendedItem(MakeItem("BBBB"));

        Assert.False(ExtendedItemIdComparer.Instance.Equals(a, b));
    }
}
