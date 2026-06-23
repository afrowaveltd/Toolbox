using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Results;
using Afrowave.Toolbox.WhenItFails.Configuration;
using Afrowave.Toolbox.WhenItFails.Definitions;
using Afrowave.Toolbox.WhenItFails.Descriptors;
using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Initialization;
using Afrowave.Toolbox.WhenItFails.Interfaces;
using Afrowave.Toolbox.WhenItFails.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Afrowave.Toolbox.WhenItFails.Tests.Integration;

public sealed class ErrorCatalogRuntimeIntegrationTests
{
   [Fact]
   public async Task Runtime_ShouldInitializeAndProvideCatalogOperations()
   {
      string rootDirectory = CreateTemporaryRootDirectory();

      try
      {
         ServiceCollection services = new();

         services.AddWhenItFails();

         using ServiceProvider serviceProvider =
             services.BuildServiceProvider(
                 new ServiceProviderOptions
                 {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                 });

         IErrorCatalogRuntime runtime =
             serviceProvider.GetRequiredService<
                 IErrorCatalogRuntime>();

         JsonsOptions options = new()
         {
            RootDirectory = rootDirectory,
            PackageDirectoryName = "WhenItFails"
         };

         Response<ErrorCatalogInitializationPayload>
             initializationResponse =
                 await runtime.InitializeAsync(options);

         Assert.True(initializationResponse.IsSuccess);
         Assert.Equal(
             ResultStatus.Success,
             initializationResponse.Status);

         Assert.NotNull(initializationResponse.Data);
         Assert.NotNull(initializationResponse.Data.Bootstrap);
         Assert.NotNull(initializationResponse.Data.Context);

         Assert.True(
             Directory.Exists(
                 initializationResponse
                     .Data
                     .Bootstrap
                     .PackageDirectoryPath));

         Response<ErrorDescriptor> descriptorResponse =
             runtime.FromId("afw net 0001");

         Assert.True(descriptorResponse.IsSuccess);
         Assert.Equal(
             ResultStatus.Success,
             descriptorResponse.Status);

         Assert.NotNull(descriptorResponse.Data);

         Assert.Equal(
             "AFW_NET_0001",
             descriptorResponse.Data.Id);

         Assert.Equal(
             "NETWORKUNAVAILABLE",
             descriptorResponse.Data.Name);

         Assert.Equal(
             "NETWORK",
             descriptorResponse.Data.PrimaryCategory);

         Response<IReadOnlyList<ErrorDefinition>>
             profileResponse =
                 runtime.ResolveProfile("web");

         Assert.True(profileResponse.IsSuccess);
         Assert.Equal(
             ResultStatus.Success,
             profileResponse.Status);

         Assert.NotNull(profileResponse.Data);
         Assert.NotEmpty(profileResponse.Data);

         Assert.Contains(
             profileResponse.Data,
             error =>
                 error.Id == "AFW_NET_0001");

         Assert.DoesNotContain(
             profileResponse.Data,
             error =>
                 error.Tags.Contains(
                     "INTERNAL_ONLY",
                     StringComparer.OrdinalIgnoreCase));
      }
      finally
      {
         DeleteDirectoryIfExists(rootDirectory);
      }
   }

   [Fact]
   public async Task Runtime_ShouldInitializeFromRegisteredOptions()
   {
      string rootDirectory = CreateTemporaryRootDirectory();

      try
      {
         ServiceCollection services = new();

         services.AddWhenItFails(options =>
         {
            options.Jsons.RootDirectory =
                   rootDirectory;

            options.Jsons.PackageDirectoryName =
                   "ConfiguredWhenItFails";
         });

         using ServiceProvider serviceProvider =
             services.BuildServiceProvider(
                 new ServiceProviderOptions
                 {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                 });

         IErrorCatalogRuntime runtime =
             serviceProvider.GetRequiredService<
                 IErrorCatalogRuntime>();

         Response<ErrorCatalogInitializationPayload>
             initializationResponse =
                 await runtime.InitializeAsync();

         Assert.True(
             initializationResponse.IsSuccess);

         Assert.Equal(
             ResultStatus.Success,
             initializationResponse.Status);

         Assert.NotNull(
             initializationResponse.Data);

         Assert.NotNull(
             initializationResponse.Data.Bootstrap);

         Assert.NotNull(
             initializationResponse.Data.Context);

         string expectedPackageDirectoryPath =
             Path.Combine(
                 rootDirectory,
                 "ConfiguredWhenItFails");

         Assert.Equal(
             expectedPackageDirectoryPath,
             initializationResponse
                 .Data
                 .Bootstrap
                 .PackageDirectoryPath);

         Assert.True(
             Directory.Exists(
                 expectedPackageDirectoryPath));

         Response<ErrorDescriptor> descriptorResponse =
             runtime.FromId(
                 "afw net 0001");

         Assert.True(
             descriptorResponse.IsSuccess);

         Assert.NotNull(
             descriptorResponse.Data);

         Assert.Equal(
             "AFW_NET_0001",
             descriptorResponse.Data.Id);

         Assert.Equal(
             "NETWORKUNAVAILABLE",
             descriptorResponse.Data.Name);

         Response<IReadOnlyList<ErrorDefinition>>
             profileResponse =
                 runtime.ResolveProfile(
                     "web");

         Assert.True(
             profileResponse.IsSuccess);

         Assert.NotNull(
             profileResponse.Data);

         Assert.Contains(
             profileResponse.Data,
             error =>
                 error.Id == "AFW_NET_0001");
      }
      finally
      {
         DeleteDirectoryIfExists(
             rootDirectory);
      }
   }

   [Fact]
   public async Task Runtime_ShouldActivateBuiltInDefaults_WhenProjectCatalogIsInvalid()
   {
      string rootDirectory = CreateTemporaryRootDirectory();

      JsonsOptions jsonsOptions = new()
      {
         RootDirectory = rootDirectory,
         PackageDirectoryName = "BrokenWhenItFails"
      };

      try
      {
         Directory.CreateDirectory(
             jsonsOptions.PackageDirectoryPath);

         const string invalidProjectCatalog =
             "{ this is not valid json";

         await File.WriteAllTextAsync(
             jsonsOptions.ErrorCatalogFilePath,
             invalidProjectCatalog);

         ServiceCollection services = new();

         services.AddWhenItFails(
             new WhenItFailsOptions
             {
                Jsons = jsonsOptions,

                InitializationMode =
                     ErrorCatalogInitializationMode.Flexible
             });

         using ServiceProvider serviceProvider =
             services.BuildServiceProvider(
                 new ServiceProviderOptions
                 {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                 });

         IErrorCatalogRuntime runtime =
             serviceProvider.GetRequiredService<
                 IErrorCatalogRuntime>();

         Response<ErrorCatalogInitializationPayload>
             initializationResponse =
                 await runtime.InitializeAsync();

         Assert.True(
             initializationResponse.IsSuccess);

         Assert.Equal(
             ResultStatus.SuccessWithWarnings,
             initializationResponse.Status);

         Assert.True(
             initializationResponse.HasWarnings);

         Assert.NotNull(
             initializationResponse.Data);

         Assert.Equal(
             ErrorCatalogContextSource.BuiltInDefaults,
             initializationResponse.Data.ContextSource);

         Assert.True(
             initializationResponse.Data.UsedFallback);

         Assert.False(
             initializationResponse.Data.KeptPreviousContext);

         Assert.True(
             initializationResponse.Data.IsDegraded);

         Assert.Equal(
             "WIF_DEFAULT_FALLBACK_ACTIVATED",
             initializationResponse.Issues[0].Code);

         Assert.True(
             initializationResponse.Metadata.TryGet(
                 "WhenItFails.RecoveryReasonCode",
                 out string? recoveryReasonCode));

         Assert.False(
             string.IsNullOrWhiteSpace(
                 recoveryReasonCode));

         Assert.True(
             initializationResponse.Metadata.TryGet(
                 "WhenItFails.RecoveryStatus",
                 out string? recoveryStatus));

         Assert.False(
             string.IsNullOrWhiteSpace(
                 recoveryStatus));

         Assert.True(
             initializationResponse.Metadata.TryGet(
                 "WhenItFails.RecoveryMessage",
                 out string? recoveryMessage));

         Assert.False(
             string.IsNullOrWhiteSpace(
                 recoveryMessage));

         Response<ErrorCatalogRuntimeStatus> statusResponse =
             runtime.GetStatus();

         Assert.True(
             statusResponse.IsSuccess);

         Assert.Equal(
             ResultStatus.Success,
             statusResponse.Status);

         Assert.NotNull(
             statusResponse.Data);

         Assert.Equal(
             ErrorCatalogContextSource.BuiltInDefaults,
             statusResponse.Data.ContextSource);

         Assert.True(
             statusResponse.Data.IsDegraded);

         Assert.False(
             statusResponse.Data.KeptPreviousContext);

         Assert.True(
             statusResponse.Data.UsedFallback);

         Assert.Equal(
             recoveryReasonCode,
             statusResponse.Data.RecoveryReasonCode);

         Assert.Equal(
             recoveryStatus,
             statusResponse.Data.RecoveryStatus?.ToString());

         Assert.Equal(
             recoveryMessage,
             statusResponse.Data.RecoveryMessage);

         Assert.Equal(
             jsonsOptions.PackageDirectoryPath,
             statusResponse.Data.PackageDirectoryPath);

         Assert.NotEqual(
             default,
             statusResponse.Data.ActivatedAtUtc);




         Response<ErrorDescriptor> descriptorResponse =
             runtime.FromId(
                 "afw net 0001");

         Assert.True(
             descriptorResponse.IsSuccess);

         Assert.NotNull(
             descriptorResponse.Data);

         Assert.Equal(
             "AFW_NET_0001",
             descriptorResponse.Data.Id);

         Assert.Equal(
             "NETWORKUNAVAILABLE",
             descriptorResponse.Data.Name);

         string preservedProjectCatalog =
             await File.ReadAllTextAsync(
                 jsonsOptions.ErrorCatalogFilePath);

         Assert.Equal(
             invalidProjectCatalog,
             preservedProjectCatalog);
      }
      finally
      {
         DeleteDirectoryIfExists(
             rootDirectory);
      }
   }


   [Fact]
   public async Task Runtime_ShouldResetToBuiltInDefaults_WithoutChangingProjectFiles()
   {
      string rootDirectory = CreateTemporaryRootDirectory();

      JsonsOptions jsonsOptions = new()
      {
         RootDirectory = rootDirectory,
         PackageDirectoryName = "ResetTestWhenItFails"
      };

      try
      {
         ServiceCollection services = new();

         services.AddWhenItFails(
             new WhenItFailsOptions
             {
                Jsons = jsonsOptions,
                InitializationMode =
                     ErrorCatalogInitializationMode.Flexible
             });

         using ServiceProvider serviceProvider =
             services.BuildServiceProvider(
                 new ServiceProviderOptions
                 {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                 });

         IErrorCatalogRuntime runtime =
             serviceProvider.GetRequiredService<
                 IErrorCatalogRuntime>();

         Response<ErrorCatalogInitializationPayload>
             initializationResponse =
                 await runtime.InitializeAsync();

         Assert.True(
             initializationResponse.IsSuccess);

         Assert.Equal(
             ResultStatus.Success,
             initializationResponse.Status);

         Assert.NotNull(
             initializationResponse.Data);

         Assert.Equal(
             ErrorCatalogContextSource.ProjectCatalog,
             initializationResponse.Data.ContextSource);

         Dictionary<string, string> originalFileContents =
             Directory
                 .EnumerateFiles(
                     jsonsOptions.PackageDirectoryPath,
                     "*.json",
                     SearchOption.TopDirectoryOnly)
                 .ToDictionary(
                     filePath =>
                         Path.GetFileName(filePath),
                     filePath =>
                         File.ReadAllText(filePath),
                     StringComparer.OrdinalIgnoreCase);

         Assert.NotEmpty(
             originalFileContents);

         Response<ErrorCatalogInitializationPayload>
             resetResponse =
                 await runtime.ResetToDefaultsAsync();

         Assert.True(
             resetResponse.IsSuccess);

         Assert.Equal(
             ResultStatus.Success,
             resetResponse.Status);

         Assert.NotNull(
             resetResponse.Data);

         Assert.Equal(
             ErrorCatalogContextSource.BuiltInDefaults,
             resetResponse.Data.ContextSource);

         Assert.False(
             resetResponse.Data.KeptPreviousContext);

         Assert.False(
             resetResponse.Data.UsedFallback);

         Assert.False(
             resetResponse.Data.IsDegraded);

         foreach(KeyValuePair<string, string> originalFile
             in originalFileContents)
         {
            string currentFilePath =
                Path.Combine(
                    jsonsOptions.PackageDirectoryPath,
                    originalFile.Key);

            Assert.True(
                File.Exists(currentFilePath));

            string currentContent =
                await File.ReadAllTextAsync(
                    currentFilePath);

            Assert.Equal(
                originalFile.Value,
                currentContent);
         }

         Response<ErrorDescriptor> descriptorResponse =
             runtime.FromId(
                 "afw net 0001");

         Assert.True(
             descriptorResponse.IsSuccess);

         Assert.NotNull(
             descriptorResponse.Data);

         Assert.Equal(
             "AFW_NET_0001",
             descriptorResponse.Data.Id);

         Assert.Equal(
             "NETWORKUNAVAILABLE",
             descriptorResponse.Data.Name);
      }
      finally
      {
         DeleteDirectoryIfExists(
             rootDirectory);
      }
   }




   private static string CreateTemporaryRootDirectory()
   {
      return Path.Combine(
          Path.GetTempPath(),
          $"when-it-fails-runtime-{Guid.NewGuid():N}");
   }

   private static void DeleteDirectoryIfExists(
       string directoryPath)
   {
      if(!Directory.Exists(directoryPath))
      {
         return;
      }

      Directory.Delete(
          directoryPath,
          recursive: true);
   }
}
