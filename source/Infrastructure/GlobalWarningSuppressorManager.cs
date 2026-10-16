using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;

namespace DSCons.Revit.Starter.Infrastructure;

/// <summary>
/// Trình quản lý Ẩn Cảnh Báo Toàn Cục (Global Warning Suppressor Manager).
/// Đón đầu sự kiện Application.FailuresProcessing của Revit để triệt tiêu popup cảnh báo
/// khi người dùng vẽ, chỉnh sửa hoặc copy cấu kiện trong mô hình.
/// 
/// NGUYÊN TẮC BẢO TOÀN DỮ LIỆU:
/// 1. Chỉ gọi accessor.DeleteWarning(f) trên các thông báo ở mức độ Warning.
/// 2. TUYỆT ĐỐI KHÔNG can thiệp, xóa hay sửa bất kỳ Element nào trong mô hình.
/// 3. Giữ nguyên 100% các thông báo Error để bảo vệ tính toàn vẹn của mô hình.
/// </summary>
public static class GlobalWarningSuppressorManager
{
    private static bool _isEnabled;
    private static Application? _registeredApp;
    private static int _totalSuppressedInSession;

    public static bool IsEnabled => _isEnabled;
    public static int TotalSuppressedInSession => _totalSuppressedInSession;

    public static bool Toggle(Application app)
    {
        if (_isEnabled)
        {
            if (_registeredApp != null)
            {
                _registeredApp.FailuresProcessing -= OnFailuresProcessing;
                _registeredApp = null;
            }
            _isEnabled = false;
            Log.Write("GlobalWarningSuppressor deactivated by user.");
            return false;
        }
        else
        {
            _registeredApp = app;
            _registeredApp.FailuresProcessing += OnFailuresProcessing;
            _isEnabled = true;
            Log.Write("GlobalWarningSuppressor activated by user.");
            return true;
        }
    }

    private static void OnFailuresProcessing(object sender, FailuresProcessingEventArgs e)
    {
        var accessor = e.GetFailuresAccessor();
        if (accessor == null) return;

        IList<FailureMessageAccessor> failureMessages = accessor.GetFailureMessages();
        int suppressedCount = 0;

        foreach (FailureMessageAccessor failure in failureMessages)
        {
            if (failure.GetSeverity() == FailureSeverity.Warning)
            {
                // Xóa bản ghi thông báo (Failure Message) ra khỏi hàng đợi
                // HOÀN TOÀN KHÔNG XÓA ELEMENT TRONG MÔ HÌNH
                accessor.DeleteWarning(failure);
                suppressedCount++;
                _totalSuppressedInSession++;
            }
        }

        if (suppressedCount > 0)
        {
            // Báo cho Revit tiếp tục giao dịch mà không hiển thị popup
            e.SetProcessingResult(FailureProcessingResult.Continue);
            Log.Write($"[Global Suppressor] Đã ẩn ngầm {suppressedCount} cảnh báo. Mô hình giữ nguyên 100%.");
        }
    }
}
