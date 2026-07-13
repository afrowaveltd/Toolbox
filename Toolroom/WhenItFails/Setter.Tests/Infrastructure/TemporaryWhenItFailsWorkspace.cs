namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Infrastructure;

internal sealed class TemporaryWhenItFailsWorkspace : IDisposable
{
    private TemporaryWhenItFailsWorkspace(string projectRootPath)
    {
        ProjectRootPath = projectRootPath;
        WhenItFailsJsonsPath = Path.Combine(
            projectRootPath,
            "Jsons",
            "WhenItFails");
    }

    public string ProjectRootPath { get; }

    public string WhenItFailsJsonsPath { get; }

    public static async Task<TemporaryWhenItFailsWorkspace> CreateInitializedAsync()
    {
        TemporaryWhenItFailsWorkspace temporaryWorkspace = CreateEmpty();

        WhenItFailsWorkspaceInitializer initializer = new();
        var initializeResponse =
            await initializer.InitializeAsync(temporaryWorkspace.ProjectRootPath);

        Assert.True(initializeResponse.IsSuccess);

        return temporaryWorkspace;
    }

    public static TemporaryWhenItFailsWorkspace CreateEmpty()
    {
        string projectRootPath = Path.Combine(
            Path.GetTempPath(),
            "afrowave-whenitfails-setter-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(projectRootPath);

        return new TemporaryWhenItFailsWorkspace(projectRootPath);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(ProjectRootPath))
            {
                Directory.Delete(ProjectRootPath, recursive: true);
            }
        }
        catch
        {
            // Test cleanup should not hide the real test result.
        }
    }
}
