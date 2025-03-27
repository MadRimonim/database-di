#if NET8_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Rimonim.Database.Internal;

internal sealed class KeyedDbConnectionFactoryConfiguration<T>(string key, IServiceCollection services)
    : IKeyedDbConnectionFactoryConfiguration
    where T : DbConnection
{
    public void TryRegisterAsNonTypedAlias() => services.TryAddTransient<IDbConnectionFactory>(CreateFactoryAccessor());

    public void TryRegisterAsNonKeyedAlias() => services.TryAddTransient(CreateFactoryAccessor());

    public void TryRegisterAsKeyedNonTypedAlias()
        => services.TryAddKeyedTransient<IDbConnectionFactory>(key, ResolveKeyedFactory);

    private Func<IServiceProvider, IDbConnectionFactory<T>> CreateFactoryAccessor()
    {
        string myKey = key;
        return serviceProvider => serviceProvider.GetRequiredKeyedService<IDbConnectionFactory<T>>(myKey);
    }

    private static IDbConnectionFactory<T> ResolveKeyedFactory(IServiceProvider services, object? key)
        => services.GetRequiredKeyedService<IDbConnectionFactory<T>>(key);
}
#endif
