using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasMetadataExtensionsTests
{
   [Fact]
   public void HasAnyMetadata_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasMetadata? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.HasAnyMetadata());
   }

   [Fact]
   public void HasAnyMetadata_WhenMetadataIsEmpty_ReturnsFalse()
   {
      var value = new TestHasMetadata(new MetadataBag());

      var actual = value.HasAnyMetadata();

      Assert.False(actual);
   }

   [Fact]
   public void HasAnyMetadata_WhenMetadataContainsValue_ReturnsTrue()
   {
      var metadata = new MetadataBag();
      metadata.Set("key", "value");

      var value = new TestHasMetadata(metadata);

      var actual = value.HasAnyMetadata();

      Assert.True(actual);
   }

   [Fact]
   public void TryGetMetadata_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasMetadata? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.TryGetMetadata("key", out _));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void TryGetMetadata_WhenKeyIsInvalid_ThrowsArgumentException(
       string? key)
   {
      var value = new TestHasMetadata(new MetadataBag());

      Assert.ThrowsAny<ArgumentException>(() =>
          value.TryGetMetadata(key!, out _));
   }

   [Fact]
   public void TryGetMetadata_WhenKeyDoesNotExist_ReturnsFalseAndNullValue()
   {
      var value = new TestHasMetadata(new MetadataBag());

      var actual = value.TryGetMetadata("missing", out var metadataValue);

      Assert.False(actual);
      Assert.Null(metadataValue);
   }

   [Fact]
   public void TryGetMetadata_WhenKeyExists_ReturnsTrueAndValue()
   {
      var metadata = new MetadataBag();
      metadata.Set("key", "value");

      var value = new TestHasMetadata(metadata);

      var actual = value.TryGetMetadata("key", out var metadataValue);

      Assert.True(actual);
      Assert.Equal("value", metadataValue);
   }

   [Fact]
   public void TryGetMetadata_UsesCaseInsensitiveKeyLookup()
   {
      var metadata = new MetadataBag();
      metadata.Set("OriginalKey", "value");

      var value = new TestHasMetadata(metadata);

      var actual = value.TryGetMetadata("originalkey", out var metadataValue);

      Assert.True(actual);
      Assert.Equal("value", metadataValue);
   }

   [Fact]
   public void GetMetadataOrDefault_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasMetadata? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.GetMetadataOrDefault("key"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void GetMetadataOrDefault_WhenKeyIsInvalid_ThrowsArgumentException(
       string? key)
   {
      var value = new TestHasMetadata(new MetadataBag());

      Assert.ThrowsAny<ArgumentException>(() =>
          value.GetMetadataOrDefault(key!));
   }

   [Fact]
   public void GetMetadataOrDefault_WhenKeyDoesNotExistAndFallbackIsNotProvided_ReturnsNull()
   {
      var value = new TestHasMetadata(new MetadataBag());

      var actual = value.GetMetadataOrDefault("missing");

      Assert.Null(actual);
   }

   [Fact]
   public void GetMetadataOrDefault_WhenKeyDoesNotExist_ReturnsFallback()
   {
      var value = new TestHasMetadata(new MetadataBag());

      var actual = value.GetMetadataOrDefault("missing", "fallback");

      Assert.Equal("fallback", actual);
   }

   [Fact]
   public void GetMetadataOrDefault_WhenKeyExists_ReturnsValue()
   {
      var metadata = new MetadataBag();
      metadata.Set("key", "value");

      var value = new TestHasMetadata(metadata);

      var actual = value.GetMetadataOrDefault("key", "fallback");

      Assert.Equal("value", actual);
   }

   [Fact]
   public void GetMetadataOrDefault_UsesCaseInsensitiveKeyLookup()
   {
      var metadata = new MetadataBag();
      metadata.Set("OriginalKey", "value");

      var value = new TestHasMetadata(metadata);

      var actual = value.GetMetadataOrDefault("originalkey", "fallback");

      Assert.Equal("value", actual);
   }

   [Fact]
   public void CopyMetadata_WhenValueIsNull_ThrowsArgumentNullException()
   {
      IHasMetadata? value = null;

      Assert.Throws<ArgumentNullException>(() =>
          value!.CopyMetadata());
   }

   [Fact]
   public void CopyMetadata_ReturnsNewMetadataBagWithCopiedValues()
   {
      var metadata = new MetadataBag();
      metadata.Set("first", "one");
      metadata.Set("second", "two");

      var value = new TestHasMetadata(metadata);

      var copy = value.CopyMetadata();

      Assert.NotSame(metadata, copy);
      Assert.Equal(metadata.Count, copy.Count);

      Assert.True(copy.TryGet("first", out var first));
      Assert.True(copy.TryGet("second", out var second));

      Assert.Equal("one", first);
      Assert.Equal("two", second);
   }

   [Fact]
   public void CopyMetadata_WhenOriginalIsChanged_DoesNotChangeCopy()
   {
      var metadata = new MetadataBag();
      metadata.Set("key", "original");

      var value = new TestHasMetadata(metadata);

      var copy = value.CopyMetadata();

      metadata.Set("key", "changed");

      Assert.True(metadata.TryGet("key", out var originalValue));
      Assert.True(copy.TryGet("key", out var copiedValue));

      Assert.Equal("changed", originalValue);
      Assert.Equal("original", copiedValue);
   }
   [Fact]
   public void HasAnyMetadata_WhenMetadataPropertyIsNull_ReturnsFalse()
   {
      var value = new TestHasMetadata(null!);

      var actual = value.HasAnyMetadata();

      Assert.False(actual);
   }

   [Fact]
   public void TryGetMetadata_WhenMetadataPropertyIsNull_ReturnsFalseAndNullValue()
   {
      var value = new TestHasMetadata(null!);

      var actual = value.TryGetMetadata(
         "key",
         out var metadataValue);

      Assert.False(actual);
      Assert.Null(metadataValue);
   }

   [Fact]
   public void GetMetadataOrDefault_WhenMetadataPropertyIsNull_ReturnsFallback()
   {
      var value = new TestHasMetadata(null!);

      var actual = value.GetMetadataOrDefault(
         "key",
         "fallback");

      Assert.Equal("fallback", actual);
   }

   [Fact]
   public void GetMetadataOrDefault_WhenMetadataPropertyIsNullAndFallbackIsNotProvided_ReturnsNull()
   {
      var value = new TestHasMetadata(null!);

      var actual = value.GetMetadataOrDefault("key");

      Assert.Null(actual);
   }

   [Fact]
   public void CopyMetadata_WhenMetadataPropertyIsNull_ReturnsEmptyMetadataBag()
   {
      var value = new TestHasMetadata(null!);

      var copy = value.CopyMetadata();

      Assert.NotNull(copy);
      Assert.True(copy.IsEmpty);
   }

   private sealed class TestHasMetadata : IHasMetadata
   {
      public TestHasMetadata(MetadataBag metadata)
      {
         Metadata = metadata;
      }

      public MetadataBag Metadata { get; }
   }
}