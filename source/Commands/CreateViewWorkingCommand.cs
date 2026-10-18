using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;
using DSCons.Revit.Starter.ViewModels;
using DSCons.Revit.Starter.Views;

namespace DSCons.Revit.Starter.Commands;

/// <summary>
/// Lệnh mở Cửa sổ WPF Tạo View Làm Việc (Create View Working)
/// Gộp 3 chức năng: Mặt Bằng MEP, Mặt Cắt MEP và 3D Tầng MEP.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class CreateViewWorkingCommand : IExternalCommand
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
                TaskDialog.Show("Create View Working", "Vui lòng mở một dự án Revit trước khi khởi chạy công cụ.");
                return Result.Cancelled;
            }

            var vm = new CreateViewWorkingViewModel(uidoc);
            var window = new CreateViewWorkingWindow(vm, uiapp.MainWindowHandle);
            window.ShowDialog();

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            Log.Write("CreateViewWorkingCommand Error: " + ex);
            message = "Lỗi khi khởi chạy Create View Working: " + ex.Message;
            return Result.Failed;
        }
    }
}
