using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SumParameters.Services;
using SumParameters.Views;

namespace SumParameters.Commands;

[Transaction(TransactionMode.Manual)]
public class SumParametersCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        if (WindowController.Focus<MainWindow>()) return Result.Succeeded;
        var view = new MainWindow();
        WindowController.Show(view, commandData.Application.MainWindowHandle);
        return Result.Succeeded;
    }
}