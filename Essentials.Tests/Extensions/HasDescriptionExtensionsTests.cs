using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasDescriptionExtensionsTests
{
   [Fact]
   public void HasDescription_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDescription? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasDescription());
   }

   [Theory]
   [InlineData(null, false)]
   [InlineData("", false)]
   [InlineData("   ", false)]
   [InlineData("Description text", true)]
   [InlineData(" description ", true)]
   public void HasDescription_ReturnsExpectedResult(
       string? description,
       bool expected)
   {
      var value = new TestHasDescription(description);

      var actual = value.HasDescription();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasDescriptionWithDescription_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDescription? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasDescription("Description text"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void HasDescriptionWithDescription_WhenDescriptionArgumentIsInvalid_ThrowsArgumentException(
       string? description)
   {
      var value = new TestHasDescription("Description text");

      Assert.ThrowsAny<ArgumentException>(() =>
          value.HasDescription(description!));
   }

   [Theory]
   [InlineData("Description text", "Description text", true)]
   [InlineData("Description text", "description text", true)]
   [InlineData("DESCRIPTION TEXT", "description text", true)]
   [InlineData("Afrowave Toolbox Description", "afrowave toolbox description", true)]
   [InlineData("Description text", "Other description", false)]
   [InlineData(null, "Description text", false)]
   [InlineData("", "Description text", false)]
   [InlineData("   ", "Description text", false)]
   public void HasDescriptionWithDescription_ReturnsExpectedResult(
       string? actualDescription,
       string expectedDescription,
       bool expected)
   {
      var value = new TestHasDescription(actualDescription);

      var actual = value.HasDescription(expectedDescription);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void DescriptionContains_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDescription? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.DescriptionContains("description"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void DescriptionContains_WhenTextIsInvalid_ThrowsArgumentException(
       string? text)
   {
      var value = new TestHasDescription("Description text");

      Assert.ThrowsAny<ArgumentException>(() =>
          value.DescriptionContains(text!));
   }

   [Theory]
   [InlineData("This is a description text.", "description", true)]
   [InlineData("This is a description text.", "DESCRIPTION", true)]
   [InlineData("Afrowave Toolbox Essentials description", "toolbox", true)]
   [InlineData("Afrowave Toolbox Essentials description", "ESSENTIALS", true)]
   [InlineData("Afrowave Toolbox Essentials description", "missing", false)]
   [InlineData(null, "missing", false)]
   [InlineData("", "missing", false)]
   [InlineData("   ", "missing", false)]
   public void DescriptionContains_ReturnsExpectedResult(
       string? description,
       string text,
       bool expected)
   {
      var value = new TestHasDescription(description);

      var actual = value.DescriptionContains(text);

      Assert.Equal(expected, actual);
   }

   private sealed class TestHasDescription : IHasDescription
   {
      public TestHasDescription(string? description)
      {
         Description = description;
      }

      public string? Description { get; }
   }
}