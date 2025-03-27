namespace Rimonim.Database;

/// <summary>
/// Specifies options for registering database contexts in the dependency injection container.
/// </summary>
[Flags]
[PublicAPI]
public enum DbRegistrationOptions
{
    /// <summary>
    /// Register only the typed IDbContext&lt;T&gt; as the default implementation.
    /// </summary>
    DefaultTyped = 1,

    /// <summary>
    /// Register the non-generic IDbContext interface, mapping to the default typed implementation.
    /// </summary>
    DefaultNonGeneric = 2,

#if !NET6_0
    /// <summary>
    /// Register the typed IDbContext&lt;T&gt; with a specific key.
    /// </summary>
    KeyedTyped = 4,

    /// <summary>
    /// Register the non-generic IDbContext interface with a specific key.
    /// </summary>
    KeyedNonGeneric = 8,
#endif

    /// <summary>
    /// Common combinations
    /// </summary>

    /// <summary>
    /// Register default IDbContext&lt;T&gt; and IDbContext
    /// </summary>
    DefaultBoth = DefaultTyped | DefaultNonGeneric,

#if !NET6_0
    /// <summary>
    /// Register keyed IDbContext&lt;T&gt; and IDbContext
    /// </summary>
    KeyedBoth = KeyedTyped | KeyedNonGeneric,

    /// <summary>
    /// Register both default and keyed IDbContext&lt;T&gt;
    /// </summary>
    KeyedAndDefaultTyped = DefaultTyped | KeyedTyped,

    /// <summary>
    /// Register both default and keyed IDbContext&lt;T&gt; and IDbContext
    /// </summary>
    KeyedAndDefaultBoth = DefaultTyped | DefaultNonGeneric | KeyedTyped | KeyedNonGeneric
#endif
}