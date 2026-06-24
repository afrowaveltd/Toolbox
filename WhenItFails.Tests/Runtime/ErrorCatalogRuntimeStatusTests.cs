using Afrowave.Toolbox.WhenItFails.Enums;
using Afrowave.Toolbox.WhenItFails.Runtime;

namespace Afrowave.Toolbox.WhenItFails.Tests.Runtime;

public sealed class ErrorCatalogRuntimeStatusTests
{
   [Fact]
   public void State_ShouldReturnProjectCatalog()
   {
      ErrorCatalogRuntimeStatus status = new()
      {
         ContextSource =
              ErrorCatalogContextSource.ProjectCatalog,

         IsDegraded = false,
         KeptPreviousContext = false,
         UsedFallback = false
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.ProjectCatalog,
          status.State);
   }

   [Fact]
   public void State_ShouldReturnPreviousContextRecovery()
   {
      ErrorCatalogRuntimeStatus status = new()
      {
         ContextSource =
              ErrorCatalogContextSource.PreviousContext,

         IsDegraded = true,
         KeptPreviousContext = true,
         UsedFallback = false
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.PreviousContextRecovery,
          status.State);
   }

   [Fact]
   public void State_ShouldReturnBuiltInFallback()
   {
      ErrorCatalogRuntimeStatus status = new()
      {
         ContextSource =
              ErrorCatalogContextSource.BuiltInDefaults,

         IsDegraded = true,
         KeptPreviousContext = false,
         UsedFallback = true
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.BuiltInFallback,
          status.State);
   }

   [Fact]
   public void State_ShouldReturnBuiltInDefaults_ForExplicitReset()
   {
      ErrorCatalogRuntimeStatus status = new()
      {
         ContextSource =
              ErrorCatalogContextSource.BuiltInDefaults,

         IsDegraded = false,
         KeptPreviousContext = false,
         UsedFallback = false
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.BuiltInDefaults,
          status.State);
   }

   [Fact]
   public void State_ShouldReturnUnknown_ForInconsistentPreviousContextState()
   {
      ErrorCatalogRuntimeStatus status = new()
      {
         ContextSource =
              ErrorCatalogContextSource.PreviousContext,

         IsDegraded = false,
         KeptPreviousContext = false,
         UsedFallback = false
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.Unknown,
          status.State);
   }
}