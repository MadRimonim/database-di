#if NET8_0_OR_GREATER
namespace Rimonim.Database;

public interface IKeyedDbConnectionFactoryConfiguration : IDbConnectionFactoryConfiguration
{

    [PublicAPI] void TryRegisterAsNonKeyedAlias();

    [PublicAPI] void TryRegisterAsKeyedNonTypedAlias();
}
#endif
