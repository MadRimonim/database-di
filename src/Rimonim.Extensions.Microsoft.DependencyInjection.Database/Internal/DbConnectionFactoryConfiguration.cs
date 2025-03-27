using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Rimonim.Database.Internal;

internal sealed class DbConnectionFactoryConfiguration<T>(IServiceCollection services)
    : IDbConnectionFactoryConfiguration
    where T : DbConnection
{
    public void TryRegisterAsNonTypedAlias()
        => services.TryAddTransient<IDbConnectionFactory>(s => s.GetRequiredService<IDbConnectionFactory<T>>());
}
