using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class MetadataBagExtensionsTests
{
   [Fact]
   public void SetIfNotWhiteSpace_WhenMetadataIsNull_ThrowsArgumentNullException()
   {
      MetadataBag? metadata = null;

      Assert.Throws<ArgumentNullException>(() =>
          metadata!.SetIfNotWhiteSpace("key", "value"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void SetIfNotWhiteSpace_WhenKeyIsInvalid_ThrowsArgumentException(
       string? key)
   {
      var metadata = new MetadataBag();

      Assert.ThrowsAny<ArgumentException>(() =>
          metadata.SetIfNotWhiteSpace(key!, "value"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void SetIfNotWhiteSpace_WhenValueIsNullEmptyOrWhiteSpace_DoesNotSetValue(
       string? value)
   {
      var metadata = new MetadataBag();

      var actual = metadata.SetIfNotWhiteSpace("key", value);

      Assert.Same(metadata, actual);
      Assert.False(metadata.TryGet("key", out _));
      Assert.True(metadata.IsEmpty);
   }

   [Fact]
   public void SetIfNotWhiteSpace_WhenValueHasText_SetsValue()
   {
      var metadata = new MetadataBag();

      var actual = metadata.SetIfNotWhiteSpace("key", "value");

      Assert.Same(metadata, actual);
      Assert.True(metadata.TryGet("key", out var value));
      Assert.Equal("value", value);
   }

   [Fact]
   public void SetIfNotNull_WhenMetadataIsNull_ThrowsArgumentNullException()
   {
      MetadataBag? metadata = null;

      Assert.Throws<ArgumentNullException>(() =>
          metadata!.SetIfNotNull("key", "value"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void SetIfNotNull_WhenKeyIsInvalid_ThrowsArgumentException(
       string? key)
   {
      var metadata = new MetadataBag();

      Assert.ThrowsAny<ArgumentException>(() =>
          metadata.SetIfNotNull(key!, "value"));
   }

   [Fact]
   public void SetIfNotNull_WhenValueIsNull_DoesNotSetValue()
   {
      var metadata = new MetadataBag();

      var actual = metadata.SetIfNotNull("key", null);

      Assert.Same(metadata, actual);
      Assert.False(metadata.TryGet("key", out _));
      Assert.True(metadata.IsEmpty);
   }

   [Fact]
   public void SetIfNotNull_WhenValueIsString_SetsValue()
   {
      var metadata = new MetadataBag();

      var actual = metadata.SetIfNotNull("key", "value");

      Assert.Same(metadata, actual);
      Assert.True(metadata.TryGet("key", out var value));
      Assert.Equal("value", value);
   }

   [Fact]
   public void SetIfNotNull_WhenValueIsNumber_SetsStringRepresentation()
   {
      var metadata = new MetadataBag();

      var actual = metadata.SetIfNotNull("answer", 42);

      Assert.Same(metadata, actual);
      Assert.True(metadata.TryGet("answer", out var value));
      Assert.Equal("42", value);
   }

   [Fact]
   public void MergeFrom_WhenMetadataIsNull_ThrowsArgumentNullException()
   {
      MetadataBag? metadata = null;
      var other = new MetadataBag();

      Assert.Throws<ArgumentNullException>(() =>
          metadata!.MergeFrom(other));
   }

   [Fact]
   public void MergeFrom_WhenOtherIsNull_ThrowsArgumentNullException()
   {
      var metadata = new MetadataBag();
      MetadataBag? other = null;

      Assert.Throws<ArgumentNullException>(() =>
          metadata.MergeFrom(other!));
   }

   [Fact]
   public void MergeFrom_WhenOtherIsEmpty_DoesNotChangeTarget()
   {
      var metadata = new MetadataBag();
      metadata.Set("existing", "value");

      var other = new MetadataBag();

      var actual = metadata.MergeFrom(other);

      Assert.Same(metadata, actual);
      Assert.Equal(1, metadata.Count);
      Assert.True(metadata.TryGet("existing", out var value));
      Assert.Equal("value", value);
   }

   [Fact]
   public void MergeFrom_CopiesAllValuesFromOther()
   {
      var metadata = new MetadataBag();

      var other = new MetadataBag();
      other.Set("first", "one");
      other.Set("second", "two");

      var actual = metadata.MergeFrom(other);

      Assert.Same(metadata, actual);
      Assert.Equal(2, metadata.Count);

      Assert.True(metadata.TryGet("first", out var first));
      Assert.True(metadata.TryGet("second", out var second));

      Assert.Equal("one", first);
      Assert.Equal("two", second);
   }

   [Fact]
   public void MergeFrom_WhenKeyAlreadyExists_OverwritesExistingValue()
   {
      var metadata = new MetadataBag();
      metadata.Set("key", "old");

      var other = new MetadataBag();
      other.Set("key", "new");

      metadata.MergeFrom(other);

      Assert.True(metadata.TryGet("key", out var value));
      Assert.Equal("new", value);
   }

   [Fact]
   public void Copy_WhenMetadataIsNull_ThrowsArgumentNullException()
   {
      MetadataBag? metadata = null;

      Assert.Throws<ArgumentNullException>(() =>
          metadata!.Copy());
   }

   [Fact]
   public void Copy_ReturnsNewMetadataBagWithCopiedValues()
   {
      var metadata = new MetadataBag();
      metadata.Set("first", "one");
      metadata.Set("second", "two");

      var copy = metadata.Copy();

      Assert.NotSame(metadata, copy);
      Assert.Equal(metadata.Count, copy.Count);

      Assert.True(copy.TryGet("first", out var first));
      Assert.True(copy.TryGet("second", out var second));

      Assert.Equal("one", first);
      Assert.Equal("two", second);
   }

   [Fact]
   public void Copy_WhenOriginalIsChanged_DoesNotChangeCopy()
   {
      var metadata = new MetadataBag();
      metadata.Set("key", "original");

      var copy = metadata.Copy();

      metadata.Set("key", "changed");

      Assert.True(metadata.TryGet("key", out var originalValue));
      Assert.True(copy.TryGet("key", out var copiedValue));

      Assert.Equal("changed", originalValue);
      Assert.Equal("original", copiedValue);
   }

   [Fact]
   public void Copy_PreservesCaseInsensitiveLookupBehavior()
   {
      var metadata = new MetadataBag();
      metadata.Set("OriginalKey", "value");

      var copy = metadata.Copy();

      Assert.True(copy.TryGet("originalkey", out var value));
      Assert.Equal("value", value);
   }
}