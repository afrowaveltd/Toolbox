namespace Afrowave.Toolbox.ExchangeHouse.Markdown;

/// <summary>
/// Explicit disk-backed Markdown paging strategies. These are practical
/// defaults, not automatically detected hardware or measured benchmarks.
/// </summary>
public enum MarkdownPagingProfile
{
    /// <summary>Existing 40-line pages with a checkpoint every 16 pages.</summary>
    Balanced,

    /// <summary>Fewer replay seeks on rotational disks, using more index space.</summary>
    Hdd,

    /// <summary>Favor compact checkpoint indexes when random seeks are cheaper.</summary>
    Ssd,

    /// <summary>Use short source pages to reduce the per-operation memory bound.</summary>
    LowRam
}

/// <summary>
/// Editable snapshot of a preset's source-page and checkpoint policies.
/// Use a 'with' expression to override an individual limit when needed.
/// </summary>
public sealed record MarkdownPagingSettings
{
    /// <summary>Chosen preset; labeling only if individual values are overridden.</summary>
    public MarkdownPagingProfile Profile { get; init; } = MarkdownPagingProfile.Balanced;

    /// <summary>Physical lines in one decoded source page, 1..128.</summary>
    public int LinesPerPage { get; init; } = 40;

    /// <summary>Saved structural checkpoint interval, 1..256 source pages.</summary>
    public int CheckpointStride { get; init; } = 16;

    /// <summary>Maximum UTF-8 physical line length, 1..1 MiB bytes.</summary>
    public int MaxLineBytes { get; init; } = 64 * 1024;

    /// <summary>Maximum temporary byte-offset index length.</summary>
    public long MaxSourceIndexBytes { get; init; } = 256L * 1024 * 1024;

    /// <summary>Maximum temporary structural checkpoint index length.</summary>
    public long MaxCheckpointIndexBytes { get; init; } = 256L * 1024 * 1024;

    /// <summary>Validate source and structural index limits together.</summary>
    public void Validate()
    {
        if (!Enum.IsDefined(Profile))
            throw new ArgumentOutOfRangeException(nameof(Profile));
        if (CheckpointStride is < 1 or > 256)
            throw new ArgumentOutOfRangeException(nameof(CheckpointStride),
                "Checkpoint stride must be 1..256.");
        if (MaxCheckpointIndexBytes < 512)
            throw new ArgumentOutOfRangeException(nameof(MaxCheckpointIndexBytes));
        _ = CreateSourceOptions();
    }

    /// <summary>Translate source policy to the existing index options.</summary>
    public MarkdownDiskPageOptions CreateSourceOptions()
    {
        var options = new MarkdownDiskPageOptions
        {
            LinesPerPage = LinesPerPage,
            MaxLineBytes = MaxLineBytes,
            MaxIndexBytes = MaxSourceIndexBytes
        };
        options.Validate();
        return options;
    }
}

/// <summary>
/// Hardware-oriented, deterministic profile defaults. Never infer disk type
/// from a guest VM flag or change presets automatically while reading.
/// </summary>
public static class MarkdownPagingProfiles
{
    /// <summary>Build an independent, customizable settings snapshot.</summary>
    public static MarkdownPagingSettings For(MarkdownPagingProfile profile)
    {
        var settings = profile switch
        {
            MarkdownPagingProfile.Balanced => new MarkdownPagingSettings(),
            MarkdownPagingProfile.Hdd => new MarkdownPagingSettings
            {
                Profile = MarkdownPagingProfile.Hdd,
                LinesPerPage = 64,
                CheckpointStride = 1
            },
            MarkdownPagingProfile.Ssd => new MarkdownPagingSettings
            {
                Profile = MarkdownPagingProfile.Ssd,
                LinesPerPage = 64,
                CheckpointStride = 32
            },
            MarkdownPagingProfile.LowRam => new MarkdownPagingSettings
            {
                Profile = MarkdownPagingProfile.LowRam,
                LinesPerPage = 8,
                CheckpointStride = 16
            },
            _ => throw new ArgumentOutOfRangeException(nameof(profile))
        };
        settings.Validate();
        return settings;
    }
}
