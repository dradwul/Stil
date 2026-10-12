namespace Stil.Blazor.Components;

/// <summary>Generates element ids for labels, ARIA relationships and tab panels.</summary>
internal static class StilIds
{
    private static long _next;

    public static string Next(string prefix) => $"stil-{prefix}-{Interlocked.Increment(ref _next)}";
}
