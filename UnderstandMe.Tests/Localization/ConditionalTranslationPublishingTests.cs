using Afrowave.Toolbox.UnderstandMe.Localization;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Tests.Localization;

public sealed class ConditionalTranslationPublishingTests
{
    [Fact]
    public void Missing_target_can_be_written_without_touching_source()
    {
        using var fixture = new StoreFixture();
        fixture.Store.TryAdd("en", "greeting", "Hello {name}");
        Assert.True(fixture.Store.TryPublish("en", "cs", "greeting", "Hello {name}", null, "Ahoj {name}"));
        Assert.Equal("Ahoj {name}", fixture.Store.Load("cs")["greeting"]);
        Assert.Equal("Hello {name}", fixture.Store.Load("en")["greeting"]);
        Assert.False(fixture.Store.TryPublish("en", "cs", "greeting", "Hello {name}", null, "Nazdar {name}"));
    }

    [Fact]
    public void Manual_edit_and_source_changes_are_never_overwritten()
    {
        using var fixture = new StoreFixture();
        fixture.Store.TryAdd("en", "key", "Hello {name}");
        fixture.Store.TryAdd("cs", "key", "Manuálně {name}");
        Assert.False(fixture.Store.TryPublish("en", "cs", "key", "Hello {name}", "Old {name}", "AI {name}"));
        Assert.Equal("Manuálně {name}", fixture.Store.Load("cs")["key"]);
        Assert.False(fixture.Store.TryPublish("en", "cs", "key", "Outdated {name}", "Manuálně {name}", "AI {name}"));
        Assert.False(fixture.Store.TryPublish("en", "cs", "absent", "Hello {name}", null, "AI {name}"));
    }

    [Theory]
    [InlineData("Překlad {wrong}")]
    [InlineData("Překlad {name} {name}")]
    [InlineData("Překlad {name")]
    public void Invalid_placeholder_candidates_do_not_publish(string candidate)
    {
        using var fixture = new StoreFixture();
        fixture.Store.TryAdd("en", "key", "Hello {name}");
        Assert.False(fixture.Store.TryPublish("en", "cs", "key", "Hello {name}", null, candidate));
        Assert.Empty(fixture.Store.Load("cs"));
    }

    [Fact]
    public void Valid_replacement_uses_exact_old_value_and_preserves_other_entries()
    {
        using var fixture = new StoreFixture();
        fixture.Store.TryAdd("en", "key", "Hello {name}");
        fixture.Store.TryAdd("cs", "key", "Broken {other}");
        fixture.Store.TryAdd("cs", "other", "Unchanged");
        Assert.True(fixture.Store.TryPublish("en", "cs", "key", "Hello {name}", "Broken {other}", "Opraveno {name}"));
        Assert.Equal("Unchanged", fixture.Store.Load("cs")["other"]);
        Assert.Equal("Opraveno {name}", fixture.Store.Load("cs")["key"]);
        Assert.False(fixture.Store.TryPublish("en", "cs", "key", "Hello {name}", "Broken {other}", "Again {name}"));
    }

    [Fact]
    public void Source_culture_and_empty_inputs_are_rejected()
    {
        using var fixture = new StoreFixture();
        Assert.Throws<ArgumentException>(() => fixture.Store.TryPublish("en", "EN", "key", "source", null, "target"));
        Assert.Throws<ArgumentException>(() => fixture.Store.TryPublish("en", "cs", " ", "source", null, "target"));
        Assert.Throws<ArgumentNullException>(() => fixture.Store.TryPublish("en", "cs", "key", null!, null, "target"));
    }

    [Fact]
    public void Same_store_parallel_writes_have_exactly_one_winner()
    {
        using var fixture = new StoreFixture();
        fixture.Store.TryAdd("en", "key", "Hello");
        int accepted = 0;
        Parallel.For(0, 30, i =>
        {
            if (fixture.Store.TryPublish("en", "cs", "key", "Hello", null, "Version " + i))
                Interlocked.Increment(ref accepted);
        });
        Assert.Equal(1, accepted);
        Assert.Single(fixture.Store.Load("cs"));
    }

    private sealed class StoreFixture : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "um-publish-" + Guid.NewGuid().ToString("N"));
        public FileJsonDictionaryStore Store { get; }
        public StoreFixture() => Store = new FileJsonDictionaryStore(_directory);
        public void Dispose() { if (Directory.Exists(_directory)) Directory.Delete(_directory, true); }
    }
}
