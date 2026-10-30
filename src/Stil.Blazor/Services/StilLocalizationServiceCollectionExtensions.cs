using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Stil.Blazor;

namespace Microsoft.Extensions.DependencyInjection;

public static class StilLocalizationServiceCollectionExtensions
{
    /// <summary>
    /// Translates the app's own texts from JSON files through the standard
    /// <see cref="IStringLocalizer{T}"/>: <c>@inject IStringLocalizer&lt;Home&gt; L</c>, then
    /// <c>@L["Save"]</c>. Put the files in <c>Locales</c> (<c>sv.json</c> for shared texts,
    /// <c>Pages/Home.sv.json</c> for one component); the Stil package embeds them in the app.
    /// Keys under <c>Stil</c> in the shared files override Stil's own built-in texts.
    /// </summary>
    /// <remarks>This replaces the resx-based <see cref="IStringLocalizerFactory"/> of <c>AddLocalization()</c>.</remarks>
    public static IServiceCollection AddStilLocalization(this IServiceCollection services, Action<StilLocalizationOptions>? configure = null)
    {
        var options = new StilLocalizationOptions();
        configure?.Invoke(options);

        services.TryAddSingleton(_ => new StilJsonTextStore(options));
        services.Replace(ServiceDescriptor.Singleton<IStringLocalizerFactory>(sp =>
            new StilJsonStringLocalizerFactory(sp.GetRequiredService<StilJsonTextStore>(), sp.GetService<ILoggerFactory>())));
        services.TryAddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
        services.TryAddSingleton<IStilLocalizer>(StilLocalizer.Create);
        return services;
    }
}
