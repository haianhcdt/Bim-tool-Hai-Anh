namespace DSCons.Revit.Starter.Core.BatchTag;

public sealed class BatchTagCandidate
{
    public long ElementId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public double MidX { get; set; }
    public double MidY { get; set; }
    public double MidZ { get; set; }

    public static BatchTagCandidate FromCurveEndpoints(long elementId, string categoryName, double startX, double startY, double startZ, double endX, double endY, double endZ)
    {
        return new BatchTagCandidate
        {
            ElementId = elementId,
            CategoryName = categoryName,
            MidX = (startX + endX) / 2.0,
            MidY = (startY + endY) / 2.0,
            MidZ = (startZ + endZ) / 2.0
        };
    }

    public static BatchTagCandidate FromPoint(long elementId, string categoryName, double x, double y, double z)
    {
        return new BatchTagCandidate
        {
            ElementId = elementId,
            CategoryName = categoryName,
            MidX = x,
            MidY = y,
            MidZ = z
        };
    }
}