using System.Collections.Generic;
using DSCons.Revit.Starter.Core.BatchTag;
using Xunit;

namespace DSCons.Revit.Starter.Core.Tests;

public sealed class BatchTagFilterTests
{
    [Fact]
    public void FilterUntagged_AllUntagged_ReturnsAll()
    {
        var candidates = new List<BatchTagCandidate>
        {
            new() { ElementId = 101, CategoryName = "Ducts" },
            new() { ElementId = 102, CategoryName = "Ducts" }
        };
        var tagged = new HashSet<long>();

        var result = BatchTagFilter.FilterUntagged(candidates, tagged);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void FilterUntagged_SomeAlreadyTagged_ReturnsOnlyUntagged()
    {
        var candidates = new List<BatchTagCandidate>
        {
            new() { ElementId = 101, CategoryName = "Ducts" },
            new() { ElementId = 102, CategoryName = "Ducts" },
            new() { ElementId = 103, CategoryName = "Ducts" }
        };
        var tagged = new HashSet<long> { 102 };

        var result = BatchTagFilter.FilterUntagged(candidates, tagged);

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, c => c.ElementId == 102);
    }

    [Fact]
    public void FilterUntagged_AllAlreadyTagged_ReturnsEmpty()
    {
        var candidates = new List<BatchTagCandidate>
        {
            new() { ElementId = 101, CategoryName = "Ducts" },
            new() { ElementId = 102, CategoryName = "Ducts" }
        };
        var tagged = new HashSet<long> { 101, 102 };

        var result = BatchTagFilter.FilterUntagged(candidates, tagged);

        Assert.Empty(result);
    }

    [Fact]
    public void FromCurveEndpoints_ComputesCorrectMidpoint()
    {
        var candidate = BatchTagCandidate.FromCurveEndpoints(200, "Ducts", 0, 10, 20, 100, 50, 60);

        Assert.Equal(200, candidate.ElementId);
        Assert.Equal(50, candidate.MidX);
        Assert.Equal(30, candidate.MidY);
        Assert.Equal(40, candidate.MidZ);
    }

    [Fact]
    public void BuildSummaryMessage_AllAlreadyTagged_ReportsNoDuplicatesCreated()
    {
        var res = new BatchTagResult
        {
            TotalFound = 5,
            AlreadyTagged = 5,
            NewlyTagged = 0
        };

        var message = res.BuildSummaryMessage("Ống gió");

        Assert.Contains("Tất cả 5 đối tượng Ống gió trong View đều đã có tag từ trước", message);
        Assert.Contains("Không tạo thêm tag để tránh đè nét", message);
    }

    [Fact]
    public void BuildSummaryMessage_NewlyTagged_ReportsSuccess()
    {
        var res = new BatchTagResult
        {
            TotalFound = 5,
            AlreadyTagged = 2,
            NewlyTagged = 3
        };

        var message = res.BuildSummaryMessage("Ống gió");

        Assert.Contains("Đã gắn tag thành công cho 3 đối tượng Ống gió!", message);
        Assert.Contains("Bỏ qua 2 đối tượng đã có sẵn tag", message);
    }
}