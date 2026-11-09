using System.Globalization;
using System.Text;

namespace Stil.Blazor.Components;

/// <summary>How often a recurring event repeats.</summary>
public enum StilFrequency
{
    Daily,
    Weekly,
    Monthly,
    Yearly,
}

/// <summary>
/// A rule for a recurring calendar item, like an iCalendar RRULE: every <see cref="Interval"/>
/// days, weeks (on <see cref="Days"/>), months (on the start's day of the month) or years, until
/// <see cref="Until"/> or for <see cref="Count"/> times, without the <see cref="Exceptions"/>.
/// The calendar, agenda and week planner show each occurrence; the item's Start and End are the
/// first one.
/// </summary>
public sealed record StilRecurrence
{
    private const int MaxOccurrences = 10_000;

    public StilFrequency Frequency { get; init; } = StilFrequency.Weekly;

    /// <summary>Repeats every this many days, weeks, months or years. The default is 1.</summary>
    public int Interval { get; init; } = 1;

    /// <summary>For weekly rules, the days of the week; the start's day when empty.</summary>
    public IReadOnlyCollection<DayOfWeek> Days { get; init; } = [];

    /// <summary>The last day an occurrence can start on.</summary>
    public DateOnly? Until { get; init; }

    /// <summary>How many occurrences there are, the first included (and exceptions too, as in iCalendar).</summary>
    public int? Count { get; init; }

    /// <summary>Days with no occurrence, for example a cancelled or moved meeting.</summary>
    public IReadOnlyCollection<DateOnly> Exceptions { get; init; } = [];

    public static StilRecurrence Daily(int interval = 1) => new() { Frequency = StilFrequency.Daily, Interval = interval };

    public static StilRecurrence Weekly(params DayOfWeek[] days) => new() { Frequency = StilFrequency.Weekly, Days = days };

    public static StilRecurrence Monthly(int interval = 1) => new() { Frequency = StilFrequency.Monthly, Interval = interval };

    /// <summary>The same rule without the occurrence on <paramref name="date"/>, for one that was moved or cancelled.</summary>
    public StilRecurrence Except(DateOnly date) => this with { Exceptions = [.. Exceptions, date] };

    /// <summary>
    /// The start of each occurrence, in order, for an item that first starts at <paramref name="start"/>.
    /// It ends at <see cref="Until"/> or <see cref="Count"/>, and otherwise after 10 000 occurrences.
    /// </summary>
    public IEnumerable<DateTime> Occurrences(DateTime start)
    {
        var interval = Math.Max(1, Interval);
        var weekDays = Frequency == StilFrequency.Weekly && Days.Count > 0 ? Days : [start.DayOfWeek];
        var produced = 0;
        for (var period = 0; produced < MaxOccurrences && period < 100_000; period++)
        {
            foreach (var candidate in Candidates(start, period * interval, weekDays))
            {
                if (candidate < start)
                {
                    continue;
                }

                var date = DateOnly.FromDateTime(candidate);
                if (Until is { } until && date > until || Count is { } count && produced >= count)
                {
                    yield break;
                }

                produced++;
                if (!Exceptions.Contains(date))
                {
                    yield return candidate;
                }
            }
        }
    }

    /// <summary>Whether an occurrence starts on <paramref name="date"/>; its start when it does.</summary>
    public DateTime? OccurrenceOn(DateTime start, DateOnly date)
    {
        if (date < DateOnly.FromDateTime(start) || Until is { } until && date > until)
        {
            return null;
        }

        foreach (var occurrence in Occurrences(start))
        {
            var day = DateOnly.FromDateTime(occurrence);
            if (day == date)
            {
                return occurrence;
            }

            if (day > date)
            {
                break;
            }
        }

        return null;
    }

    // The occurrences of one period: a day, a week (on the chosen days), a month or a year.
    private IEnumerable<DateTime> Candidates(DateTime start, int steps, IReadOnlyCollection<DayOfWeek> weekDays)
    {
        switch (Frequency)
        {
            case StilFrequency.Daily:
                yield return start.AddDays(steps);
                break;
            case StilFrequency.Weekly:
                var weekStart = start.Date.AddDays(-(((int)start.DayOfWeek + 6) % 7)).AddDays(7 * steps);
                foreach (var day in weekDays.OrderBy(d => ((int)d + 6) % 7))
                {
                    yield return weekStart.AddDays(((int)day + 6) % 7) + start.TimeOfDay;
                }

                break;
            case StilFrequency.Monthly:
                // A month without the day (the 31st in April) is skipped, as in iCalendar.
                var month = start.AddMonths(steps);
                if (month.Day == start.Day)
                {
                    yield return month;
                }

                break;
            case StilFrequency.Yearly:
                var year = start.AddYears(steps);
                if (year.Day == start.Day)
                {
                    yield return year;
                }

                break;
        }
    }

    /// <summary>The rule as an iCalendar RRULE value, for example <c>FREQ=WEEKLY;BYDAY=MO,WE;COUNT=10</c>. Exceptions are not part of it (they are EXDATE in iCalendar).</summary>
    public override string ToString()
    {
        var rule = new StringBuilder($"FREQ={Frequency.ToString().ToUpperInvariant()}");
        if (Interval > 1)
        {
            rule.Append(CultureInfo.InvariantCulture, $";INTERVAL={Interval}");
        }

        if (Days.Count > 0)
        {
            rule.Append(";BYDAY=").AppendJoin(',', Days.OrderBy(d => ((int)d + 6) % 7).Select(d => d.ToString()[..2].ToUpperInvariant()));
        }

        if (Until is { } until)
        {
            rule.Append(CultureInfo.InvariantCulture, $";UNTIL={until:yyyyMMdd}");
        }

        if (Count is { } count)
        {
            rule.Append(CultureInfo.InvariantCulture, $";COUNT={count}");
        }

        return rule.ToString();
    }

    /// <summary>Reads an iCalendar RRULE value (FREQ, INTERVAL, BYDAY, UNTIL and COUNT; an optional "RRULE:" prefix).</summary>
    public static StilRecurrence Parse(string rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        var result = new StilRecurrence();
        foreach (var part in rule.Trim().Replace("RRULE:", "", StringComparison.OrdinalIgnoreCase).Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var (name, value) = part.Split('=', 2) is [var n, var v] ? (n.Trim().ToUpperInvariant(), v.Trim().ToUpperInvariant()) : throw new FormatException($"'{part}' is not NAME=VALUE.");
            result = name switch
            {
                "FREQ" => result with { Frequency = Enum.Parse<StilFrequency>(value, ignoreCase: true) },
                "INTERVAL" => result with { Interval = int.Parse(value, CultureInfo.InvariantCulture) },
                "COUNT" => result with { Count = int.Parse(value, CultureInfo.InvariantCulture) },
                "UNTIL" => result with { Until = DateOnly.ParseExact(value[..8], "yyyyMMdd", CultureInfo.InvariantCulture) },
                "BYDAY" => result with { Days = [.. value.Split(',').Select(Day)] },
                _ => throw new FormatException($"{name} is not supported."),
            };
        }

        return result;
    }

    private static DayOfWeek Day(string code) => code[^2..] switch
    {
        "MO" => DayOfWeek.Monday,
        "TU" => DayOfWeek.Tuesday,
        "WE" => DayOfWeek.Wednesday,
        "TH" => DayOfWeek.Thursday,
        "FR" => DayOfWeek.Friday,
        "SA" => DayOfWeek.Saturday,
        "SU" => DayOfWeek.Sunday,
        _ => throw new FormatException($"'{code}' is not a day."),
    };
}
