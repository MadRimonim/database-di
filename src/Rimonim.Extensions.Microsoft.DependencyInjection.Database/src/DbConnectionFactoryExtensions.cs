using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rimonim.Database.Internal;

namespace Rimonim.Database;

public static class DbConnectionFactoryExtensions
{
    private const string DefaultConnectionName = "Default";

    [PublicAPI]
    public static IServiceCollection AddDbConnectionFactory<T>(this IServiceCollection services,
                                                               Func<string, IServiceProvider, T> factory,
                                                               string? name = null,
                                                               Action<IDbConnectionFactoryConfiguration>?
                                                                   configure = null,
                                                               ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where T : DbConnection
        => AddDbConnectionFactory<IDbConnectionFactory, T>(services, factory, name, configure, lifetime);

    [PublicAPI]
    public static IServiceCollection AddDbConnectionFactory<T>(this IServiceCollection services,
                                                               string? name = null,
                                                               Action<IDbConnectionFactoryConfiguration>?
                                                                   configure = null,
                                                               ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where T : DbConnection, new()
        => services.AddDbConnectionFactory(CreateConnection<T>, name, configure, lifetime);

    [PublicAPI]
    public static IServiceCollection AddTypedDbConnectionFactory<T>(this IServiceCollection services,
                                                                    Func<string, IServiceProvider, T> factory,
                                                                    string? name = null,
                                                                    Action<IDbConnectionFactoryConfiguration>?
                                                                        configure = null,
                                                                    ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where T : DbConnection
        => AddDbConnectionFactory<IDbConnectionFactory<T>, T>(services, factory, name, configure, lifetime);

    [PublicAPI]
    public static IServiceCollection AddTypedDbConnectionFactory<T>(this IServiceCollection services,
                                                                    string? name = null,
                                                                    Action<IDbConnectionFactoryConfiguration>?
                                                                        configure = null,
                                                                    ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where T : DbConnection, new()
        => services.AddTypedDbConnectionFactory(CreateConnection<T>, name, configure, lifetime);

    private static IServiceCollection
        AddDbConnectionFactory<TFactory, TConnection>(this IServiceCollection services,
                                                      Func<string, IServiceProvider, TConnection> factory,
                                                      string? name,
                                                      Action<IDbConnectionFactoryConfiguration>? configure,
                                                      ServiceLifetime lifetime)
        where TFactory : IDbConnectionFactory
        where TConnection : DbConnection
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);
        services.Add(new ServiceDescriptor(typeof(TFactory), CreateFactory, lifetime));

        configure?.Invoke(new DbConnectionFactoryConfiguration<TConnection>(services));

        return services;

        object CreateFactory(IServiceProvider serviceProvider)
            => new DbConnectionFactory<TConnection>(factory,
                                                    serviceProvider,
                                                    serviceProvider.GetRequiredService<IConfiguration>(),
                                                    name ?? DefaultConnectionName);
    }

#if NET8_0_OR_GREATER
    [PublicAPI]
    public static IServiceCollection AddKeyedDbConnectionFactory<T>(this IServiceCollection services,
                                                                    Func<string, IServiceProvider, T> factory,
                                                                    string key,
                                                                    Action<IKeyedDbConnectionFactoryConfiguration>?
                                                                        configure = null,
                                                                    ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where T : DbConnection
        => AddKeyedDbConnectionFactory<IDbConnectionFactory, T>(services, factory, key, configure, lifetime);

    [PublicAPI]
    public static IServiceCollection AddKeyedDbConnectionFactory<T>(this IServiceCollection services,
                                                                    string key,
                                                                    Action<IKeyedDbConnectionFactoryConfiguration>?
                                                                        configure = null,
                                                                    ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where T : DbConnection, new()
        => services.AddKeyedDbConnectionFactory(CreateConnection<T>, key, configure, lifetime);


    [PublicAPI]
    public static IServiceCollection AddKeyedTypedDbConnectionFactory<T>(this IServiceCollection services,
                                                                         Func<string, IServiceProvider, T> factory,
                                                                         string key,
                                                                         Action<IKeyedDbConnectionFactoryConfiguration>?
                                                                             configure = null,
                                                                         ServiceLifetime lifetime
                                                                             = ServiceLifetime.Scoped)
        where T : DbConnection
        => AddKeyedDbConnectionFactory<IDbConnectionFactory<T>, T>(services, factory, key, configure, lifetime);

    [PublicAPI]
    public static IServiceCollection AddKeyedTypedDbConnectionFactory<T>(this IServiceCollection services,
                                                                         string key,
                                                                         Action<IKeyedDbConnectionFactoryConfiguration>?
                                                                             configure = null,
                                                                         ServiceLifetime lifetime
                                                                             = ServiceLifetime.Scoped)
        where T : DbConnection, new()
        => services.AddKeyedTypedDbConnectionFactory(CreateConnection<T>, key, configure, lifetime);

    private static IServiceCollection
        AddKeyedDbConnectionFactory<TFactory, TConnection>(this IServiceCollection services,
                                                           Func<string, IServiceProvider, TConnection> factory,
                                                           string key,
                                                           Action<IKeyedDbConnectionFactoryConfiguration>? configure,
                                                           ServiceLifetime lifetime)
        where TFactory : IDbConnectionFactory
        where TConnection : DbConnection
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(key);

        services.Add(new ServiceDescriptor(typeof(TFactory), key, CreateFactory, lifetime));

        configure?.Invoke(new KeyedDbConnectionFactoryConfiguration<TConnection>(key, services));

        return services;

        object CreateFactory(IServiceProvider serviceProvider, object? createKey)
            => new DbConnectionFactory<TConnection>(factory,
                                                    serviceProvider,
                                                    serviceProvider.GetRequiredService<IConfiguration>(),
                                                    GetName(createKey));

        static string GetName(object? createKey)
            => createKey as string
               ?? throw new InvalidOperationException($"Invalid key for connection factory: '{createKey}'");
    }
#endif

    private static T CreateConnection<T>(string name, IServiceProvider _) where T : DbConnection, new()
        => new() { ConnectionString = name };
}
