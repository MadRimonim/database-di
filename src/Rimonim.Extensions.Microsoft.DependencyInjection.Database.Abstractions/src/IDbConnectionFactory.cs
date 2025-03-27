using System.Data.Common;
using JetBrains.Annotations;

namespace Rimonim.Database;

public interface IDbConnectionFactory
{
    [PublicAPI] DbConnection CreateConnection();
}

public interface IDbConnectionFactory<out T> : IDbConnectionFactory where T : DbConnection
{
    new T CreateConnection();

    DbConnection IDbConnectionFactory.CreateConnection() => CreateConnection();
}
