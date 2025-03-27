namespace Rimonim.Database;

public interface IDbContext
{
    [PublicAPI] DbConnection CreateConnection();
}

public interface IDbContext<out T> : IDbContext where T : DbConnection
{
    new T CreateConnection();

    DbConnection IDbContext.CreateConnection() => CreateConnection();
}
