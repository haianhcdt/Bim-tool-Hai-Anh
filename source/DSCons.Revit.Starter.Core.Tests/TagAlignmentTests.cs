using System;
using System.Collections.Generic;
using DSCons.Revit.Starter.Core.BatchTag;
using Xunit;

namespace DSCons.Revit.Starter.Core.Tests;

public sealed class TagAlignmentTests
{
    [Fact]
    public void CalculateAlignment_Horizontal_FirstSelected_SetsSameYAsFirstItem()
    {
        var items = new List<TagPositionItem>
        {
            new() { ElementId = 1, TagName = "Tag 1", CurrentX = 10, CurrentY = 20, CurrentZ = 0 },
            new() { ElementId = 2, TagName = "Tag 2", CurrentX = 15, CurrentY = 25, CurrentZ = 0 },
            new() { ElementId = 3, TagName = "Tag 3", CurrentX = 20, CurrentY = 18, CurrentZ = 0 }
        };

        var result = TagAlignmentCalculator.CalculateAlignment(
            items, TagAlignmentDirection.Horizontal, TagAlignmentReference.FirstSelected);

        Assert.Equal(3, result.Count);
        // All Y should be equal to first item's Y (20)
        Assert.Equal(20, result[0].NewY);
        Assert.Equal(20, result[1].NewY);
        Assert.Equal(20, result[2].NewY);
        // X coordinates should remain unchanged
        Assert.Equal(10, result[0].NewX);
        Assert.Equal(15, result[1].NewX);
        Assert.Equal(20, result[2].NewX);
    }

    [Fact]
    public void CalculateAlignment_Horizontal_Average_SetsAverageY()
    {
        var items = new List<TagPositionItem>
        {
            new() { ElementId = 1, TagName = "Tag 1", CurrentX = 10, CurrentY = 10, CurrentZ = 0 },
            new() { ElementId = 2, TagName = "Tag 2", CurrentX = 15, CurrentY = 20, CurrentZ = 0 },
            new() { ElementId = 3, TagName = "Tag 3", CurrentX = 20, CurrentY = 30, CurrentZ = 0 }
        };

        var result = TagAlignmentCalculator.CalculateAlignment(
            items, TagAlignmentDirection.Horizontal, TagAlignmentReference.Average);

        // Average Y = (10 + 20 + 30) / 3 = 20
        Assert.Equal(20, result[0].NewY);
        Assert.Equal(20, result[1].NewY);
        Assert.Equal(20, result[2].NewY);
    }

    [Fact]
    public void CalculateAlignment_Vertical_FirstSelected_SetsSameXAsFirstItem()
    {
        var items = new List<TagPositionItem>
        {
            new() { ElementId = 1, TagName = "Tag 1", CurrentX = 12, CurrentY = 5, CurrentZ = 0 },
            new() { ElementId = 2, TagName = "Tag 2", CurrentX = 16, CurrentY = 10, CurrentZ = 0 }
        };

        var result = TagAlignmentCalculator.CalculateAlignment(
            items, TagAlignmentDirection.Vertical, TagAlignmentReference.FirstSelected);

        Assert.Equal(12, result[0].NewX);
        Assert.Equal(12, result[1].NewX);
        // Y coordinates should remain unchanged
        Assert.Equal(5, result[0].NewY);
        Assert.Equal(10, result[1].NewY);
    }

    [Fact]
    public void CalculateAlignment_Vertical_Average_SetsAverageX()
    {
        var items = new List<TagPositionItem>
        {
            new() { ElementId = 1, TagName = "Tag 1", CurrentX = 10, CurrentY = 5, CurrentZ = 0 },
            new() { ElementId = 2, TagName = "Tag 2", CurrentX = 20, CurrentY = 15, CurrentZ = 0 }
        };

        var result = TagAlignmentCalculator.CalculateAlignment(
            items, TagAlignmentDirection.Vertical, TagAlignmentReference.Average);

        // Average X = 15
        Assert.Equal(15, result[0].NewX);
        Assert.Equal(15, result[1].NewX);
    }

    [Fact]
    public void CalculateAlignment_LessThanTwoItems_ReturnsUnchanged()
    {
        var items = new List<TagPositionItem>
        {
            new() { ElementId = 1, TagName = "Tag 1", CurrentX = 10, CurrentY = 20, CurrentZ = 0 }
        };

        var result = TagAlignmentCalculator.CalculateAlignment(
            items, TagAlignmentDirection.Horizontal, TagAlignmentReference.FirstSelected);

        Assert.Single(result);
    }

    [Fact]
    public void CalculateAlignment_Delta_CalculatesCorrectShiftDistance()
    {
        var items = new List<TagPositionItem>
        {
            new() { ElementId = 1, TagName = "Tag 1", CurrentX = 0, CurrentY = 0, CurrentZ = 0 },
            new() { ElementId = 2, TagName = "Tag 2", CurrentX = 0, CurrentY = 3, CurrentZ = 0 }
        };

        var result = TagAlignmentCalculator.CalculateAlignment(
            items, TagAlignmentDirection.Horizontal, TagAlignmentReference.FirstSelected);

        // First item doesn't move: delta = 0
        Assert.Equal(0, result[0].Delta);
        // Second item moved from Y=3 to Y=0: delta = 3
        Assert.Equal(3, result[1].Delta);
    }
}