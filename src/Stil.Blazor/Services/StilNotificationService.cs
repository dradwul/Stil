using Stil.Blazor.Components;

namespace Stil.Blazor;

/// <summary>A notification kept for later, in the <c>StilNotificationCenter</c>.</summary>
public sealed record StilNotification(string Message, string? Title, StilVariant Variant, DateTime Time)
{
    public string Id { get; } = StilIds.Next("notification");

    /// <summary>A page the notification opens when it is chosen.</summary>
    public string? Href { get; init; }

    /// <summary>True once the user has read it.</summary>
    public bool Read { get; internal set; }
}

/// <summary>
/// The notifications behind <c>StilNotificationCenter</c>, for the user's session (one per circuit).
/// Add them yourself, or show a toast with <c>keep: true</c> (or set <see cref="StilOptions.KeepToasts"/>)
/// so it is also kept here after it closes.
/// </summary>
public interface IStilNotificationService
{
    /// <summary>The notifications, newest first.</summary>
    IReadOnlyList<StilNotification> Notifications { get; }

    /// <summary>How many are unread.</summary>
    int UnreadCount { get; }

    StilNotification Add(string message, string? title = null, StilVariant variant = StilVariant.Info, string? href = null);

    void MarkRead(StilNotification notification);

    void MarkAllRead();

    void Remove(StilNotification notification);

    void Clear();

    /// <summary>Raised when a notification is added, read or removed.</summary>
    event Action? NotificationsChanged;
}

internal sealed class StilNotificationService(StilOptions options) : IStilNotificationService
{
    private readonly List<StilNotification> _notifications = [];

    public IReadOnlyList<StilNotification> Notifications => _notifications;

    public int UnreadCount => _notifications.Count(n => !n.Read);

    public event Action? NotificationsChanged;

    public StilNotification Add(string message, string? title = null, StilVariant variant = StilVariant.Info, string? href = null)
    {
        var notification = new StilNotification(message, title, variant, DateTime.Now) { Href = href };
        _notifications.Insert(0, notification);
        if (_notifications.Count > options.MaxNotifications)
        {
            _notifications.RemoveRange(options.MaxNotifications, _notifications.Count - options.MaxNotifications);
        }

        NotificationsChanged?.Invoke();
        return notification;
    }

    public void MarkRead(StilNotification notification)
    {
        if (!notification.Read && _notifications.Contains(notification))
        {
            notification.Read = true;
            NotificationsChanged?.Invoke();
        }
    }

    public void MarkAllRead()
    {
        var unread = _notifications.Where(n => !n.Read).ToList();
        unread.ForEach(n => n.Read = true);
        if (unread.Count > 0)
        {
            NotificationsChanged?.Invoke();
        }
    }

    public void Remove(StilNotification notification)
    {
        if (_notifications.Remove(notification))
        {
            NotificationsChanged?.Invoke();
        }
    }

    public void Clear()
    {
        _notifications.Clear();
        NotificationsChanged?.Invoke();
    }
}
