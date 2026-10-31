using System.Reflection;

namespace Afrowave.Toolbox.UnderstandMe.Configurator;

/// <summary>Stable, offline help topic registry; topic identity is independent of translated UI labels.</summary>
public sealed record ConfiguratorHelpTopic(string Id, string TitleKey);

/// <summary>Reads bundled English Markdown from this executable, even without an Afutrans host.</summary>
public static class ConfiguratorHelpCatalog
{
    private static readonly IReadOnlyList<ConfiguratorHelpTopic> BundledTopics =
    [
        new("getting-started", "help.topic.getting-started"),
        new("register-provider", "help.topic.register-provider"),
        new("configure-storage", "help.topic.configure-storage"),
        new("test-connections", "help.topic.test-connections"),
        new("validation-and-saving", "help.topic.validation-and-saving")
    ];

    /// <summary>Offline help topics identified by nonlocalized keys.</summary>
    public static IReadOnlyList<ConfiguratorHelpTopic> Topics => BundledTopics;

    /// <summary>Load a known topic from embedded resources; never follow an arbitrary caller-provided path.</summary>
    public static string ReadEnglish(string topicId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicId);
        var topic = BundledTopics.FirstOrDefault(
            item => item.Id.Equals(topicId, StringComparison.Ordinal));
        if (topic is null)
            throw new ArgumentException("Unknown help topic.", nameof(topicId));

        var assembly = typeof(ConfiguratorHelpCatalog).Assembly;
        using var stream = assembly.GetManifestResourceStream("Configurator.Help." + topic.Id + ".en.md")
            ?? throw new InvalidOperationException("Missing embedded help document: " + topic.Id);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
