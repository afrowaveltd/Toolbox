using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasStatusExtensionsTests
{
   [Fact]
   public void HasStatus_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasStatus<OperationStatus>? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasStatus(OperationStatus.Completed));
   }

   [Theory]
   [InlineData(OperationStatus.Unknown, OperationStatus.Unknown, true)]
   [InlineData(OperationStatus.Pending, OperationStatus.Pending, true)]
   [InlineData(OperationStatus.Running, OperationStatus.Running, true)]
   [InlineData(OperationStatus.Completed, OperationStatus.Completed, true)]
   [InlineData(OperationStatus.Failed, OperationStatus.Failed, true)]
   [InlineData(OperationStatus.Completed, OperationStatus.Failed, false)]
   [InlineData(OperationStatus.Pending, OperationStatus.Running, false)]
   public void HasStatus_ReturnsExpectedResult(
       OperationStatus currentStatus,
       OperationStatus expectedStatus,
       bool expected)
   {
      var value = new TestHasStatus<OperationStatus>(currentStatus);

      var actual = value.HasStatus(expectedStatus);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasAnyStatus_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasStatus<OperationStatus>? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasAnyStatus(OperationStatus.Completed));
   }

   [Fact]
   public void HasAnyStatus_WhenStatusesIsNull_ThrowsArgumentNullException()
   {
      var value = new TestHasStatus<OperationStatus>(OperationStatus.Completed);

      OperationStatus[]? statuses = null;

      Assert.Throws<ArgumentNullException>(() =>
          value.HasAnyStatus(statuses!));
   }

   [Fact]
   public void HasAnyStatus_WhenStatusesAreEmpty_ReturnsFalse()
   {
      var value = new TestHasStatus<OperationStatus>(OperationStatus.Completed);

      var actual = value.HasAnyStatus();

      Assert.False(actual);
   }

   [Fact]
   public void HasAnyStatus_WhenCurrentStatusIsContained_ReturnsTrue()
   {
      var value = new TestHasStatus<OperationStatus>(OperationStatus.Running);

      var actual = value.HasAnyStatus(
          OperationStatus.Pending,
          OperationStatus.Running,
          OperationStatus.Completed);

      Assert.True(actual);
   }

   [Fact]
   public void HasAnyStatus_WhenCurrentStatusIsNotContained_ReturnsFalse()
   {
      var value = new TestHasStatus<OperationStatus>(OperationStatus.Failed);

      var actual = value.HasAnyStatus(
          OperationStatus.Pending,
          OperationStatus.Running,
          OperationStatus.Completed);

      Assert.False(actual);
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, ResultStatus.Unknown, true)]
   [InlineData(ResultStatus.Success, ResultStatus.Success, true)]
   [InlineData(ResultStatus.SuccessWithWarnings, ResultStatus.SuccessWithWarnings, true)]
   [InlineData(ResultStatus.Failed, ResultStatus.Failed, true)]
   [InlineData(ResultStatus.Success, ResultStatus.Failed, false)]
   [InlineData(ResultStatus.NotFound, ResultStatus.Success, false)]
   public void HasStatus_WorksWithDifferentStatusEnumTypes(
       ResultStatus currentStatus,
       ResultStatus expectedStatus,
       bool expected)
   {
      var value = new TestHasStatus<ResultStatus>(currentStatus);

      var actual = value.HasStatus(expectedStatus);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasAnyStatus_WorksWithDifferentStatusEnumTypes()
   {
      var value = new TestHasStatus<ResultStatus>(ResultStatus.NotFound);

      var actual = value.HasAnyStatus(
          ResultStatus.Success,
          ResultStatus.NotFound,
          ResultStatus.Failed);

      Assert.True(actual);
   }

   private sealed class TestHasStatus<TStatus> : IHasStatus<TStatus>
       where TStatus : struct, Enum
   {
      public TestHasStatus(TStatus status)
      {
         Status = status;
      }

      public TStatus Status { get; }
   }
}