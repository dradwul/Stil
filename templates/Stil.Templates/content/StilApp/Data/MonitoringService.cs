namespace StilApp.Data;

/// <summary>
/// The servers, errors and events the monitoring pages show. The app registers
/// <see cref="SampleMonitoringService"/>, which simulates them; replace it in Program.cs with one that
/// reads your own metrics. The pages do not change.
/// </summary>
public interface IMonitoringService
{
    /// <summary>How often the readings change, and the pages refresh.</summary>
    TimeSpan Interval { get; }

    /// <summary>Each server as it is now, with its recent readings.</summary>
    IReadOnlyList<ServerStatus> Servers();

    /// <summary>Errors per day and hour, the last seven days.</summary>
    IReadOnlyList<ErrorCount> Errors();

    /// <summary>Alerts, deployments and other events, newest first.</summary>
    IReadOnlyList<MonitorEvent> Events();
}

/// <param name="Cpu">CPU use now, from 0 to 1.</param>
/// <param name="CpuHistory">CPU use over the last readings, oldest first.</param>
public sealed record ServerStatus(string Name, string Role, double Cpu, double Memory, double Disk, int ResponseMs,
    IReadOnlyList<double> CpuHistory);

public sealed record ErrorCount(string Day, int Hour, int Errors);

public sealed record MonitorEvent(int Id, DateTime Time, string Title, string Source, string Type, string? Details = null);

/// <summary>
/// Simulated readings for four servers. Each reading is computed from the time, so every visitor
/// sees the same values and nothing runs in the background.
/// </summary>
public sealed class SampleMonitoringService : IMonitoringService
{
    private const int History = 30;
    private const double AlertLevel = 0.9;

    private static readonly (string Name, string Role, double Cpu, double Memory, double Disk)[] Fleet =
    [
        ("web-1", "Web server", 0.38, 0.52, 0.41),
        ("web-2", "Web server", 0.35, 0.49, 0.43),
        ("api-1", "API", 0.66, 0.71, 0.58),
        ("db-1", "Database", 0.47, 0.83, 0.77),
    ];

    private static readonly string[] Days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

    public TimeSpan Interval { get; } = TimeSpan.FromSeconds(5);

    private long Tick(DateTime time) => time.Ticks / Interval.Ticks;

    public IReadOnlyList<ServerStatus> Servers()
    {
        var now = Tick(DateTime.Now);
        return [.. Fleet.Select((server, s) =>
        {
            var history = Enumerable.Range(0, History).Select(i => Cpu(s, now - History + 1 + i)).ToList();
            var cpu = history[^1];
            return new ServerStatus(server.Name, server.Role, cpu,
                Math.Clamp(server.Memory + Noise(s + 10, now) * 0.03, 0, 1),
                server.Disk,
                (int)(80 + cpu * 180 + Noise(s + 20, now) * 15),
                history);
        })];
    }

    public IReadOnlyList<ErrorCount> Errors() =>
    [
        .. from day in Enumerable.Range(0, 7)
           from hour in Enumerable.Range(0, 24)
           let busy = Math.Exp(-Math.Pow(hour - 14, 2) / 18) * (day < 5 ? 1 : 0.4)
           let nightlyJob = hour == 2 ? 6 : 0
           select new ErrorCount(Days[day], hour, (int)Math.Round(busy * 14 + nightlyJob + (day * 31 + hour * 7) % 3))
    ];

    public IReadOnlyList<MonitorEvent> Events()
    {
        var now = Tick(DateTime.Now);
        var today = DateTime.Today;
        var events = new List<MonitorEvent>
        {
            new(1, today.AddHours(2), "Backup completed", "db-1", "Backup", "Nightly backup, 18.4 GB, 11 minutes"),
            new(2, today.AddDays(-1).AddHours(14.5), "Deployed version 2.14.0", "api-1", "Deployment", "Release notes in the wiki"),
            new(3, today.AddDays(-1).AddHours(2), "Backup completed", "db-1", "Backup", "Nightly backup, 18.3 GB, 12 minutes"),
            new(4, today.AddDays(-2).AddHours(9.25), "Disk above 75%", "db-1", "Alert", "Old log files were removed"),
        };

        // An alert for every reading in the last hour where a server went above the alert level.
        var first = now - (long)(TimeSpan.FromHours(1) / Interval);
        for (var s = 0; s < Fleet.Length; s++)
        {
            for (var tick = first; tick <= now; tick++)
            {
                if (Cpu(s, tick) >= AlertLevel && Cpu(s, tick - 1) < AlertLevel)
                {
                    events.Add(new(events.Count + 1, new DateTime(tick * Interval.Ticks), $"CPU above {AlertLevel:P0}",
                        Fleet[s].Name, "Alert", $"{Cpu(s, tick):P0} for at least {Interval.TotalSeconds:N0} seconds"));
                }
            }
        }

        return [.. events.OrderByDescending(e => e.Time)];
    }

    // A slow wave per server, plus noise, and every 15 minutes a short spike on the API server.
    private static double Cpu(int server, long tick)
    {
        var wave = Math.Sin(tick / 9.0 + server * 1.7) * 0.12;
        var spike = server == 2 && tick % 180 is >= 0 and < 4 ? 0.3 : 0;
        return Math.Clamp(Fleet[server].Cpu + wave + Noise(server, tick) * 0.05 + spike, 0.02, 0.99);
    }

    // The same number between -1 and 1 for the same server and tick.
    private static double Noise(int server, long tick) =>
        new Random(unchecked((int)(tick * 397 ^ server * 7919))).NextDouble() * 2 - 1;
}
