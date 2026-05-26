using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasCodeExtensionsTests
{
   [Fact]
   public void HasCode_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasCode? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasCode("TEST_CODE"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void HasCode_WhenCodeIsInvalid_ThrowsArgumentException(
       string? code)
   {
      var value = new TestHasCode("TEST_CODE");

      Assert.ThrowsAny<ArgumentException>(() =>
          value.HasCode(code!));
   }

   [Theory]
   [InlineData("TEST_CODE", "TEST_CODE", true)]
   [InlineData("TEST_CODE", "test_code", true)]
   [InlineData("test_code", "TEST_CODE", true)]
   [InlineData("Afw.Config.Invalid", "afw.config.invalid", true)]
   [InlineData("TEST_CODE", "OTHER_CODE", false)]
   [InlineData("", "TEST_CODE", false)]
   [InlineData("   ", "TEST_CODE", false)]
   public void HasCode_ReturnsExpectedResult(
       string actualCode,
       string expectedCode,
       bool expected)
   {
      var value = new TestHasCode(actualCode);

      var actual = value.HasCode(expectedCode);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasSameCodeAs_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasCode? value = null;
      var other = new TestHasCode("TEST_CODE");

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasSameCodeAs(other));
   }

   [Fact]
   public void HasSameCodeAs_WhenOtherIsNull_ThrowsArgumentNullException()
   {
      var value = new TestHasCode("TEST_CODE");
      IHasCode? other = null;

      Assert.Throws<ArgumentNullException>(() =>
          value.HasSameCodeAs(other!));
   }

   [Theory]
   [InlineData("TEST_CODE", "TEST_CODE", true)]
   [InlineData("TEST_CODE", "test_code", true)]
   [InlineData("test_code", "TEST_CODE", true)]
   [InlineData("Afw.Config.Invalid", "afw.config.invalid", true)]
   [InlineData("TEST_CODE", "OTHER_CODE", false)]
   [InlineData("", "", true)]
   [InlineData("   ", "   ", true)]
   [InlineData("", "   ", false)]
   public void HasSameCodeAs_ReturnsExpectedResult(
       string firstCode,
       string secondCode,
       bool expected)
   {
      var value = new TestHasCode(firstCode);
      var other = new TestHasCode(secondCode);

      var actual = value.HasSameCodeAs(other);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasEmptyCode_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasCode? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasEmptyCode());
   }

   [Theory]
   [InlineData("", true)]
   [InlineData("   ", true)]
   [InlineData("TEST_CODE", false)]
   [InlineData(" test ", false)]
   public void HasEmptyCode_ReturnsExpectedResult(
       string code,
       bool expected)
   {
      var value = new TestHasCode(code);

      var actual = value.HasEmptyCode();

      Assert.Equal(expected, actual);
   }

   private sealed class TestHasCode : IHasCode
   {
      public TestHasCode(string code)
      {
         Code = code;
      }

      public string Code { get; }
   }
}