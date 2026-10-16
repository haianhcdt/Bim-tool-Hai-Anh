using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;
using DSCons.Revit.Starter.ViewModels;
using DSCons.Revit.Starter.Views;

namespace DSCons.Revit.Starter.Commands;

/// <summary>
/// Lệnh mở Cửa sổ WPF Bộ Dọn Dẹp Mô Hình (Model Cleanup & Optimization Suite)
/// </summary>
[Transaction(TransactionMode.Manual)]
public class ModelCleanupCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc?.Document;

            if (uidoc == null || doc == null)
            {
                TaskDialog.Show("Model Cleanup Suite", "Vui lòng mở một dự án Revit trước khi khởi chạy công cụ.");
                return Result.Cancelled;
            }

            var vm = new ModelCleanupViewModel(uidoc);
            var window = new ModelCleanupWindow(vm, uiapp.MainWindowHandle);
            window.ShowDialog();

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            Log.Write("ModelCleanupCommand Error: " + ex);
            message = "Lỗi khi khởi chạy Model Cleanup Suite: " + ex.Message;
            return Result.Failed;
        }
    }
}
