using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SumParameters.Models;
using SumParameters.Services;

namespace SumParameters.ViewModels;

public sealed class MainWindowVM : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly Document _doc;
    private readonly UIDocument _uiDoc;
    private ObservableCollection<SumParameterItem> _sumParameterItems = [];

    private List<decimal> _ratios =
    [
        0.0001m,
        0.001m,
        0.01m,
        0.1m,
        1m,
        10m,
        100m,
        1000m,
        10000m
    ];


    public List<decimal> Ratios
    {
        get => _ratios;
        set
        {
            if (Equals(value, _ratios)) return;
            _ratios = value ?? throw new ArgumentNullException(nameof(value));
            OnPropertyChanged();
        }
    }

    private int _valueRounding = 0;

    public int ValueRounding
    {
        get => _valueRounding;
        set
        {
            if (value == _valueRounding) return;
            _valueRounding = value;
            UpdateValues();
            OnPropertyChanged();
        }
    }


    private decimal _selectedRatio = 1;
    private readonly JsonDataLoader _dataLoader;

    public decimal SelectedRatio
    {
        get => _selectedRatio;
        set
        {
            if (value.Equals(_selectedRatio)) return;
            _selectedRatio = value;
            UpdateValuesCoefficient();
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SumParameterItem> SumParameterItems
    {
        get => _sumParameterItems;
        set
        {
            if (Equals(value, _sumParameterItems)) return;
            _sumParameterItems = value ?? throw new ArgumentNullException(nameof(value));
            OnPropertyChanged();
        }
    }

    private void UpdateValuesCoefficient()
    {
        foreach (var sumParameterItem in SumParameterItems)
        {
            sumParameterItem.UpdateValueCoefficient(SelectedRatio);
        }
    }

    private void UpdateValues()
    {
        foreach (var sumParameterItem in SumParameterItems)
        {
            sumParameterItem.RoundingValue(ValueRounding);
            sumParameterItem.UpdateValueCoefficient(SelectedRatio);
        }
    }

    public ICommand UpValueRoundingCommand { get; }
    public ICommand DownValueRoundingCommand { get; }
    public ICommand GetCommand { get; }

    public MainWindowVM(ExternalCommandData commandData)
    {
        _uiDoc = commandData.Application.ActiveUIDocument;
        _doc = _uiDoc.Document;
        UpValueRoundingCommand = new RelayCommand(UpValueRounding, CanUpValueRounding);
        DownValueRoundingCommand = new RelayCommand(DownValueRounding, CanDownValueRounding);
        GetCommand = new RelayCommand(Get);
        GetSumParameterItems();
        _dataLoader = new JsonDataLoader("SumParameters.json");
        var loadData = _dataLoader.LoadData<SumParametersDTO>();
        if (loadData != null)
        {
            SelectedRatio = loadData.SelectedRatio;
            ValueRounding = loadData.ValueRounding;
        }
    }

    private void GetSumParameterItems()
    {
        var selectedIds = _uiDoc.Selection.GetElementIds();
        if (selectedIds.Count == 0) return;
        List<Element> elements = [];
        elements.AddRange(selectedIds.Select(elementId => _doc.GetElement(elementId)));
        var genericParameters = GetGenericParameters(elements)
            .OrderBy(x => x.Definition.Name);
        foreach (var parameter in genericParameters)
        {
            SumParameterItems.Add(new SumParameterItem(parameter, elements, SelectedRatio, ValueRounding));
        }
    }

    public void SaveData()
    {
        if (_dataLoader != null)
        {
            _dataLoader.SaveData(new SumParametersDTO()
            {
                SelectedRatio = SelectedRatio,
                ValueRounding = ValueRounding
            });
        }
    }

    private void Get()
    {
        SumParameterItems.Clear();
        GetSumParameterItems();
    }

    private void DownValueRounding()
    {
        ValueRounding--;
    }

    private bool CanDownValueRounding()
    {
        return ValueRounding > 0;
    }


    private void UpValueRounding()
    {
        ValueRounding++;
    }

    private bool CanUpValueRounding()
    {
        return ValueRounding < 10;
    }

    private List<Parameter> GetGenericParameters(List<Element> elements)
    {
        if (elements == null || !elements.Any())
            return new List<Parameter>();
        var firstElement = elements.First();
        if (elements.Count == 1)
        {
            return firstElement.Parameters
                .Cast<Parameter>()
                .Where(p =>
                    p.StorageType is StorageType.Double or StorageType.Integer).ToList();
        }

        // Получаем определений параметров первого элемента
        var firstElementParameterIds = firstElement.Parameters
            .Cast<Parameter>()
            .Where(p =>
                p.StorageType is StorageType.Double or StorageType.Integer)
            .Select(p => p.Definition.Name)
            .ToHashSet();

        // Находим пересечение с параметрами остальных элементов
        foreach (var element in elements.Skip(1))
        {
            var elementParameterIds = element.Parameters.Cast<Parameter>()
                .Select(p => p.Definition.Name)
                .ToHashSet();

            firstElementParameterIds.IntersectWith(elementParameterIds);
        }

        // Получаем параметры с общими из первого элемента
        return firstElement.Parameters.Cast<Parameter>()
            .Where(p => firstElementParameterIds.Contains(p.Definition.Name))
            .ToList();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}