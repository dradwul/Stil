using System.Globalization;

namespace Stil.Blazor.Components;

/// <summary>Relative times in the built-in texts, shared by the activity log, chat and notifications.</summary>
internal static class StilRelativeTime
{
    /// <summary>
    /// "Just now", "5 minutes ago" or "2 hours ago" within six hours on the same day, otherwise
    /// the time of day; another day also gets its date.
    /// </summary>
    public static string Format(DateTime time, DateTime now, Func<string, string> text, CultureInfo culture, bool withDate = false)
    {
        var ago = now - time;
        if (ago < TimeSpan.Zero || ago.TotalMinutes < 1)
        {
            return text("ActivityLog.JustNow");
        }

        if (ago.TotalMinutes < 60)
        {
            var minutes = (int)ago.TotalMinutes;
            return minutes == 1 ? text("ActivityLog.MinuteAgo") : string.Format(culture, text("ActivityLog.MinutesAgoFormat"), minutes);
        }

        if (ago.TotalHours < 6 && time.Date == now.Date)
        {
            var hours = (int)ago.TotalHours;
            return hours == 1 ? text("ActivityLog.HourAgo") : string.Format(culture, text("ActivityLog.HoursAgoFormat"), hours);
        }

        return withDate && time.Date != now.Date ? time.ToString("g", culture) : time.ToString("t", culture);
    }
}
