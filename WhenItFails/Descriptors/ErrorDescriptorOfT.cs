using System.Text.Json.Serialization;

namespace Afrowave.Toolbox.WhenItFails.Descriptors;

/// <summary>
/// Represents one concrete runtime error occurrence with a strongly typed attachment.
/// </summary>
/// <typeparam name="TAttachment">
/// Type of optional additional data attached to this error occurrence.
/// </typeparam>
public sealed class ErrorDescriptor<TAttachment> : ErrorDescriptor
{
   /// <summary>
   /// Gets or sets optional strongly typed additional data attached to this error occurrence.
   /// </summary>
   [JsonPropertyName("attachment")]
   public TAttachment? Attachment { get; set; }
}