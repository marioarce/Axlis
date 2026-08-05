using Axlis.Customizations.Abstractions;

namespace Axlis.Customizations.Abstractions.Tests;

public class AxlisControlConstantsTests
{
    [Fact]
    public void ControlPrefix_IsAxlis()
    {
        Assert.Equal("Axlis", AxlisControlConstants.ControlPrefix);
    }

    [Fact]
    public void QueryPrefix_MatchesSitecoreQuerySourceConvention()
    {
        Assert.Equal("query:", AxlisControlConstants.QueryPrefix);
    }
}
