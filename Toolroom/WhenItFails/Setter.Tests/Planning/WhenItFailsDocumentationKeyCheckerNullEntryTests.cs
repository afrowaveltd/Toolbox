using Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Planning;
using Afrowave.Toolbox.WhenItFails.Definitions;

namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Tests.Planning;

public sealed class WhenItFailsDocumentationKeyCheckerNullEntryTests
{
    [Fact]
    public void Check_WithNullErrorEntry_ThrowsArgumentException()
    {
        ErrorCatalogDocument catalog = new()
        {
            Errors = [null!]
        };

        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => new WhenItFailsDocumentationKeyChecker().Check(catalog));

        Assert.Equal("catalog", exception.ParamName);
        Assert.Contains(
            "Error catalog entries cannot be null.",
            exception.Message,
            StringComparison.Ordinal);
    }
}
