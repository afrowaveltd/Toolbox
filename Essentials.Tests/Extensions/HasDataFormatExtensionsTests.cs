using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasDataFormatExtensionsTests
{
   [Fact]
   public void HasDataFormat_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDataFormat? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasDataFormat(DataFormat.Json));
   }

   [Theory]
   [InlineData(DataFormat.Unknown, DataFormat.Unknown, true)]
   [InlineData(DataFormat.GenericText, DataFormat.GenericText, true)]
   [InlineData(DataFormat.PlainText, DataFormat.PlainText, true)]
   [InlineData(DataFormat.Json, DataFormat.Json, true)]
   [InlineData(DataFormat.Ajis, DataFormat.Ajis, true)]
   [InlineData(DataFormat.Xml, DataFormat.Xml, true)]
   [InlineData(DataFormat.Csv, DataFormat.Csv, true)]
   [InlineData(DataFormat.Markdown, DataFormat.Markdown, true)]
   [InlineData(DataFormat.Html, DataFormat.Html, true)]
   [InlineData(DataFormat.Yaml, DataFormat.Yaml, true)]
   [InlineData(DataFormat.GenericBinary, DataFormat.GenericBinary, true)]
   [InlineData(DataFormat.Custom, DataFormat.Custom, true)]
   [InlineData(DataFormat.Json, DataFormat.Ajis, false)]
   [InlineData(DataFormat.GenericText, DataFormat.GenericBinary, false)]
   [InlineData(DataFormat.Unknown, DataFormat.Custom, false)]
   public void HasDataFormat_ReturnsExpectedResult(
       DataFormat currentDataFormat,
       DataFormat expectedDataFormat,
       bool expected)
   {
      var value = new TestHasDataFormat(currentDataFormat);

      var actual = value.HasDataFormat(expectedDataFormat);

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasTextBasedDataFormat_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDataFormat? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasTextBasedDataFormat());
   }

   [Theory]
   [InlineData(DataFormat.Unknown, false)]
   [InlineData(DataFormat.GenericText, true)]
   [InlineData(DataFormat.PlainText, true)]
   [InlineData(DataFormat.Json, true)]
   [InlineData(DataFormat.Ajis, true)]
   [InlineData(DataFormat.Xml, true)]
   [InlineData(DataFormat.Csv, true)]
   [InlineData(DataFormat.Markdown, true)]
   [InlineData(DataFormat.Html, true)]
   [InlineData(DataFormat.Yaml, true)]
   [InlineData(DataFormat.GenericBinary, false)]
   [InlineData(DataFormat.Custom, false)]
   public void HasTextBasedDataFormat_ReturnsExpectedResult(
       DataFormat dataFormat,
       bool expected)
   {
      var value = new TestHasDataFormat(dataFormat);

      var actual = value.HasTextBasedDataFormat();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasBinaryDataFormat_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDataFormat? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasBinaryDataFormat());
   }

   [Theory]
   [InlineData(DataFormat.Unknown, false)]
   [InlineData(DataFormat.GenericText, false)]
   [InlineData(DataFormat.PlainText, false)]
   [InlineData(DataFormat.Json, false)]
   [InlineData(DataFormat.Ajis, false)]
   [InlineData(DataFormat.Xml, false)]
   [InlineData(DataFormat.Csv, false)]
   [InlineData(DataFormat.Markdown, false)]
   [InlineData(DataFormat.Html, false)]
   [InlineData(DataFormat.Yaml, false)]
   [InlineData(DataFormat.GenericBinary, true)]
   [InlineData(DataFormat.Custom, false)]
   public void HasBinaryDataFormat_ReturnsExpectedResult(
       DataFormat dataFormat,
       bool expected)
   {
      var value = new TestHasDataFormat(dataFormat);

      var actual = value.HasBinaryDataFormat();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasStructuredTextDataFormat_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDataFormat? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasStructuredTextDataFormat());
   }

   [Theory]
   [InlineData(DataFormat.Unknown, false)]
   [InlineData(DataFormat.GenericText, false)]
   [InlineData(DataFormat.PlainText, false)]
   [InlineData(DataFormat.Json, true)]
   [InlineData(DataFormat.Ajis, true)]
   [InlineData(DataFormat.Xml, true)]
   [InlineData(DataFormat.Csv, true)]
   [InlineData(DataFormat.Markdown, true)]
   [InlineData(DataFormat.Html, true)]
   [InlineData(DataFormat.Yaml, true)]
   [InlineData(DataFormat.GenericBinary, false)]
   [InlineData(DataFormat.Custom, false)]
   public void HasStructuredTextDataFormat_ReturnsExpectedResult(
       DataFormat dataFormat,
       bool expected)
   {
      var value = new TestHasDataFormat(dataFormat);

      var actual = value.HasStructuredTextDataFormat();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasUnknownDataFormat_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDataFormat? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasUnknownDataFormat());
   }

   [Theory]
   [InlineData(DataFormat.Unknown, true)]
   [InlineData(DataFormat.GenericText, false)]
   [InlineData(DataFormat.PlainText, false)]
   [InlineData(DataFormat.Json, false)]
   [InlineData(DataFormat.Ajis, false)]
   [InlineData(DataFormat.Xml, false)]
   [InlineData(DataFormat.Csv, false)]
   [InlineData(DataFormat.Markdown, false)]
   [InlineData(DataFormat.Html, false)]
   [InlineData(DataFormat.Yaml, false)]
   [InlineData(DataFormat.GenericBinary, false)]
   [InlineData(DataFormat.Custom, false)]
   public void HasUnknownDataFormat_ReturnsExpectedResult(
       DataFormat dataFormat,
       bool expected)
   {
      var value = new TestHasDataFormat(dataFormat);

      var actual = value.HasUnknownDataFormat();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void HasCustomDataFormat_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasDataFormat? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasCustomDataFormat());
   }

   [Theory]
   [InlineData(DataFormat.Unknown, false)]
   [InlineData(DataFormat.GenericText, false)]
   [InlineData(DataFormat.PlainText, false)]
   [InlineData(DataFormat.Json, false)]
   [InlineData(DataFormat.Ajis, false)]
   [InlineData(DataFormat.Xml, false)]
   [InlineData(DataFormat.Csv, false)]
   [InlineData(DataFormat.Markdown, false)]
   [InlineData(DataFormat.Html, false)]
   [InlineData(DataFormat.Yaml, false)]
   [InlineData(DataFormat.GenericBinary, false)]
   [InlineData(DataFormat.Custom, true)]
   public void HasCustomDataFormat_ReturnsExpectedResult(
       DataFormat dataFormat,
       bool expected)
   {
      var value = new TestHasDataFormat(dataFormat);

      var actual = value.HasCustomDataFormat();

      Assert.Equal(expected, actual);
   }

   private sealed class TestHasDataFormat : IHasDataFormat
   {
      public TestHasDataFormat(DataFormat dataFormat)
      {
         DataFormat = dataFormat;
      }

      public DataFormat DataFormat { get; }
   }
}