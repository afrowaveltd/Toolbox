using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Bootstrap;
using Afrowave.Toolbox.WhenItFails.Configuration;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter;

/// <summary>
/// Initializes a project-local WhenItFails JSON workspace.
/// </summary>
internal sealed class WhenItFailsWorkspaceInitializer
{
   /// <summary>
   /// Ensures that the WhenItFails JSON workspace exists under the specified project root.
   /// </summary>
   /// <param name="projectRootPath">Project root path.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>Bootstrap response.</returns>
   public Task<Response<JsonsBootstrapPayload>> InitializeAsync(
      string projectRootPath,
      CancellationToken cancellationToken = default)
   {
      ArgumentException.ThrowIfNullOrWhiteSpace(projectRootPath);

      string fullProjectRootPath = Path.GetFullPath(projectRootPath);

      JsonsOptions options = new()
      {
         RootDirectory = Path.Combine(fullProjectRootPath, "Jsons"),
         PackageDirectoryName = "WhenItFails"
      };

      JsonsBootstrapper bootstrapper = new(
         new DefaultJsonsTemplateProvider());

      return bootstrapper.EnsureWorkspaceAsync(
         options,
         cancellationToken);
   }
}
