using Afrowave.Toolbox.Essentials.Enums;
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

      Assert.True(
          status.IsConsistent);
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
         UsedFallback = false,

         RecoveryReasonCode =
        "ProjectCatalogInvalid",

         RecoveryStatus =
        ResultStatus.Invalid,

         RecoveryMessage =
        "The project catalog failed."
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.PreviousContextRecovery,
          status.State);

      Assert.True(
          status.IsConsistent);
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
         UsedFallback = true,

         RecoveryReasonCode =
         "ProjectCatalogInvalid",

         RecoveryStatus =
         ResultStatus.Invalid,

         RecoveryMessage =
         "The project catalog failed."
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.BuiltInFallback,
          status.State);

      Assert.True(
          status.IsConsistent);
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

      Assert.True(
          status.IsConsistent);
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

      Assert.False(
          status.IsConsistent);
   }

   [Fact]
   public void State_ShouldReturnUnknown_WhenProjectCatalogIsMarkedAsDegraded()
   {
      ErrorCatalogRuntimeStatus status = new()
      {
         ContextSource =
              ErrorCatalogContextSource.ProjectCatalog,

         IsDegraded = true,
         KeptPreviousContext = false,
         UsedFallback = false
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.Unknown,
          status.State);

      Assert.False(
          status.IsConsistent);
   }

   [Fact]
   public void State_ShouldReturnUnknown_WhenProjectCatalogClaimsFallbackWasUsed()
   {
      ErrorCatalogRuntimeStatus status = new()
      {
         ContextSource =
              ErrorCatalogContextSource.ProjectCatalog,

         IsDegraded = false,
         KeptPreviousContext = false,
         UsedFallback = true
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.Unknown,
          status.State);

      Assert.False(
          status.IsConsistent);
   }

   [Fact]
   public void State_ShouldReturnUnknown_WhenBuiltInFallbackIsNotDegraded()
   {
      ErrorCatalogRuntimeStatus status = new()
      {
         ContextSource =
              ErrorCatalogContextSource.BuiltInDefaults,

         IsDegraded = false,
         KeptPreviousContext = false,
         UsedFallback = true
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.Unknown,
          status.State);

      Assert.False(
          status.IsConsistent);
   }

   [Fact]
   public void State_ShouldReturnUnknown_WhenBuiltInDefaultsKeepPreviousContext()
   {
      ErrorCatalogRuntimeStatus status = new()
      {
         ContextSource =
              ErrorCatalogContextSource.BuiltInDefaults,

         IsDegraded = true,
         KeptPreviousContext = true,
         UsedFallback = true
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.Unknown,
          status.State);

      Assert.False(
          status.IsConsistent);
   }

   [Fact]
   public void IsConsistent_ShouldReturnFalse_WhenNormalStateContainsRecoveryDetails()
   {
      ErrorCatalogRuntimeStatus status = new()
      {
         ContextSource =
              ErrorCatalogContextSource.ProjectCatalog,

         IsDegraded = false,
         KeptPreviousContext = false,
         UsedFallback = false,

         RecoveryReasonCode =
              "UnexpectedFailure",

         RecoveryStatus =
              ResultStatus.Invalid,

         RecoveryMessage =
              "Recovery details should not be present."
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.ProjectCatalog,
          status.State);

      Assert.False(
          status.IsConsistent);
   }

   [Fact]
   public void IsConsistent_ShouldReturnFalse_WhenRecoveryReasonCodeIsMissing()
   {
      ErrorCatalogRuntimeStatus status = new()
      {
         ContextSource =
              ErrorCatalogContextSource.BuiltInDefaults,

         IsDegraded = true,
         KeptPreviousContext = false,
         UsedFallback = true,

         RecoveryStatus =
              ResultStatus.Invalid,

         RecoveryMessage =
              "The project catalog failed."
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.BuiltInFallback,
          status.State);

      Assert.False(
          status.IsConsistent);
   }

   [Fact]
   public void IsConsistent_ShouldReturnFalse_WhenRecoveryStatusIsMissing()
   {
      ErrorCatalogRuntimeStatus status = new()
      {
         ContextSource =
              ErrorCatalogContextSource.PreviousContext,

         IsDegraded = true,
         KeptPreviousContext = true,
         UsedFallback = false,

         RecoveryReasonCode =
              "ProjectCatalogInvalid",

         RecoveryMessage =
              "The project catalog failed."
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.PreviousContextRecovery,
          status.State);

      Assert.False(
          status.IsConsistent);
   }

   [Fact]
   public void IsConsistent_ShouldReturnFalse_WhenRecoveryMessageIsMissing()
   {
      ErrorCatalogRuntimeStatus status = new()
      {
         ContextSource =
              ErrorCatalogContextSource.BuiltInDefaults,

         IsDegraded = true,
         KeptPreviousContext = false,
         UsedFallback = true,

         RecoveryReasonCode =
              "ProjectCatalogInvalid",

         RecoveryStatus =
              ResultStatus.Invalid
      };

      Assert.Equal(
          ErrorCatalogRuntimeState.BuiltInFallback,
          status.State);

      Assert.False(
          status.IsConsistent);
   }
}