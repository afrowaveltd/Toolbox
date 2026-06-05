using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.WhenItFails.Interfaces;

/// <summary>
/// Provides bundled JSON template files used to initialize a project-local workspace.
/// </summary>
public interface IJsonsTemplateProvider
{
    /// <summary>
    /// Gets JSON template files for the specified options.
    /// </summary>
    /// <param name="options">JSON workspace options.</param>
    /// <returns>Template files that should exist in the project-local workspace.</returns>
    IReadOnlyList<JsonsTemplateFile> GetTemplateFiles(JsonsOptions options);
}