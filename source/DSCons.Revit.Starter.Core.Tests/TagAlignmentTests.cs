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

    [Fact]
    public void CalculateAlignment_Horizontal_WithCollisionSpacing_SeparatesOverlappingTags()
    {
        // Ca lỗi cũ: 2 tag gần trùng tọa độ X (10.0 và 10.5) khi căn ngang
        var items = new List<TagPositionItem>
        {
            new() { ElementId = 1, TagName = "Duct Tag 1", CurrentX = 10.0, CurrentY = 20.0, CurrentZ = 0 },
            new() { ElementId = 2, TagName = "Duct Tag 2", CurrentX = 10.5, CurrentY = 35.0, CurrentZ = 0 }
        };

        var result = TagAlignmentCalculator.CalculateAlignment(
            items, TagAlignmentDirection.Horizontal, TagAlignmentReference.FirstSelected, minSpacing: 3.0);

        Assert.Equal(2, result.Count);
        // Cả 2 tag thẳng hàng ngang theo Y của tag đầu tiên (20.0)
        Assert.Equal(20.0, result[0].NewY);
        Assert.Equal(20.0, result[1].NewY);

        // Tag 1 giữ nguyên vị trí X ban đầu (10.0)
        Assert.Equal(10.0, result[0].NewX);
        // Tag 2 được tự động tách khoảng cách an toàn >= 3.0 (10.0 + 3.0 = 13.0) để không trùng đè
        Assert.Equal(13.0, result[1].NewX);
        Assert.True(result[1].NewX - result[0].NewX >= 3.0);
    }

    [Fact]
    public void CalculateAlignment_Horizontal_NormalNonCollision_PreservesNaturalSpacing()
    {
        // Ca bình thường: 2 tag có khoảng cách X đủ rộng (10.0 và 25.0)
        var items = new List<TagPositionItem>
        {
            new() { ElementId = 1, TagName = "Duct Tag 1", CurrentX = 10.0, CurrentY = 20.0, CurrentZ = 0 },
            new() { ElementId = 2, TagName = "Duct Tag 2", CurrentX = 25.0, CurrentY = 35.0, CurrentZ = 0 }
        };

        var result = TagAlignmentCalculator.CalculateAlignment(
            items, TagAlignmentDirection.Horizontal, TagAlignmentReference.FirstSelected, minSpacing: 3.0);

        Assert.Equal(2, result.Count);
        // Cả 2 tag thẳng hàng ngang Y = 20.0
        Assert.Equal(20.0, result[0].NewY);
        Assert.Equal(20.0, result[1].NewY);

        // Khoảng cách tự nhiên 15.0 >= 3.0 nên cả 2 giữ nguyên tọa độ X
        Assert.Equal(10.0, result[0].NewX);
        Assert.Equal(25.0, result[1].NewX);
    }

    [Fact]
    public void CalculateAlignment_Vertical_WithCollisionSpacing_SeparatesOverlappingTags()
    {
        // Ca kiểm thử phương dọc: 2 tag gần trùng tọa độ Y (5.0 và 6.0) khi căn dọc
        var items = new List<TagPositionItem>
        {
            new() { ElementId = 1, TagName = "Duct Tag 1", CurrentX = 15.0, CurrentY = 5.0, CurrentZ = 0 },
            new() { ElementId = 2, TagName = "Duct Tag 2", CurrentX = 30.0, CurrentY = 6.0, CurrentZ = 0 }
        };

        var result = TagAlignmentCalculator.CalculateAlignment(
            items, TagAlignmentDirection.Vertical, TagAlignmentReference.FirstSelected, minSpacing: 2.5);

        Assert.Equal(2, result.Count);
        // Cả 2 tag thẳng hàng dọc X = 15.0
        Assert.Equal(15.0, result[0].NewX);
        Assert.Equal(15.0, result[1].NewX);

        // Tag 1 giữ nguyên Y = 5.0, Tag 2 được tách thành 5.0 + 2.5 = 7.5
        Assert.Equal(5.0, result[0].NewY);
        Assert.Equal(7.5, result[1].NewY);
        Assert.True(result[1].NewY - result[0].NewY >= 2.5);
    }
}
