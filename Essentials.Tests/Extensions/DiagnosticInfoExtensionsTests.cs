using Afrowave.Toolbox.Essentials.Diagnostics;
using Afrowave.Toolbox.Essentials.Extensions;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class DiagnosticInfoExtensionsTests
{
   [Fact]
   public void HasCode_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;

      Assert.Throws<ArgumentNullException>(() =>
          diagnostic!.HasCode());
   }

   [Theory]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("AFW001", true)]
   [InlineData(" diagnostic.code ", true)]
   public void HasCode_ReturnsExpectedResult(
       string code,
       bool expected)
   {
      var diagnostic = new DiagnosticInfo
      {
         Code = code
      };

      var actual = diagnostic.HasCode();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasMessage_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;

      Assert.Throws<ArgumentNullException>(() =>
          diagnostic!.HasMessage());
   }

   [Theory]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("Diagnostic message.", true)]
   [InlineData(" message ", true)]
   public void HasMessage_ReturnsExpectedResult(
       string message,
       bool expected)
   {
      var diagnostic = new DiagnosticInfo
      {
         Message = message
      };

      var actual = diagnostic.HasMessage();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasDetails_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;

      Assert.Throws<ArgumentNullException>(() =>
          diagnostic!.HasDetails());
   }

   [Theory]
   [InlineData(null, false)]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("Detailed diagnostic information.", true)]
   [InlineData(" details ", true)]
   public void HasDetails_ReturnsExpectedResult(
       string? details,
       bool expected)
   {
      var diagnostic = new DiagnosticInfo
      {
         Details = details
      };

      var actual = diagnostic.HasDetails();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasLocation_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;

      Assert.Throws<ArgumentNullException>(() =>
          diagnostic!.HasLocation());
   }

   [Fact]
   public void HasLocation_WhenLocationIsNull_ReturnsFalse()
   {
      var diagnostic = new DiagnosticInfo();

      var actual = diagnostic.HasLocation();

      Assert.False(actual);
   }

   [Fact]
   public void HasLocation_WhenLocationIsSet_ReturnsTrue()
   {
      var diagnostic = new DiagnosticInfo
      {
         Location = new DiagnosticLocation()
      };

      var actual = diagnostic.HasLocation();

      Assert.True(actual);
   }

   [Fact]
   public void HasLocationInfo_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;

      Assert.Throws<ArgumentNullException>(() =>
          diagnostic!.HasLocationInfo());
   }

   [Fact]
   public void HasLocationInfo_WhenLocationIsNull_ReturnsFalse()
   {
      var diagnostic = new DiagnosticInfo();

      var actual = diagnostic.HasLocationInfo();

      Assert.False(actual);
   }

   [Fact]
   public void HasLocationInfo_WhenLocationIsEmpty_ReturnsFalse()
   {
      var diagnostic = new DiagnosticInfo
      {
         Location = new DiagnosticLocation()
      };

      var actual = diagnostic.HasLocationInfo();

      Assert.False(actual);
   }

   [Fact]
   public void HasLocationInfo_WhenLocationHasInformation_ReturnsTrue()
   {
      var diagnostic = new DiagnosticInfo
      {
         Location = new DiagnosticLocation
         {
            Source = "input.ajis"
         }
      };

      var actual = diagnostic.HasLocationInfo();

      Assert.True(actual);
   }

   [Fact]
   public void HasSpans_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;

      Assert.Throws<ArgumentNullException>(() =>
          diagnostic!.HasSpans());
   }

   [Fact]
   public void HasSpans_WhenSpansAreEmpty_ReturnsFalse()
   {
      var diagnostic = new DiagnosticInfo
      {
         Spans = []
      };

      var actual = diagnostic.HasSpans();

      Assert.False(actual);
   }

   [Fact]
   public void HasSpans_WhenSpansContainItem_ReturnsTrue()
   {
      var diagnostic = new DiagnosticInfo
      {
         Spans =
          [
              new DiagnosticSpan()
          ]
      };

      var actual = diagnostic.HasSpans();

      Assert.True(actual);
   }

   [Fact]
   public void HasSpanInfo_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;

      Assert.Throws<ArgumentNullException>(() =>
          diagnostic!.HasSpanInfo());
   }

   [Fact]
   public void HasSpanInfo_WhenSpansAreEmpty_ReturnsFalse()
   {
      var diagnostic = new DiagnosticInfo
      {
         Spans = []
      };

      var actual = diagnostic.HasSpanInfo();

      Assert.False(actual);
   }

   [Fact]
   public void HasSpanInfo_WhenSpanIsEmpty_ReturnsFalse()
   {
      var diagnostic = new DiagnosticInfo
      {
         Spans =
          [
              new DiagnosticSpan()
          ]
      };

      var actual = diagnostic.HasSpanInfo();

      Assert.False(actual);
   }

   [Fact]
   public void HasSpanInfo_WhenAnySpanHasInformation_ReturnsTrue()
   {
      var diagnostic = new DiagnosticInfo
      {
         Spans =
          [
              new DiagnosticSpan(),
                new DiagnosticSpan
                {
                    Start = new DiagnosticLocation
                    {
                        Line = 10
                    }
                }
          ]
      };

      var actual = diagnostic.HasSpanInfo();

      Assert.True(actual);
   }

   [Fact]
   public void HasHints_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;

      Assert.Throws<ArgumentNullException>(() =>
          diagnostic!.HasHints());
   }

   [Fact]
   public void HasHints_WhenHintsAreEmpty_ReturnsFalse()
   {
      var diagnostic = new DiagnosticInfo
      {
         Hints = []
      };

      var actual = diagnostic.HasHints();

      Assert.False(actual);
   }

   [Fact]
   public void HasHints_WhenHintsContainItem_ReturnsTrue()
   {
      var diagnostic = new DiagnosticInfo
      {
         Hints =
          [
              new DiagnosticHint()
          ]
      };

      var actual = diagnostic.HasHints();

      Assert.True(actual);
   }

   [Fact]
   public void HasHintMessages_WhenDiagnosticIsNull_ThrowsArgumentNullException()
   {
      DiagnosticInfo? diagnostic = null;

      Assert.Throws<ArgumentNullException>(() =>
          diagnostic!.HasHintMessages());
   }

   [Fact]
   public void HasHintMessages_WhenHintsAreEmpty_ReturnsFalse()
   {
      var diagnostic = new DiagnosticInfo
      {
         Hints = []
      };

      var actual = diagnostic.HasHintMessages();

      Assert.False(actual);
   }

   [Fact]
   public void HasHintMessages_WhenHintsContainOnlyEmptyMessages_ReturnsFalse()
   {
      var diagnostic = new DiagnosticInfo
      {
         Hints =
          [
              new DiagnosticHint
                {
                    Message = ""
                },
                new DiagnosticHint
                {
                    Message = "   "
                }
          ]
      };

      var actual = diagnostic.HasHintMessages();

      Assert.False(actual);
   }

   [Fact]
   public void HasHintMessages_WhenAnyHintHasMessage_ReturnsTrue()
   {
      var diagnostic = new DiagnosticInfo
      {
         Hints =
          [
              new DiagnosticHint
                {
                    Message = ""
                },
                new DiagnosticHint
                {
                    Message = "Use a valid value."
                }
          ]
      };

      var actual = diagnostic.HasHintMessages();

      Assert.True(actual);
   }
   [Fact]
   public void HasSpans_WhenSpansIsNull_ReturnsFalse()
   {
      var diagnostic = new DiagnosticInfo
      {
         Spans = null!
      };

      var actual = diagnostic.HasSpans();

      Assert.False(actual);
   }

   [Fact]
   public void HasSpanInfo_WhenSpansIsNull_ReturnsFalse()
   {
      var diagnostic = new DiagnosticInfo
      {
         Spans = null!
      };

      var actual = diagnostic.HasSpanInfo();

      Assert.False(actual);
   }

   [Fact]
   public void HasHints_WhenHintsIsNull_ReturnsFalse()
   {
      var diagnostic = new DiagnosticInfo
      {
         Hints = null!
      };

      var actual = diagnostic.HasHints();

      Assert.False(actual);
   }

   [Fact]
   public void HasHintMessages_WhenHintsIsNull_ReturnsFalse()
   {
      var diagnostic = new DiagnosticInfo
      {
         Hints = null!
      };

      var actual = diagnostic.HasHintMessages();

      Assert.False(actual);
   }
}