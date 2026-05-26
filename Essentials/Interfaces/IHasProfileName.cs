using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that has a profile name.
/// </summary>
public interface IHasProfileName
{
   /// <summary>
   /// Gets the profile name associated with the object.
   /// </summary>
   ProfileName ProfileName { get; }
}