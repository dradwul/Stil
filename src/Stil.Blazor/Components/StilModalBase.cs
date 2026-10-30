using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Stil.Blazor.Components;

/// <summary>
/// Shared behaviour of StilDialog and StilDrawer: a native modal <c>&lt;dialog&gt;</c> opened and
/// closed through stil.js and kept in sync with the <see cref="Open"/> parameter. Focus is trapped,
/// Escape closes it and the page behind is inert. Needs an interactive render mode.
/// </summary>
public abstract class StilModalBase<TSelf> : StilComponentBase, IAsyncDisposable
    where TSelf : StilModalBase<TSelf>
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<TSelf>? _self;
    private bool _shown;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    /// <summary>The rendered &lt;dialog&gt; element.</summary>
    protected ElementReference Element;

    /// <summary>Whether it is open. Supports two-way binding (@bind-Open).</summary>
    [Parameter]
    public bool Open { get; set; }

    /// <summary>Raised when it opens or closes.</summary>
    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>The heading, also the accessible name.</summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>Shows a close button in the header. The default is true.</summary>
    [Parameter]
    public bool ShowClose { get; set; } = true;

    /// <summary>The accessible name of the close button.</summary>
    [Parameter]
    public string? CloseLabel { get; set; }

    /// <summary>Raised after it closes, whatever closed it (the close button, Escape or code).</summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>Actions in the footer, aligned to the end.</summary>
    [Parameter]
    public RenderFragment? Footer { get; set; }

    /// <summary>The body content.</summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected string TitleId { get; } = StilIds.Next("dialog-title");

    /// <summary>Closes it and updates the bound Open value.</summary>
    public async Task CloseAsync()
    {
        if (!Open)
        {
            return;
        }

        Open = false;
        await OpenChanged.InvokeAsync(false);
        await OnClose.InvokeAsync();
        StateHasChanged();
    }

    /// <summary>Called by stil.js when the browser closes the dialog, for example on Escape.</summary>
    [JSInvokable]
    public async Task OnDialogClosed()
    {
        _shown = false;
        await CloseAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Open == _shown)
        {
            return;
        }

        _module ??= await JS.InvokeAsync<IJSObjectReference>("import", "./_content/Stil/stil.js");
        if (Open)
        {
            _self ??= DotNetObjectReference.Create((TSelf)this);
            await _module.InvokeVoidAsync("showDialog", Element, _self);
        }
        else
        {
            await _module.InvokeVoidAsync("closeDialog", Element);
        }

        _shown = Open;
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // The circuit is gone; there is nothing to clean up in the browser.
            }
        }

        _self?.Dispose();
        GC.SuppressFinalize(this);
    }
}
