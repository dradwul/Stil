using Stil.Blazor;
using Stil.Blazor.Components;

namespace Stil.Demo;

/// <summary>A visitor expected today, with the code on their invitation or badge.</summary>
public sealed record Visitor(string Code, string Name, string Company, string Host);

/// <summary>An entry in the reception's log.</summary>
public sealed record DeskEntry(int Id, DateTime Time, string Title, string Actor, string Type, StilVariant? Variant = null, string? Icon = null, string? Details = null);

/// <summary>A chat message between the reception and the hosts.</summary>
public sealed record DeskMessage(int Id, DateTime Time, string Author, string Text);

/// <summary>
/// The check-in desk's shared state: one instance for the whole app, so every open window sees
/// the same log and chat. Changed tells the pages to redraw; with interactive server rendering
/// that is the transport between browser windows.
/// </summary>
public sealed class CheckInDesk
{
    private readonly Lock _lock = new();
    private readonly List<DeskEntry> _log = [];
    private readonly List<DeskMessage> _messages = [];
    private readonly Dictionary<string, DateTime> _typing = [];
    private int _next = 1;

    public CheckInDesk()
    {
        var today = DateTime.Today;
        Add(today.AddHours(7.5), "Desk opened", "Reception", "System", StilVariant.Info, StilIcons.Info);
        Add(today.AddHours(8.2), "Checked in", "Jonas Ek", "Visit", StilVariant.Success, StilIcons.Check, "Visiting Karin Dahl · Ek Bygg AB");
        _messages.Add(new(_next++, today.AddHours(8.25), "Karin Dahl", "Jonas is here for the 8:30, please send him up."));
        _messages.Add(new(_next++, today.AddHours(8.3), "Reception", "On his way."));
    }

    /// <summary>Raised after any change, from whichever window made it.</summary>
    public event Action? Changed;

    public IReadOnlyList<Visitor> Expected { get; } =
    [
        new("VIS-1042", "Maja Lind", "Nordvik Supply", "Karin Dahl"),
        new("VIS-1043", "Erik Holm", "Holm & Berg", "Oskar Lund"),
        new("VIS-1044", "Sara Berg", "Berglund Outdoor", "Karin Dahl"),
        new("VIS-1045", "Lina Ström", "Kallio Design", "Per Ahl"),
    ];

    public Visitor? Find(string code) =>
        Expected.FirstOrDefault(v => string.Equals(v.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<DeskEntry> Log
    {
        get { lock (_lock) { return [.. _log]; } }
    }

    public IReadOnlyList<DeskMessage> Messages
    {
        get { lock (_lock) { return [.. _messages]; } }
    }

    public bool IsCheckedIn(Visitor visitor)
    {
        lock (_lock)
        {
            return _log.LastOrDefault(e => e.Actor == visitor.Name && e.Type == "Visit") is { Title: "Checked in" };
        }
    }

    public void CheckIn(Visitor visitor, StilSignature signature)
    {
        var signed = signature.TypedName is { Length: > 0 } name ? $"signed as \"{name}\"" : "signed by hand";
        Add(DateTime.Now, "Checked in", visitor.Name, "Visit", StilVariant.Success, StilIcons.Check,
            $"Visiting {visitor.Host} · {visitor.Company} · {signed}");
        Send("Reception", $"{visitor.Name} from {visitor.Company} is here for {visitor.Host}.");
    }

    public void CheckOut(Visitor visitor) =>
        Add(DateTime.Now, "Checked out", visitor.Name, "Visit", null, null, $"Visited {visitor.Host}");

    public void Unknown(string code) =>
        Add(DateTime.Now, "Unknown code scanned", "Reception", "Alert", StilVariant.Warning, StilIcons.Warning, code);

    public void Send(string author, string text)
    {
        lock (_lock)
        {
            _messages.Add(new(_next++, DateTime.Now, author, text));
            _typing.Remove(author);
        }

        Changed?.Invoke();
    }

    public void Typing(string author)
    {
        lock (_lock)
        {
            _typing[author] = DateTime.UtcNow;
        }

        Changed?.Invoke();
    }

    /// <summary>Who else typed in the last four seconds.</summary>
    public IReadOnlyList<string> TypingOthers(string me)
    {
        lock (_lock)
        {
            return [.. _typing.Where(t => t.Key != me && DateTime.UtcNow - t.Value < TimeSpan.FromSeconds(4)).Select(t => t.Key)];
        }
    }

    private void Add(DateTime time, string title, string actor, string type, StilVariant? variant = null, string? icon = null, string? details = null)
    {
        lock (_lock)
        {
            _log.Add(new(_next++, time, title, actor, type, variant, icon, details));
        }

        Changed?.Invoke();
    }
}
