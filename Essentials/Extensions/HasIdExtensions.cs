using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects implementing <see cref="IHasId{TId}"/>.
/// </summary>
public static class HasIdExtensions
{
    /// <summary>
    /// Determines whether the object has a non-null identifier.
    /// </summary>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <returns><c>true</c> if the identifier is not null; otherwise, <c>false</c>.</returns>
    public static bool HasId<TId>(this IHasId<TId> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Id is not null;
    }

    /// <summary>
    /// Determines whether the object has the specified identifier.
    /// </summary>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <param name="source">The source object.</param>
    /// <param name="id">The identifier to compare with.</param>
    /// <returns><c>true</c> if the identifier matches; otherwise, <c>false</c>.</returns>
    public static bool HasId<TId>(
       this IHasId<TId> source,
       TId id)
    {
        ArgumentNullException.ThrowIfNull(source);

        return EqualityComparer<TId>.Default.Equals(
           source.Id,
           id);
    }
}