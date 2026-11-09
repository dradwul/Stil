using Stil.Blazor.Components;

namespace Stil.Blazor;

/// <summary>Shows toast notifications. Requires <c>&lt;StilToastHost /&gt;</c> in an interactive layout.</summary>
public interface IStilToastService
{
    /// <summary>
    /// Shows a toast. It closes after <paramref name="duration"/> (default
    /// <see cref="StilOptions.DefaultToastDuration"/>); <see cref="TimeSpan.Zero"/> keeps it open
    /// until the user closes it. With <paramref name="keep"/> (or <see cref="StilOptions.KeepToasts"/>)
    /// it is also kept in the <c>StilNotificationCenter</c>.
    /// </summary>
    StilToast Show(string message, string? title = null, StilVariant variant = StilVariant.Info, TimeSpan? duration = null, bool keep = false);

    StilToast Success(string message, string? title = null) => Show(message, title, StilVariant.Success);
    StilToast Error(string message, string? title = null) => Show(message, title, StilVariant.Danger);
    StilToast Warning(string message, string? title = null) => Show(message, title, StilVariant.Warning);
    StilToast Info(string message, string? title = null) => Show(message, title, StilVariant.Info);

    void Close(StilToast toast);

    /// <summary>The visible toasts, oldest first.</summary>
    IReadOnlyList<StilToast> Toasts { get; }

    /// <summary>Raised when a toast is shown or closed.</summary>
    event Action? ToastsChanged;
}

public sealed record StilToast(string Message, string? Title, StilVariant Variant, TimeSpan Duration)
{
    public string Id { get; } = StilIds.Next("toast");
}

internal sealed class StilToastService(StilOptions options, IStilNotificationService notifications) : IStilToastService
{
    private readonly List<StilToast> _toasts = [];

    public IReadOnlyList<StilToast> Toasts => _toasts;

    public event Action? ToastsChanged;

    public StilToast Show(string message, string? title = null, StilVariant variant = StilVariant.Info, TimeSpan? duration = null, bool keep = false)
    {
        var toast = new StilToast(message, title, variant, duration ?? options.DefaultToastDuration);
        _toasts.Add(toast);
        if (keep || options.KeepToasts)
        {
            notifications.Add(message, title, variant);
        }

        ToastsChanged?.Invoke();
        return toast;
    }

    public void Close(StilToast toast)
    {
        if (_toasts.Remove(toast))
        {
            ToastsChanged?.Invoke();
        }
    }
}
