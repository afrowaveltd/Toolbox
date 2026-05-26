using Afrowave.Toolbox.Essentials.Enums;

namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with <see cref="SortDirection"/> values.
/// </summary>
public static class SortDirectionExtensions
{
   /// <summary>
   /// Determines whether the sort direction is ascending.
   /// </summary>
   /// <param name="sortDirection">The sort direction.</param>
   /// <returns><c>true</c> if the sort direction is <see cref="SortDirection.Ascending"/>; otherwise, <c>false</c>.</returns>
   public static bool IsAscending(this SortDirection sortDirection)
   {
      return sortDirection == SortDirection.Ascending;
   }

   /// <summary>
   /// Determines whether the sort direction is descending.
   /// </summary>
   /// <param name="sortDirection">The sort direction.</param>
   /// <returns><c>true</c> if the sort direction is <see cref="SortDirection.Descending"/>; otherwise, <c>false</c>.</returns>
   public static bool IsDescending(this SortDirection sortDirection)
   {
      return sortDirection == SortDirection.Descending;
   }

   /// <summary>
   /// Determines whether the sort direction was specified.
   /// </summary>
   /// <param name="sortDirection">The sort direction.</param>
   /// <returns><c>true</c> if the sort direction is not <see cref="SortDirection.None"/>; otherwise, <c>false</c>.</returns>
   public static bool IsSpecified(this SortDirection sortDirection)
   {
      return sortDirection != SortDirection.None;
   }

   /// <summary>
   /// Reverses the sort direction.
   /// </summary>
   /// <param name="sortDirection">The sort direction.</param>
   /// <returns>
   /// <see cref="SortDirection.Descending"/> for <see cref="SortDirection.Ascending"/>,
   /// <see cref="SortDirection.Ascending"/> for <see cref="SortDirection.Descending"/>,
   /// otherwise <see cref="SortDirection.None"/>.
   /// </returns>
   public static SortDirection Reverse(this SortDirection sortDirection)
   {
      return sortDirection switch
      {
         SortDirection.Ascending => SortDirection.Descending,
         SortDirection.Descending => SortDirection.Ascending,
         _ => SortDirection.None
      };
   }
}