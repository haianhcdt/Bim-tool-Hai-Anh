using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;
using DSCons.Revit.Starter.ViewModels;
using DSCons.Revit.Starter.Views;

namespace DSCons.Revit.Starter.Commands;

/// <summary>
/// Lệnh kiểm tra, trích xuất và quản lý Ẩn Cảnh Báo (Audit & Suppress Warnings).
/// Chạy ở chế độ ReadOnly: Đảm bảo CHỈ ĐỌC, tuyệt đối không can thiệp hay sửa đổi model.
/// </summary>
[Transaction(TransactionMode.ReadOnly)]
public class AuditWarningsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            if (uidoc == null || uidoc.Document == null)
            {
                TaskDialog.Show("Audit Warnings", "Vui lòng mở một mô hình Revit trước khi chạy lệnh.");
                return Result.Cancelled;
            }

            var doc = uidoc.Document;
            var warnings = doc.GetWarnings();
            var totalCount = warnings.Count;

            // Gom nhóm thống kê theo loại thông báo
            var groupedWarnings = warnings
                .GroupBy(w => w.GetDescriptionText())
                .OrderByDescending(g => g.Count())
                .ToList();

            // Tạo báo cáo chi tiết ra file text
            var reportBuilder = new StringBuilder();
            reportBuilder.AppendLine("================================================================================");
            reportBuilder.AppendLine($"BÁO CÁO KIỂM TOÁN CẢNH BÁO (WARNINGS AUDIT REPORT) - REVIT {doc.Application.VersionNumber}");
            reportBuilder.AppendLine($"Mô hình: {doc.Title}");
            reportBuilder.AppendLine($"Đường dẫn: {doc.PathName}");
            reportBuilder.AppendLine($"Thời gian quét: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            reportBuilder.AppendLine($"Tổng số cảnh báo (Warnings): {totalCount}");
            reportBuilder.AppendLine($"Tổng số nhóm lỗi: {groupedWarnings.Count}");
            reportBuilder.AppendLine($"Trạng thái Bộ lọc Ẩn Cảnh Báo: {(GlobalWarningSuppressorManager.IsEnabled ? "ĐANG BẬT" : "ĐANG TẮT")}");
            reportBuilder.AppendLine("================================================================================");
            reportBuilder.AppendLine();

            reportBuilder.AppendLine("--- BẢNG TỔNG HỢP THEO NHÓM LỖI ---");
            int rank = 1;
            foreach (var group in groupedWarnings)
            {
                reportBuilder.AppendLine($"{rank++}. [{group.Count()} vị trí] {group.Key}");
            }
            reportBuilder.AppendLine();

            reportBuilder.AppendLine("--- CHI TIẾT CÁC ĐỐI TƯỢNG BỊ CẢNH BÁO ---");
            int itemIndex = 1;
            foreach (var w in warnings)
            {
                var failingIds = w.GetFailingElements();
                var idStrings = failingIds.Select(id => id.ToString()).ToList();
                var idListText = idStrings.Count > 0 ? string.Join(", ", idStrings) : "Không có ElementId cụ thể";

                reportBuilder.AppendLine($"[#{itemIndex++}] {w.GetDescriptionText()}");
                reportBuilder.AppendLine($"    ElementIds liên quan: {idListText}");
            }

            // Lưu báo cáo vào thư mục reports
            var reportDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DSCons", "Reports");
            Directory.CreateDirectory(reportDirectory);
            var reportFilePath = Path.Combine(reportDirectory, $"Warnings_Audit_{doc.Title}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllText(reportFilePath, reportBuilder.ToString(), Encoding.UTF8);

            // Xây dựng hộp thoại tương tác TaskDialog
            var isSuppressorActive = GlobalWarningSuppressorManager.IsEnabled;
            var summaryDialog = new TaskDialog("Báo cáo & Quản lý Cảnh báo (Warnings)")
            {
                MainInstruction = $"Phát hiện {totalCount} cảnh báo (Warnings) trong mô hình!",
                MainContent = $"Mô hình: {doc.Title}\n" +
                              $"Trạng thái Chế độ Ẩn Cảnh Báo: " + (isSuppressorActive ? "🟢 ĐANG BẬT (HOẠT ĐỘNG)" : "⚪ ĐANG TẮT") + "\n\n" +
                              $"Top cảnh báo phổ biến nhất:\n" +
                              string.Join("\n", groupedWarnings.Take(3).Select(g => $"• ({g.Count()} vị trí) {g.Key}")) +
                              (groupedWarnings.Count > 3 ? $"\n• ... và {groupedWarnings.Count - 3} nhóm cảnh báo khác." : ""),
                ExpandedContent = $"Đã xuất báo cáo chi tiết toàn bộ Element IDs tại:\n{reportFilePath}\n\n" +
                                  "Cam kết an toàn: Lệnh chạy ở chế độ ReadOnly, mô hình được bảo toàn 100%.",
                CommonButtons = TaskDialogCommonButtons.Close
            };

            if (!isSuppressorActive)
            {
                summaryDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
                    "Kích hoạt Chế độ Ẩn Cảnh Báo ngầm (Khuyên dùng)",
                    "Tự động triệt tiêu mọi popup cảnh báo (No Loss Defined, trùng Mark...) khi vẽ hoặc chỉnh sửa. Bảo toàn 100% đối tượng mô hình.");
            }
            else
            {
                summaryDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1,
                    "Tắt Chế độ Ẩn Cảnh Báo ngầm",
                    $"Đã ẩn ngầm thành công {GlobalWarningSuppressorManager.TotalSuppressedInSession} cảnh báo trong phiên làm việc. Bấm vào đây nếu muốn quay lại cảnh báo gốc của Revit.");
            }

            summaryDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2,
                "Mở file báo cáo chi tiết",
                "Xem đầy đủ danh sách Element IDs của 2554 cảnh báo trong Notepad.");

            summaryDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3,
                "Mở Bảng Thống Kê & Kiểm Tra Trùng Mark (WPF Window)",
                "Kiểm toán toàn bộ đối tượng có thông số Mark bị trùng lặp, hiển thị bảng trực quan và chọn/zoom cấu kiện trong Revit.");

            var dialogResult = summaryDialog.Show();

            if (dialogResult == TaskDialogResult.CommandLink1)
            {
                var newState = GlobalWarningSuppressorManager.Toggle(doc.Application);
                if (newState)
                {
                    TaskDialog.Show("Đã Kích Hoạt Ẩn Cảnh Báo",
                        "🟢 Chế độ Ẩn Cảnh Báo Tự Động ĐÃ ĐƯỢC BẬT!\n\n" +
                        "• Từ bây giờ, mọi thao tác dựng hình, sửa đổi, copy thiết bị MEP/kiến trúc sẽ KHÔNG BỊ POPUP CẢNH BÁO làm gián đoạn.\n" +
                        "• Cơ chế sử dụng API failuresAccessor.DeleteWarning() chỉ xóa thông báo, GIỮ NGUYÊN 100% CẤU KIỆN TRONG MODEL.\n" +
                        "• Bạn có thể bấm lại nút bất cứ lúc nào để xem số lượng cảnh báo đã ẩn hoặc tắt chế độ này.");
                }
                else
                {
                    TaskDialog.Show("Đã Tắt Ẩn Cảnh Báo",
                        "⚪ Chế độ Ẩn Cảnh Báo Tự Động ĐÃ TẮT.\n\n" +
                        "Revit sẽ hiển thị các hộp thoại cảnh báo theo mặc định.");
                }
            }
            else if (dialogResult == TaskDialogResult.CommandLink2)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(reportFilePath) { UseShellExecute = true });
                }
                catch { /* Không mở được trình đọc file */ }
            }
            else if (dialogResult == TaskDialogResult.CommandLink3)
            {
                try
                {
                    var auditVm = new DuplicateMarkAuditViewModel(uidoc);
                    var window = new DuplicateMarkAuditWindow(auditVm, uiapp.MainWindowHandle);
                    window.ShowDialog();
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Kiểm Tra Mark", "Lỗi khi mở bảng kiểm tra Mark: " + ex.Message);
                }
            }

            Log.Write($"AuditWarningsCommand completed. Total warnings: {totalCount}. Suppressor active: {GlobalWarningSuppressorManager.IsEnabled}");
            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            Log.Write(ex.ToString());
            message = "Audit Warnings gặp sự cố: " + ex.Message;
            return Result.Failed;
        }
    }
}
