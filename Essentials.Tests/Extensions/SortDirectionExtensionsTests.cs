using Afrowave.Toolbox.Essentials.Enums;
using Afrowave.Toolbox.Essentials.Extensions;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class SortDirectionExtensionsTests
{
   [Theory]
   [InlineData(SortDirection.None, false)]
   [InlineData(SortDirection.Ascending, true)]
   [InlineData(SortDirection.Descending, false)]
   public void IsAscending_ReturnsExpectedResult(
       SortDirection sortDirection,
       bool expected)
   {
      var actual = sortDirection.IsAscending();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(SortDirection.None, false)]
   [InlineData(SortDirection.Ascending, false)]
   [InlineData(SortDirection.Descending, true)]
   public void IsDescending_ReturnsExpectedResult(
       SortDirection sortDirection,
       bool expected)
   {
      var actual = sortDirection.IsDescending();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(SortDirection.None, false)]
   [InlineData(SortDirection.Ascending, true)]
   [InlineData(SortDirection.Descending, true)]
   public void IsSpecified_ReturnsExpectedResult(
       SortDirection sortDirection,
       bool expected)
   {
      var actual = sortDirection.IsSpecified();

      Assert.Equal(expected, actual);
   }

   [Theory]
   [InlineData(SortDirection.None, SortDirection.None)]
   [InlineData(SortDirection.Ascending, SortDirection.Descending)]
   [InlineData(SortDirection.Descending, SortDirection.Ascending)]
   public void Reverse_ReturnsExpectedResult(
       SortDirection sortDirection,
       SortDirection expected)
   {
      var actual = sortDirection.Reverse();

      Assert.Equal(expected, actual);
   }
}