using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class OperationRiskLevelExtensionsTests
{
   [Theory]
   [InlineData(OperationRiskLevel.None, false)]
   [InlineData(OperationRiskLevel.ReadOnly, true)]
   [InlineData(OperationRiskLevel.Low, false)]
   [InlineData(OperationRiskLevel.Medium, false)]
   [InlineData(OperationRiskLevel.High, false)]
   [InlineData(OperationRiskLevel.Dangerous, false)]
   public void IsReadOnly_ReturnsExpectedResult(
       OperationRiskLevel riskLevel,
       bool expected)
   {
      var actual = riskLevel.IsReadOnly();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(OperationRiskLevel.None, false)]
   [InlineData(OperationRiskLevel.ReadOnly, false)]
   [InlineData(OperationRiskLevel.Low, false)]
   [InlineData(OperationRiskLevel.Medium, true)]
   [InlineData(OperationRiskLevel.High, true)]
   [InlineData(OperationRiskLevel.Dangerous, true)]
   public void IsMediumOrHigher_ReturnsExpectedResult(
       OperationRiskLevel riskLevel,
       bool expected)
   {
      var actual = riskLevel.IsMediumOrHigher();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(OperationRiskLevel.None, false)]
   [InlineData(OperationRiskLevel.ReadOnly, false)]
   [InlineData(OperationRiskLevel.Low, false)]
   [InlineData(OperationRiskLevel.Medium, false)]
   [InlineData(OperationRiskLevel.High, true)]
   [InlineData(OperationRiskLevel.Dangerous, true)]
   public void IsHighOrHigher_ReturnsExpectedResult(
       OperationRiskLevel riskLevel,
       bool expected)
   {
      var actual = riskLevel.IsHighOrHigher();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(OperationRiskLevel.None, false)]
   [InlineData(OperationRiskLevel.ReadOnly, false)]
   [InlineData(OperationRiskLevel.Low, false)]
   [InlineData(OperationRiskLevel.Medium, false)]
   [InlineData(OperationRiskLevel.High, false)]
   [InlineData(OperationRiskLevel.Dangerous, true)]
   public void IsDangerous_ReturnsExpectedResult(
       OperationRiskLevel riskLevel,
       bool expected)
   {
      var actual = riskLevel.IsDangerous();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(OperationRiskLevel.None, false)]
   [InlineData(OperationRiskLevel.ReadOnly, false)]
   [InlineData(OperationRiskLevel.Low, false)]
   [InlineData(OperationRiskLevel.Medium, true)]
   [InlineData(OperationRiskLevel.High, true)]
   [InlineData(OperationRiskLevel.Dangerous, true)]
   public void UsuallyRequiresApproval_ReturnsExpectedResult(
       OperationRiskLevel riskLevel,
       bool expected)
   {
      var actual = riskLevel.UsuallyRequiresApproval();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(OperationRiskLevel.None, true)]
   [InlineData(OperationRiskLevel.ReadOnly, true)]
   [InlineData(OperationRiskLevel.Low, true)]
   [InlineData(OperationRiskLevel.Medium, false)]
   [InlineData(OperationRiskLevel.High, false)]
   [InlineData(OperationRiskLevel.Dangerous, false)]
   public void IsUsuallySafeForAutomaticExecution_ReturnsExpectedResult(
       OperationRiskLevel riskLevel,
       bool expected)
   {
      var actual = riskLevel.IsUsuallySafeForAutomaticExecution();

      Assert.Equal(expected, actual);
   }
}