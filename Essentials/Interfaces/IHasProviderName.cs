using Afrowave.Toolbox.Essentials.ValueObjects;

namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that has a provider name.
/// </summary>
public interface IHasProviderName
{
   /// <summary>
   /// Gets the provider name associated with the object.
   /// </summary>
   ProviderName ProviderName { get; }
}