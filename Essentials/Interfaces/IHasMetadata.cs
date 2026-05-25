using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that carries metadata.
/// </summary>
public interface IHasMetadata
{
   /// <summary>
   /// Gets metadata associated with the object.
   /// </summary>
   MetadataBag Metadata { get; }
}