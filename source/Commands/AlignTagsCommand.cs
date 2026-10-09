using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Core.BatchTag;
using DSCons.Revit.Starter.Infrastructure;
using DSCons.Revit.Starter.ViewModels;
using DSCons.Revit.Starter.Views;

namespace DSCons.Revit.Starter.Commands;

[Transaction(TransactionMode.Manual)]
public class AlignTagsCommand : IExternalCommand
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

            var activeView = doc.ActiveView;
            if (activeView == null || activeView.IsTemplate)
            {
                message = "View hiện hành không hợp lệ hoặc là View Template.";
                return Result.Failed;
            }

            if (activeView.ViewType == ViewType.ThreeD)
            {
                var view3d = activeView as View3D;
                if (view3d != null && !view3d.IsLocked)
                {
                    TaskDialog.Show("BIM TOOL - Align Tags",
                        "View 3D chưa được khóa hướng nhìn (Orientation Locked).\nVui lòng khóa View 3D hoặc chuyển sang Mặt bằng / Mặt cắt để thao tác nhãn.");
                    return Result.Cancelled;
                }
            }

            // 1. Lấy danh sách phần tử đang được chọn
            var selectedIds = uiDoc.Selection.GetElementIds();
            var tags = new List<IndependentTag>();

            foreach (var id in selectedIds)
            {
                var elem = doc.GetElement(id);
                if (elem is IndependentTag tag)
                {
                    tags.Add(tag);
                }
            }

            // 2. Kiểm tra Guard Clause (phải có từ 2 tag trở lên)
            if (tags.Count < 2)
            {
                TaskDialog.Show("BIM TOOL - Align Tags",
                    "Vui lòng chọn từ 2 thẻ ghi chú (Tag) trở lên trên màn hình trước khi bấm căn chỉnh.\n\n" +
                    "Mẹo: Giữ phím Ctrl và nhấp chuột chọn các nhãn Tag cần gióng thẳng hàng.");
                return Result.Cancelled;
            }

            // 3. Chuẩn bị danh sách vị trí phục vụ Xem trước (Preview)
            var tagMap = new Dictionary<long, IndependentTag>();
            var items = new List<TagPositionItem>();

            foreach (var tag in tags)
            {
                var longId = tag.Id.AsLong();
                tagMap[longId] = tag;
                var pos = tag.TagHeadPosition;
                var name = tag.Name;

                items.Add(new TagPositionItem
                {
                    ElementId = longId,
                    TagName = string.IsNullOrWhiteSpace(name) ? "Tag" : name,
                    CurrentX = pos.X,
                    CurrentY = pos.Y,
                    CurrentZ = pos.Z
                });
            }

            // 4. Mở cửa sổ WPF Xem trước
            var viewModel = new AlignTagsViewModel(items);
            var window = new AlignTagsWindow(viewModel, commandData.Application.MainWindowHandle);

            var dialogResult = window.ShowDialog();

            // Nếu người dùng bấm Hủy hoặc đóng cửa sổ
            if (viewModel.DialogResult != true)
            {
                return Result.Cancelled;
            }

            // 5. Ghi thay đổi vào model trong DUY NHẤT 1 Transaction
            using (var tx = new Transaction(doc, "BIM TOOL - Align Tags"))
            {
                var status = tx.Start();
                if (status != TransactionStatus.Started)
                {
                    message = "Không thể bắt đầu Transaction trong Revit.";
                    return Result.Failed;
                }

                int updatedCount = 0;
                foreach (var item in viewModel.PreviewItems)
                {
                    if (tagMap.TryGetValue(item.ElementId, out var tag))
                    {
                        tag.TagHeadPosition = new XYZ(item.NewX, item.NewY, item.NewZ);
                        updatedCount++;
                    }
                }

                tx.Commit();

                string alignType = viewModel.IsHorizontal ? "Phương ngang (cùng Y)" : "Phương dọc (cùng X)";
                TaskDialog.Show("BIM TOOL - Align Tags",
                    $"Đã căn chỉnh thẳng hàng thành công cho {updatedCount} thẻ ghi chú!\n" +
                    $"- Chế độ: {alignType}\n" +
                    "- Bạn có thể bấm Ctrl+Z để hoàn tác vị trí vừa chỉnh.");

                Log.Write($"AlignTags: Successfully aligned {updatedCount} tags ({alignType}).");
                return Result.Succeeded;
            }
        }
        catch (Exception ex)
        {
            Log.Write($"AlignTags error: {ex}");
            message = $"Lỗi khi căn chỉnh tag: {ex.Message}";
            return Result.Failed;
        }
    }
}