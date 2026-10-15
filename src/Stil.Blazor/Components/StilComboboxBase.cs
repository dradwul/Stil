using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Stil.Blazor.Components;

/// <summary>
/// Shared search, list and keyboard logic of StilAutocomplete and StilMultiSelect, following the
/// WAI-ARIA combobox pattern: a text input with role="combobox" that controls a listbox. Focus
/// stays in the input; the active option is pointed to with aria-activedescendant.
/// </summary>
public abstract class StilComboboxBase<TItem, TValue> : StilInputBase<TValue>
{
    private CancellationTokenSource? _searchCts;
    private readonly string _listId = StilIds.Next("listbox");

    /// <summary>The options, searched in memory by their text (case-insensitive "contains").</summary>
    [Parameter]
    public IEnumerable<TItem>? Items { get; set; }

    /// <summary>
    /// Searches for options, for example on a server. Called after typing pauses (Debounce) with
    /// the text and a token that is cancelled when the text changes again. Takes precedence over Items.
    /// </summary>
    [Parameter]
    public Func<string, CancellationToken, Task<IEnumerable<TItem>>>? Search { get; set; }

    /// <summary>The text of an option, also shown in the input. The default is ToString().</summary>
    [Parameter]
    public Func<TItem, string>? ItemText { get; set; }

    /// <summary>Custom content for an option in the list.</summary>
    [Parameter]
    public RenderFragment<TItem>? ItemTemplate { get; set; }

    /// <summary>
    /// Identifies an option, when options are new objects on every search (for example from a
    /// server). The default compares the items themselves.
    /// </summary>
    [Parameter]
    public Func<TItem, object>? ItemKey { get; set; }

    /// <summary>The number of characters typed before searching. The default is 1 with Search and 0 with Items.</summary>
    [Parameter]
    public int? MinLength { get; set; }

    /// <summary>Milliseconds to wait after typing before calling Search. The default is 250.</summary>
    [Parameter]
    public int Debounce { get; set; } = 250;

    /// <summary>The most options to show. The default is 50.</summary>
    [Parameter]
    public int MaxResults { get; set; } = 50;

    /// <summary>Text shown while the input is empty.</summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>Small, Medium (default) or Large.</summary>
    [Parameter]
    public StilSize Size { get; set; }

    /// <summary>The message when no option matches.</summary>
    [Parameter]
    public string NoResultsText { get; set; } = "No results";

    /// <summary>The message while Search is running.</summary>
    [Parameter]
    public string SearchingText { get; set; } = "Searching…";

    protected string Text { get; set; } = "";

    protected bool IsOpen { get; private set; }

    protected bool Searching { get; private set; }

    protected int ActiveIndex { get; private set; } = -1;

    protected IReadOnlyList<TItem> Results { get; private set; } = [];

    protected string ListId => $"{InputId}-{_listId}";

    protected string OptionId(int index) => $"{ListId}-{index}";

    protected string? ActiveDescendant => IsOpen && ActiveIndex >= 0 && ActiveIndex < Results.Count ? OptionId(ActiveIndex) : null;

    private int EffectiveMinLength => MinLength ?? (Search is null ? 0 : 1);

    /// <summary>Whether the list shows a "no results" or "searching" message.</summary>
    protected bool ShowMessage => Results.Count == 0 && (Searching || QueryText.Trim().Length >= EffectiveMinLength);

    protected string InputCss => $"stil-input {StilCssClasses.Size("stil-input", Size)}".TrimEnd();

    protected string TextOf(TItem? item) => item is null ? "" : ItemText?.Invoke(item) ?? item.ToString() ?? "";

    protected object KeyOf(TItem item) => ItemKey?.Invoke(item) ?? (object?)item ?? "";

    protected bool SameItem(TItem? a, TItem? b) =>
        a is null || b is null ? a is null && b is null : KeyOf(a).Equals(KeyOf(b));

    /// <summary>Selects an option (Enter or a click).</summary>
    protected abstract Task ChooseAsync(TItem item);

    /// <summary>Called when the user types, before searching.</summary>
    protected virtual Task OnTextChangedAsync() => Task.CompletedTask;

    /// <summary>Called when focus leaves the input.</summary>
    protected virtual Task OnClosedByBlurAsync() => Task.CompletedTask;

    /// <summary>Called for keys the base does not handle, for example Backspace.</summary>
    protected virtual Task OnOtherKeyAsync(KeyboardEventArgs e) => Task.CompletedTask;

    /// <summary>Notifies the binding and the EditContext of a new value.</summary>
    protected async Task SetValueAsync(TValue? value)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
        if (EditContext is not null && Field is { } field)
        {
            EditContext.NotifyFieldChanged(field);
        }
    }

    protected async Task OnInputAsync(ChangeEventArgs e)
    {
        Text = e.Value?.ToString() ?? "";
        await OnTextChangedAsync();
        await OpenAsync(debounce: true);
    }

    protected async Task OnInputClickAsync()
    {
        if (!IsOpen)
        {
            await OpenAsync(debounce: false);
        }
    }

    protected async Task OnKeyDownAsync(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowDown" when !IsOpen:
                await OpenAsync(debounce: false);
                ActiveIndex = Results.Count > 0 ? 0 : -1;
                break;
            case "ArrowDown":
                ActiveIndex = Results.Count == 0 ? -1 : (ActiveIndex + 1) % Results.Count;
                break;
            case "ArrowUp" when IsOpen:
                ActiveIndex = Results.Count == 0 ? -1 : (ActiveIndex - 1 + Results.Count) % Results.Count;
                break;
            case "Enter" when IsOpen && ActiveIndex >= 0 && ActiveIndex < Results.Count:
                await ChooseAsync(Results[ActiveIndex]);
                break;
            case "Escape" when IsOpen:
                Close();
                break;
            default:
                await OnOtherKeyAsync(e);
                break;
        }
    }

    protected async Task OnFocusOutAsync()
    {
        if (IsOpen)
        {
            Close();
        }

        await OnClosedByBlurAsync();
    }

    protected Task OnOptionClickAsync(TItem item) => ChooseAsync(item);

    protected void Close()
    {
        _searchCts?.Cancel();
        IsOpen = false;
        Searching = false;
        ActiveIndex = -1;
    }

    private async Task OpenAsync(bool debounce)
    {
        IsOpen = true;
        await UpdateResultsAsync(debounce);
    }

    /// <summary>The text to search for. The default is the typed text.</summary>
    protected virtual string QueryText => Text;

    /// <summary>Searches again for the current text.</summary>
    protected async Task UpdateResultsAsync(bool debounce)
    {
        _searchCts?.Cancel();
        var query = QueryText.Trim();

        if (query.Length < EffectiveMinLength)
        {
            Results = [];
            Searching = false;
            ActiveIndex = -1;
            return;
        }

        if (Search is null)
        {
            Results = (Items ?? [])
                .Where(item => query.Length == 0 || TextOf(item).Contains(query, StringComparison.CurrentCultureIgnoreCase))
                .Take(MaxResults)
                .ToList();
            ActiveIndex = Results.Count > 0 ? 0 : -1;
            return;
        }

        var cts = _searchCts = new CancellationTokenSource();
        Searching = true;
        StateHasChanged();

        try
        {
            if (debounce && Debounce > 0)
            {
                await Task.Delay(Debounce, cts.Token);
            }

            var found = await Search(query, cts.Token);
            if (cts.IsCancellationRequested)
            {
                return;
            }

            Results = found.Take(MaxResults).ToList();
            ActiveIndex = Results.Count > 0 ? 0 : -1;
            Searching = false;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
        }
    }

    protected override void DisposeField()
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();
    }
}
