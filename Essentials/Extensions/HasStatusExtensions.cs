using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for objects carrying a status value.
/// </summary>
public static class HasStatusExtensions
{
   /// <summary>
   /// Determines whether the object has the specified status.
   /// </summary>
   /// <typeparam name="TStatus">The status enum type.</typeparam>
   /// <param name="value">The object carrying a status value.</param>
   /// <param name="status">The status to compare with.</param>
   /// <returns><c>true</c> if the object has the specified status; otherwise, <c>false</c>.</returns>
   public static bool HasStatus<TStatus>(
       this IHasStatus<TStatus> value,
       TStatus status)
       where TStatus : struct, Enum
   {
      ArgumentNullException.ThrowIfNull(value);

      return EqualityComparer<TStatus>.Default.Equals(value.Status, status);
   }

   /// <summary>
   /// Determines whether the object has any of the specified statuses.
   /// </summary>
   /// <typeparam name="TStatus">The status enum type.</typeparam>
   /// <param name="value">The object carrying a status value.</param>
   /// <param name="statuses">The statuses to compare with.</param>
   /// <returns><c>true</c> if the object has any of the specified statuses; otherwise, <c>false</c>.</returns>
   public static bool HasAnyStatus<TStatus>(
       this IHasStatus<TStatus> value,
       params TStatus[] statuses)
       where TStatus : struct, Enum
   {
      ArgumentNullException.ThrowIfNull(value);
      ArgumentNullException.ThrowIfNull(statuses);

      return statuses.Contains(value.Status);
   }
}