using System.Reflection;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class ConfigurationPublicApiContractTests
{
    [Fact]
    public void ConfigurationModels_PreservePublishedPropertyShape()
    {
        Type optionsType = typeof(WhenItFailsOptions);
        Type jsonsType = typeof(JsonsOptions);

        Assert.True(optionsType.IsPublic && optionsType.IsSealed);
        Assert.True(jsonsType.IsPublic && jsonsType.IsSealed);
        Assert.Equal(
            "Afrowave.Toolbox.WhenItFails.Configuration",
            optionsType.Namespace);
        Assert.Equal(optionsType.Namespace, jsonsType.Namespace);

        AssertPublicParameterlessConstructor(optionsType);
        AssertPublicParameterlessConstructor(jsonsType);

        AssertExactPropertySet(
            optionsType,
            ("Jsons", typeof(JsonsOptions), true),
            ("InitializationMode", typeof(ErrorCatalogInitializationMode), true),
            ("HideRecoverableFailures", typeof(bool?), true));

        AssertExactPropertySet(
            jsonsType,
            ("RootDirectory", typeof(string), true),
            ("PackageDirectoryName", typeof(string), true),
            ("ErrorCatalogFileName", typeof(string), true),
            ("CategoryCatalogFileName", typeof(string), true),
            ("CodeGroupCatalogFileName", typeof(string), true),
            ("OwnerCatalogFileName", typeof(string), true),
            ("ProfilesFileName", typeof(string), true),
            ("PackageDirectoryPath", typeof(string), false),
            ("ErrorCatalogFilePath", typeof(string), false),
            ("CategoryCatalogFilePath", typeof(string), false),
            ("CodeGroupCatalogFilePath", typeof(string), false),
            ("OwnerCatalogFilePath", typeof(string), false),
            ("ProfilesFilePath", typeof(string), false));
    }

    [Fact]
    public void ConfigurationModels_PreserveDefaultsAndIndependentNestedOptions()
    {
        WhenItFailsOptions first = new();
        WhenItFailsOptions second = new();

        Assert.Equal(
            ErrorCatalogInitializationMode.Flexible,
            first.InitializationMode);
        Assert.Null(first.HideRecoverableFailures);
        Assert.NotNull(first.Jsons);
        Assert.NotSame(first.Jsons, second.Jsons);

        JsonsOptions jsons = first.Jsons;

        Assert.Equal("Jsons", jsons.RootDirectory);
        Assert.Equal("WhenItFails", jsons.PackageDirectoryName);
        Assert.Equal("errors.en.json", jsons.ErrorCatalogFileName);
        Assert.Equal("categories.en.json", jsons.CategoryCatalogFileName);
        Assert.Equal("code-groups.en.json", jsons.CodeGroupCatalogFileName);
        Assert.Equal("owners.en.json", jsons.OwnerCatalogFileName);
        Assert.Equal("profiles.json", jsons.ProfilesFileName);

        Assert.Equal(
            Path.Combine("Jsons", "WhenItFails"),
            jsons.PackageDirectoryPath);
        Assert.Equal(
            Path.Combine("Jsons", "WhenItFails", "errors.en.json"),
            jsons.ErrorCatalogFilePath);
    }

    [Fact]
    public void JsonsOptions_ComputedPathsFollowCurrentConfigAndPlatformSeparators()
    {
        JsonsOptions jsons = new()
        {
            RootDirectory = "project-root",
            PackageDirectoryName = "catalogs",
            ErrorCatalogFileName = "errors.json",
            CategoryCatalogFileName = "categories.json",
            CodeGroupCatalogFileName = "groups.json",
            OwnerCatalogFileName = "owners.json",
            ProfilesFileName = "profiles.json"
        };

        AssertAllPaths(jsons, "project-root", "catalogs");

        jsons.RootDirectory = "second-root";
        jsons.PackageDirectoryName = "nested";
        jsons.ErrorCatalogFileName = "second-errors.json";
        jsons.CategoryCatalogFileName = "second-categories.json";
        jsons.CodeGroupCatalogFileName = "second-groups.json";
        jsons.OwnerCatalogFileName = "second-owners.json";
        jsons.ProfilesFileName = "second-profiles.json";

        AssertAllPaths(jsons, "second-root", "nested");
    }

    [Fact]
    public void AddWhenItFails_StoresIndependentSnapshotOfAllConfigurationFields()
    {
        WhenItFailsOptions source = new()
        {
            InitializationMode = ErrorCatalogInitializationMode.Strict,
            HideRecoverableFailures = true,
            Jsons = new JsonsOptions
            {
                RootDirectory = "initial-root",
                PackageDirectoryName = "initial-package",
                ErrorCatalogFileName = "errors-first.json",
                CategoryCatalogFileName = "categories-first.json",
                CodeGroupCatalogFileName = "groups-first.json",
                OwnerCatalogFileName = "owners-first.json",
                ProfilesFileName = "profiles-first.json"
            }
        };

        ServiceCollection services = new();
        services.AddWhenItFails(source);

        source.InitializationMode = ErrorCatalogInitializationMode.Flexible;
        source.HideRecoverableFailures = false;
        source.Jsons.RootDirectory = "changed-root";
        source.Jsons.PackageDirectoryName = "changed-package";
        source.Jsons.ErrorCatalogFileName = "errors-changed.json";
        source.Jsons.CategoryCatalogFileName = "categories-changed.json";
        source.Jsons.CodeGroupCatalogFileName = "groups-changed.json";
        source.Jsons.OwnerCatalogFileName = "owners-changed.json";
        source.Jsons.ProfilesFileName = "profiles-changed.json";

        using ServiceProvider provider = services.BuildServiceProvider();
        WhenItFailsOptions registered =
            provider.GetRequiredService<WhenItFailsOptions>();

        Assert.NotSame(source, registered);
        Assert.NotSame(source.Jsons, registered.Jsons);
        Assert.Equal(
            ErrorCatalogInitializationMode.Strict,
            registered.InitializationMode);
        Assert.True(registered.HideRecoverableFailures);

        JsonsOptions snapshot = registered.Jsons;
        Assert.Equal("initial-root", snapshot.RootDirectory);
        Assert.Equal("initial-package", snapshot.PackageDirectoryName);
        Assert.Equal("errors-first.json", snapshot.ErrorCatalogFileName);
        Assert.Equal("categories-first.json", snapshot.CategoryCatalogFileName);
        Assert.Equal("groups-first.json", snapshot.CodeGroupCatalogFileName);
        Assert.Equal("owners-first.json", snapshot.OwnerCatalogFileName);
        Assert.Equal("profiles-first.json", snapshot.ProfilesFileName);
        AssertAllPaths(snapshot, "initial-root", "initial-package");
    }

    private static void AssertAllPaths(
        JsonsOptions jsons,
        string rootDirectory,
        string packageDirectoryName)
    {
        string packagePath = Path.Combine(
            rootDirectory,
            packageDirectoryName);

        Assert.Equal(packagePath, jsons.PackageDirectoryPath);
        Assert.Equal(
            Path.Combine(packagePath, jsons.ErrorCatalogFileName),
            jsons.ErrorCatalogFilePath);
        Assert.Equal(
            Path.Combine(packagePath, jsons.CategoryCatalogFileName),
            jsons.CategoryCatalogFilePath);
        Assert.Equal(
            Path.Combine(packagePath, jsons.CodeGroupCatalogFileName),
            jsons.CodeGroupCatalogFilePath);
        Assert.Equal(
            Path.Combine(packagePath, jsons.OwnerCatalogFileName),
            jsons.OwnerCatalogFilePath);
        Assert.Equal(
            Path.Combine(packagePath, jsons.ProfilesFileName),
            jsons.ProfilesFilePath);
    }

    private static void AssertPublicParameterlessConstructor(Type type)
    {
        ConstructorInfo? constructor =
            type.GetConstructor(Type.EmptyTypes);

        Assert.NotNull(constructor);
        Assert.True(constructor.IsPublic);
    }

    private static void AssertExactPropertySet(
        Type type,
        params (string Name, Type PropertyType, bool HasSetter)[] expected)
    {
        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Public |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly);

        Assert.Equal(expected.Length, properties.Length);

        foreach (var (name, propertyType, hasSetter) in expected)
        {
            PropertyInfo? property = type.GetProperty(
                name,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            Assert.NotNull(property);
            Assert.Equal(propertyType, property.PropertyType);
            Assert.True(property.GetMethod?.IsPublic == true);

            if (hasSetter)
            {
                Assert.True(property.SetMethod?.IsPublic == true);
            }
            else
            {
                Assert.Null(property.SetMethod);
            }
        }
    }
}
