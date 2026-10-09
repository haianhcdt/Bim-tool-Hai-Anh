using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Views;

namespace DSCons.Revit.Starter.Commands;

[Transaction(TransactionMode.Manual)]
public class AboutMeCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var window = new AboutMeWindow(commandData.Application.MainWindowHandle);
        window.ShowDialog();
        return Result.Succeeded;
    }
}