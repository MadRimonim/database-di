namespace Rimonim.Database;

public interface IDbConnectionFactoryConfiguration
{
    [PublicAPI] void TryRegisterAsNonTypedAlias();
}
