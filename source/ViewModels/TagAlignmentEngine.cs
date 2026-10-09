using System;
using System.Collections.Generic;
using System.Linq;
using DSCons.Revit.Starter.Core.BatchTag;

namespace DSCons.Revit.Starter.ViewModels;

internal static class TagAlignmentEngine
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
