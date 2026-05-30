using Afrowave.Toolbox.Essentials.Metadata;

namespace Afrowave.Toolbox.Essentials.Tests.Metadata;

public sealed class MetadataBagFactoryTests
{
   [Fact]
   public void Empty_ReturnsEmptyMetadataBag()
   {
      var metadata = MetadataBagFactory.Empty();

      Assert.NotNull(metadata);
      Assert.True(metadata.IsEmpty);
      Assert.Equal(0, metadata.Count);
   }

   [Fact]
   public void Empty_ReturnsNewInstanceEachTime()
   {
      var first = MetadataBagFactory.Empty();
      var second = MetadataBagFactory.Empty();

      Assert.NotSame(first, second);
   }

   [Fact]
   public void From_WithSingleKeyValue_CreatesMetadataBagWithValue()
   {
      var metadata = MetadataBagFactory.From("provider", "ollama-local");

      Assert.False(metadata.IsEmpty);
      Assert.Equal(1, metadata.Count);

      Assert.True(metadata.TryGet("provider", out var value));
      Assert.Equal("ollama-local", value);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void From_WithSingleKeyValue_WhenKeyIsInvalid_ThrowsArgumentException(
       string? key)
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          MetadataBagFactory.From(key!, "value"));
   }

   [Fact]
   public void From_WithKeyValuePairs_WhenItemsIsNull_ThrowsArgumentNullException()
   {
      IEnumerable<KeyValuePair<string, string>>? items = null;

      Assert.Throws<ArgumentNullException>(() =>
          MetadataBagFactory.From(items!));
   }

   [Fact]
   public void From_WithKeyValuePairs_WhenItemsAreEmpty_ReturnsEmptyMetadataBag()
   {
      var items = Array.Empty<KeyValuePair<string, string>>();

      var metadata = MetadataBagFactory.From(items);

      Assert.NotNull(metadata);
      Assert.True(metadata.IsEmpty);
      Assert.Equal(0, metadata.Count);
   }

   [Fact]
   public void From_WithKeyValuePairs_CreatesMetadataBagWithValues()
   {
      var items = new[]
      {
            new KeyValuePair<string, string>("provider", "ollama-local"),
            new KeyValuePair<string, string>("model", "llama3.2"),
            new KeyValuePair<string, string>("profile", "markdown-refine")
        };

      var metadata = MetadataBagFactory.From(items);

      Assert.Equal(3, metadata.Count);

      Assert.True(metadata.TryGet("provider", out var provider));
      Assert.True(metadata.TryGet("model", out var model));
      Assert.True(metadata.TryGet("profile", out var profile));

      Assert.Equal("ollama-local", provider);
      Assert.Equal("llama3.2", model);
      Assert.Equal("markdown-refine", profile);
   }

   [Fact]
   public void From_WithKeyValuePairs_WhenDuplicateKeysAreProvided_UsesLastValue()
   {
      var items = new[]
      {
            new KeyValuePair<string, string>("key", "first"),
            new KeyValuePair<string, string>("key", "second")
        };

      var metadata = MetadataBagFactory.From(items);

      Assert.Equal(1, metadata.Count);
      Assert.True(metadata.TryGet("key", out var value));
      Assert.Equal("second", value);
   }

   [Fact]
   public void From_WithKeyValuePairs_PreservesCaseInsensitiveLookupBehavior()
   {
      var items = new[]
      {
            new KeyValuePair<string, string>("OriginalKey", "value")
        };

      var metadata = MetadataBagFactory.From(items);

      Assert.True(metadata.TryGet("originalkey", out var value));
      Assert.Equal("value", value);
   }

   [Fact]
   public void From_WithKeyValuePairs_WhenAnyKeyIsInvalid_ThrowsArgumentException()
   {
      var items = new[]
      {
            new KeyValuePair<string, string>("valid", "value"),
            new KeyValuePair<string, string>("   ", "invalid")
        };

      Assert.ThrowsAny<ArgumentException>(() =>
          MetadataBagFactory.From(items));
   }

   [Fact]
   public void From_WithTuples_WhenItemsIsNull_ThrowsArgumentNullException()
   {
      (string Key, string Value)[]? items = null;

      Assert.Throws<ArgumentNullException>(() =>
          MetadataBagFactory.From(items!));
   }

   [Fact]
   public void From_WithTuples_WhenItemsAreEmpty_ReturnsEmptyMetadataBag()
   {
      var metadata = MetadataBagFactory.From(Array.Empty<(string Key, string Value)>());

      Assert.NotNull(metadata);
      Assert.True(metadata.IsEmpty);
      Assert.Equal(0, metadata.Count);
   }

   [Fact]
   public void From_WithTuples_CreatesMetadataBagWithValues()
   {
      var metadata = MetadataBagFactory.From(
          ("provider", "ollama-local"),
          ("model", "llama3.2"),
          ("profile", "markdown-refine"));

      Assert.Equal(3, metadata.Count);

      Assert.True(metadata.TryGet("provider", out var provider));
      Assert.True(metadata.TryGet("model", out var model));
      Assert.True(metadata.TryGet("profile", out var profile));

      Assert.Equal("ollama-local", provider);
      Assert.Equal("llama3.2", model);
      Assert.Equal("markdown-refine", profile);
   }

   [Fact]
   public void From_WithTuples_WhenDuplicateKeysAreProvided_UsesLastValue()
   {
      var metadata = MetadataBagFactory.From(
          ("key", "first"),
          ("key", "second"));

      Assert.Equal(1, metadata.Count);
      Assert.True(metadata.TryGet("key", out var value));
      Assert.Equal("second", value);
   }

   [Fact]
   public void From_WithTuples_PreservesCaseInsensitiveLookupBehavior()
   {
      var metadata = MetadataBagFactory.From(
          ("OriginalKey", "value"));

      Assert.True(metadata.TryGet("originalkey", out var value));
      Assert.Equal("value", value);
   }

   [Fact]
   public void From_WithTuples_WhenAnyKeyIsInvalid_ThrowsArgumentException()
   {
      Assert.ThrowsAny<ArgumentException>(() =>
          MetadataBagFactory.From(
              ("valid", "value"),
              ("   ", "invalid")));
   }

   [Fact]
   public void CopyFrom_WhenMetadataIsNull_ThrowsArgumentNullException()
   {
      MetadataBag? metadata = null;

      Assert.Throws<ArgumentNullException>(() =>
          MetadataBagFactory.CopyFrom(metadata!));
   }

   [Fact]
   public void CopyFrom_WhenMetadataIsEmpty_ReturnsEmptyMetadataBag()
   {
      var metadata = new MetadataBag();

      var copy = MetadataBagFactory.CopyFrom(metadata);

      Assert.NotSame(metadata, copy);
      Assert.True(copy.IsEmpty);
      Assert.Equal(0, copy.Count);
   }

   [Fact]
   public void CopyFrom_ReturnsNewMetadataBagWithCopiedValues()
   {
      var metadata = new MetadataBag();
      metadata.Set("first", "one");
      metadata.Set("second", "two");

      var copy = MetadataBagFactory.CopyFrom(metadata);

      Assert.NotSame(metadata, copy);
      Assert.Equal(metadata.Count, copy.Count);

      Assert.True(copy.TryGet("first", out var first));
      Assert.True(copy.TryGet("second", out var second));

      Assert.Equal("one", first);
      Assert.Equal("two", second);
   }

   [Fact]
   public void CopyFrom_WhenOriginalIsChanged_DoesNotChangeCopy()
   {
      var metadata = new MetadataBag();
      metadata.Set("key", "original");

      var copy = MetadataBagFactory.CopyFrom(metadata);

      metadata.Set("key", "changed");

      Assert.True(metadata.TryGet("key", out var originalValue));
      Assert.True(copy.TryGet("key", out var copiedValue));

      Assert.Equal("changed", originalValue);
      Assert.Equal("original", copiedValue);
   }

   [Fact]
   public void CopyFrom_PreservesCaseInsensitiveLookupBehavior()
   {
      var metadata = new MetadataBag();
      metadata.Set("OriginalKey", "value");

      var copy = MetadataBagFactory.CopyFrom(metadata);

      Assert.True(copy.TryGet("originalkey", out var value));
      Assert.Equal("value", value);
   }

   [Fact]
   public void CopyWith_WhenMetadataIsNull_ThrowsArgumentNullException()
   {
      MetadataBag? metadata = null;

      Assert.Throws<ArgumentNullException>(() =>
          MetadataBagFactory.CopyWith(
              metadata!,
              "key",
              "value"));
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   [InlineData("   ")]
   public void CopyWith_WhenKeyIsInvalid_ThrowsArgumentException(
       string? key)
   {
      var metadata = new MetadataBag();

      Assert.ThrowsAny<ArgumentException>(() =>
          MetadataBagFactory.CopyWith(
              metadata,
              key!,
              "value"));
   }

   [Fact]
   public void CopyWith_WhenMetadataIsEmpty_ReturnsCopyWithValue()
   {
      var metadata = new MetadataBag();

      var copy = MetadataBagFactory.CopyWith(
          metadata,
          "source",
          "unit-test");

      Assert.NotSame(metadata, copy);
      Assert.True(metadata.IsEmpty);

      Assert.Equal(1, copy.Count);
      Assert.True(copy.TryGet("source", out var value));
      Assert.Equal("unit-test", value);
   }

   [Fact]
   public void CopyWith_CopiesExistingValuesAndSetsNewValue()
   {
      var metadata = new MetadataBag();
      metadata.Set("first", "one");
      metadata.Set("second", "two");

      var copy = MetadataBagFactory.CopyWith(
          metadata,
          "third",
          "three");

      Assert.NotSame(metadata, copy);

      Assert.Equal(2, metadata.Count);
      Assert.Equal(3, copy.Count);

      Assert.True(copy.TryGet("first", out var first));
      Assert.True(copy.TryGet("second", out var second));
      Assert.True(copy.TryGet("third", out var third));

      Assert.Equal("one", first);
      Assert.Equal("two", second);
      Assert.Equal("three", third);
   }

   [Fact]
   public void CopyWith_WhenKeyAlreadyExists_OverridesValueInCopyOnly()
   {
      var metadata = new MetadataBag();
      metadata.Set("key", "original");

      var copy = MetadataBagFactory.CopyWith(
          metadata,
          "key",
          "changed");

      Assert.NotSame(metadata, copy);

      Assert.True(metadata.TryGet("key", out var originalValue));
      Assert.True(copy.TryGet("key", out var copiedValue));

      Assert.Equal("original", originalValue);
      Assert.Equal("changed", copiedValue);
   }

   [Fact]
   public void CopyWith_PreservesCaseInsensitiveLookupBehavior()
   {
      var metadata = new MetadataBag();
      metadata.Set("OriginalKey", "value");

      var copy = MetadataBagFactory.CopyWith(
          metadata,
          "AnotherKey",
          "another-value");

      Assert.True(copy.TryGet("originalkey", out var originalValue));
      Assert.True(copy.TryGet("anotherkey", out var anotherValue));

      Assert.Equal("value", originalValue);
      Assert.Equal("another-value", anotherValue);
   }

   [Fact]
   public void CopyWith_WhenKeyDiffersOnlyByCase_OverridesExistingValue()
   {
      var metadata = new MetadataBag();
      metadata.Set("OriginalKey", "original");

      var copy = MetadataBagFactory.CopyWith(
          metadata,
          "originalkey",
          "changed");

      Assert.Equal(1, copy.Count);

      Assert.True(copy.TryGet("OriginalKey", out var value));
      Assert.Equal("changed", value);
   }
}