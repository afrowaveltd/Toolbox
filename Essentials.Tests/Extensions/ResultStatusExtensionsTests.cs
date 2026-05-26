using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class ResultStatusExtensionsTests
{
   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, true)]
   [InlineData(ResultStatus.SuccessWithWarnings, true)]
   [InlineData(ResultStatus.Partial, false)]
   [InlineData(ResultStatus.NotFound, false)]
   [InlineData(ResultStatus.Invalid, false)]
   [InlineData(ResultStatus.NotSupported, false)]
   [InlineData(ResultStatus.Cancelled, false)]
   [InlineData(ResultStatus.Failed, false)]
   public void IsSuccess_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var actual = status.IsSuccess();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, false)]
   [InlineData(ResultStatus.SuccessWithWarnings, true)]
   [InlineData(ResultStatus.Partial, false)]
   [InlineData(ResultStatus.NotFound, false)]
   [InlineData(ResultStatus.Invalid, false)]
   [InlineData(ResultStatus.NotSupported, false)]
   [InlineData(ResultStatus.Cancelled, false)]
   [InlineData(ResultStatus.Failed, false)]
   public void HasWarnings_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var actual = status.HasWarnings();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, false)]
   [InlineData(ResultStatus.SuccessWithWarnings, false)]
   [InlineData(ResultStatus.Partial, true)]
   [InlineData(ResultStatus.NotFound, false)]
   [InlineData(ResultStatus.Invalid, false)]
   [InlineData(ResultStatus.NotSupported, false)]
   [InlineData(ResultStatus.Cancelled, false)]
   [InlineData(ResultStatus.Failed, false)]
   public void IsPartial_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var actual = status.IsPartial();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, false)]
   [InlineData(ResultStatus.SuccessWithWarnings, false)]
   [InlineData(ResultStatus.Partial, false)]
   [InlineData(ResultStatus.NotFound, false)]
   [InlineData(ResultStatus.Invalid, true)]
   [InlineData(ResultStatus.NotSupported, true)]
   [InlineData(ResultStatus.Cancelled, true)]
   [InlineData(ResultStatus.Failed, true)]
   public void IsFailure_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var actual = status.IsFailure();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, false)]
   [InlineData(ResultStatus.SuccessWithWarnings, false)]
   [InlineData(ResultStatus.Partial, false)]
   [InlineData(ResultStatus.NotFound, true)]
   [InlineData(ResultStatus.Invalid, false)]
   [InlineData(ResultStatus.NotSupported, false)]
   [InlineData(ResultStatus.Cancelled, false)]
   [InlineData(ResultStatus.Failed, false)]
   public void IsNotFound_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var actual = status.IsNotFound();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(ResultStatus.Unknown, false)]
   [InlineData(ResultStatus.Success, true)]
   [InlineData(ResultStatus.SuccessWithWarnings, true)]
   [InlineData(ResultStatus.Partial, true)]
   [InlineData(ResultStatus.NotFound, true)]
   [InlineData(ResultStatus.Invalid, true)]
   [InlineData(ResultStatus.NotSupported, true)]
   [InlineData(ResultStatus.Cancelled, true)]
   [InlineData(ResultStatus.Failed, true)]
   public void IsFinal_ReturnsExpectedResult(
       ResultStatus status,
       bool expected)
   {
      var actual = status.IsFinal();

      Assert.Equal(expected, actual);
   }
}