using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;
using DSCons.Revit.Starter.ViewModels;
using DSCons.Revit.Starter.Views;

namespace DSCons.Revit.Starter.Commands;

[Transaction(TransactionMode.Manual)]
public class WorkingViewsCommand : IExternalCommand
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

            var viewModel = new WorkingViewsViewModel(uiDoc);
            IntPtr mainWindowHandle = commandData.Application.MainWindowHandle;

            var window = new WorkingViewsWindow(viewModel, mainWindowHandle);
            window.ShowDialog();

            if (viewModel.PendingActiveView != null)
            {
                try
                {
                    uiDoc.ActiveView = viewModel.PendingActiveView;
                }
                catch (Exception ex)
                {
                    Log.Write("Không thể chuyển góc nhìn sang View mới: " + ex.Message);
                }
            }

            return Result.Succeeded;
        }
        catch (Exception exception)
        {
            Log.Write(exception.ToString());
            message = $"Lỗi không mong muốn trong lệnh Working Views: {exception.Message}";
            return Result.Failed;
        }
    }
}