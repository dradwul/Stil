using Microsoft.AspNetCore.Components;

namespace Stil.Blazor;

/// <summary>
/// Supplies icons from another icon set to <c>StilIcon</c>. Register one with
/// <c>AddStil(o =&gt; o.IconProvider = …)</c>. Return null for a name the provider does not know,
/// and StilIcon falls back to Stil's built-in icons.
/// </summary>
public interface IStilIconProvider
{
    /// <summary>The icon markup for <paramref name="name"/>, or null when this provider has no such icon.</summary>
    RenderFragment? GetIcon(string name);
}

/// <summary>
/// Icons from a ligature font, such as Google's Material Symbols: the name is written as text in
/// an element with the font's class. Load the font's stylesheet yourself.
/// </summary>
/// <example>
/// <code>
/// // In App.razor: &lt;link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" /&gt;
/// builder.Services.AddStil(o =&gt; o.IconProvider = new StilLigatureIconProvider("material-symbols-outlined"));
/// // &lt;StilIcon Name="shopping_cart" /&gt;
/// </code>
/// </example>
public sealed class StilLigatureIconProvider(string cssClass, Func<string, bool>? hasIcon = null) : IStilIconProvider
{
    public RenderFragment? GetIcon(string name)
    {
        if (hasIcon is not null && !hasIcon(name))
        {
            return null;
        }

        return builder =>
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", cssClass);
            builder.AddContent(2, name);
            builder.CloseElement();
        };
    }
}
