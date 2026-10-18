using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;
using DSCons.Revit.Starter.ViewModels;
using DSCons.Revit.Starter.Views;

namespace DSCons.Revit.Starter.Commands;

/// <summary>
/// Lệnh mở Cửa sổ WPF Tạo Mặt Cắt MEP Làm Việc (Working Section Generator)
/// </summary>
[Transaction(TransactionMode.Manual)]
public class WorkingSectionCommand : IExternalCommand
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
                TaskDialog.Show("Mặt Cắt MEP", "Vui lòng mở một dự án Revit trước khi khởi chạy công cụ.");
                return Result.Cancelled;
            }

            // Kiểm tra View hiện hành phải là một mặt bằng (ViewPlan)
            if (!(uidoc.ActiveView is ViewPlan activePlanView))
            {
                TaskDialog.Show("Mặt Cắt MEP", "Vui lòng mở một View mặt bằng (Floor Plan) để công cụ xác định vị trí và chiều dài mặt cắt.");
                return Result.Cancelled;
            }

            var vm = new WorkingSectionViewModel(uidoc, activePlanView);
            var window = new WorkingSectionWindow(vm, uiapp.MainWindowHandle);
            window.ShowDialog();

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            Log.Write("WorkingSectionCommand Error: " + ex);
            message = "Lỗi khi khởi chạy Working Section Generator: " + ex.Message;
            return Result.Failed;
        }
    }
}