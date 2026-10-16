using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;

namespace DSCons.Revit.Starter.Commands;

[Transaction(TransactionMode.ReadOnly)]
public class ModelSizeDiagnosticCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var uiapp = commandData.Application;
            var doc = uiapp.ActiveUIDocument?.Document;
            if (doc == null)
            {
                TaskDialog.Show("Diagnostic", "Không có mô hình nào đang mở.");
                return Result.Cancelled;
            }

            double fileSizeMb = 0;
            if (!string.IsNullOrEmpty(doc.PathName) && File.Exists(doc.PathName))
            {
                fileSizeMb = Math.Round(new FileInfo(doc.PathName).Length / (1024.0 * 1024.0), 2);
            }

            // 1. Kiểm tra CAD Imports & Links
            var importInstances = new FilteredElementCollector(doc)
                .OfClass(typeof(ImportInstance))
                .Cast<ImportInstance>()
                .ToList();

            var directCadImports = new List<string>();
            var linkedCadImports = new List<string>();

            foreach (var inst in importInstances)
            {
                var type = doc.GetElement(inst.GetTypeId()) as ElementType;
                var typeName = type?.Name ?? "Unnamed CAD";
                var ownerView = doc.GetElement(inst.OwnerViewId) as View;
                var viewName = ownerView?.Name ?? "Model Space (All Views)";

                if (inst.IsLinked)
                {
                    linkedCadImports.Add($"[LINK] {typeName} (Tại View: {viewName})");
                }
                else
                {
                    directCadImports.Add($"[IMPORT TRỰC TIẾP] {typeName} (Tại View: {viewName})");
                }
            }

            // 2. Kiểm tra Images / Raster / PDF
            var imageTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(ImageType))
                .Cast<ImageType>()
                .Select(img => img.Name)
                .ToList();

            // 3. Thống kê số lượng phần tử theo Category lớn nhất
            var elementCountsByCategory = new Dictionary<string, int>();
            var allElements = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .ToList();

            foreach (var el in allElements)
            {
                if (el.Category != null)
                {
                    var catName = el.Category.Name;
                    if (elementCountsByCategory.ContainsKey(catName))
                        elementCountsByCategory[catName]++;
                    else
                        elementCountsByCategory[catName] = 1;
                }
            }

            var topCategories = elementCountsByCategory
                .OrderByDescending(kv => kv.Value)
                .Take(7)
                .ToList();

            // 4. Đếm số lượng Families, Views, Sheets
            var familyCount = new FilteredElementCollector(doc).OfClass(typeof(Family)).GetElementCount();
            var viewCount = new FilteredElementCollector(doc).OfClass(typeof(View)).Cast<View>().Count(v => !v.IsTemplate && !(v is ViewSheet));
            var sheetCount = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).GetElementCount();

            // 5. Tạo báo cáo chi tiết
            var sb = new StringBuilder();
            sb.AppendLine("=== CHẨN ĐOÁN DUNG LƯỢNG MÔ HÌNH (FILE SIZE DIAGNOSTIC) ===");
            sb.AppendLine($"Tệp: {doc.Title}");
            sb.AppendLine($"Đường dẫn: {doc.PathName}");
            sb.AppendLine($"Dung lượng trên đĩa: {fileSizeMb} MB");
            sb.AppendLine($"Tổng số cấu kiện hình học (3D elements): {allElements.Count:N0}");
            sb.AppendLine($"Tổng số Family trong model: {familyCount}");
            sb.AppendLine($"Số lượng View: {viewCount} | Số lượng Sheet: {sheetCount}");
            sb.AppendLine();

            sb.AppendLine($"--- 1. DANH SÁCH BẢN VẼ CAD (Tổng: {importInstances.Count}) ---");
            sb.AppendLine($"• CAD Import trực tiếp (Nguy cơ gây nặng file lớn nhất): {directCadImports.Count} tệp");
            foreach (var cad in directCadImports) sb.AppendLine($"   - {cad}");
            sb.AppendLine($"• CAD Dạng Link: {linkedCadImports.Count} tệp");
            foreach (var cad in linkedCadImports) sb.AppendLine($"   - {cad}");
            sb.AppendLine();

            sb.AppendLine($"--- 2. HÌNH ẢNH RASTER / PDF (Tổng: {imageTypes.Count}) ---");
            foreach (var img in imageTypes) sb.AppendLine($"   - {img}");
            sb.AppendLine();

            sb.AppendLine("--- 3. TOP CÁC HẠNG MỤC CHIẾM NHIỀU ĐỐI TƯỢNG NHẤT ---");
            foreach (var cat in topCategories)
            {
                sb.AppendLine($"• {cat.Key}: {cat.Value:N0} đối tượng");
            }

            var reportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DSCons", "Reports", $"Size_Diagnostic_{doc.Title}.txt");
            File.WriteAllText(reportPath, sb.ToString(), Encoding.UTF8);

            // Hiển thị TaskDialog
            var td = new TaskDialog("Chẩn đoán Dung lượng Mô hình")
            {
                MainInstruction = $"Dung lượng hiện tại: {fileSizeMb} MB",
                MainContent = $"Mô hình: {doc.Title}\n" +
                              $"Tổng số đối tượng hình học: {allElements.Count:N0}\n" +
                              $"CAD Import trực tiếp: {directCadImports.Count} bản vẽ (Cần chú ý!)\n" +
                              $"Ảnh raster / PDF nhúng: {imageTypes.Count}\n" +
                              $"Tổng số Families: {familyCount}\n\n" +
                              $"Top đối tượng trong model:\n" +
                              string.Join("\n", topCategories.Take(4).Select(c => $"• {c.Key}: {c.Value:N0}")),
                ExpandedContent = $"Đã ghi chi tiết toàn bộ tên file CAD và vị trí View tại:\n{reportPath}",
                CommonButtons = TaskDialogCommonButtons.Close
            };

            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Mở báo cáo chẩn đoán chi tiết", "Xem danh sách từng file CAD/Ảnh đang nằm trong view nào.");
            var res = td.Show();
            if (res == TaskDialogResult.CommandLink1)
            {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(reportPath) { UseShellExecute = true }); } catch { }
            }

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            Log.Write(ex.ToString());
            message = ex.Message;
            return Result.Failed;
        }
    }
}
