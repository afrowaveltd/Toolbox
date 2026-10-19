namespace Afrowave.Toolbox.UnderstandMe.LanguageCatalog;

/// <summary>Optional editing capability; a read-only catalog need not implement this interface.</summary>
/// <remarks>Importing upstream data and resolving local overrides are separate future capabilities.</remarks>
public interface ILanguageCatalogEditor
{
    /// <summary>Creates or updates one language, without deleting unrelated languages or custom fields.</summary>
    /// <returns>True if a change was saved, false if the entry was identical.</returns>
    bool Upsert(LanguageCatalogEntry entry);

    /// <summary>Explicitly deletes one selected catalog entry.</summary>
    /// <returns>True if an entry was deleted.</returns>
    bool Remove(string code);
}
