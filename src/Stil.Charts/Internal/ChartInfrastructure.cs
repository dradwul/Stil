using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Stil.Charts.Internal;

/// <summary>Generates element ids for chart titles and descriptions.</summary>
internal static class ChartIds
{
    private static long _next;

    public static string Next() => $"stil-chart-id-{Interlocked.Increment(ref _next)}";
}

/// <summary>
/// Renders its content after the components before it in the same parent. Charts put it after
/// their ChildContent, so every series has registered (and received its latest parameters)
/// before the chart draws, already in the first render and in static SSR.
/// </summary>
internal sealed class ChartDefer : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, ChildContent);
}

/// <summary>One series, read from the items: its name, color and a value per category.</summary>
internal sealed record ChartSeriesData(int Index, string Name, string Color, string? Format, IReadOnlyList<double?> Values)
{
    /// <summary>The values that are present.</summary>
    public IEnumerable<double> Present => Values.Where(v => v.HasValue).Select(v => v!.Value);
}

/// <summary>The chart's data, read once per render.</summary>
internal sealed record ChartData(IReadOnlyList<string> Categories, IReadOnlyList<ChartSeriesData> Series)
{
    public static readonly ChartData Empty = new([], []);

    public bool HasValues => Categories.Count > 0 && Series.Any(s => s.Values.Any(v => v.HasValue));
}
