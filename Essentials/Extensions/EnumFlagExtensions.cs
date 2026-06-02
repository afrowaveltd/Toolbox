namespace Afrowave.Toolbox.Essentials.Extensions;

/// <summary>
/// Provides helper methods for working with enum flags.
/// </summary>
public static class EnumFlagExtensions
{
   /// <summary>
   /// Determines whether all specified flags are present.
   /// </summary>
   /// <typeparam name="TEnum">The enum type.</typeparam>
   /// <param name="value">The enum value to check.</param>
   /// <param name="flags">The flags that must be present.</param>
   /// <returns><c>true</c> if all specified flags are present; otherwise, <c>false</c>.</returns>
   public static bool HasAll<TEnum>(this TEnum value, TEnum flags)
    where TEnum : struct, Enum
   {
      var left = Convert.ToInt64(value);
      var right = Convert.ToInt64(flags);

      return (left & right) == right;
   }


   /// <summary>
   /// Determines whether any of the specified flags are present.
   /// </summary>
   /// <typeparam name="TEnum">The enum type.</typeparam>
   /// <param name="value">The enum value to check.</param>
   /// <param name="flags">The flags to check.</param>
   /// <returns><c>true</c> if at least one specified flag is present; otherwise, <c>false</c>.</returns>
   public static bool HasAny<TEnum>(this TEnum value, TEnum flags)
    where TEnum : struct, Enum
   {
      var left = Convert.ToInt64(value);
      var right = Convert.ToInt64(flags);

      return (left & right) != 0;
   }

   /// <summary>
   /// Adds the specified flags to an enum value.
   /// </summary>
   /// <typeparam name="TEnum">The enum type.</typeparam>
   /// <param name="value">The original enum value.</param>
   /// <param name="flags">The flags to add.</param>
   /// <returns>The enum value with the specified flags added.</returns>
   public static TEnum With<TEnum>(this TEnum value, TEnum flags)
     where TEnum : struct, Enum
   {
      var left = Convert.ToInt64(value);
      var right = Convert.ToInt64(flags);

      return (TEnum)Enum.ToObject(typeof(TEnum), left | right);
   }

   /// <summary>
   /// Removes the specified flags from an enum value.
   /// </summary>
   /// <typeparam name="TEnum">The enum type.</typeparam>
   /// <param name="value">The original enum value.</param>
   /// <param name="flags">The flags to remove.</param>
   /// <returns>The enum value with the specified flags removed.</returns>
   public static TEnum Without<TEnum>(this TEnum value, TEnum flags)
    where TEnum : struct, Enum
   {
      var left = Convert.ToInt64(value);
      var right = Convert.ToInt64(flags);

      return (TEnum)Enum.ToObject(typeof(TEnum), left & ~right);
   }
}