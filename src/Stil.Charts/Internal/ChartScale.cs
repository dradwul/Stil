namespace Stil.Charts.Internal;

/// <summary>
/// A linear value scale with "nice" round ticks (steps of 1, 2, 2.5 or 5 times a power of ten),
/// for example 0, 1,000, 2,000, 3,000.
/// </summary>
internal sealed class ChartScale
{
    private static readonly double[] NiceSteps = [1, 2, 2.5, 5, 10];

    private ChartScale(double min, double max, double step, IReadOnlyList<double> ticks)
    {
        Min = min;
        Max = max;
        Step = step;
        Ticks = ticks;
    }

    /// <summary>The lower end of the axis.</summary>
    public double Min { get; }

    /// <summary>The upper end of the axis.</summary>
    public double Max { get; }

    /// <summary>The distance between two ticks.</summary>
    public double Step { get; }

    /// <summary>The tick values, from <see cref="Min"/> up.</summary>
    public IReadOnlyList<double> Ticks { get; }

    /// <summary>Decimals needed to write every tick exactly (0 for a step of 500, 1 for 2.5).</summary>
    public int Decimals => DecimalsOf(Step);

    /// <summary>
    /// Where zero sits, or the end of the axis closest to zero when zero is outside it. Bars and
    /// areas grow from here.
    /// </summary>
    public double Baseline => Math.Clamp(0, Min, Max);

    /// <summary>The position of a value as a fraction of the axis: 0 at Min and 1 at Max.</summary>
    public double Fraction(double value) => Max == Min ? 0 : (value - Min) / (Max - Min);

    /// <summary>
    /// Builds a scale that covers <paramref name="dataMin"/> to <paramref name="dataMax"/> with at
    /// most about <paramref name="maxTicks"/> intervals.
    /// </summary>
    /// <param name="dataMin">The smallest value to show.</param>
    /// <param name="dataMax">The largest value to show.</param>
    /// <param name="maxTicks">The most intervals between ticks.</param>
    /// <param name="includeZero">Extends the axis to zero (bar charts and areas need this).</param>
    /// <param name="fixedMin">An exact lower end set by the app.</param>
    /// <param name="fixedMax">An exact upper end set by the app.</param>
    public static ChartScale Create(double dataMin, double dataMax, int maxTicks = 5, bool includeZero = true,
        double? fixedMin = null, double? fixedMax = null)
    {
        maxTicks = Math.Max(1, maxTicks);
        if (!double.IsFinite(dataMin) || !double.IsFinite(dataMax))
        {
            dataMin = 0;
            dataMax = 0;
        }

        if (dataMin > dataMax)
        {
            (dataMin, dataMax) = (dataMax, dataMin);
        }

        if (includeZero)
        {
            dataMin = Math.Min(dataMin, 0);
            dataMax = Math.Max(dataMax, 0);
        }

        var low = fixedMin ?? dataMin;
        var high = fixedMax ?? dataMax;
        if (low > high)
        {
            (low, high) = (high, low);
        }

        if (low == high)
        {
            // A flat line: give it room. Zero gets 0–1; any other value a band around it.
            var pad = low == 0 ? 1 : Math.Abs(low) * 0.5;
            if (fixedMin is null)
            {
                low = includeZero && low >= 0 ? 0 : low - pad;
            }

            if (fixedMax is null)
            {
                high = includeZero && high <= 0 && low < 0 ? 0 : high + pad;
            }

            if (low == high)
            {
                high = low + 1;
            }
        }

        var step = NiceStep((high - low) / maxTicks);
        var min = fixedMin ?? Math.Floor(Round(low / step)) * step;
        var max = fixedMax ?? Math.Ceiling(Round(high / step)) * step;

        // Rounding out can add an interval at each end; take a bigger step if there are too many.
        while ((max - min) / step > maxTicks + 0.5 && fixedMin is null && fixedMax is null)
        {
            step = NiceStep(step * 1.01);
            min = Math.Floor(Round(low / step)) * step;
            max = Math.Ceiling(Round(high / step)) * step;
        }

        var decimals = DecimalsOf(step);
        var ticks = new List<double>();
        var first = Math.Ceiling(Round(min / step)) * step;
        for (var tick = first; tick <= max + step * 1e-9; tick += step)
        {
            ticks.Add(Math.Round(tick, Math.Min(15, decimals + 2)));
            if (ticks.Count > 100)
            {
                break;
            }
        }

        return new ChartScale(Math.Round(min, Math.Min(15, decimals + 2)), Math.Round(max, Math.Min(15, decimals + 2)), step, ticks);
    }

    /// <summary>The smallest step of 1, 2, 2.5, 5 or 10 times a power of ten that is at least <paramref name="rough"/>.</summary>
    public static double NiceStep(double rough)
    {
        if (!(rough > 0) || !double.IsFinite(rough))
        {
            return 1;
        }

        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(rough)));
        foreach (var nice in NiceSteps)
        {
            var step = nice * magnitude;
            if (step >= rough * (1 - 1e-9))
            {
                return step;
            }
        }

        return 10 * magnitude;
    }

    /// <summary>Decimals needed to write multiples of <paramref name="step"/> exactly.</summary>
    public static int DecimalsOf(double step)
    {
        for (var decimals = 0; decimals < 10; decimals++)
        {
            var scaled = step * Math.Pow(10, decimals);
            if (Math.Abs(scaled - Math.Round(scaled)) < 1e-6 * Math.Max(1, scaled))
            {
                return decimals;
            }
        }

        return 10;
    }

    // Removes floating-point noise such as 2.9999999999999996 before Floor and Ceiling.
    private static double Round(double value) => Math.Round(value, 9);
}
