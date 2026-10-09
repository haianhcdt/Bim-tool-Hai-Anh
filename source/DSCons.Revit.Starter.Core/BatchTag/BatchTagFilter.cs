using System;
using System.Collections.Generic;
using System.Linq;

namespace DSCons.Revit.Starter.Core.BatchTag;

public static class BatchTagFilter
{
    /// <summary>
    /// Lọc ra các phần tử chưa có tag trong View hiện hành để ngăn ngừa tạo trùng (Idempotency).
    /// </summary>
    public static IReadOnlyList<BatchTagCandidate> FilterUntagged(
        IEnumerable<BatchTagCandidate> candidates,
        ISet<long> alreadyTaggedElementIds)
    {
        if (candidates == null) return Array.Empty<BatchTagCandidate>();
        var taggedSet = alreadyTaggedElementIds ?? new HashSet<long>();

        return candidates
            .Where(c => c != null && !taggedSet.Contains(c.ElementId))
            .ToList();
    }
}