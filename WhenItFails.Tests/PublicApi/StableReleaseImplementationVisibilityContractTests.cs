using Afrowave.Toolbox.WhenItFails.Catalog;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Resolution;
using Afrowave.Toolbox.WhenItFails.Services;

namespace Afrowave.Toolbox.WhenItFails.Tests.PublicApi;

public sealed class StableReleaseImplementationVisibilityContractTests
{
    [Fact]
    public void DefaultOrchestrationImplementations_AreNotExported()
    {
        Type[] implementationTypes =
        [
            typeof(BuiltInErrorCatalogContextProvider),
            typeof(ErrorCatalog),
            typeof(ErrorCatalogContextProvider),
            typeof(ErrorCatalogFactory),
            typeof(ErrorCatalogProvider),
            typeof(ErrorCategoryCatalogProvider),
            typeof(ErrorCodeGroupCatalogProvider),
            typeof(ErrorOwnerCatalogProvider),
            typeof(ErrorProfileCatalogProvider),
            typeof(ErrorDefinitionResolver),
            typeof(ErrorCatalogInitializer),
            typeof(ErrorProfileSelectionService),
            typeof(ErrorDescriptorFactory),
            typeof(ErrorDescriptorResolver),
            typeof(ErrorCatalogContextStore),
            typeof(ErrorCatalogRuntime),
            typeof(ErrorDescriptorService)
        ];

        foreach (Type implementationType in implementationTypes)
        {
            Assert.False(
                implementationType.IsVisible,
                implementationType.FullName);
        }
    }
}
