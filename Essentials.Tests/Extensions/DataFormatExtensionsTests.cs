using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class DataFormatExtensionsTests
{
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
   public void IsTextBased_ReturnsExpectedResult(
       DataFormat format,
       bool expected)
   {
      var actual = format.IsTextBased();

      Assert.Equal(expected, actual);
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
   public void IsBinary_ReturnsExpectedResult(
       DataFormat format,
       bool expected)
   {
      var actual = format.IsBinary();

      Assert.Equal(expected, actual);
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
   public void IsStructuredText_ReturnsExpectedResult(
       DataFormat format,
       bool expected)
   {
      var actual = format.IsStructuredText();

      Assert.Equal(expected, actual);
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
   public void IsUnknown_ReturnsExpectedResult(
       DataFormat format,
       bool expected)
   {
      var actual = format.IsUnknown();

      Assert.Equal(expected, actual);
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
   public void IsCustom_ReturnsExpectedResult(
       DataFormat format,
       bool expected)
   {
      var actual = format.IsCustom();

      Assert.Equal(expected, actual);
   }
}