namespace Afrowave.Toolbox.Essentials.Interfaces;

/// <summary>
/// Represents an object that has a status value.
/// </summary>
/// <typeparam name="TStatus">The status enum type.</typeparam>
public interface IHasStatus<out TStatus>
    where TStatus : struct, Enum
{
   /// <summary>
   /// Gets the status of the object.
   /// </summary>
   TStatus Status { get; }
}