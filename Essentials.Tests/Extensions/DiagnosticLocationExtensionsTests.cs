using Afrowave.Toolbox.Essentials.Diagnostics;
using Afrowave.Toolbox.Essentials.Extensions;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class DiagnosticLocationExtensionsTests
{
   [Fact]
   public void HasSource_WhenLocationIsNull_ThrowsArgumentNullException()
   {
      DiagnosticLocation? location = null;

      Assert.Throws<ArgumentNullException>(() =>
          location!.HasSource());
   }

   [Theory]
   [InlineData(null, false)]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("appsettings.json", true)]
   [InlineData(" input ", true)]
   public void HasSource_ReturnsExpectedResult(
       string? source,
       bool expected)
   {
      var location = new DiagnosticLocation
      {
         Source = source
      };

      var actual = location.HasSource();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasLine_WhenLocationIsNull_ThrowsArgumentNullException()
   {
      DiagnosticLocation? location = null;

      Assert.Throws<ArgumentNullException>(() =>
          location!.HasLine());
   }

   [Theory]
   [InlineData(null, false)]
   [InlineData(0, true)]
   [InlineData(1, true)]
   [InlineData(42, true)]
   public void HasLine_ReturnsExpectedResult(
       int? line,
       bool expected)
   {
      var location = new DiagnosticLocation
      {
         Line = line
      };

      var actual = location.HasLine();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasColumn_WhenLocationIsNull_ThrowsArgumentNullException()
   {
      DiagnosticLocation? location = null;

      Assert.Throws<ArgumentNullException>(() =>
          location!.HasColumn());
   }

   [Theory]
   [InlineData(null, false)]
   [InlineData(0, true)]
   [InlineData(1, true)]
   [InlineData(20, true)]
   public void HasColumn_ReturnsExpectedResult(
       int? column,
       bool expected)
   {
      var location = new DiagnosticLocation
      {
         Column = column
      };

      var actual = location.HasColumn();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasOffset_WhenLocationIsNull_ThrowsArgumentNullException()
   {
      DiagnosticLocation? location = null;

      Assert.Throws<ArgumentNullException>(() =>
          location!.HasOffset());
   }

   [Theory]
   [InlineData(null, false)]
   [InlineData(0L, true)]
   [InlineData(1L, true)]
   [InlineData(1024L, true)]
   public void HasOffset_ReturnsExpectedResult(
       long? offset,
       bool expected)
   {
      var location = new DiagnosticLocation
      {
         Offset = offset
      };

      var actual = location.HasOffset();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasAnyLocationInfo_WhenLocationIsNull_ThrowsArgumentNullException()
   {
      DiagnosticLocation? location = null;

      Assert.Throws<ArgumentNullException>(() =>
          location!.HasAnyLocationInfo());
   }

   [Fact]
   public void HasAnyLocationInfo_WhenLocationIsEmpty_ReturnsFalse()
   {
      var location = new DiagnosticLocation();

      var actual = location.HasAnyLocationInfo();

      Assert.False(actual);
   }

   [Fact]
   public void HasAnyLocationInfo_WhenSourceIsSet_ReturnsTrue()
   {
      var location = new DiagnosticLocation
      {
         Source = "input.ajis"
      };

      var actual = location.HasAnyLocationInfo();

      Assert.True(actual);
   }

   [Fact]
   public void HasAnyLocationInfo_WhenLineIsSet_ReturnsTrue()
   {
      var location = new DiagnosticLocation
      {
         Line = 1
      };

      var actual = location.HasAnyLocationInfo();

      Assert.True(actual);
   }

   [Fact]
   public void HasAnyLocationInfo_WhenColumnIsSet_ReturnsTrue()
   {
      var location = new DiagnosticLocation
      {
         Column = 1
      };

      var actual = location.HasAnyLocationInfo();

      Assert.True(actual);
   }

   [Fact]
   public void HasAnyLocationInfo_WhenOffsetIsSet_ReturnsTrue()
   {
      var location = new DiagnosticLocation
      {
         Offset = 0
      };

      var actual = location.HasAnyLocationInfo();

      Assert.True(actual);
   }

   [Fact]
   public void IsEmpty_WhenLocationIsNull_ThrowsArgumentNullException()
   {
      DiagnosticLocation? location = null;

      Assert.Throws<ArgumentNullException>(() =>
          location!.IsEmpty());
   }

   [Fact]
   public void IsEmpty_WhenLocationHasNoInformation_ReturnsTrue()
   {
      var location = new DiagnosticLocation();

      var actual = location.IsEmpty();

      Assert.True(actual);
   }

   [Fact]
   public void IsEmpty_WhenLocationHasAnyInformation_ReturnsFalse()
   {
      var location = new DiagnosticLocation
      {
         Source = "input.ajis"
      };

      var actual = location.IsEmpty();

      Assert.False(actual);
   }
}