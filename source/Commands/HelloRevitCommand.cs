using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSCons.Revit.Starter.Infrastructure;

namespace DSCons.Revit.Starter.Commands;

[Transaction(TransactionMode.Manual)]
public class HelloRevitCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var documentTitle = commandData.Application.ActiveUIDocument?.Document.Title ?? "khA'ng cA3 model mY";
            TaskDialog.Show("DSCons Revit API Kit", $"Xin chAo! Kit `A chy thAnh cA'ng trAn: {documentTitle}");
            Log.Write("Hello Revit executed successfully.");
            return Result.Succeeded;
        }
        catch (System.Exception exception)
        {
            Log.Write(exception.ToString());
            message = "Hello Revit failed. Check the DSCons log file and copy the exact error to your AI agent.";
            return Result.Failed;
        }
    }
}