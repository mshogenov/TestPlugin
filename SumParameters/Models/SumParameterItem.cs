using System;
using System.Collections.Generic;
using System.Globalization;
using Autodesk.Revit.DB;

namespace SumParameters.Models;

public class SumParameterItem
{
    public string ParameterName { get; set; }
    public double ParameterValue { get; set; }
    public UnitOfMeasurement UnitOfMeasurement { get; set; }
    public double ParameterValueCoefficient { get; set; }
    public string ToUnitLabel { get; set; }

    public SumParameterItem(Parameter parameter, List<Element> elements)
    {
        if (parameter == null) return;
        ParameterName = parameter.Definition.Name;
        ParameterValueCoefficient = 1.0;
        // Получаем единицы измерения параметра
        // GetParameterUnits(parameter);
        ToUnitLabel = GetUnitSymbolSimple(parameter);
        foreach (var element in elements)
        {
            var param = element.get_Parameter(parameter.Definition);
            if (param != null && !string.IsNullOrEmpty(param.AsValueString()))
            {
                if (double.TryParse(param.AsValueString(),
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double value))
                {
                    ParameterValue += value;
                }
            }
        }
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

    private void GetParameterUnits(Parameter parameter)
    {
        var specTypeId = parameter.Definition.GetDataType();

        if (UnitUtils.IsMeasurableSpec(specTypeId))
        {
            // Получаем единицы измерения из документа
            var doc = parameter.Element.Document;
            var units = doc.GetUnits();
            var formatOptions = units.GetFormatOptions(specTypeId);
            var unitTypeId = formatOptions.GetUnitTypeId();

            // Получаем символ единицы измерения
            ToUnitLabel = LabelUtils.GetLabelForUnit(unitTypeId);

            // Альтернативный способ получить читаемое имя
            // ToUnitLabel = unitTypeId.TypeId;
        }
        else
        {
            ToUnitLabel = "No Units";
        }
    }

// Альтернативный метод для получения значения с конвертацией
    private double GetParameterValueInDisplayUnits(Parameter param)
    {
        if (param == null || !param.HasValue) return 0;
        try
        {
            var specTypeId = param.Definition.GetDataType();

            if (UnitUtils.IsMeasurableSpec(specTypeId))
            {
                var doc = param.Element.Document;
                var units = doc.GetUnits();
                var formatOptions = units.GetFormatOptions(specTypeId);
                var unitTypeId = formatOptions.GetUnitTypeId();

                // Конвертируем из внутренних единиц в единицы отображения
                double internalValue = param.AsDouble();
                return UnitUtils.ConvertFromInternalUnits(internalValue, unitTypeId);
            }
        }
        catch (Exception e)
        {
            return 0;
        }

        return 0;
    }

    public double GetValueInSpecificUnits(Parameter param, ForgeTypeId targetUnitTypeId)
    {
        if (param == null || !param.HasValue) return 0;

        double internalValue = param.AsDouble();
        return UnitUtils.ConvertFromInternalUnits(internalValue, targetUnitTypeId);
    }

    private bool HasUnits(ForgeTypeId specTypeId)
    {
        // Проверяем, имеет ли тип данных единицы измерения
        return specTypeId == SpecTypeId.Length ||
               specTypeId == SpecTypeId.Area ||
               specTypeId == SpecTypeId.Volume ||
               specTypeId == SpecTypeId.Angle ||
               specTypeId == SpecTypeId.Mass ||
               specTypeId == SpecTypeId.Force ||
               specTypeId == SpecTypeId.Acceleration ||
               specTypeId == SpecTypeId.Energy;
    }
}