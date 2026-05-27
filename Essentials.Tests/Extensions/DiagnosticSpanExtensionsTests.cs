using Afrowave.Toolbox.Essentials.Diagnostics;
using Afrowave.Toolbox.Essentials.Extensions;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class DiagnosticSpanExtensionsTests
{
   [Fact]
   public void HasEnd_WhenSpanIsNull_ThrowsArgumentNullException()
   {
      DiagnosticSpan? span = null;

      Assert.Throws<ArgumentNullException>(() =>
          span!.HasEnd());
   }

   [Fact]
   public void HasEnd_WhenEndIsNull_ReturnsFalse()
   {
      var span = new DiagnosticSpan();

      var actual = span.HasEnd();

      Assert.False(actual);
   }

   [Fact]
   public void HasEnd_WhenEndIsSet_ReturnsTrue()
   {
      var span = new DiagnosticSpan
      {
         End = new DiagnosticLocation
         {
            Line = 2
         }
      };

      var actual = span.HasEnd();

      Assert.True(actual);
   }

   [Fact]
   public void HasLabel_WhenSpanIsNull_ThrowsArgumentNullException()
   {
      DiagnosticSpan? span = null;

      Assert.Throws<ArgumentNullException>(() =>
          span!.HasLabel());
   }

   [Theory]
   [InlineData(null, false)]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("highlight", true)]
   [InlineData(" label ", true)]
   public void HasLabel_ReturnsExpectedResult(
       string? label,
       bool expected)
   {
      var span = new DiagnosticSpan
      {
         Label = label
      };

      var actual = span.HasLabel();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasAnySpanInfo_WhenSpanIsNull_ThrowsArgumentNullException()
   {
      DiagnosticSpan? span = null;

      Assert.Throws<ArgumentNullException>(() =>
          span!.HasAnySpanInfo());
   }

   [Fact]
   public void HasAnySpanInfo_WhenSpanIsEmpty_ReturnsFalse()
   {
      var span = new DiagnosticSpan();

      var actual = span.HasAnySpanInfo();

      Assert.False(actual);
   }

   [Fact]
   public void HasAnySpanInfo_WhenStartHasInformation_ReturnsTrue()
   {
      var span = new DiagnosticSpan
      {
         Start = new DiagnosticLocation
         {
            Source = "input.ajis"
         }
      };

      var actual = span.HasAnySpanInfo();

      Assert.True(actual);
   }

   [Fact]
   public void HasAnySpanInfo_WhenEndHasInformation_ReturnsTrue()
   {
      var span = new DiagnosticSpan
      {
         End = new DiagnosticLocation
         {
            Line = 10
         }
      };

      var actual = span.HasAnySpanInfo();

      Assert.True(actual);
   }

   [Fact]
   public void HasAnySpanInfo_WhenEndExistsButIsEmpty_ReturnsFalse()
   {
      var span = new DiagnosticSpan
      {
         End = new DiagnosticLocation()
      };

      var actual = span.HasAnySpanInfo();

      Assert.False(actual);
   }

   [Fact]
   public void HasAnySpanInfo_WhenLabelIsSet_ReturnsTrue()
   {
      var span = new DiagnosticSpan
      {
         Label = "problem area"
      };

      var actual = span.HasAnySpanInfo();

      Assert.True(actual);
   }

   [Fact]
   public void IsEmpty_WhenSpanIsNull_ThrowsArgumentNullException()
   {
      DiagnosticSpan? span = null;

      Assert.Throws<ArgumentNullException>(() =>
          span!.IsEmpty());
   }

   [Fact]
   public void IsEmpty_WhenSpanHasNoInformation_ReturnsTrue()
   {
      var span = new DiagnosticSpan();

      var actual = span.IsEmpty();

      Assert.True(actual);
   }

   [Fact]
   public void IsEmpty_WhenStartHasInformation_ReturnsFalse()
   {
      var span = new DiagnosticSpan
      {
         Start = new DiagnosticLocation
         {
            Offset = 0
         }
      };

      var actual = span.IsEmpty();

      Assert.False(actual);
   }

   [Fact]
   public void IsEmpty_WhenEndHasInformation_ReturnsFalse()
   {
      var span = new DiagnosticSpan
      {
         End = new DiagnosticLocation
         {
            Column = 5
         }
      };

      var actual = span.IsEmpty();

      Assert.False(actual);
   }

   [Fact]
   public void IsEmpty_WhenLabelIsSet_ReturnsFalse()
   {
      var span = new DiagnosticSpan
      {
         Label = "highlight"
      };

      var actual = span.IsEmpty();

      Assert.False(actual);
   }
}