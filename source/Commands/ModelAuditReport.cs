using System;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;

namespace DSCons.Revit.Starter.Commands;

public class ModelAuditReport
{
    public string FileName { get; set; } = string.Empty;
    public double FileSizeMb { get; set; }
    public int DirectCadImportCount { get; set; }
    public int LinkedCadCount { get; set; }
    public int ImageTypeCount { get; set; }
    public int ViewCount { get; set; }
    public int SheetCount { get; set; }
    public int FamilyCount { get; set; }
    public int WarningCount { get; set; }

    public static ModelAuditReport Analyze(Document doc)
    {
        var report = new ModelAuditReport
        {
            FileName = doc.Title,
            WarningCount = doc.GetWarnings().Count
        };

        try
        {
            if (!string.IsNullOrWhiteSpace(doc.PathName) && File.Exists(doc.PathName))
            {
                var fi = new FileInfo(doc.PathName);
                report.FileSizeMb = Math.Round(fi.Length / (1024.0 * 1024.0), 2);
            }
        }
        catch { }

        // Kiểm tra CAD Imports (Trực tiếp vs Link)
        var cadInstances = new FilteredElementCollector(doc)
            .OfClass(typeof(ImportInstance))
            .Cast<ImportInstance>()
            .ToList();

        report.DirectCadImportCount = cadInstances.Count(x => !x.IsLinked);
        report.LinkedCadCount = cadInstances.Count(x => x.IsLinked);

        // Kiểm tra Image Types (Ảnh raster, logo, scan)
        report.ImageTypeCount = new FilteredElementCollector(doc)
            .OfClass(typeof(ImageType))
            .GetElementCount();

        // Kiểm tra Views & Sheets
        var views = new FilteredElementCollector(doc)
            .OfClass(typeof(View))
            .Cast<View>()
            .Where(v => !v.IsTemplate)
            .ToList();

        report.ViewCount = views.Count(v => !(v is ViewSheet));
        report.SheetCount = views.Count(v => v is ViewSheet);

        // Kiểm tra Families
        report.FamilyCount = new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .GetElementCount();

        return report;
    }
}
