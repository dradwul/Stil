using System.Globalization;

namespace Stil.Blazor.Components;

/// <summary>Converts the doubles of range inputs to and from the bound number type.</summary>
internal static class StilNumbers
{
    public static T? To<T>(double value)
    {
        var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        return (T?)Convert.ChangeType(type == typeof(decimal) ? (decimal)value : value, type, CultureInfo.InvariantCulture);
    }

    public static double? From<T>(T? value) =>
        value is null ? null : Convert.ToDouble(value, CultureInfo.InvariantCulture);
}
