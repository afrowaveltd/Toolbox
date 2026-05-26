using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasRiskLevelExtensionsTests
{
   [Fact]
   public void HasRiskLevel_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasRiskLevel? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasRiskLevel(OperationRiskLevel.Low));
   }

   [Theory]
   [InlineData(OperationRiskLevel.None, OperationRiskLevel.None, true)]
   [InlineData(OperationRiskLevel.ReadOnly, OperationRiskLevel.ReadOnly, true)]
   [InlineData(OperationRiskLevel.Low, OperationRiskLevel.Low, true)]
   [InlineData(OperationRiskLevel.Medium, OperationRiskLevel.Medium, true)]
   [InlineData(OperationRiskLevel.High, OperationRiskLevel.High, true)]
   [InlineData(OperationRiskLevel.Dangerous, OperationRiskLevel.Dangerous, true)]
   [InlineData(OperationRiskLevel.Low, OperationRiskLevel.High, false)]
   [InlineData(OperationRiskLevel.High, OperationRiskLevel.Low, false)]
   public void HasRiskLevel_ReturnsExpectedResult(
       OperationRiskLevel currentRiskLevel,
       OperationRiskLevel expectedRiskLevel,
       bool expected)
   {
      var value = new TestHasRiskLevel(currentRiskLevel);

      var actual = value.HasRiskLevel(expectedRiskLevel);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsReadOnlyRisk_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasRiskLevel? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.IsReadOnlyRisk());
   }

   [Theory]
   [InlineData(OperationRiskLevel.None, false)]
   [InlineData(OperationRiskLevel.ReadOnly, true)]
   [InlineData(OperationRiskLevel.Low, false)]
   [InlineData(OperationRiskLevel.Medium, false)]
   [InlineData(OperationRiskLevel.High, false)]
   [InlineData(OperationRiskLevel.Dangerous, false)]
   public void IsReadOnlyRisk_ReturnsExpectedResult(
       OperationRiskLevel riskLevel,
       bool expected)
   {
      var value = new TestHasRiskLevel(riskLevel);

      var actual = value.IsReadOnlyRisk();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasMediumOrHigherRisk_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasRiskLevel? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasMediumOrHigherRisk());
   }

   [Theory]
   [InlineData(OperationRiskLevel.None, false)]
   [InlineData(OperationRiskLevel.ReadOnly, false)]
   [InlineData(OperationRiskLevel.Low, false)]
   [InlineData(OperationRiskLevel.Medium, true)]
   [InlineData(OperationRiskLevel.High, true)]
   [InlineData(OperationRiskLevel.Dangerous, true)]
   public void HasMediumOrHigherRisk_ReturnsExpectedResult(
       OperationRiskLevel riskLevel,
       bool expected)
   {
      var value = new TestHasRiskLevel(riskLevel);

      var actual = value.HasMediumOrHigherRisk();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasHighOrHigherRisk_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasRiskLevel? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasHighOrHigherRisk());
   }

   [Theory]
   [InlineData(OperationRiskLevel.None, false)]
   [InlineData(OperationRiskLevel.ReadOnly, false)]
   [InlineData(OperationRiskLevel.Low, false)]
   [InlineData(OperationRiskLevel.Medium, false)]
   [InlineData(OperationRiskLevel.High, true)]
   [InlineData(OperationRiskLevel.Dangerous, true)]
   public void HasHighOrHigherRisk_ReturnsExpectedResult(
       OperationRiskLevel riskLevel,
       bool expected)
   {
      var value = new TestHasRiskLevel(riskLevel);

      var actual = value.HasHighOrHigherRisk();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsDangerousRisk_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasRiskLevel? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.IsDangerousRisk());
   }

   [Theory]
   [InlineData(OperationRiskLevel.None, false)]
   [InlineData(OperationRiskLevel.ReadOnly, false)]
   [InlineData(OperationRiskLevel.Low, false)]
   [InlineData(OperationRiskLevel.Medium, false)]
   [InlineData(OperationRiskLevel.High, false)]
   [InlineData(OperationRiskLevel.Dangerous, true)]
   public void IsDangerousRisk_ReturnsExpectedResult(
       OperationRiskLevel riskLevel,
       bool expected)
   {
      var value = new TestHasRiskLevel(riskLevel);

      var actual = value.IsDangerousRisk();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void UsuallyRequiresApproval_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasRiskLevel? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.UsuallyRequiresApproval());
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
      var value = new TestHasRiskLevel(riskLevel);

      var actual = value.UsuallyRequiresApproval();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsUsuallySafeForAutomaticExecution_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasRiskLevel? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.IsUsuallySafeForAutomaticExecution());
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
      var value = new TestHasRiskLevel(riskLevel);

      var actual = value.IsUsuallySafeForAutomaticExecution();

      Assert.Equal(expected, actual);
   }

   private sealed class TestHasRiskLevel : IHasRiskLevel
   {
      public TestHasRiskLevel(OperationRiskLevel riskLevel)
      {
         RiskLevel = riskLevel;
      }

      public OperationRiskLevel RiskLevel { get; }
   }
}