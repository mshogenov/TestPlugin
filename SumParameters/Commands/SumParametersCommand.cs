using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SumParameters.Views;

namespace SumParameters.Commands;

[Transaction(TransactionMode.Manual)]
public class SumParametersCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var view = new MainWindow();
        view.ShowDialog();
        return Result.Succeeded;
    }
}