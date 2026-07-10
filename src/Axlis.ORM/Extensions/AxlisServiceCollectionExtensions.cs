using Axlis.ORM.Caching;
using Axlis.ORM.Core;
using Axlis.ORM.Diagnostics;
using Axlis.ORM.GraphQL;
using Axlis.ORM.GraphQL.Transport;
using Axlis.ORM.LazyLoader;
using Axlis.ORM.Results;
using Axlis.ORM.Services;
using Axlis.ORM.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PowerCSharp.Feature.Cache.Abstractions;
using PowerCSharp.Feature.Cache.Abstractions.NoOp;

namespace Axlis.ORM.Extensions;

/// <summary>
/// Extension methods for registering Axlis.ORM services with <see cref="IServiceCollection"/>.
/// </summary>
public static class AxlisORMServiceCollectionExtensions
{
    /// <summary>
    /// Registers core Axlis.ORM services: <see cref="SitecoreItemCacheManager"/>,
    /// <see cref="IItemLazyLoader"/>, <see cref="ISitecoreFacade"/>, and the default
    /// <see cref="IAxlisDiagnosticsSink"/>.
    /// <para>
    /// Requires <see cref="ICacheService"/> to already be registered (or uses the built-in
    /// <see cref="NoOpCacheService"/> if none is found). Call <see cref="AddAxlisORMGraphQL"/>
    /// to also register the data access layer.
    /// </para>
    /// <para>
    /// After building the <see cref="IServiceProvider"/>, call
    /// <see cref="UseAxlis(IServiceProvider)"/> to wire the ambient lazy-loader into
    /// <see cref="ExtendedItem"/>.
    /// </para>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional delegate to configure <see cref="AxlisOptions"/>.</param>
    public static IServiceCollection AddAxlisORM(
        this IServiceCollection services,
        Action<AxlisOptions>? configure = null)
    {
        var opts = new OptionsBuilder<AxlisOptions>(services, Options.DefaultName);
        if (configure != null) opts.Configure(configure);

        // Register a NoOpCacheService as the default. Consumers who want stampede-safe
        // in-memory caching should register a real ICacheService (e.g. via
        // AddBitFasterCache() from PowerCSharp.Feature.Cache.BitFaster) BEFORE calling AddAxlisORM().
        // ASP.NET Core DI resolves the first matching registration, so a provider registered
        // earlier in the pipeline takes precedence over this fallback.
        services.AddSingleton<ICacheService, NoOpCacheService>();

        services.AddSingleton<SitecoreItemCacheManager>();
        services.AddSingleton<IItemLazyLoader, SitecoreItemLazyLoader>();
        services.AddSingleton<IAxlisDiagnosticsSink, LoggerAxlisDiagnosticsSink>();
        services.AddSingleton<ISitecoreFacade, SitecoreFacade>();

        return services;
    }

    /// <summary>
    /// Registers the Axlis.ORM GraphQL data-access layer: <see cref="IGraphQLTransportFactory"/>,
    /// <see cref="ISitecoreService"/>, and the named <see cref="System.Net.Http.HttpClient"/>
    /// used by the default transport.
    /// <para>
    /// Call this <b>after</b> <see cref="AddAxlisORM"/> (or independently when only the
    /// service layer is needed without the facade).
    /// </para>
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional delegate to configure <see cref="AxlisGraphQLOptions"/>.</param>
    public static IServiceCollection AddAxlisORMGraphQL(
        this IServiceCollection services,
        Action<AxlisGraphQLOptions>? configure = null)
    {
        var opts = new OptionsBuilder<AxlisGraphQLOptions>(services, Options.DefaultName);
        if (configure != null) opts.Configure(configure);

        // Register the named HttpClient used by the default transport factory.
        // Base address and API key are applied from AxlisGraphQLOptions at runtime.
        services.AddHttpClient(HttpGraphQLTransportFactory.DefaultClientName,
            (sp, client) =>
            {
                var graphQLOpts = sp.GetRequiredService<IOptions<AxlisGraphQLOptions>>().Value;

                if (!string.IsNullOrEmpty(graphQLOpts.Endpoint))
                {
                    client.BaseAddress = new Uri(graphQLOpts.Endpoint);
                }

                if (!string.IsNullOrEmpty(graphQLOpts.ApiKey))
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        AxlisGraphQLOptions.ApiKeyHeaderName,
                        graphQLOpts.ApiKey);
                }

                client.Timeout = TimeSpan.FromSeconds(graphQLOpts.TimeoutSeconds);
            });

        services.AddSingleton<IGraphQLTransportFactory, HttpGraphQLTransportFactory>();
        services.AddSingleton<ISitecoreService, SitecoreService>();

        return services;
    }

    /// <summary>
    /// Wires the ambient <see cref="IItemLazyLoader"/> into <see cref="ExtendedItem"/>
    /// so that lazy tree-traversal (parent, children, siblings) works at runtime.
    /// Call this immediately after <c>app.Build()</c> or on the root <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="serviceProvider">The built service provider.</param>
    /// <returns>The same <paramref name="serviceProvider"/> for chaining.</returns>
    public static IServiceProvider UseAxlis(this IServiceProvider serviceProvider)
    {
        var lazyLoader = serviceProvider.GetRequiredService<IItemLazyLoader>();
        ExtendedItem.Initialize(lazyLoader);
        return serviceProvider;
    }
}
