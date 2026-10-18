using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;
using DSCons.Revit.Starter.ViewModels;
using DSCons.Revit.Starter.Views;

namespace DSCons.Revit.Starter.Commands;

/// <summary>
/// Lệnh mở Cửa sổ WPF Tạo View 3D Theo Tầng (Floor 3D View Generator)
/// </summary>
[Transaction(TransactionMode.Manual)]
public class Floor3DViewCommand : IExternalCommand
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
                TaskDialog.Show("3D Tầng MEP", "Vui lòng mở một dự án Revit trước khi khởi chạy công cụ.");
                return Result.Cancelled;
            }

            var vm = new Floor3DViewViewModel(uidoc);
            var window = new Floor3DViewWindow(vm, uiapp.MainWindowHandle);
            window.ShowDialog();

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            Log.Write("Floor3DViewCommand Error: " + ex);
            message = "Lỗi khi khởi chạy Floor 3D View Generator: " + ex.Message;
            return Result.Failed;
        }
    }
}