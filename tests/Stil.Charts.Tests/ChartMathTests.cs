using System.Globalization;
using Stil.Charts.Internal;

namespace Stil.Charts.Tests;

public class ChartMathTests
{
    [Theory]
    [InlineData(0.7, 1)]
    [InlineData(1, 1)]
    [InlineData(1.2, 2)]
    [InlineData(2.2, 2.5)]
    [InlineData(3, 5)]
    [InlineData(6, 10)]
    [InlineData(860, 1000)]
    [InlineData(0.013, 0.02)]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    public void NiceStepRoundsUpToOneTwoTwoAndAHalfOrFive(double rough, double expected)
    {
        Assert.Equal(expected, ChartScale.NiceStep(rough), 9);
    }

    [Fact]
    public void ScaleCoversTheDataWithRoundTicksFromZero()
    {
        var scale = ChartScale.Create(1200, 4300, maxTicks: 5);

        Assert.Equal(0, scale.Min);
        Assert.Equal(5000, scale.Max);
        Assert.Equal(1000, scale.Step);
        Assert.Equal([0, 1000, 2000, 3000, 4000, 5000], scale.Ticks);
        Assert.Equal(0, scale.Decimals);
        Assert.Equal(0, scale.Baseline);
    }

    [Fact]
    public void ScaleWithoutZeroZoomsInOnTheData()
    {
        var scale = ChartScale.Create(96.2, 98.9, maxTicks: 5, includeZero: false);

        Assert.Equal(96, scale.Min);
        Assert.Equal(99, scale.Max);
        Assert.Equal(1, scale.Step);
        Assert.Equal(0, scale.Decimals);
        Assert.Equal(scale.Min, scale.Baseline);
    }

    [Fact]
    public void ScaleWithNegativeValuesSpansZero()
    {
        var scale = ChartScale.Create(-320, 780, maxTicks: 6);

        Assert.True(scale.Min <= -320);
        Assert.True(scale.Max >= 780);
        Assert.Contains(0.0, scale.Ticks);
        Assert.Equal(0, scale.Baseline);
        Assert.InRange(scale.Fraction(0), 0.01, 0.99);
    }

    [Fact]
    public void ScaleOfOnlyNegativeValuesEndsAtZero()
    {
        var scale = ChartScale.Create(-40, -5);

        Assert.Equal(0, scale.Max);
        Assert.True(scale.Min <= -40);
        Assert.Equal(1, scale.Fraction(0));
    }

    [Fact]
    public void FlatDataGetsRoom()
    {
        var zero = ChartScale.Create(0, 0);
        Assert.Equal(0, zero.Min);
        Assert.True(zero.Max > 0);

        var flat = ChartScale.Create(50, 50, includeZero: false);
        Assert.True(flat.Min < 50 && flat.Max > 50);
    }

    [Fact]
    public void FixedMinAndMaxAreKept()
    {
        var scale = ChartScale.Create(12, 87, maxTicks: 4, fixedMin: 0, fixedMax: 100);

        Assert.Equal(0, scale.Min);
        Assert.Equal(100, scale.Max);
        Assert.Equal(0, scale.Ticks[0]);
        Assert.Equal(100, scale.Ticks[^1]);
    }

    [Fact]
    public void TickCountStaysNearTheMaximum()
    {
        foreach (var max in new[] { 7.0, 13, 99, 101, 999, 1001, 12345, 0.37 })
        {
            var scale = ChartScale.Create(0, max, maxTicks: 5);
            Assert.InRange(scale.Ticks.Count, 2, 7);
            Assert.True(scale.Max >= max);
        }
    }

    [Fact]
    public void TicksHaveNoFloatingPointNoise()
    {
        var scale = ChartScale.Create(0, 0.7, maxTicks: 7);

        Assert.All(scale.Ticks, t => Assert.Equal(Math.Round(t, 6), t));
        Assert.Contains(0.3, scale.Ticks);
    }

    [Fact]
    public void FractionMapsTheDomainToZeroToOne()
    {
        var scale = ChartScale.Create(0, 100, fixedMin: 0, fixedMax: 100);

        Assert.Equal(0, scale.Fraction(0));
        Assert.Equal(0.25, scale.Fraction(25));
        Assert.Equal(1, scale.Fraction(100));
    }

    [Theory]
    [InlineData(1000, 0)]
    [InlineData(2.5, 1)]
    [InlineData(0.25, 2)]
    [InlineData(0.1, 1)]
    public void DecimalsFollowTheStep(double step, int decimals)
    {
        Assert.Equal(decimals, ChartScale.DecimalsOf(step));
    }

    [Fact]
    public void SvgNumbersAreInvariantWhateverTheCulture()
    {
        CultureInfo.CurrentCulture = new CultureInfo("sv-SE");

        Assert.Equal("12.5", ChartText.N(12.5));
        Assert.Equal("33.333%", ChartText.Pct(1.0 / 3));
        Assert.Equal("0", ChartText.N(-0.0001));
    }

    [Fact]
    public void ValuesUseTheCurrentCultureAndFormat()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        Assert.Equal("1,234.5", ChartText.Value(1234.5, null));
        Assert.Equal("$1,235", ChartText.Value(1234.6, "C0"));

        CultureInfo.CurrentCulture = new CultureInfo("sv-SE");
        Assert.Equal($"1{CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator}234,5", ChartText.Value(1234.5, null));
        Assert.Equal("–", ChartText.Value(null, null, "–"));
    }

    [Fact]
    public void CategoriesUseTheirFormat()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        Assert.Equal("Mar", ChartText.Category(new DateTime(2026, 3, 1), "MMM"));
        Assert.Equal("Widgets", ChartText.Category("Widgets", "MMM"));
        Assert.Equal("", ChartText.Category(null, null));
    }

    [Fact]
    public void ToDoubleAcceptsEveryNumberTypeAndMissingValues()
    {
        Assert.Equal(1.5, ChartText.ToDouble(1.5m));
        Assert.Equal(3, ChartText.ToDouble(3));
        Assert.Equal(7, ChartText.ToDouble((long?)7));
        Assert.Null(ChartText.ToDouble(null));
        Assert.Null(ChartText.ToDouble(double.NaN));
        Assert.Throws<InvalidCastException>(() => ChartText.ToDouble(new object()));
    }

    [Fact]
    public void TruncateAddsAnEllipsis()
    {
        Assert.Equal("Short", ChartText.Truncate("Short", 10));
        Assert.Equal("Fjällbacka…", ChartText.Truncate("Fjällbacka Café AB", 11));
    }

    [Fact]
    public void RunsSplitAtMissingValues()
    {
        var runs = ChartPaths.Runs([1, 2, null, 4, null, null, 7, 8]);

        Assert.Equal(3, runs.Count);
        Assert.Equal([0, 1], runs[0].Select(p => p.Index));
        Assert.Equal([3], runs[1].Select(p => p.Index));
        Assert.Equal([6, 7], runs[2].Select(p => p.Index));
    }

    [Fact]
    public void LineAndAreaPaths()
    {
        var points = new List<(double, double)> { (10, 50), (30, 20.5) };

        Assert.Equal("M10 50 L30 20.5", ChartPaths.Line(points));
        Assert.Equal("M10 100 L10 50 L30 20.5 L30 100 Z", ChartPaths.Area(points, 100));
        Assert.Equal("", ChartPaths.Area([], 100));
    }

    [Fact]
    public void DonutSliceStartsAtTwelveOClock()
    {
        var quarter = ChartPaths.DonutSlice(0, 0, 100, 60, 0, Math.PI / 2);

        Assert.StartsWith("M0 -100 A100 100 0 0 1 100 0 L60 0 A60 60 0 0 0 0 -60 Z", quarter);
        Assert.Contains(" 1 1 ", ChartPaths.DonutSlice(0, 0, 100, 60, 0, Math.PI * 1.5));
        // A full circle is drawn as two halves.
        Assert.Equal(2, ChartPaths.DonutSlice(0, 0, 100, 60, 0, Math.PI * 2).Split('M').Length - 1);
    }

    [Theory]
    [InlineData(12, 36, 260, 2)]
    [InlineData(12, 36, 760, 1)]
    [InlineData(1, 500, 10, 1)]
    [InlineData(30, 36, 300, 4)]
    public void ThinningShowsEveryNthLabel(int count, double labelWidth, double plotWidth, int step)
    {
        Assert.Equal(step, ChartLayout.ThinningStep(count, labelWidth, plotWidth));
    }

    [Fact]
    public void ThinningClassesHideLabelsPerWidthTier()
    {
        var layout = new ChartLayout { Height = 300, PlotTop = 10, PlotBottom = 274, Start = 40, End = 12 };

        // 12 labels of 30px: every second on phones (s), all on medium (m) and large (l) charts.
        Assert.Null(layout.ThinningClass(0, 12, 30));
        Assert.Equal("stil-chart-hide-s", layout.ThinningClass(1, 12, 30));
        Assert.Null(layout.ThinningClass(2, 12, 30));
    }
}
