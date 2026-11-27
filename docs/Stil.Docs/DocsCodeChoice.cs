using Microsoft.JSInterop;

namespace Stil.Docs;

/// <summary>
/// Whether the examples show their code as Razor or as HTML. The choice applies to every example
/// and is kept in local storage, so someone who uses Stil CSS without Blazor makes it once.
/// </summary>
public sealed class DocsCodeChoice(IJSRuntime js)
{
    private const string Key = "stil-docs-code";

    private bool _loaded;

    /// <summary>True when the examples show HTML.</summary>
    public bool Html { get; private set; }

    /// <summary>Raised when the choice changes, here or from storage.</summary>
    public event Action? Changed;

    /// <summary>Reads the stored choice once. Storage is in the browser, so this runs after the first render.</summary>
    public async Task LoadAsync()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        try
        {
            if (await js.InvokeAsync<string?>("localStorage.getItem", Key) == "html")
            {
                Html = true;
                Changed?.Invoke();
            }
        }
        catch (JSException)
        {
            // Storage can be blocked; Razor is fine.
        }
    }

    public async Task SetAsync(bool html)
    {
        if (Html == html)
        {
            return;
        }

        Html = html;
        Changed?.Invoke();
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", Key, html ? "html" : "razor");
        }
        catch (JSException)
        {
        }
    }
}
