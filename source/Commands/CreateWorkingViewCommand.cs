using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Core.Views;
using DSCons.Revit.Starter.Infrastructure;

namespace DSCons.Revit.Starter.Commands;

[Transaction(TransactionMode.Manual)]
public class CreateWorkingViewCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            if (uiDoc == null || uiDoc.Document == null)
            {
                message = "Không có mô hình Revit nào đang mở.";
                return Result.Cancelled;
            }

            var doc = uiDoc.Document;
            if (doc.IsReadOnly)
            {
                message = "Mô hình đang ở chế độ chỉ đọc (Read-only).";
                return Result.Failed;
            }

            string targetViewName = WorkingViewNaming.BuildWorkingViewName("HaiAnh");

            // 1. Kiểm tra chống tạo trùng lặp (Idempotent)
            var existingView = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(v => !v.IsTemplate && string.Equals(v.Name, targetViewName, StringComparison.OrdinalIgnoreCase));

            if (existingView != null)
            {
                uiDoc.ActiveView = existingView;
                TaskDialog.Show("BIM TOOL - Working View",
                    $"[Hot Reload ⚡] View 3D làm việc '{targetViewName}' đã có sẵn trong dự án!\nĐã tự động chuyển góc nhìn sang View này (Cập nhật không cần tắt Revit).");
                Log.Write($"Switched to existing working view: {targetViewName}");
                return Result.Succeeded;
            }

            // 2. Tìm kiểu View 3D (ViewFamilyType)
            var viewFamilyType = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(x => x.ViewFamily == ViewFamily.ThreeDimensional);

            if (viewFamilyType == null)
            {
                message = "Không tìm thấy kiểu View 3D (ViewFamilyType.ThreeDimensional) trong dự án.";
                return Result.Failed;
            }

            // 3. Tạo View 3D trong Transaction an toàn
            using (var transaction = new Transaction(doc, "Tạo 3D View làm việc"))
            {
                var status = transaction.Start();
                if (status != TransactionStatus.Started)
                {
                    message = "Không thể bắt đầu Transaction trong Revit.";
                    return Result.Failed;
                }

                try
                {
                    var newView = View3D.CreateIsometric(doc, viewFamilyType.Id);
                    newView.Name = targetViewName;

                    // Cấu hình hiển thị chuẩn
                    newView.DetailLevel = ViewDetailLevel.Fine;
                    newView.DisplayStyle = DisplayStyle.ShadingWithEdges;

                    // Gán thông tin phân loại nếu có
                    var commentsParam = newView.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION);
                    if (commentsParam != null && !commentsParam.IsReadOnly)
                    {
                        commentsParam.Set("Working View - Nguyễn Tôn Hải Anh");
                    }

                    transaction.Commit();

                    // Kích hoạt hiển thị View mới
                    uiDoc.ActiveView = newView;

                    TaskDialog.Show("BIM TOOL - Working View",
                        $"Đã tạo thành công View 3D làm việc: '{targetViewName}'!\n" +
                        "- Mức chi tiết: Fine\n" +
                        "- Kiểu hiển thị: Shaded with Edges\n" +
                        "- Đã mở trực tiếp trên màn hình.");

                    Log.Write($"Created new working view: {targetViewName}");
                    return Result.Succeeded;
                }
                catch (Exception ex)
                {
                    if (transaction.HasStarted() && !transaction.HasEnded())
                    {
                        transaction.RollBack();
                    }
                    Log.Write(ex.ToString());
                    message = $"Lỗi khi tạo View 3D làm việc: {ex.Message}";
                    return Result.Failed;
                }
            }
        }
        catch (Exception exception)
        {
            Log.Write(exception.ToString());
            message = $"Lỗi không mong muốn trong lệnh Working View: {exception.Message}";
            return Result.Failed;
        }
    }
}

