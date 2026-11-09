using Microsoft.Extensions.DependencyInjection.Extensions;
using Stil.Blazor;

namespace Microsoft.Extensions.DependencyInjection;

public static class StilServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Stil dialog and toast services and the global options. Add
    /// <c>&lt;StilDialogHost /&gt;</c> and <c>&lt;StilToastHost /&gt;</c> to an interactive layout.
    /// The components' built-in texts follow the UI culture, with overrides from
    /// <see cref="StilOptions.Texts"/>.
    /// </summary>
    public static IServiceCollection AddStil(this IServiceCollection services, Action<StilOptions>? configure = null)
    {
        var options = new StilOptions();
        configure?.Invoke(options);
        options.Font?.Validate();

        services.TryAddSingleton(options);
        services.TryAddScoped<IStilDialogService, StilDialogService>();
        services.TryAddScoped<IStilNotificationService, StilNotificationService>();
        services.TryAddScoped<IStilToastService, StilToastService>();
        services.TryAddSingleton<IStilLocalizer>(StilLocalizer.Create);
        return services;
    }
}
