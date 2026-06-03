using System.Text.Json.Serialization;

namespace Afrowave.Toolbox.WhenItFails.Descriptors;

/// <summary>
/// Represents one concrete runtime error occurrence with a strongly typed attachment.
/// </summary>
/// <typeparam name="TAttachment">
/// Type of optional additional data attached to this error occurrence.
/// </typeparam>
/// <remarks>
/// Use this type when an error needs to carry additional structured data,
/// for example validation details, configuration key information, HTTP response data,
/// database command information, or any other project-specific diagnostic payload.
/// </remarks>
public sealed class ErrorDescriptor<TAttachment> : ErrorDescriptor
{
   /// <summary>
   /// Gets or sets optional strongly typed additional data attached to this error occurrence.
   /// </summary>
   [JsonPropertyName("attachment")]
   public TAttachment? Attachment { get; set; }
}