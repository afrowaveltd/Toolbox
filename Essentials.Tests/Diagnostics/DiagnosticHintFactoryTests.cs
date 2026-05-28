using Afrowave.Toolbox.Essentials.Diagnostics;

namespace Afrowave.Toolbox.Essentials.Tests.Diagnostics;

public sealed class DiagnosticHintFactoryTests
{
   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Create_WhenMessageIsInvalid_ThrowsArgumentException(
       string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticHintFactory.Create(
              DiagnosticHintKind.Note,
              message!));
   }

   [Theory]
   [InlineData(DiagnosticHintKind.Note)]
   [InlineData(DiagnosticHintKind.Help)]
   [InlineData(DiagnosticHintKind.Suggestion)]
   [InlineData(DiagnosticHintKind.Example)]
   public void Create_CreatesHintWithSpecifiedKind(
       DiagnosticHintKind kind)
   {
      var hint = DiagnosticHintFactory.Create(
          kind,
          "Use a valid value.");

      Assert.Equal(kind, hint.Kind);
      Assert.Equal("Use a valid value.", hint.Message);
   }

   [Fact]
   public void Note_CreatesNoteHint()
   {
      var hint = DiagnosticHintFactory.Note("This is a note.");

      Assert.Equal(DiagnosticHintKind.Note, hint.Kind);
      Assert.Equal("This is a note.", hint.Message);
   }

   [Fact]
   public void Help_CreatesHelpHint()
   {
      var hint = DiagnosticHintFactory.Help("Use a valid value.");

      Assert.Equal(DiagnosticHintKind.Help, hint.Kind);
      Assert.Equal("Use a valid value.", hint.Message);
   }

   [Fact]
   public void Suggestion_CreatesSuggestionHint()
   {
      var hint = DiagnosticHintFactory.Suggestion("Try another value.");

      Assert.Equal(DiagnosticHintKind.Suggestion, hint.Kind);
      Assert.Equal("Try another value.", hint.Message);
   }

   [Fact]
   public void Example_CreatesExampleHint()
   {
      var hint = DiagnosticHintFactory.Example("Example: true");

      Assert.Equal(DiagnosticHintKind.Example, hint.Kind);
      Assert.Equal("Example: true", hint.Message);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Note_WhenMessageIsInvalid_ThrowsArgumentException(
       string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticHintFactory.Note(message!));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Help_WhenMessageIsInvalid_ThrowsArgumentException(
       string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticHintFactory.Help(message!));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Suggestion_WhenMessageIsInvalid_ThrowsArgumentException(
       string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticHintFactory.Suggestion(message!));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void Example_WhenMessageIsInvalid_ThrowsArgumentException(
       string? message)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          DiagnosticHintFactory.Example(message!));
   }
}