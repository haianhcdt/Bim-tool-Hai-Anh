using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace DSCons.Revit.Starter.Commands;

[Transaction(TransactionMode.Manual)]
public class CreateWorkingViewCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        return new WorkingViewsCommand().Execute(commandData, ref message, elements);
    }
}