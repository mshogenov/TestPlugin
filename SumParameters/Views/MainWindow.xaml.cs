using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using SumParameters.ViewModels;

namespace SumParameters.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowVM _viewModel;

    public MainWindow(MainWindowVM viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        Closing += MainWindow_Closing;
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        _viewModel.SaveData();
    }

    private void CopyButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            string value = button.Tag?.ToString();
            if (!string.IsNullOrEmpty(value))
            {
                Clipboard.SetText(value);
                ShowCopyPopup("Скопировано в буфер");
            }
        }
    }
    private void ShowCopyPopup(string message)
    {
        PopupMessage.Text = message;
    
        CopyPopup.PlacementTarget = this;
        CopyPopup.Placement = PlacementMode.Custom;
        CopyPopup.CustomPopupPlacementCallback = PlacePopupOnTitleBar;
    
        CopyPopup.IsOpen = true;

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        timer.Tick += (s, e) =>
        {
            CopyPopup.IsOpen = false;
            timer.Stop();
        };
        timer.Start();
    }

    private CustomPopupPlacement[] PlacePopupOnTitleBar(Size popupSize, Size targetSize, Point offset)
    {
        // Получаем высоту заголовка окна
        double titleBarHeight = SystemParameters.WindowCaptionHeight + SystemParameters.ResizeFrameHorizontalBorderHeight;
    
        // Центрируем по горизонтали
        double x = (targetSize.Width - popupSize.Width) / 2;
    
        // Позиционируем на заголовке окна
        double y = -titleBarHeight  ; 
    
        return
        [
            new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.Horizontal)
        ];
    }

}