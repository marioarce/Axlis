using Axlis.ORM.Core;

namespace Axlis.ORM.Core.Tests.Domain;

public class ItemTests
{
    private static Item MakeItem(
        string id = "AAAA",
        string path = "/sitecore/content/home",
        string name = "Home",
        bool hasChildren = false,
        IItem? parent = null,
        List<Item>? children = null,
        int? childrenTotal = null)
        => new(id, path, name, null, 1, hasChildren, null, parent, null, children, childrenTotal);

    // ── IsFullyLoaded ─────────────────────────────────────────────────────────

    [Fact]
    public void IsFullyLoaded_LeafItemWithParentAndRootPath_ReturnsTrue()
    {
        var parent = MakeItem(id: "ROOT", path: "/sitecore");
        var item = MakeItem(parent: parent, hasChildren: false);
        Assert.True(item.IsFullyLoaded);
    }

    [Fact]
    public void IsFullyLoaded_MissingId_ReturnsFalse()
    {
        var parent = MakeItem();
        var item = new Item(string.Empty, "/sitecore/content/home", "Home", null, 1, false, null, parent, null, null);
        Assert.False(item.IsFullyLoaded);
    }

    [Fact]
    public void IsFullyLoaded_HasChildrenButNotLoaded_ReturnsFalse()
    {
        var parent = MakeItem();
        var item = MakeItem(hasChildren: true, parent: parent);
        Assert.False(item.IsFullyLoaded);
    }

    [Fact]
    public void IsFullyLoaded_SitecoreRootPath_DoesNotRequireParent()
    {
        var item = MakeItem(path: "/sitecore", hasChildren: false);
        Assert.True(item.IsFullyLoaded);
    }

    [Fact]
    public void IsFullyLoaded_NonRootPathWithNoParent_ReturnsFalse()
    {
        var item = MakeItem(path: "/sitecore/content/home", parent: null, hasChildren: false);
        Assert.False(item.IsFullyLoaded);
    }

    // ── AreChildrenLoaded ─────────────────────────────────────────────────────

    [Fact]
    public void AreChildrenLoaded_NullChildrenData_ReturnsFalse()
    {
        var item = MakeItem(hasChildren: true);
        Assert.False(item.AreChildrenLoaded);
    }

    [Fact]
    public void AreChildrenLoaded_LoadedChildren_ReturnsTrue()
    {
        var child = MakeItem(id: "CHILD");
        var item = MakeItem(hasChildren: true, children: new List<Item> { child });
        Assert.True(item.AreChildrenLoaded);
    }

    [Fact]
    public void AreChildrenLoaded_PartialLoad_ReturnsFalse()
    {
        var child = MakeItem(id: "CHILD");
        var item = MakeItem(hasChildren: true, children: new List<Item> { child }, childrenTotal: 5);
        Assert.False(item.AreChildrenLoaded);
    }

    // ── SetChildrenData ───────────────────────────────────────────────────────

    [Fact]
    public void SetChildrenData_NullInput_DoesNothing()
    {
        var item = MakeItem();
        item.SetChildrenData(null);
        Assert.Null(item.ChildrenData);
    }

    [Fact]
    public void SetChildrenData_ValidList_UpdatesChildrenData()
    {
        var item = MakeItem(hasChildren: true);
        var child = MakeItem(id: "C1");
        item.SetChildrenData(new List<IItem> { child }.AsReadOnly(), 1);
        Assert.True(item.AreChildrenLoaded);
    }
}
