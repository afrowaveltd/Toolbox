using Afrowave.Toolbox.Essentials.Extensions;
using Afrowave.Toolbox.Essentials.Interfaces;

namespace Afrowave.Toolbox.Essentials.Tests.Extensions;

public sealed class HasIdExtensionsTests
{
    [Fact]
    public void HasId_WhenSourceIsNull_ThrowsArgumentNullException()
    {
        IHasId<string>? source = null;

        Assert.Throws<ArgumentNullException>(() =>
           source!.HasId());
    }

    [Fact]
    public void HasId_WhenReferenceTypeIdIsNull_ReturnsFalse()
    {
        var source = new TestIdSource<string?>
        {
            Id = null
        };

        var actual = source.HasId();

        Assert.False(actual);
    }

    [Fact]
    public void HasId_WhenReferenceTypeIdIsNotNull_ReturnsTrue()
    {
        var source = new TestIdSource<string?>
        {
            Id = "abc"
        };

        var actual = source.HasId();

        Assert.True(actual);
    }

    [Fact]
    public void HasId_WhenValueTypeIdIsDefault_ReturnsTrue()
    {
        var source = new TestIdSource<int>
        {
            Id = 0
        };

        var actual = source.HasId();

        Assert.True(actual);
    }

    [Fact]
    public void HasId_WhenGuidIsEmpty_ReturnsTrue()
    {
        var source = new TestIdSource<Guid>
        {
            Id = Guid.Empty
        };

        var actual = source.HasId();

        Assert.True(actual);
    }

    [Fact]
    public void HasId_WithExpectedId_WhenSourceIsNull_ThrowsArgumentNullException()
    {
        IHasId<string>? source = null;

        Assert.Throws<ArgumentNullException>(() =>
           source!.HasId("abc"));
    }

    [Fact]
    public void HasId_WithExpectedId_WhenStringIdMatches_ReturnsTrue()
    {
        var source = new TestIdSource<string>
        {
            Id = "abc"
        };

        var actual = source.HasId("abc");

        Assert.True(actual);
    }

    [Fact]
    public void HasId_WithExpectedId_WhenStringIdDoesNotMatch_ReturnsFalse()
    {
        var source = new TestIdSource<string>
        {
            Id = "abc"
        };

        var actual = source.HasId("xyz");

        Assert.False(actual);
    }

    [Fact]
    public void HasId_WithExpectedId_WhenStringCaseDiffers_ReturnsFalse()
    {
        var source = new TestIdSource<string>
        {
            Id = "ABC"
        };

        var actual = source.HasId("abc");

        Assert.False(actual);
    }

    [Fact]
    public void HasId_WithExpectedId_WhenNullableStringIdIsNullAndExpectedIsNull_ReturnsTrue()
    {
        var source = new TestIdSource<string?>
        {
            Id = null
        };

        var actual = source.HasId(null);

        Assert.True(actual);
    }

    [Fact]
    public void HasId_WithExpectedId_WhenNullableStringIdIsNullAndExpectedIsNotNull_ReturnsFalse()
    {
        var source = new TestIdSource<string?>
        {
            Id = null
        };

        var actual = source.HasId("abc");

        Assert.False(actual);
    }

    [Fact]
    public void HasId_WithExpectedId_WhenIntIdMatches_ReturnsTrue()
    {
        var source = new TestIdSource<int>
        {
            Id = 42
        };

        var actual = source.HasId(42);

        Assert.True(actual);
    }

    [Fact]
    public void HasId_WithExpectedId_WhenIntIdDoesNotMatch_ReturnsFalse()
    {
        var source = new TestIdSource<int>
        {
            Id = 42
        };

        var actual = source.HasId(7);

        Assert.False(actual);
    }

    [Fact]
    public void HasId_WithExpectedId_WhenGuidIdMatches_ReturnsTrue()
    {
        var id = Guid.NewGuid();

        var source = new TestIdSource<Guid>
        {
            Id = id
        };

        var actual = source.HasId(id);

        Assert.True(actual);
    }

    [Fact]
    public void HasId_WithExpectedId_WhenGuidIdDoesNotMatch_ReturnsFalse()
    {
        var source = new TestIdSource<Guid>
        {
            Id = Guid.NewGuid()
        };

        var actual = source.HasId(Guid.NewGuid());

        Assert.False(actual);
    }

    private sealed class TestIdSource<TId> : IHasId<TId>
    {
        public TId Id { get; set; } = default!;
    }
}