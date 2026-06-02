namespace Afrowave.Toolbox.Essentials.Guards;

/// <summary>
/// Provides lightweight guard methods for validating arguments.
/// </summary>
public static class Guard
{
   /// <summary>
   /// Ensures that a value is not null.
   /// </summary>
   /// <typeparam name="T">The value type.</typeparam>
   /// <param name="value">The value to check.</param>
   /// <param name="paramName">The parameter name.</param>
   /// <returns>The original value when it is not null.</returns>
   /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
   public static T NotNull<T>(T? value, string paramName)
       where T : class
   {
      return value ?? throw new ArgumentNullException(paramName);
   }

   /// <summary>
   /// Ensures that a string value is not null, empty, or whitespace.
   /// </summary>
   /// <param name="value">The string value to check.</param>
   /// <param name="paramName">The parameter name.</param>
   /// <returns>The original value when it is not null, empty, or whitespace.</returns>
   /// <exception cref="ArgumentException">Thrown when the value is null, empty, or whitespace.</exception>
   public static string NotNullOrWhiteSpace(string? value, string paramName)
   {
      if (string.IsNullOrWhiteSpace(value))
      {
         throw new ArgumentException("Value cannot be null, empty, or whitespace.", paramName);
      }

      return value;
   }

   /// <summary>
   /// Ensures that an integer value is not negative.
   /// </summary>
   /// <param name="value">The integer value to check.</param>
   /// <param name="paramName">The parameter name.</param>
   /// <returns>The original value when it is zero or positive.</returns>
   /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
   public static int NotNegative(int value, string paramName)
   {
      if (value < 0)
      {
         throw new ArgumentOutOfRangeException(paramName, value, "Value cannot be negative.");
      }

      return value;
   }

   /// <summary>
   /// Ensures that a long value is not negative.
   /// </summary>
   /// <param name="value">The long value to check.</param>
   /// <param name="paramName">The parameter name.</param>
   /// <returns>The original value when it is zero or positive.</returns>
   /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
   public static long NotNegative(long value, string paramName)
   {
      if (value < 0)
      {
         throw new ArgumentOutOfRangeException(paramName, value, "Value cannot be negative.");
      }

      return value;
   }

   /// <summary>
   /// Ensures that an integer value is greater than zero.
   /// </summary>
   /// <param name="value">The integer value to check.</param>
   /// <param name="paramName">The parameter name.</param>
   /// <returns>The original value when it is greater than zero.</returns>
   /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is zero or negative.</exception>
   public static int Positive(int value, string paramName)
   {
      if (value <= 0)
      {
         throw new ArgumentOutOfRangeException(paramName, value, "Value must be greater than zero.");
      }

      return value;
   }

   /// <summary>
   /// Ensures that a long value is greater than zero.
   /// </summary>
   /// <param name="value">The long value to check.</param>
   /// <param name="paramName">The parameter name.</param>
   /// <returns>The original value when it is greater than zero.</returns>
   /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is zero or negative.</exception>
   public static long Positive(long value, string paramName)
   {
      if (value <= 0)
      {
         throw new ArgumentOutOfRangeException(paramName, value, "Value must be greater than zero.");
      }

      return value;
   }

   /// <summary>
   /// Ensures that an integer value is within an inclusive range.
   /// </summary>
   /// <param name="value">The integer value to check.</param>
   /// <param name="minimum">The inclusive minimum value.</param>
   /// <param name="maximum">The inclusive maximum value.</param>
   /// <param name="paramName">The parameter name.</param>
   /// <returns>The original value when it is within the range.</returns>
   /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is outside the range.</exception>
   public static int InRange(int value, int minimum, int maximum, string paramName)
   {
      if (minimum > maximum)
      {
         throw new ArgumentException(
             "Minimum value cannot be greater than maximum value.",
             nameof(minimum));
      }

      if (value < minimum || value > maximum)
      {
         throw new ArgumentOutOfRangeException(
             paramName,
             value,
             $"Value must be between {minimum} and {maximum}.");
      }

      return value;
   }
}