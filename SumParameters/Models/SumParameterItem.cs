using Autodesk.Revit.DB;

namespace SumParameters.Models;

public class SumParameterItem
{
    public string ParameterName { get; set; }
    public double ParameterValue { get; set; }
    public UnitOfMeasurement UnitOfMeasurement { get; set; }
    public double ParameterValueCoefficient { get; set; }
    // Добавляем свойство для отображения единиц на русском
    public string UnitDisplayName => GetRussianUnitName(UnitOfMeasurement);
    public SumParameterItem(Parameter parameter)
    {
        if (parameter == null) return;
    
        ParameterName = parameter.Definition.Name;
        ParameterValueCoefficient = 1.0;
    
        if (parameter.StorageType == StorageType.Double)
        {
            double value = parameter.AsDouble();
            var specTypeId = parameter.Definition.GetDataType();
        
            (UnitOfMeasurement, ParameterValue) = specTypeId switch
            {
                var x when x == SpecTypeId.Length => 
                    (UnitOfMeasurement.Millimeters, UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.Millimeters)),
            
                var x when x == SpecTypeId.Area => 
                    (UnitOfMeasurement.SquareMillimeters, UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.SquareMillimeters)),
            
                var x when x == SpecTypeId.Volume => 
                    (UnitOfMeasurement.CubicMillimeters, UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.CubicMillimeters)),
            
                var x when x == SpecTypeId.Angle => 
                    (UnitOfMeasurement.Degrees, UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.Degrees)),
            
                var x when x == SpecTypeId.Mass => 
                    (UnitOfMeasurement.Kilograms, UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.Kilograms)),
            
                _ => (UnitOfMeasurement.Number, value)
            };
        }
        else
        {
            UnitOfMeasurement = UnitOfMeasurement.Number;
            ParameterValue = 0;
        }
    }
    private string GetRussianUnitName(UnitOfMeasurement unit)
    {
        return unit switch
        {
            UnitOfMeasurement.Millimeters => "мм",
            UnitOfMeasurement.Centimeters => "см",
            UnitOfMeasurement.Meters => "м",
            UnitOfMeasurement.Inches => "дюйм",
            UnitOfMeasurement.Feet => "фут",
            
            UnitOfMeasurement.SquareMillimeters => "мм²",
            UnitOfMeasurement.SquareCentimeters => "см²",
            UnitOfMeasurement.SquareMeters => "м²",
            UnitOfMeasurement.SquareInches => "дюйм²",
            UnitOfMeasurement.SquareFeet => "фут²",
            
            UnitOfMeasurement.CubicMillimeters => "мм³",
            UnitOfMeasurement.CubicCentimeters => "см³",
            UnitOfMeasurement.CubicMeters => "м³",
            UnitOfMeasurement.CubicInches => "дюйм³",
            UnitOfMeasurement.CubicFeet => "фут³",
            UnitOfMeasurement.Liters => "л",
            
            UnitOfMeasurement.Degrees => "°",
            UnitOfMeasurement.Radians => "рад",
            
            UnitOfMeasurement.Kilograms => "кг",
            UnitOfMeasurement.Grams => "г",
            UnitOfMeasurement.Pounds => "фунт",
            
            UnitOfMeasurement.Celsius => "°C",
            UnitOfMeasurement.Fahrenheit => "°F",
            UnitOfMeasurement.Kelvin => "K",
            
            UnitOfMeasurement.Number => "",
            UnitOfMeasurement.Percent => "%",
            
            _ => unit.ToString()
        };
    }
}