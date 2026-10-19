namespace Afrowave.Toolbox.UnderstandMe.LanguageCatalog;

/// <summary>Reads language metadata independently of JSON, databases, or external catalogs.</summary>
public interface ILanguageCatalogReader
{
    /// <summary>Returns a detached, deterministic snapshot of known languages.</summary>
    IReadOnlyList<LanguageCatalogEntry> GetAll();

    /// <summary>Finds a language by identifier, or returns null if it is absent.</summary>
    LanguageCatalogEntry? Find(string code);
}
