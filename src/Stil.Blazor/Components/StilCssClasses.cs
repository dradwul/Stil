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

    /// <summary>The breakpoint infix used in class names: sm, md, lg or xl.</summary>
    public static string Breakpoint(StilBreakpoint breakpoint) => breakpoint switch
    {
        StilBreakpoint.Small => "sm",
        StilBreakpoint.Medium => "md",
        StilBreakpoint.ExtraLarge => "xl",
        _ => "lg"
    };

    /// <summary>A stil-gap-{n} class for a spacing step from 0 to 7, or null.</summary>
    public static string? Gap(int? gap) => gap is { } value
        ? $"stil-gap-{Math.Clamp(value, 0, 7).ToString(System.Globalization.CultureInfo.InvariantCulture)}"
        : null;

    public static string? Justify(StilJustify? justify) => justify switch
    {
        null => null,
        StilJustify.Between => "stil-justify-content-between",
        StilJustify.Around => "stil-justify-content-around",
        StilJustify.Evenly => "stil-justify-content-evenly",
        StilJustify.Center => "stil-justify-content-center",
        StilJustify.End => "stil-justify-content-end",
        _ => "stil-justify-content-start"
    };

    public static string? Align(StilAlign? align) => align switch
    {
        null => null,
        StilAlign.Center => "stil-align-items-center",
        StilAlign.End => "stil-align-items-end",
        StilAlign.Baseline => "stil-align-items-baseline",
        StilAlign.Stretch => "stil-align-items-stretch",
        _ => "stil-align-items-start"
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
