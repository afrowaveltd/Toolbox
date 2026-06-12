using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.Toolroom.WhenItFails.Setter;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Loading;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Editing;

public sealed class WhenItFailsWorkspaceEditorTests
{
    private const string TestErrorId = "AFW_NET_0001";
    private const string OriginalTitle = "Network unavailable";
    private const string OriginalMessage = "The network is unavailable.";
    private const string OriginalDeveloperHint = "Check connectivity, DNS, firewall, proxy, VPN, and host availability.";
    private const string OriginalDocumentationKey = "when-it-fails/errors/network/network-unavailable";
    private const string OriginalSeverity = "Error";

    [Fact]
    public async Task SetErrorTitleAsync_ShouldChangeTitleAndCreateBackup()
    {
        using TemporaryWorkspace temporaryWorkspace = await TemporaryWorkspace.CreateAsync();

        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> response =
            await editor.SetErrorTitleAsync(
                temporaryWorkspace.ProjectRootPath,
                TestErrorId,
                "Network is not available");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal("Network is not available", response.Data.Title);

        ErrorDefinition savedErrorDefinition =
            await temporaryWorkspace.LoadErrorDefinitionAsync(TestErrorId);

        Assert.Equal("Network is not available", savedErrorDefinition.Title);
        AssertBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task SetErrorMessageAsync_ShouldChangeMessageAndCreateBackup()
    {
        using TemporaryWorkspace temporaryWorkspace = await TemporaryWorkspace.CreateAsync();

        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> response =
            await editor.SetErrorMessageAsync(
                temporaryWorkspace.ProjectRootPath,
                TestErrorId,
                "The network connection is not available.");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal("The network connection is not available.", response.Data.Message);

        ErrorDefinition savedErrorDefinition =
            await temporaryWorkspace.LoadErrorDefinitionAsync(TestErrorId);

        Assert.Equal("The network connection is not available.", savedErrorDefinition.Message);
        AssertBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task SetErrorDeveloperHintAsync_ShouldChangeDeveloperHintAndCreateBackup()
    {
        using TemporaryWorkspace temporaryWorkspace = await TemporaryWorkspace.CreateAsync();

        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> response =
            await editor.SetErrorDeveloperHintAsync(
                temporaryWorkspace.ProjectRootPath,
                TestErrorId,
                "Check DNS, proxy, VPN and firewall configuration.");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal("Check DNS, proxy, VPN and firewall configuration.", response.Data.DeveloperHint);

        ErrorDefinition savedErrorDefinition =
            await temporaryWorkspace.LoadErrorDefinitionAsync(TestErrorId);

        Assert.Equal("Check DNS, proxy, VPN and firewall configuration.", savedErrorDefinition.DeveloperHint);
        AssertBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task SetErrorSeverityAsync_ShouldChangeDefaultSeverityAndCreateBackup()
    {
        using TemporaryWorkspace temporaryWorkspace = await TemporaryWorkspace.CreateAsync();

        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> response =
            await editor.SetErrorSeverityAsync(
                temporaryWorkspace.ProjectRootPath,
                TestErrorId,
                "warning");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal("Warning", response.Data.DefaultSeverity);

        ErrorDefinition savedErrorDefinition =
            await temporaryWorkspace.LoadErrorDefinitionAsync(TestErrorId);

        Assert.Equal("Warning", savedErrorDefinition.DefaultSeverity);
        AssertBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task SetErrorDocumentationKeyAsync_ShouldChangeDocumentationKeyAndCreateBackup()
    {
        using TemporaryWorkspace temporaryWorkspace = await TemporaryWorkspace.CreateAsync();

        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> response =
            await editor.SetErrorDocumentationKeyAsync(
                temporaryWorkspace.ProjectRootPath,
                TestErrorId,
                "when-it-fails/errors/network/network-unavailable-v2");

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Data);
        Assert.Equal(
            "when-it-fails/errors/network/network-unavailable-v2",
            response.Data.DocumentationKey);

        ErrorDefinition savedErrorDefinition =
            await temporaryWorkspace.LoadErrorDefinitionAsync(TestErrorId);

        Assert.Equal(
            "when-it-fails/errors/network/network-unavailable-v2",
            savedErrorDefinition.DocumentationKey);

        AssertBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task SetErrorSeverityAsync_ShouldReturnInvalid_WhenSeverityIsUnsupported()
    {
        using TemporaryWorkspace temporaryWorkspace = await TemporaryWorkspace.CreateAsync();

        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> response =
            await editor.SetErrorSeverityAsync(
                temporaryWorkspace.ProjectRootPath,
                TestErrorId,
                "Banana");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "UnsupportedSeverity");

        ErrorDefinition savedErrorDefinition =
            await temporaryWorkspace.LoadErrorDefinitionAsync(TestErrorId);

        Assert.Equal(OriginalSeverity, savedErrorDefinition.DefaultSeverity);
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task SetErrorTitleAsync_ShouldReturnInvalid_WhenTitleIsEmpty()
    {
        using TemporaryWorkspace temporaryWorkspace = await TemporaryWorkspace.CreateAsync();

        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> response =
            await editor.SetErrorTitleAsync(
                temporaryWorkspace.ProjectRootPath,
                TestErrorId,
                "   ");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "TitleIsEmpty");

        ErrorDefinition savedErrorDefinition =
            await temporaryWorkspace.LoadErrorDefinitionAsync(TestErrorId);

        Assert.Equal(OriginalTitle, savedErrorDefinition.Title);
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task SetErrorMessageAsync_ShouldReturnInvalid_WhenMessageIsEmpty()
    {
        using TemporaryWorkspace temporaryWorkspace = await TemporaryWorkspace.CreateAsync();

        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> response =
            await editor.SetErrorMessageAsync(
                temporaryWorkspace.ProjectRootPath,
                TestErrorId,
                "   ");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "MessageIsEmpty");

        ErrorDefinition savedErrorDefinition =
            await temporaryWorkspace.LoadErrorDefinitionAsync(TestErrorId);

        Assert.Equal(OriginalMessage, savedErrorDefinition.Message);
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task SetErrorDeveloperHintAsync_ShouldReturnInvalid_WhenDeveloperHintIsEmpty()
    {
        using TemporaryWorkspace temporaryWorkspace = await TemporaryWorkspace.CreateAsync();

        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> response =
            await editor.SetErrorDeveloperHintAsync(
                temporaryWorkspace.ProjectRootPath,
                TestErrorId,
                "   ");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "DeveloperHintIsEmpty");

        ErrorDefinition savedErrorDefinition =
            await temporaryWorkspace.LoadErrorDefinitionAsync(TestErrorId);

        Assert.Equal(OriginalDeveloperHint, savedErrorDefinition.DeveloperHint);
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task SetErrorDocumentationKeyAsync_ShouldReturnInvalid_WhenDocumentationKeyIsEmpty()
    {
        using TemporaryWorkspace temporaryWorkspace = await TemporaryWorkspace.CreateAsync();

        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> response =
            await editor.SetErrorDocumentationKeyAsync(
                temporaryWorkspace.ProjectRootPath,
                TestErrorId,
                "   ");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "DocumentationKeyIsEmpty");

        ErrorDefinition savedErrorDefinition =
            await temporaryWorkspace.LoadErrorDefinitionAsync(TestErrorId);

        Assert.Equal(OriginalDocumentationKey, savedErrorDefinition.DocumentationKey);
        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    [Fact]
    public async Task SetErrorTitleAsync_ShouldReturnNotFound_WhenErrorDoesNotExist()
    {
        using TemporaryWorkspace temporaryWorkspace = await TemporaryWorkspace.CreateAsync();

        WhenItFailsWorkspaceEditor editor = new();

        Response<ErrorDefinition> response =
            await editor.SetErrorTitleAsync(
                temporaryWorkspace.ProjectRootPath,
                "AFW_UNKNOWN_9999",
                "Unknown title");

        Assert.False(response.IsSuccess);
        Assert.Contains(
            response.Issues,
            issue => issue.Code == "ErrorDefinitionNotFound");

        AssertNoBackupWasCreated(temporaryWorkspace.WhenItFailsJsonsPath);
    }

    private static void AssertBackupWasCreated(string whenItFailsJsonsPath)
    {
        string[] backupFiles =
            Directory.GetFiles(
                whenItFailsJsonsPath,
                "errors.en.*.bak.json",
                SearchOption.TopDirectoryOnly);

        Assert.NotEmpty(backupFiles);
    }

    private static void AssertNoBackupWasCreated(string whenItFailsJsonsPath)
    {
        string[] backupFiles =
            Directory.GetFiles(
                whenItFailsJsonsPath,
                "errors.en.*.bak.json",
                SearchOption.TopDirectoryOnly);

        Assert.Empty(backupFiles);
    }

    private sealed class TemporaryWorkspace : IDisposable
    {
        private TemporaryWorkspace(string projectRootPath)
        {
            ProjectRootPath = projectRootPath;
            WhenItFailsJsonsPath = Path.Combine(
                projectRootPath,
                "Jsons",
                "WhenItFails");
        }

        public string ProjectRootPath { get; }

        public string WhenItFailsJsonsPath { get; }

        public static async Task<TemporaryWorkspace> CreateAsync()
        {
            string projectRootPath = Path.Combine(
                Path.GetTempPath(),
                "afrowave-whenitfails-setter-tests",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(projectRootPath);

            TemporaryWorkspace temporaryWorkspace = new(projectRootPath);

            WhenItFailsWorkspaceInitializer initializer = new();

            var initializeResponse =
                await initializer.InitializeAsync(projectRootPath);

            Assert.True(initializeResponse.IsSuccess);

            return temporaryWorkspace;
        }

        public async Task<ErrorDefinition> LoadErrorDefinitionAsync(string errorId)
        {
            string errorCatalogFilePath = Path.Combine(
                WhenItFailsJsonsPath,
                "errors.en.json");

            JsonErrorCatalogLoader loader = new();

            Response<ErrorCatalogDocument> loadResponse =
                await loader.LoadFromFileAsync(errorCatalogFilePath);

            Assert.True(loadResponse.IsSuccess);
            Assert.NotNull(loadResponse.Data);

            ErrorCatalogDocument normalizedDocument =
                new ErrorCatalogDocumentNormalizer().Normalize(loadResponse.Data);

            ErrorDefinition? errorDefinition =
                normalizedDocument.Errors.FirstOrDefault(error =>
                    string.Equals(
                        error.Id,
                        errorId,
                        StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(errorDefinition);

            return errorDefinition;
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(ProjectRootPath))
                {
                    Directory.Delete(
                        ProjectRootPath,
                        recursive: true);
                }
            }
            catch
            {
                // Test cleanup should not hide the real test result.
            }
        }
    }
}
