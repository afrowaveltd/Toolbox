using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasNumberExtensionsTests
{
   [Fact]
   public void HasNumber_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasNumber? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasNumber());
   }

   [Fact]
   public void HasNumber_WhenNumberIsNull_ReturnsFalse()
   {
      var value = new TestHasNumber(null);

      var actual = value.HasNumber();

      Assert.False(actual);
   }

   [Theory]
   [InlineData(0)]
   [InlineData(1)]
   [InlineData(42)]
   [InlineData(-1)]
   public void HasNumber_WhenNumberIsSet_ReturnsTrue(int number)
   {
      var value = new TestHasNumber(number);

      var actual = value.HasNumber();

      Assert.True(actual);
   }

   [Fact]
   public void HasNumberWithNumber_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasNumber? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasNumber(42));
   }

   [Theory]
   [InlineData(null, 0, false)]
   [InlineData(null, 42, false)]
   [InlineData(0, 0, true)]
   [InlineData(1, 1, true)]
   [InlineData(42, 42, true)]
   [InlineData(-1, -1, true)]
   [InlineData(42, 24, false)]
   [InlineData(0, 1, false)]
   public void HasNumberWithNumber_ReturnsExpectedResult(
       int? currentNumber,
       int expectedNumber,
       bool expected)
   {
      var value = new TestHasNumber(currentNumber);

      var actual = value.HasNumber(expectedNumber);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasSameNumberAs_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasNumber? value = null;
      var other = new TestHasNumber(42);

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasSameNumberAs(other));
   }

   [Fact]
   public void HasSameNumberAs_WhenOtherIsNull_ThrowsArgumentNullException()
   {
      var value = new TestHasNumber(42);
      IHasNumber? other = null;

      Assert.Throws<ArgumentNullException>(() =>
          value.HasSameNumberAs(other!));
   }

   [Theory]
   [InlineData(null, null, true)]
   [InlineData(null, 0, false)]
   [InlineData(0, null, false)]
   [InlineData(0, 0, true)]
   [InlineData(1, 1, true)]
   [InlineData(42, 42, true)]
   [InlineData(-1, -1, true)]
   [InlineData(42, 24, false)]
   [InlineData(0, 1, false)]
   public void HasSameNumberAs_ReturnsExpectedResult(
       int? firstNumber,
       int? secondNumber,
       bool expected)
   {
      var value = new TestHasNumber(firstNumber);
      var other = new TestHasNumber(secondNumber);

      var actual = value.HasSameNumberAs(other);

      Assert.Equal(expected, actual);
   }

   private sealed class TestHasNumber : IHasNumber
   {
      public TestHasNumber(int? number)
      {
         Number = number;
      }

      public int? Number { get; }
   }
}