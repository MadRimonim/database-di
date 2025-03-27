using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

namespace Rimonim.Database.Internal;

internal sealed class DbConnectionFactory<T>(Func<string, IServiceProvider, T> createConnection,
                                             IServiceProvider serviceProvider,
                                             IConfiguration configuration,
                                             string connectionName)
    : IDbConnectionFactory<T> where T : DbConnection
{
    private readonly IServiceProvider _serviceProvider = CheckNoNull(serviceProvider);
    private readonly IConfiguration _configuration = CheckNoNull(configuration);
    private readonly string _connectionName = CheckNoNull(connectionName);
    private readonly Func<string, IServiceProvider, T> _createConnection = CheckNoNull(createConnection);

    private string ConnectionString
        => _configuration.GetConnectionString(_connectionName)
           ?? throw new InvalidOperationException($"Connection string not found for" +
                                                  $" connection name: '{_connectionName}'");

    public T CreateConnection() => _createConnection(ConnectionString, _serviceProvider);

    private static TArg CheckNoNull<TArg>(TArg argument,
                                          [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(argument, paramName);
        return argument;
    }
}
