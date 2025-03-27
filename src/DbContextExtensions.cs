using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rimonim.Database.Internal;

namespace Rimonim.Database;

public static class DbContextExtensions
{
    private const DbRegistrationOptions DefaultKeyedRegistrationOptions =
#if NET6_0
            DbRegistrationOptions.DefaultBoth;
#else
            DbRegistrationOptions.KeyedAndDefaultBoth;
#endif

    /// <summary>
    /// Adds a named database context to the service collection.
    /// </summary>
    /// <typeparam name="T">The type of DbConnection to use.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The name of the database context.</param>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="registrationOptions">Options for how to register the database context.</param>
    /// <returns>The service collection for chaining.</returns>
    [PublicAPI]
    public static IServiceCollection AddDbContext<T>(this IServiceCollection services,
                                                     string name,
                                                     ServiceLifetime lifetime = ServiceLifetime.Scoped,
                                                     DbRegistrationOptions registrationOptions
                                                         = DefaultKeyedRegistrationOptions)
        where T : DbConnection, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(name);

        services.Add(GetServiceDescriptors<T>(name, lifetime, registrationOptions));
        return services;
    }

    /// <summary>
    /// Adds a default database context to the service collection.
    /// </summary>
    /// <typeparam name="T">The type of DbConnection to use.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="registrationOptions">Options for how to register the database context.</param>
    /// <returns>The service collection for chaining.</returns>
    [PublicAPI]
    public static IServiceCollection AddDbContext<T>(this IServiceCollection services,
                                                     ServiceLifetime lifetime = ServiceLifetime.Scoped,
                                                     DbRegistrationOptions registrationOptions
                                                         = DbRegistrationOptions.DefaultBoth)
        where T : DbConnection, new()
    {
        ArgumentNullException.ThrowIfNull(services);

#if !NET6_0
        if ((registrationOptions & DbRegistrationOptions.KeyedBoth) != 0)
        {
            throw new ArgumentException("Cannot register keyed services without a key. Use the overload that accepts a name parameter.", nameof(registrationOptions));
        }
#endif

        services.Add(GetServiceDescriptors<T>(null, lifetime, registrationOptions));
        return services;
    }



    private static IEnumerable<ServiceDescriptor> GetServiceDescriptors<T>(string? name,
                                                                           ServiceLifetime lifetime,
                                                                           DbRegistrationOptions registrationOptions)
        where T : DbConnection, new()
    {

        (
            ServiceDescriptor initialDescriptor,
            Func<IServiceProvider, object> retriever,
            DbRegistrationOptions initialType
        ) = GetInitialServiceDescriptor<T>(name, lifetime, registrationOptions);

        yield return initialDescriptor;

        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (initialType) {
#if !NET6_0
            case DbRegistrationOptions.KeyedTyped:
                if ((registrationOptions & DbRegistrationOptions.DefaultTyped) != 0) {
                    yield return new ServiceDescriptor(typeof(IDbContext<T>), retriever, lifetime);
                }
                goto case DbRegistrationOptions.DefaultTyped;
#endif
            case DbRegistrationOptions.DefaultTyped:
#if !NET6_0
                if ((registrationOptions & DbRegistrationOptions.KeyedNonGeneric) != 0) {
                    yield return new ServiceDescriptor(typeof(IDbContext), name, (s, _) => retriever(s), lifetime);
                }

                goto case DbRegistrationOptions.KeyedNonGeneric;
            case DbRegistrationOptions.KeyedNonGeneric:
#endif
                if ((registrationOptions & DbRegistrationOptions.DefaultNonGeneric) != 0) {
                    yield return new ServiceDescriptor(typeof(IDbContext), retriever, lifetime);
                }

                break;
        }
    }

    private static (ServiceDescriptor descriptor, Func<IServiceProvider, object> retriever, DbRegistrationOptions type)
        GetInitialServiceDescriptor<T>(string? name, ServiceLifetime lifetime,
                                       DbRegistrationOptions registrationOptions)
        where T : DbConnection, new()
    {
#if !NET6_0
        if ((registrationOptions & DbRegistrationOptions.KeyedTyped) != 0)
        {
            return (
                new ServiceDescriptor(typeof(IDbContext<T>), name, typeof(DbContext<T>), lifetime),
                s => s.GetRequiredKeyedService<IDbContext<T>>(name),
                DbRegistrationOptions.KeyedTyped
            );
        }
#endif

        if ((registrationOptions & DbRegistrationOptions.DefaultTyped) != 0)
        {
            return (
                new ServiceDescriptor(typeof(IDbContext<T>), GetFactory<T>(name), lifetime),
                s => s.GetRequiredService<IDbContext<T>>(),
                DbRegistrationOptions.DefaultTyped
            );
        }

#if !NET6_0
        if ((registrationOptions & DbRegistrationOptions.KeyedNonGeneric) != 0)
        {
            return (
                new ServiceDescriptor(typeof(IDbContext), name, typeof(DbContext<T>), lifetime),
                s => s.GetRequiredKeyedService<IDbContext>(name),
                DbRegistrationOptions.KeyedNonGeneric
            );
        }
#endif

        if ((registrationOptions & DbRegistrationOptions.DefaultNonGeneric) != 0)
        {
            return (
                new ServiceDescriptor(typeof(IDbContext), GetFactory<T>(name), lifetime),
                // ReSharper disable once NullableWarningSuppressionIsUsed - retriever is never used
                null!,
                DbRegistrationOptions.DefaultNonGeneric
            );
        }

        throw new ArgumentException("No registration options were selected "
                                    + "or the options are not supported on this platform.",
                                    nameof(registrationOptions));
    }

    private static Func<IServiceProvider, object> GetFactory<T>(string? name) where T : DbConnection, new()
    {
        string realName = name ?? "Default";
        return s => new DbContext<T>(s.GetRequiredService<IConfiguration>(), realName);
    }
}
