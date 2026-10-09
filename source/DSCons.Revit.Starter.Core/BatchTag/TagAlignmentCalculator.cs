using System;
using System.Collections.Generic;
using System.Linq;

namespace DSCons.Revit.Starter.Core.BatchTag;

public enum TagAlignmentDirection
{
    Horizontal, // Căn thẳng hàng ngang (Cùng tọa độ Y)
    Vertical    // Căn thẳng hàng dọc (Cùng tọa độ X)
}

public enum TagAlignmentReference
{
    FirstSelected, // Theo vị trí Tag đầu tiên chọn
    Average        // Theo vị trí trung bình của các Tag
}

public sealed class TagPositionItem
{
    public long ElementId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public double CurrentX { get; set; }
    public double CurrentY { get; set; }
    public double CurrentZ { get; set; }
    public double NewX { get; set; }
    public double NewY { get; set; }
    public double NewZ { get; set; }

    public double Delta => Math.Sqrt(Math.Pow(NewX - CurrentX, 2) + Math.Pow(NewY - CurrentY, 2));

    public string CurrentPositionText => $"X: {CurrentX:F2}, Y: {CurrentY:F2}";
    public string NewPositionText => $"X: {NewX:F2}, Y: {NewY:F2}";
    public string DeltaText => $"{Delta:F3}";
}

public static class TagAlignmentCalculator
{
    public static List<TagPositionItem> CalculateAlignment(
        IEnumerable<TagPositionItem> items,
        TagAlignmentDirection direction,
        TagAlignmentReference reference,
        double minSpacing = 0.0)
    {
        if (items == null) return new List<TagPositionItem>();
        var list = items.ToList();
        if (list.Count < 2) return list;

        if (direction == TagAlignmentDirection.Horizontal)
        {
            double targetY = reference == TagAlignmentReference.FirstSelected
                ? list[0].CurrentY
                : list.Average(i => i.CurrentY);

            foreach (var item in list)
            {
                item.NewX = item.CurrentX;
                item.NewY = targetY;
                item.NewZ = item.CurrentZ;
            }

            if (minSpacing > 0.0)
            {
                var sorted = list.OrderBy(i => i.CurrentX).ToList();
                for (int i = 1; i < sorted.Count; i++)
                {
                    double minAllowedX = sorted[i - 1].NewX + minSpacing;
                    if (sorted[i].NewX < minAllowedX)
                    {
                        sorted[i].NewX = minAllowedX;
                    }
                }
            }
        }
        else // Vertical
        {
            double targetX = reference == TagAlignmentReference.FirstSelected
                ? list[0].CurrentX
                : list.Average(i => i.CurrentX);

            foreach (var item in list)
            {
                item.NewX = targetX;
                item.NewY = item.CurrentY;
                item.NewZ = item.CurrentZ;
            }

            if (minSpacing > 0.0)
            {
                var sorted = list.OrderBy(i => i.CurrentY).ToList();
                for (int i = 1; i < sorted.Count; i++)
                {
                    double minAllowedY = sorted[i - 1].NewY + minSpacing;
                    if (sorted[i].NewY < minAllowedY)
                    {
                        sorted[i].NewY = minAllowedY;
                    }
                }
            }
        }

        return list;
    }
}
