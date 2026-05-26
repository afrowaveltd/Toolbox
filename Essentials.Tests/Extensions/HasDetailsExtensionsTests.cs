using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasDetailsExtensionsTests
{
   [Fact]
   public void HasDetails_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDetails? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasDetails());
   }

   [Theory]
   [InlineData(null, false)]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("Detailed information", true)]
   [InlineData(" details ", true)]
   public void HasDetails_ReturnsExpectedResult(
       string? details,
       bool expected)
   {
      var value = new TestHasDetails(details);

      var actual = value.HasDetails();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasDetailsWithDetails_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDetails? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasDetails("Detailed information"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void HasDetailsWithDetails_WhenDetailsArgumentIsInvalid_ThrowsArgumentException(
       string? details)
   {
      var value = new TestHasDetails("Detailed information");

      Assert.ThrowsAny<ArgumentException>(() =>
          value.HasDetails(details!));
   }

   [Theory]
   [InlineData("Detailed information", "Detailed information", true)]
   [InlineData("Detailed information", "detailed information", true)]
   [InlineData("DETAILED INFORMATION", "detailed information", true)]
   [InlineData("Afrowave Toolbox Details", "afrowave toolbox details", true)]
   [InlineData("Detailed information", "Other information", false)]
   [InlineData(null, "Detailed information", false)]
   [InlineData("", "Detailed information", false)]
   [InlineData("   ", "Detailed information", false)]
   public void HasDetailsWithDetails_ReturnsExpectedResult(
       string? actualDetails,
       string expectedDetails,
       bool expected)
   {
      var value = new TestHasDetails(actualDetails);

      var actual = value.HasDetails(expectedDetails);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void DetailsContains_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDetails? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.DetailsContains("details"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void DetailsContains_WhenTextIsInvalid_ThrowsArgumentException(
       string? text)
   {
      var value = new TestHasDetails("Detailed information");

      Assert.ThrowsAny<ArgumentException>(() =>
          value.DetailsContains(text!));
   }

   [Theory]
   [InlineData("This is detailed information.", "detailed", true)]
   [InlineData("This is detailed information.", "DETAILED", true)]
   [InlineData("Afrowave Toolbox Essentials details", "toolbox", true)]
   [InlineData("Afrowave Toolbox Essentials details", "ESSENTIALS", true)]
   [InlineData("Afrowave Toolbox Essentials details", "missing", false)]
   [InlineData(null, "missing", false)]
   [InlineData("", "missing", false)]
   [InlineData("   ", "missing", false)]
   public void DetailsContains_ReturnsExpectedResult(
       string? details,
       string text,
       bool expected)
   {
      var value = new TestHasDetails(details);

      var actual = value.DetailsContains(text);

      Assert.Equal(expected, actual);
   }

   private sealed class TestHasDetails : IHasDetails
   {
      public TestHasDetails(string? details)
      {
         Details = details;
      }

      public string? Details { get; }
   }
}