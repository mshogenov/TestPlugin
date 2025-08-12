using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SumParameters.Services;
using SumParameters.ViewModels;
using SumParameters.Views;

namespace SumParameters.Commands;

[Transaction(TransactionMode.Manual)]
public class SumParametersCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        if (WindowController.Focus<MainWindow>()) return Result.Succeeded;
        var viewModel = new MainWindowVM(commandData);
        var view = new MainWindow(viewModel);
        WindowController.Show(view, commandData.Application.MainWindowHandle);
        return Result.Succeeded;
    }
}