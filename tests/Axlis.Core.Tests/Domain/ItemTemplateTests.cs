using Axlis.Core;

namespace Axlis.Core.Tests.Domain;

public class ItemTemplateTests
{
    [Fact]
    public void Constructor_NormalizesBracesOnId()
    {
        var t = new ItemTemplate("6D1CD897-1936-4A3A-A511-289A94C2A7B1", "Test");
        Assert.Equal("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}", t.TemplateId);
    }

    [Fact]
    public void Constructor_PreservesExistingBraces()
    {
        var t = new ItemTemplate("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}", "Test");
        Assert.Equal("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}", t.TemplateId);
    }

    [Fact]
    public void TemplateName_ReturnsSuppliedName()
    {
        var t = new ItemTemplate("{ABC}", "Dictionary Entry");
        Assert.Equal("Dictionary Entry", t.TemplateName);
    }

    [Fact]
    public void Fields_ReturnsNull()
    {
        var t = new ItemTemplate("{ABC}", "Test");
        Assert.Null(t.Fields);
    }

    [Theory]
    [InlineData("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}", true)]
    [InlineData("6D1CD897-1936-4A3A-A511-289A94C2A7B1", true)]
    [InlineData("{6d1cd897-1936-4a3a-a511-289a94c2a7b1}", true)]
    [InlineData("{DIFFERENT-GUID-0000-0000-000000000000}", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void Equals_PerformsGuidNormalizedComparison(string? other, bool expected)
    {
        var t = new ItemTemplate("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}", "Test");
        Assert.Equal(expected, t.Equals(other));
    }

    [Fact]
    public void NormalizeGuid_StripsBracesAndLowercases()
    {
        var result = ItemTemplate.NormalizeGuid("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}");
        Assert.Equal("6d1cd897-1936-4a3a-a511-289a94c2a7b1", result);
    }
}
