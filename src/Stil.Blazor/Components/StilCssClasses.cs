namespace Stil.Blazor.Components;

internal static class StilCssClasses
{
    public static string Merge(
        string componentClasses,
        IReadOnlyDictionary<string, object>? additionalAttributes)
    {
        if (additionalAttributes is not null
            && additionalAttributes.TryGetValue("class", out var customClass)
            && !string.IsNullOrWhiteSpace(customClass?.ToString()))
        {
            return $"{componentClasses} {customClass}";
        }

        return componentClasses;
    }
}
