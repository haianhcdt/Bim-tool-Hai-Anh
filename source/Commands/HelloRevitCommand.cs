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
            return new BatchFloorPlanCommand().Execute(commandData, ref message, elements);
        }
        catch (System.Exception exception)
        {
            Log.Write("HelloRevitCommand delegation error: " + exception);
            message = "Batch Floor Plan Generator failed. " + exception.Message;
            return Result.Failed;
        }
    }
}
