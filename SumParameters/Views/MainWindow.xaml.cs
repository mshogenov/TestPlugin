using System.Windows;
using SumParameters.ViewModels;

namespace SumParameters.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowVM viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}