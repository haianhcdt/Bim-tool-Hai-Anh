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

    [Fact]
    public void BuildWorkingPlanName_DefaultOwner_ReturnsFormatted()
    {
        var name = WorkingViewNaming.BuildWorkingPlanName("Level 1");
        Assert.Equal("FL_Working_HaiAnh_Level_1", name);
    }

    [Fact]
    public void BuildWorkingPlanName_CustomOwnerAndLevel_ReturnsFormatted()
    {
        var name = WorkingViewNaming.BuildWorkingPlanName("Tang 2", "Nguyen Hai Anh");
        Assert.Equal("FL_Working_Nguyen_Hai_Anh_Tang_2", name);
    }

    [Fact]
    public void BuildWorkingPlanName_EmptyLevel_FallsBack()
    {
        var name = WorkingViewNaming.BuildWorkingPlanName("  ", "HaiAnh");
        Assert.Equal("FL_Working_HaiAnh_Level", name);
    }

    [Fact]
    public void BuildWorkingSectionName_Default_ReturnsFormatted()
    {
        var name = WorkingViewNaming.BuildWorkingSectionName("01");
        Assert.Equal("SEC_Working_HaiAnh_01", name);
    }
}