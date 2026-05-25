using Afrowave.Toolbox.Essentials.Guards;

namespace Afrowave.Toolbox.Essentials.ValueObjects;

/// <summary>
/// Represents a profile name, such as "default", "markdown-refine", or "live-chat-fast".
/// </summary>
public readonly record struct ProfileName
{
   /// <summary>
   /// Initializes a new instance of the <see cref="ProfileName"/> struct.
   /// </summary>
   /// <param name="value">The profile name value.</param>
   public ProfileName(string value)
   {
      Value = Guard.NotNullOrWhiteSpace(value, nameof(value)).Trim();
   }

   /// <summary>
   /// Gets the profile name value.
   /// </summary>
   public string Value { get; }

   /// <summary>
   /// Returns the profile name value.
   /// </summary>
   /// <returns>The profile name value.</returns>
   public override string ToString()
   {
      return Value;
   }

   /// <summary>
   /// Creates a profile name from a string value.
   /// </summary>
   /// <param name="value">The profile name value.</param>
   /// <returns>The created profile name.</returns>
   public static ProfileName From(string value)
   {
      return new ProfileName(value);
   }

   /// <summary>
   /// Converts a profile name to its string value.
   /// </summary>
   /// <param name="profileName">The profile name.</param>
   public static implicit operator string(ProfileName profileName)
   {
      return profileName.Value;
   }

   /// <summary>
   /// Converts a string value to a profile name.
   /// </summary>
   /// <param name="value">The profile name value.</param>
   public static explicit operator ProfileName(string value)
   {
      return new ProfileName(value);
   }
}