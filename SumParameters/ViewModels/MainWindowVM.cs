using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SumParameters.Models;

namespace SumParameters.ViewModels;

public sealed class MainWindowVM : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly Document _doc;
    private readonly UIDocument _uiDoc;
    private ObservableCollection<SumParameterItem> _sumParameterItems = [];


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

    public MainWindowVM(ExternalCommandData commandData)
    {
        _uiDoc = commandData.Application.ActiveUIDocument;
        _doc = _uiDoc.Document;

        var selectedIds = _uiDoc.Selection.GetElementIds();
        if (selectedIds.Count > 0)
        {
            List<Element> elements = [];
            foreach (var elementId in selectedIds)
            {
                elements.Add(_doc.GetElement(elementId));
            }

// Найти общие параметры
            var genericParameters = GetGenericParameters(elements)
                .OrderBy(x => x.Definition.Name);
            foreach (var parameter in genericParameters)
            {
                SumParameterItems.Add(new SumParameterItem(parameter, elements));
            }
        }
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

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}