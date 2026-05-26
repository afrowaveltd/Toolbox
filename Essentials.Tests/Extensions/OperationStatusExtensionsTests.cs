using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class OperationStatusExtensionsTests
{
   [Theory]
   [InlineData(OperationStatus.Unknown, false)]
   [InlineData(OperationStatus.Pending, true)]
   [InlineData(OperationStatus.Running, false)]
   [InlineData(OperationStatus.Completed, false)]
   [InlineData(OperationStatus.CompletedWithWarnings, false)]
   [InlineData(OperationStatus.PartiallyCompleted, false)]
   [InlineData(OperationStatus.Failed, false)]
   [InlineData(OperationStatus.Cancelled, false)]
   [InlineData(OperationStatus.Skipped, false)]
   [InlineData(OperationStatus.NotSupported, false)]
   public void IsPending_ReturnsExpectedResult(
       OperationStatus status,
       bool expected)
   {
      var actual = status.IsPending();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(OperationStatus.Unknown, false)]
   [InlineData(OperationStatus.Pending, false)]
   [InlineData(OperationStatus.Running, true)]
   [InlineData(OperationStatus.Completed, false)]
   [InlineData(OperationStatus.CompletedWithWarnings, false)]
   [InlineData(OperationStatus.PartiallyCompleted, false)]
   [InlineData(OperationStatus.Failed, false)]
   [InlineData(OperationStatus.Cancelled, false)]
   [InlineData(OperationStatus.Skipped, false)]
   [InlineData(OperationStatus.NotSupported, false)]
   public void IsRunning_ReturnsExpectedResult(
       OperationStatus status,
       bool expected)
   {
      var actual = status.IsRunning();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(OperationStatus.Unknown, false)]
   [InlineData(OperationStatus.Pending, false)]
   [InlineData(OperationStatus.Running, false)]
   [InlineData(OperationStatus.Completed, true)]
   [InlineData(OperationStatus.CompletedWithWarnings, true)]
   [InlineData(OperationStatus.PartiallyCompleted, true)]
   [InlineData(OperationStatus.Failed, false)]
   [InlineData(OperationStatus.Cancelled, false)]
   [InlineData(OperationStatus.Skipped, false)]
   [InlineData(OperationStatus.NotSupported, false)]
   public void IsCompleted_ReturnsExpectedResult(
       OperationStatus status,
       bool expected)
   {
      var actual = status.IsCompleted();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(OperationStatus.Unknown, false)]
   [InlineData(OperationStatus.Pending, false)]
   [InlineData(OperationStatus.Running, false)]
   [InlineData(OperationStatus.Completed, true)]
   [InlineData(OperationStatus.CompletedWithWarnings, false)]
   [InlineData(OperationStatus.PartiallyCompleted, false)]
   [InlineData(OperationStatus.Failed, false)]
   [InlineData(OperationStatus.Cancelled, false)]
   [InlineData(OperationStatus.Skipped, false)]
   [InlineData(OperationStatus.NotSupported, false)]
   public void IsSuccessfullyCompleted_ReturnsExpectedResult(
       OperationStatus status,
       bool expected)
   {
      var actual = status.IsSuccessfullyCompleted();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(OperationStatus.Unknown, false)]
   [InlineData(OperationStatus.Pending, false)]
   [InlineData(OperationStatus.Running, false)]
   [InlineData(OperationStatus.Completed, false)]
   [InlineData(OperationStatus.CompletedWithWarnings, true)]
   [InlineData(OperationStatus.PartiallyCompleted, false)]
   [InlineData(OperationStatus.Failed, false)]
   [InlineData(OperationStatus.Cancelled, false)]
   [InlineData(OperationStatus.Skipped, false)]
   [InlineData(OperationStatus.NotSupported, false)]
   public void HasWarnings_ReturnsExpectedResult(
       OperationStatus status,
       bool expected)
   {
      var actual = status.HasWarnings();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(OperationStatus.Unknown, false)]
   [InlineData(OperationStatus.Pending, false)]
   [InlineData(OperationStatus.Running, false)]
   [InlineData(OperationStatus.Completed, false)]
   [InlineData(OperationStatus.CompletedWithWarnings, false)]
   [InlineData(OperationStatus.PartiallyCompleted, true)]
   [InlineData(OperationStatus.Failed, false)]
   [InlineData(OperationStatus.Cancelled, false)]
   [InlineData(OperationStatus.Skipped, false)]
   [InlineData(OperationStatus.NotSupported, false)]
   public void IsPartiallyCompleted_ReturnsExpectedResult(
       OperationStatus status,
       bool expected)
   {
      var actual = status.IsPartiallyCompleted();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(OperationStatus.Unknown, false)]
   [InlineData(OperationStatus.Pending, false)]
   [InlineData(OperationStatus.Running, false)]
   [InlineData(OperationStatus.Completed, false)]
   [InlineData(OperationStatus.CompletedWithWarnings, false)]
   [InlineData(OperationStatus.PartiallyCompleted, false)]
   [InlineData(OperationStatus.Failed, true)]
   [InlineData(OperationStatus.Cancelled, false)]
   [InlineData(OperationStatus.Skipped, false)]
   [InlineData(OperationStatus.NotSupported, false)]
   public void IsFailed_ReturnsExpectedResult(
       OperationStatus status,
       bool expected)
   {
      var actual = status.IsFailed();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(OperationStatus.Unknown, false)]
   [InlineData(OperationStatus.Pending, false)]
   [InlineData(OperationStatus.Running, false)]
   [InlineData(OperationStatus.Completed, false)]
   [InlineData(OperationStatus.CompletedWithWarnings, false)]
   [InlineData(OperationStatus.PartiallyCompleted, false)]
   [InlineData(OperationStatus.Failed, false)]
   [InlineData(OperationStatus.Cancelled, true)]
   [InlineData(OperationStatus.Skipped, false)]
   [InlineData(OperationStatus.NotSupported, false)]
   public void IsCancelled_ReturnsExpectedResult(
       OperationStatus status,
       bool expected)
   {
      var actual = status.IsCancelled();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(OperationStatus.Unknown, false)]
   [InlineData(OperationStatus.Pending, false)]
   [InlineData(OperationStatus.Running, false)]
   [InlineData(OperationStatus.Completed, false)]
   [InlineData(OperationStatus.CompletedWithWarnings, false)]
   [InlineData(OperationStatus.PartiallyCompleted, false)]
   [InlineData(OperationStatus.Failed, false)]
   [InlineData(OperationStatus.Cancelled, false)]
   [InlineData(OperationStatus.Skipped, true)]
   [InlineData(OperationStatus.NotSupported, false)]
   public void IsSkipped_ReturnsExpectedResult(
       OperationStatus status,
       bool expected)
   {
      var actual = status.IsSkipped();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(OperationStatus.Unknown, false)]
   [InlineData(OperationStatus.Pending, false)]
   [InlineData(OperationStatus.Running, false)]
   [InlineData(OperationStatus.Completed, true)]
   [InlineData(OperationStatus.CompletedWithWarnings, true)]
   [InlineData(OperationStatus.PartiallyCompleted, true)]
   [InlineData(OperationStatus.Failed, true)]
   [InlineData(OperationStatus.Cancelled, true)]
   [InlineData(OperationStatus.Skipped, true)]
   [InlineData(OperationStatus.NotSupported, true)]
   public void IsFinal_ReturnsExpectedResult(
       OperationStatus status,
       bool expected)
   {
      var actual = status.IsFinal();

      Assert.Equal(expected, actual);
   }
}