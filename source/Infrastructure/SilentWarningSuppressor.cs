using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace DSCons.Revit.Starter.Infrastructure;

/// <summary>
/// Bộ lọc xử lý Warning tự động (Silent Warning Suppressor).
/// Kế thừa IFailuresPreprocessor trong Revit API.
/// 
/// NGUYÊN TẮC AN TOÀN TUYỆT ĐỐI:
/// - failuresAccessor.DeleteWarning(failure) chỉ xóa bản ghi thông báo cảnh báo (Failure Message)
///   khỏi hàng đợi của Revit.
/// - TUYỆT ĐỐI KHÔNG XÓA HAY LÀM THAY ĐỔI BẤT KỲ ĐỐI TƯỢNG (Element) NÀO TRONG MÔ HÌNH.
/// - Các lỗi nghiêm trọng (FailureSeverity.Error) được giữ nguyên để bảo vệ tính toàn vẹn của mô hình.
/// </summary>
public class SilentWarningSuppressor : IFailuresPreprocessor
{
    /// <summary>
    /// Số lượng cảnh báo đã được ẩn trong lần chạy gần nhất.
    /// </summary>
    public int SuppressedWarningCount { get; private set; }

    /// <summary>
    /// Danh sách chi tiết các cảnh báo đã được ẩn để lưu vết kiểm toán (Audit Log).
    /// </summary>
    public List<string> SuppressedLog { get; } = new List<string>();

    public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
    {
        IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();

        foreach (FailureMessageAccessor failure in failureMessages)
        {
            FailureSeverity severity = failure.GetSeverity();

            // Chỉ xử lý mức độ Warning (Cảnh báo không nghiêm trọng)
            if (severity == FailureSeverity.Warning)
            {
                var description = failure.GetDescriptionText();
                var failingIds = failure.GetFailingElementIds();
                
                SuppressedWarningCount++;
                var logEntry = $"[Suppressed Warning #{SuppressedWarningCount}] {description} (Elements liên quan: {failingIds.Count})";
                SuppressedLog.Add(logEntry);
                Log.Write(logEntry);

                // Xóa THÔNG BÁO cảnh báo (Message), giữ nguyên 100% Element
                failuresAccessor.DeleteWarning(failure);
            }
            // Nếu là Error, giữ nguyên để Revit xử lý hoặc rollback bảo vệ mô hình
        }

        return FailureProcessingResult.Continue;
    }
}
