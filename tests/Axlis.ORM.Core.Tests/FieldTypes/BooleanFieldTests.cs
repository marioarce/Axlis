using Axlis.ORM.Core;
using Axlis.ORM.Core.FieldTypes;

namespace Axlis.ORM.Core.Tests.FieldTypes;

public class BooleanFieldTests
{
    private static ItemTemplateField Field(string? value, bool? boolValue = null)
        => new("id", "IsActive", value, null, "CheckboxField", boolValue: boolValue);

    [Fact]
    public void Value_UsesExplicitBoolValue_WhenProvided()
    {
        var field = new BooleanField(Field("0", boolValue: true));
        Assert.True(field.Value);
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Value_FallsBackToRawStringComparison(string? raw, bool expected)
    {
        var field = new BooleanField(Field(raw));
        Assert.Equal(expected, field.Value);
    }

    [Fact]
    public void IsEmpty_AlwaysFalse()
    {
        Assert.False(new BooleanField(Field(null)).IsEmpty);
        Assert.False(new BooleanField(Field("1")).IsEmpty);
    }
}
