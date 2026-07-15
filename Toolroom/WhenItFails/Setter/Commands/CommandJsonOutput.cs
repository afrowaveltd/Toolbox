using System.Text.Json;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Writes stable JSON output for machine-readable Setter commands.
/// </summary>
internal static class CommandJsonOutput
{
    private const string SchemaVersion = "1.0";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Serializes one command result using the shared Setter JSON envelope.
    /// </summary>
    public static string Serialize<T>(string command, T data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        CommandJsonEnvelope<T> envelope = new(
            SchemaVersion: SchemaVersion,
            Command: command,
            Data: data);

        return JsonSerializer.Serialize(envelope, SerializerOptions);
    }

    /// <summary>
    /// Writes one command result using the shared Setter JSON envelope.
    /// </summary>
    public static void Write<T>(string command, T data)
    {
        Console.WriteLine(Serialize(command, data));
    }

    private sealed record CommandJsonEnvelope<T>(
        string SchemaVersion,
        string Command,
        T Data);
}
