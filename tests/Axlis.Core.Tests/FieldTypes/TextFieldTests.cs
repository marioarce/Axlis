using Axlis.Core;
using Axlis.Core.FieldTypes;

namespace Axlis.Core.Tests.FieldTypes;

public class TextFieldTests
{
    private static ItemTemplateField Field(string? value)
        => new("id", "MyField", value, null, "TextField");

    [Fact]
    public void Value_ReturnsRawValue()
    {
        var field = new TextField(Field("Hello World"));
        Assert.Equal("Hello World", field.Value);
    }

    [Fact]
    public void Value_NullRawValue_ReturnsNull()
    {
        var field = new TextField(Field(null));
        Assert.Null(field.Value);
    }

    [Fact]
    public void IsEmpty_EmptyString_ReturnsTrue()
    {
        var field = new TextField(Field(string.Empty));
        Assert.True(field.IsEmpty);
    }

    [Fact]
    public void IsEmpty_NonEmptyValue_ReturnsFalse()
    {
        var field = new TextField(Field("value"));
        Assert.False(field.IsEmpty);
    }

    [Fact]
    public void FieldName_ReturnsInnerFieldName()
    {
        var field = new TextField(Field("v"));
        Assert.Equal("MyField", field.FieldName);
    }
}
