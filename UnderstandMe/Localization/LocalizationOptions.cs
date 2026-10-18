namespace Afrowave.Toolbox.UnderstandMe.Localization;

/// <summary>Options for the standalone localization runtime.</summary>
public sealed class LocalizationOptions
{
    /// <summary>Source language and final fallback. Default: en.</summary>
    public string SourceCulture { get; init; } = "en";

    /// <summary>Register unknown source phrases as key=value. Default: true.</summary>
    public bool RegisterMissingSourceKeys { get; init; } = true;
}
