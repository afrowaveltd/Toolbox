using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasMessageExtensionsTests
{
   [Fact]
   public void HasMessage_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasMessage? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasMessage());
   }

   [Theory]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("Test message", true)]
   [InlineData(" test ", true)]
   public void HasMessage_ReturnsExpectedResult(
       string message,
       bool expected)
   {
      var value = new TestHasMessage(message);

      var actual = value.HasMessage();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasMessageWithMessage_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasMessage? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasMessage("Test message"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void HasMessageWithMessage_WhenMessageIsInvalid_ThrowsArgumentException(
       string? message)
   {
      var value = new TestHasMessage("Test message");

      Assert.ThrowsAny<ArgumentException>(() =>
          value.HasMessage(message!));
   }

   [Theory]
   [InlineData("Test message", "Test message", true)]
   [InlineData("Test message", "test message", true)]
   [InlineData("TEST MESSAGE", "test message", true)]
   [InlineData("Afrowave Toolbox", "afrowave toolbox", true)]
   [InlineData("Test message", "Other message", false)]
   [InlineData("", "Test message", false)]
   [InlineData("   ", "Test message", false)]
   public void HasMessageWithMessage_ReturnsExpectedResult(
       string actualMessage,
       string expectedMessage,
       bool expected)
   {
      var value = new TestHasMessage(actualMessage);

      var actual = value.HasMessage(expectedMessage);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void MessageContains_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasMessage? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.MessageContains("message"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void MessageContains_WhenTextIsInvalid_ThrowsArgumentException(
       string? text)
   {
      var value = new TestHasMessage("Test message");

      Assert.ThrowsAny<ArgumentException>(() =>
          value.MessageContains(text!));
   }

   [Theory]
   [InlineData("This is a test message.", "test", true)]
   [InlineData("This is a test message.", "TEST", true)]
   [InlineData("Afrowave Toolbox Essentials", "toolbox", true)]
   [InlineData("Afrowave Toolbox Essentials", "ESSENTIALS", true)]
   [InlineData("Afrowave Toolbox Essentials", "missing", false)]
   [InlineData("", "missing", false)]
   [InlineData("   ", "missing", false)]
   public void MessageContains_ReturnsExpectedResult(
       string message,
       string text,
       bool expected)
   {
      var value = new TestHasMessage(message);

      var actual = value.MessageContains(text);

      Assert.Equal(expected, actual);
   }
   [Fact]
   public void HasMessage_WhenMessageIsNull_ReturnsFalse()
   {
      var value = new TestHasMessage(null!);

      var actual = value.HasMessage();

      Assert.False(actual);
   }

   [Fact]
   public void MessageContains_WhenMessageIsNull_ReturnsFalse()
   {
      var value = new TestHasMessage(null!);

      var actual = value.MessageContains("message");

      Assert.False(actual);
   }
   [Fact]
   public void HasMessageWithMessage_WhenSourceMessageIsNull_ReturnsFalse()
   {
      var value = new TestHasMessage(null!);

      var actual = value.HasMessage("Test message");

      Assert.False(actual);
   }

   private sealed class TestHasMessage : IHasMessage
   {
      public TestHasMessage(string message)
      {
         Message = message;
      }

      public string Message { get; }
   }
}