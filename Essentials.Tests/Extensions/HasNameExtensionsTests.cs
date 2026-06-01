using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasNameExtensionsTests
{
   [Fact]
   public void HasName_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasName? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasName("Test Name"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void HasName_WhenNameIsInvalid_ThrowsArgumentException(
       string? name)
   {
      var value = new TestHasName("Test Name");

      Assert.ThrowsAny<ArgumentException>(() =>
          value.HasName(name!));
   }

   [Theory]
   [InlineData("Test Name", "Test Name", true)]
   [InlineData("Test Name", "test name", true)]
   [InlineData("test name", "TEST NAME", true)]
   [InlineData("Ollama Local Provider", "ollama local provider", true)]
   [InlineData("Test Name", "Other Name", false)]
   [InlineData("", "Test Name", false)]
   [InlineData("   ", "Test Name", false)]
   public void HasName_ReturnsExpectedResult(
       string actualName,
       string expectedName,
       bool expected)
   {
      var value = new TestHasName(actualName);

      var actual = value.HasName(expectedName);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasSameNameAs_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasName? value = null;
      var other = new TestHasName("Test Name");

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasSameNameAs(other));
   }

   [Fact]
   public void HasSameNameAs_WhenOtherIsNull_ThrowsArgumentNullException()
   {
      var value = new TestHasName("Test Name");
      IHasName? other = null;

      Assert.Throws<ArgumentNullException>(() =>
          value.HasSameNameAs(other!));
   }

   [Theory]
   [InlineData("Test Name", "Test Name", true)]
   [InlineData("Test Name", "test name", true)]
   [InlineData("test name", "TEST NAME", true)]
   [InlineData("Ollama Local Provider", "ollama local provider", true)]
   [InlineData("Test Name", "Other Name", false)]
   [InlineData("", "", true)]
   [InlineData("   ", "   ", true)]
   [InlineData("", "   ", false)]
   public void HasSameNameAs_ReturnsExpectedResult(
       string firstName,
       string secondName,
       bool expected)
   {
      var value = new TestHasName(firstName);
      var other = new TestHasName(secondName);

      var actual = value.HasSameNameAs(other);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasEmptyName_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasName? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasEmptyName());
   }

   [Theory]
   [InlineData("", true)]
   [InlineData("   ", true)]
   [InlineData("Test Name", false)]
   [InlineData(" test ", false)]
   public void HasEmptyName_ReturnsExpectedResult(
       string name,
       bool expected)
   {
      var value = new TestHasName(name);

      var actual = value.HasEmptyName();

      Assert.Equal(expected, actual);
   }

   private sealed class TestHasName : IHasName
   {
      public TestHasName(string name)
      {
         Name = name;
      }

      public string Name { get; }
   }
   [Fact]
   public void HasName_WhenSourceNameIsNull_ReturnsFalse()
   {
      var value = new TestHasName(null!);

      var actual = value.HasName("Test Name");

      Assert.False(actual);
   }

   [Fact]
   public void HasEmptyName_WhenNameIsNull_ReturnsTrue()
   {
      var value = new TestHasName(null!);

      var actual = value.HasEmptyName();

      Assert.True(actual);
   }
   [Fact]
   public void HasSameNameAs_WhenFirstNameIsNull_ReturnsFalse()
   {
      var value = new TestHasName(null!);
      var other = new TestHasName("Test Name");

      var actual = value.HasSameNameAs(other);

      Assert.False(actual);
   }

   [Fact]
   public void HasSameNameAs_WhenSecondNameIsNull_ReturnsFalse()
   {
      var value = new TestHasName("Test Name");
      var other = new TestHasName(null!);

      var actual = value.HasSameNameAs(other);

      Assert.False(actual);
   }

   [Fact]
   public void HasSameNameAs_WhenBothNamesAreNull_ReturnsTrue()
   {
      var value = new TestHasName(null!);
      var other = new TestHasName(null!);

      var actual = value.HasSameNameAs(other);

      Assert.True(actual);
   }
}