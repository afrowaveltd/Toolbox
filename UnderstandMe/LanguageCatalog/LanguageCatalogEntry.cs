namespace Afrowave.Toolbox.UnderstandMe.LanguageCatalog;

/// <summary>A language as presented by a configurable language-catalog source.</summary>
/// <remarks>RTL is optional because a language-level hint cannot fully describe script-specific direction.</remarks>
public sealed record LanguageCatalogEntry
{
    /// <summary>Language or locale identifier, such as en, cs, or sr-Latn.</summary>
    public required string Code { get; init; }

    /// <summary>Display name in a catalog's reference language.</summary>
    public required string Name { get; init; }

    /// <summary>Language's own display name (endonym).</summary>
    public required string NativeName { get; init; }

    /// <summary>Optional legacy language-level RTL hint; null means unspecified.</summary>
    public bool? RightToLeft { get; init; }
}
