using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;
using DSCons.Revit.Starter.ViewModels;
using DSCons.Revit.Starter.Views;

namespace DSCons.Revit.Starter.Commands;

/// <summary>
/// Lệnh mở Cửa sổ WPF Tạo Mặt Bằng MEP Tự Động (Batch Floor Plan Generator)
/// </summary>
[Transaction(TransactionMode.Manual)]
public class BatchFloorPlanCommand : IExternalCommand
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
                TaskDialog.Show("Tạo Mặt Bằng MEP", "Vui lòng mở một dự án Revit trước khi khởi chạy công cụ.");
                return Result.Cancelled;
            }

            var vm = new BatchFloorPlanViewModel(uidoc);
            var window = new BatchFloorPlanWindow(vm, uiapp.MainWindowHandle);
            window.ShowDialog();

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            Log.Write("BatchFloorPlanCommand Error: " + ex);
            message = "Lỗi khi khởi chạy Batch Floor Plan Generator: " + ex.Message;
            return Result.Failed;
        }
    }
}
