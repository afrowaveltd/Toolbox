using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Validation;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

public sealed class CommandInputErrorTests
{
    [Fact]
    public void CreateResult_ReturnsSingleErrorWithSuppliedValues()
    {
        ErrorCatalogValidationResult result = CommandInputError.CreateResult(
            "MissingPath",
            "A path is required.",
            "command <path>");

        ErrorCatalogValidationIssue issue = Assert.Single(result.Issues);

        Assert.False(result.IsValid);
        Assert.Equal(ErrorCatalogValidationSeverity.Error, issue.Severity);
        Assert.Equal("MissingPath", issue.Code);
        Assert.Equal("A path is required.", issue.Message);
        Assert.Equal("command <path>", issue.Path);
    }
}
