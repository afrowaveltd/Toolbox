using Afrowave.Toolbox.Essentials.Diagnostics;
using Afrowave.Toolbox.Essentials.Extensions;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class DiagnosticHintExtensionsTests
{
   [Fact]
   public void IsNote_WhenHintIsNull_ThrowsArgumentNullException()
   {
      DiagnosticHint? hint = null;

      Assert.Throws<ArgumentNullException>(() =>
          hint!.IsNote());
   }

   [Theory]
   [InlineData(DiagnosticHintKind.Note, true)]
   [InlineData(DiagnosticHintKind.Help, false)]
   [InlineData(DiagnosticHintKind.Suggestion, false)]
   [InlineData(DiagnosticHintKind.Example, false)]
   public void IsNote_ReturnsExpectedResult(
       DiagnosticHintKind kind,
       bool expected)
   {
      var hint = new DiagnosticHint
      {
         Kind = kind
      };

      var actual = hint.IsNote();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsHelp_WhenHintIsNull_ThrowsArgumentNullException()
   {
      DiagnosticHint? hint = null;

      Assert.Throws<ArgumentNullException>(() =>
          hint!.IsHelp());
   }

   [Theory]
   [InlineData(DiagnosticHintKind.Note, false)]
   [InlineData(DiagnosticHintKind.Help, true)]
   [InlineData(DiagnosticHintKind.Suggestion, false)]
   [InlineData(DiagnosticHintKind.Example, false)]
   public void IsHelp_ReturnsExpectedResult(
       DiagnosticHintKind kind,
       bool expected)
   {
      var hint = new DiagnosticHint
      {
         Kind = kind
      };

      var actual = hint.IsHelp();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsSuggestion_WhenHintIsNull_ThrowsArgumentNullException()
   {
      DiagnosticHint? hint = null;

      Assert.Throws<ArgumentNullException>(() =>
          hint!.IsSuggestion());
   }

   [Theory]
   [InlineData(DiagnosticHintKind.Note, false)]
   [InlineData(DiagnosticHintKind.Help, false)]
   [InlineData(DiagnosticHintKind.Suggestion, true)]
   [InlineData(DiagnosticHintKind.Example, false)]
   public void IsSuggestion_ReturnsExpectedResult(
       DiagnosticHintKind kind,
       bool expected)
   {
      var hint = new DiagnosticHint
      {
         Kind = kind
      };

      var actual = hint.IsSuggestion();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void IsExample_WhenHintIsNull_ThrowsArgumentNullException()
   {
      DiagnosticHint? hint = null;

      Assert.Throws<ArgumentNullException>(() =>
          hint!.IsExample());
   }

   [Theory]
   [InlineData(DiagnosticHintKind.Note, false)]
   [InlineData(DiagnosticHintKind.Help, false)]
   [InlineData(DiagnosticHintKind.Suggestion, false)]
   [InlineData(DiagnosticHintKind.Example, true)]
   public void IsExample_ReturnsExpectedResult(
       DiagnosticHintKind kind,
       bool expected)
   {
      var hint = new DiagnosticHint
      {
         Kind = kind
      };

      var actual = hint.IsExample();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasMessage_WhenHintIsNull_ThrowsArgumentNullException()
   {
      DiagnosticHint? hint = null;

      Assert.Throws<ArgumentNullException>(() =>
          hint!.HasMessage());
   }

   [Theory]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("This is a hint.", true)]
   [InlineData(" hint ", true)]
   public void HasMessage_ReturnsExpectedResult(
       string message,
       bool expected)
   {
      var hint = new DiagnosticHint
      {
         Message = message
      };

      var actual = hint.HasMessage();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void MessageContains_WhenHintIsNull_ThrowsArgumentNullException()
   {
      DiagnosticHint? hint = null;

      Assert.Throws<ArgumentNullException>(() =>
          hint!.MessageContains("hint"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void MessageContains_WhenTextIsInvalid_ThrowsArgumentException(
       string? text)
   {
      var hint = new DiagnosticHint
      {
         Message = "This is a hint."
      };

      Assert.ThrowsAny<ArgumentException>(() =>
          hint.MessageContains(text!));
   }

   [Theory]
   [InlineData("This is a diagnostic hint.", "diagnostic", true)]
   [InlineData("This is a diagnostic hint.", "DIAGNOSTIC", true)]
   [InlineData("Use a valid JSON value.", "json", true)]
   [InlineData("Use a valid JSON value.", "missing", false)]
   [InlineData("", "missing", false)]
   [InlineData("   ", "missing", false)]
   public void MessageContains_ReturnsExpectedResult(
       string message,
       string text,
       bool expected)
   {
      var hint = new DiagnosticHint
      {
         Message = message
      };

      var actual = hint.MessageContains(text);

      Assert.Equal(expected, actual);
   }
}