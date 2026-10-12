namespace Stil.Blazor.Components;

internal static class StilCssClasses
{
    /// <summary>
    /// Joins the component's classes (skipping empty ones) and appends a consumer-provided
    /// <c>class</c> attribute, so the component's required classes are never replaced.
    /// </summary>
    public static string Merge(IReadOnlyDictionary<string, object>? additionalAttributes, params string?[] componentClasses)
    {
        var classes = string.Join(' ', componentClasses.Where(c => !string.IsNullOrWhiteSpace(c)));

        if (additionalAttributes is not null
            && additionalAttributes.TryGetValue("class", out var customClass)
            && !string.IsNullOrWhiteSpace(customClass?.ToString()))
        {
            return $"{classes} {customClass}";
        }

        return classes;
    }

    public static string Merge(string componentClasses, IReadOnlyDictionary<string, object>? additionalAttributes) =>
        Merge(additionalAttributes, componentClasses);

    /// <summary>Returns <c>{block}-{variant}</c>, for example <c>stil-alert-danger</c>.</summary>
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

    /// <summary>Returns <c>{block}-sm</c> or <c>{block}-lg</c>, or null for the default size.</summary>
    public static string? Size(string block, StilSize size) => size switch
    {
        StilSize.Small => $"{block}-sm",
        StilSize.Large => $"{block}-lg",
        _ => null
    };

    public static string ThemeMode(StilThemeMode mode) => mode switch
    {
        StilThemeMode.Dark => "dark",
        StilThemeMode.Auto => "auto",
        _ => "light"
    };

    /// <summary>The string Blazor should render for a boolean ARIA attribute, or null to omit it.</summary>
    public static string? AriaBool(bool value) => value ? "true" : null;
}
