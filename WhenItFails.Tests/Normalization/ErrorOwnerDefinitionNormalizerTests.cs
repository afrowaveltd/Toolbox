using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Normalization;

namespace Afrowave.Toolbox.WhenItFails.Tests.Normalization;

public sealed class ErrorOwnerDefinitionNormalizerTests
{
    [Fact]
    public void Normalize_ShouldThrowArgumentNullException_WhenDefinitionIsNull()
    {
        ErrorOwnerDefinitionNormalizer normalizer = new();

        Assert.Throws<ArgumentNullException>(
            () => normalizer.Normalize(null!));
    }

    [Fact]
    public void Normalize_ShouldNormalizeBasicFields()
    {
        ErrorOwnerDefinitionNormalizer normalizer = new();

        ErrorOwnerDefinition definition = new()
        {
            Name = " afw ",
            DisplayName = " Afrowave ",
            Description = " Built-in Afrowave errors. ",
            CodeFrom = 0,
            CodeTo = 999999,
            IsBuiltIn = true,
            Aliases = ["afrowave", "AFROWAVE"],
            DefaultMappings =
            {
                ["web.httpStatusCode"] = " 500 ",
                ["support.owner"] = " platform "
            }
        };

        ErrorOwnerDefinition normalizedDefinition =
            normalizer.Normalize(definition);

        Assert.Equal("AFW", normalizedDefinition.Name);
        Assert.Equal("Afrowave", normalizedDefinition.DisplayName);
        Assert.Equal("Built-in Afrowave errors.", normalizedDefinition.Description);
        Assert.Equal(0, normalizedDefinition.CodeFrom);
        Assert.Equal(999999, normalizedDefinition.CodeTo);
        Assert.True(normalizedDefinition.IsBuiltIn);
        Assert.Equal(["AFROWAVE"], normalizedDefinition.Aliases);
        Assert.Equal("500", normalizedDefinition.DefaultMappings["WEB_HTTPSTATUSCODE"]);
        Assert.Equal("platform", normalizedDefinition.DefaultMappings["SUPPORT_OWNER"]);
    }


    [Fact]
    public void Normalize_ShouldCopyAliasesAndMappingsWithoutSharingMutableState()
    {
        ErrorOwnerDefinitionNormalizer normalizer = new();

        ErrorOwnerDefinition definition = new()
        {
            Aliases = ["afrowave"],
            DefaultMappings =
            {
                ["web.httpStatusCode"] = " 500 "
            }
        };

        ErrorOwnerDefinition normalizedDefinition =
            normalizer.Normalize(definition);

        Assert.NotSame(definition.Aliases, normalizedDefinition.Aliases);
        Assert.NotSame(definition.DefaultMappings, normalizedDefinition.DefaultMappings);

        normalizedDefinition.Aliases.Add("RUNTIME_ONLY");
        normalizedDefinition.DefaultMappings["WEB_HTTPSTATUSCODE"] = "503";
        normalizedDefinition.DefaultMappings["RUNTIME_ONLY"] = "true";

        Assert.Equal(["afrowave"], definition.Aliases);
        Assert.Equal(" 500 ", definition.DefaultMappings["web.httpStatusCode"]);
        Assert.False(definition.DefaultMappings.ContainsKey("RUNTIME_ONLY"));
    }

}
