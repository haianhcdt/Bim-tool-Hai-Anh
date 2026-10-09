using DSCons.Revit.Starter.Core.Views;
using Xunit;

namespace DSCons.Revit.Starter.Core.Tests;

public sealed class WorkingViewNamingTests
{
    [Fact]
    public void BuildWorkingViewName_Default_ReturnsHaiAnh()
    {
        var name = WorkingViewNaming.BuildWorkingViewName();
        Assert.Equal("3D_Working_HaiAnh", name);
    }

    [Fact]
    public void BuildWorkingViewName_CustomName_ReturnsFormatted()
    {
        var name = WorkingViewNaming.BuildWorkingViewName("Hai Anh");
        Assert.Equal("3D_Working_Hai_Anh", name);
    }

    [Fact]
    public void BuildWorkingViewName_Whitespace_FallsBackToDefault()
    {
        var name = WorkingViewNaming.BuildWorkingViewName("   ");
        Assert.Equal("3D_Working_HaiAnh", name);
    }
}
