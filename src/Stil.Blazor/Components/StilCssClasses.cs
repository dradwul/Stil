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

    public static string Variant(string block, StilVariant variant) => variant switch
    {
        StilVariant.Primary => $"{block}-primary",
        StilVariant.Secondary => $"{block}-secondary",
        StilVariant.Success => $"{block}-success",
        StilVariant.Danger => $"{block}-danger",
        StilVariant.Warning => $"{block}-warning",
        StilVariant.Info => $"{block}-info",
        _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, "Unknown variant.")
    };
}
