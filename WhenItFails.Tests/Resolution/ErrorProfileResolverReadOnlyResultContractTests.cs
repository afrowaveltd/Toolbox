using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Resolution;

namespace Afrowave.Toolbox.WhenItFails.Tests.Resolution;

public sealed class ErrorProfileResolverReadOnlyResultContractTests
{
    [Fact]
    public void Resolve_ShouldReturnGenuinelyReadOnlyCollection()
    {
        ErrorDefinition definition = new()
        {
            Id = "AFW_GEN_0001",
            Code = 100001,
            Name = "UnknownError",
            Owner = "AFW",
            CodeGroup = "GENERAL",
            PrimaryCategory = "GENERAL",
            Categories = ["GENERAL"],
            Title = "Unknown error",
            Message = "An unknown error occurred.",
            DefaultSeverity = "Error"
        };

        ErrorCatalogDocument catalog = new()
        {
            Errors = [definition]
        };

        ErrorProfileDefinition profile = new()
        {
            Name = "ALL",
            DisplayName = "All errors"
        };

        ErrorProfileResolver resolver = new();

        IReadOnlyList<ErrorDefinition> result =
            resolver.Resolve(catalog, profile);

        Assert.Single(result);
        Assert.Same(definition, result[0]);

        ICollection<ErrorDefinition> collection =
            Assert.IsAssignableFrom<ICollection<ErrorDefinition>>(result);

        Assert.True(collection.IsReadOnly);
        Assert.Throws<NotSupportedException>(
            () => collection.Add(new ErrorDefinition()));

        Assert.Single(result);
        Assert.Same(definition, result[0]);
    }
}
