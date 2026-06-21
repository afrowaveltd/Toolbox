using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Enums;

namespace Afrowave.Toolbox.WhenItFails.Tests.Configuration;

public sealed class WhenItFailsOptionsTests
{
    [Fact]
    public void Constructor_ShouldCreateDefaultJsonsOptions()
    {
        WhenItFailsOptions options = new();

        Assert.NotNull(options.Jsons);
    }

    [Fact]
    public void Constructor_ShouldUseFlexibleInitializationModeByDefault()
    {
        WhenItFailsOptions options = new();

        Assert.Equal(
            ErrorCatalogInitializationMode.Flexible,
            options.InitializationMode);
    }

    [Fact]
    public void Constructor_ShouldNotHideRecoverableFailuresByExplicitOverride()
    {
        WhenItFailsOptions options = new();

        Assert.Null(options.HideRecoverableFailures);
    }

    [Fact]
    public void Jsons_ShouldUseJsonWorkspaceDefaults()
    {
        WhenItFailsOptions options = new();

        Assert.Equal(
            "Jsons",
            options.Jsons.RootDirectory);

        Assert.Equal(
            "WhenItFails",
            options.Jsons.PackageDirectoryName);

        Assert.Equal(
            Path.Combine("Jsons", "WhenItFails"),
            options.Jsons.PackageDirectoryPath);
    }

    [Fact]
    public void Properties_ShouldAcceptCustomConfiguration()
    {
        JsonsOptions jsonsOptions = new()
        {
            RootDirectory = "CustomJsons",
            PackageDirectoryName = "CustomErrors"
        };

        WhenItFailsOptions options = new()
        {
            Jsons = jsonsOptions,
            InitializationMode =
                ErrorCatalogInitializationMode.Strict,
            HideRecoverableFailures = true
        };

        Assert.Same(
            jsonsOptions,
            options.Jsons);

        Assert.Equal(
            ErrorCatalogInitializationMode.Strict,
            options.InitializationMode);

        Assert.True(
            options.HideRecoverableFailures);
    }

    [Theory]
    [InlineData(ErrorCatalogInitializationMode.Strict, 0)]
    [InlineData(ErrorCatalogInitializationMode.Flexible, 1)]
    public void InitializationMode_ShouldHaveStableNumericValue(
        ErrorCatalogInitializationMode mode,
        int expectedValue)
    {
        Assert.Equal(
            expectedValue,
            (int)mode);
    }
}