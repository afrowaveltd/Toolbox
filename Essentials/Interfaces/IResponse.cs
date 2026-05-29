namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents a common response contract.
/// </summary>
public interface IResponse : IResult
{
    /// <summary>
    /// Gets a value indicating whether the response contains a data payload.
    /// </summary>
    bool HasData { get; }
}