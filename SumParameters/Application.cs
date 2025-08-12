using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace SumParameters
{
    public class Application : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            CreateRibbon(application);
            return Result.Succeeded;
        }

        private void CreateRibbon(UIControlledApplication application)
        {
            string tabName = "Тестовый плагин";
            application.CreateRibbonTab(tabName);
            RibbonPanel panel = application.CreateRibbonPanel(tabName, "Тестовая панель");
          PushButtonData buttonData = new PushButtonData(
                "SumParametersButton", // Внутреннее имя кнопки
                "Сумма параметров", // Текст на кнопке
                Assembly.GetExecutingAssembly().Location, // Путь к DLL
                "SumParameters.Commands.SumParametersCommand" // Полное имя класса команды
            );
            buttonData.LargeImage =
                new BitmapImage(new Uri("pack://application:,,,/SumParameters;component/Resources/Icon32.ico"));
            buttonData.Image =
                new BitmapImage(new Uri("pack://application:,,,/SumParameters;component/Resources/Icon16.ico"));
            PushButton button = panel.AddItem(buttonData) as PushButton;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}