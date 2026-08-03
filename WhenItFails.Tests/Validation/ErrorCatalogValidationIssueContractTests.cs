using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.WhenItFails.Tests.Validation;

public sealed class ErrorCatalogValidationIssueContractTests
{
    [Fact]
    public void NewIssue_ShouldExposeSafeDefaults()
    {
        ErrorCatalogValidationIssue issue = new();

        Assert.Equal(
            ErrorCatalogValidationSeverity.Error,
            issue.Severity);

        Assert.Equal(string.Empty, issue.Code);
        Assert.Equal(string.Empty, issue.Message);
        Assert.Null(issue.ErrorId);
        Assert.Null(issue.ErrorName);
        Assert.Null(issue.Path);
    }

    [Fact]
    public void Issue_ShouldPreserveAssignedValues()
    {
        ErrorCatalogValidationIssue issue = new()
        {
            Severity = ErrorCatalogValidationSeverity.Warning,
            Code = "UnknownCategory",
            Message = "The category is not defined.",
            ErrorId = "AFW_CFG_0001",
            ErrorName = "MissingConfigurationValue",
            Path = "errors[0].categories[0]"
        };

        Assert.Equal(
            ErrorCatalogValidationSeverity.Warning,
            issue.Severity);

        Assert.Equal("UnknownCategory", issue.Code);
        Assert.Equal(
            "The category is not defined.",
            issue.Message);

        Assert.Equal("AFW_CFG_0001", issue.ErrorId);
        Assert.Equal(
            "MissingConfigurationValue",
            issue.ErrorName);

        Assert.Equal(
            "errors[0].categories[0]",
            issue.Path);
    }
}
