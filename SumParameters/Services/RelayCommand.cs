using System;
using System.Windows.Input;

namespace SumParameters.Services;

public class RelayCommand : ICommand
{
    private readonly Action<object> _executeWithParam;
    private readonly Action _execute;
    private readonly Func<object, bool> _canExecuteWithParam;
    private readonly Func<bool> _canExecute;

    // Конструктор без параметров
    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    // Конструктор с параметрами
    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _executeWithParam = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecuteWithParam = canExecute;
    }

    

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
        if (_canExecute != null)
            return _canExecute();
            
        return _canExecuteWithParam?.Invoke(parameter) ?? true;
    }

    public void Execute(object parameter)
    {
        if (_execute != null)
            _execute();
        else
            _executeWithParam?.Invoke(parameter);
    }
}