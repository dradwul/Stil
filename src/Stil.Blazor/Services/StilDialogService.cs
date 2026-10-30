using Microsoft.AspNetCore.Components;
using Stil.Blazor.Components;

namespace Stil.Blazor;

/// <summary>Opens dialogs from code. Requires <c>&lt;StilDialogHost /&gt;</c> in an interactive layout.</summary>
public interface IStilDialogService
{
    /// <summary>Shows a confirmation dialog. Returns true when the user confirms.</summary>
    Task<bool> ConfirmAsync(string title, string message, StilConfirmOptions? options = null);

    /// <summary>
    /// Shows <typeparamref name="TComponent"/> in a dialog and returns the value it closes with
    /// (see <see cref="StilDialogReference.Close"/>), or null when the user dismisses it.
    /// </summary>
    Task<object?> OpenAsync<TComponent>(string title, IDictionary<string, object?>? parameters = null, StilDialogOptions? options = null)
        where TComponent : IComponent;

    /// <summary>The dialogs that are currently open, oldest first.</summary>
    IReadOnlyList<StilDialogReference> Dialogs { get; }

    /// <summary>Raised when a dialog opens or closes.</summary>
    event Action? DialogsChanged;
}

public sealed class StilDialogOptions
{
    public StilSize Size { get; set; }

    /// <summary>Shows the close button in the header. The default is true.</summary>
    public bool ShowClose { get; set; } = true;
}

public sealed class StilConfirmOptions
{
    /// <summary>The confirm button text. The default is the localized "OK".</summary>
    public string? ConfirmText { get; set; }
    /// <summary>The cancel button text. The default is the localized "Cancel".</summary>
    public string? CancelText { get; set; }

    /// <summary>The color of the confirm button, for example Danger for a delete.</summary>
    public StilVariant ConfirmVariant { get; set; } = StilVariant.Primary;
}

/// <summary>An open dialog. Content components receive it as a cascading parameter to close it.</summary>
public sealed class StilDialogReference
{
    private readonly TaskCompletionSource<object?> _result = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Action<StilDialogReference> _onClosed;

    internal StilDialogReference(string title, RenderFragment content, StilDialogOptions options, Action<StilDialogReference> onClosed)
    {
        Title = title;
        Content = content;
        Options = options;
        _onClosed = onClosed;
    }

    public string Id { get; } = StilIds.Next("dialog");
    public string Title { get; }
    public StilDialogOptions Options { get; }
    internal RenderFragment Content { get; }
    internal Task<object?> Result => _result.Task;

    /// <summary>Closes the dialog. <paramref name="result"/> is returned to the code that opened it.</summary>
    public void Close(object? result = null)
    {
        if (_result.TrySetResult(result))
        {
            _onClosed(this);
        }
    }
}

internal sealed class StilDialogService : IStilDialogService
{
    private readonly List<StilDialogReference> _dialogs = [];

    public IReadOnlyList<StilDialogReference> Dialogs => _dialogs;

    public event Action? DialogsChanged;

    public async Task<bool> ConfirmAsync(string title, string message, StilConfirmOptions? options = null)
    {
        var parameters = new Dictionary<string, object?>
        {
            [nameof(StilConfirmDialogContent.Message)] = message,
            [nameof(StilConfirmDialogContent.Options)] = options ?? new StilConfirmOptions(),
        };

        return await OpenAsync<StilConfirmDialogContent>(title, parameters, new StilDialogOptions { Size = StilSize.Small }) is true;
    }

    public Task<object?> OpenAsync<TComponent>(string title, IDictionary<string, object?>? parameters = null, StilDialogOptions? options = null)
        where TComponent : IComponent
    {
        RenderFragment content = builder =>
        {
            builder.OpenComponent<TComponent>(0);
            var sequence = 1;
            foreach (var (name, value) in parameters ?? new Dictionary<string, object?>())
            {
                builder.AddComponentParameter(sequence++, name, value);
            }

            builder.CloseComponent();
        };

        var dialog = new StilDialogReference(title, content, options ?? new StilDialogOptions(), Remove);
        _dialogs.Add(dialog);
        DialogsChanged?.Invoke();
        return dialog.Result;
    }

    private void Remove(StilDialogReference dialog)
    {
        _dialogs.Remove(dialog);
        DialogsChanged?.Invoke();
    }
}
