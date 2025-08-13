using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Autodesk.Revit.DB;

namespace SumParameters.Models;

public class SumParameterItem : INotifyPropertyChanged
{
    public string ParameterName { get; set; }
    private readonly double _parameterValue;
    private decimal _parameterValueDisplay;

    public decimal ParameterValueDisplay
    {
        get => _parameterValueDisplay;
        set
        {
            if (value.Equals(_parameterValueDisplay)) return;
            _parameterValueDisplay = value;
            OnPropertyChanged();
        }
    }

    private decimal _parameterValueCoefficient;

    public decimal ParameterValueCoefficient
    {
        get => _parameterValueCoefficient;
        set
        {
            if (value.Equals(_parameterValueCoefficient)) return;
            _parameterValueCoefficient = value;
            OnPropertyChanged();
        }
    }

    public string ToUnitLabel { get; set; }

    public SumParameterItem(Parameter parameter, List<Element> elements, decimal selectedRatio, int valueRounding)
    {
        ParameterName = parameter.Definition.Name;
        ToUnitLabel = GetUnitSymbolSimple(parameter);
        foreach (var element in elements)
        {
            var param = element.get_Parameter(parameter.Definition);
            if (param != null && !string.IsNullOrEmpty(param.AsValueString()))
            {
                if (double.TryParse(param.AsValueString(), NumberStyles.Float, CultureInfo.InvariantCulture,
                        out double value))
                {
                    _parameterValue += value;
                }
            }
        }

        RoundingValue(valueRounding);
        ParameterValueCoefficient = ParameterValueDisplay * selectedRatio;
    }

    public void RoundingValue(int valueRounding)
    {
        if (valueRounding < 0) return;
        double round = Math.Round(_parameterValue, valueRounding);
        ParameterValueDisplay = (decimal)round;
    }

    public void UpdateValueCoefficient(decimal selectedRatio)
    {
        ParameterValueCoefficient = ParameterValueDisplay * selectedRatio;
    }

    private string GetUnitSymbolSimple(Parameter parameter)
    {
        if (parameter == null || !parameter.HasValue)
            return "";

        string valueString = parameter.AsValueString();

        if (string.IsNullOrEmpty(valueString))
            return "";

        // Ищем последовательность букв и символов после чисел
        var regex = new System.Text.RegularExpressions.Regex(@"^[\d\s.,\-]+(.*)$");
        var match = regex.Match(valueString.Trim());

        if (match.Success && match.Groups.Count > 1)
        {
            string units = match.Groups[1].Value.Trim();

            // Проверяем, что это действительно единицы, а не число
            if (!string.IsNullOrEmpty(units) && !double.TryParse(units, out _))
            {
                return units;
            }
        }

        return "";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}