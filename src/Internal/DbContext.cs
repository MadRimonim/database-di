using Microsoft.Extensions.Configuration;
#if NET7_0_OR_GREATER
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Rimonim.Database.Internal;

internal sealed class DbContext<T> : IDbContext<T>
    where T : DbConnection, new()
{
    private readonly string _connectionString;

    public DbContext(IConfiguration configuration,
#if NET7_0_OR_GREATER
                     [ServiceKey]
#endif
                     string connectionName = "Default")
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(connectionName);

        _connectionString = configuration.GetConnectionString(connectionName)
                            ?? throw new InvalidOperationException("Connection String Not Found");
    }

    public T CreateConnection() => new() { ConnectionString = _connectionString };
}
