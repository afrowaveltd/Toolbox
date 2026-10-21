using Afrowave.Toolbox.UnderstandMe.Localization;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Tests.Localization;

public sealed class InMemoryLocalizationRepairClaimsTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 21, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Only_one_parallel_worker_can_claim_same_revision()
    {
        var claims = new InMemoryLocalizationRepairClaims();
        int successes = 0;
        Parallel.For(0, 100, _ =>
        {
            if (claims.TryClaim("cs", "hello", "Hello", Now, TimeSpan.FromMinutes(5), out _))
                Interlocked.Increment(ref successes);
        });
        Assert.Equal(1, successes);
    }

    [Fact]
    public void Expired_claim_can_be_replaced_but_stale_worker_cannot_complete_it()
    {
        var store = new InMemoryLocalizationRepairClaims();
        Assert.True(store.TryClaim("cs", "x", "Original", Now, TimeSpan.FromMinutes(1), out var first));
        Assert.True(store.TryClaim("cs", "x", "Original", Now.AddMinutes(2), TimeSpan.FromMinutes(1), out var second));
        Assert.NotEqual(first!.Token, second!.Token);
        Assert.False(store.TryComplete(first, Now.AddDays(1)));
        Assert.True(store.TryComplete(second, Now.AddMinutes(5)));
        Assert.False(store.TryClaim("cs", "x", "Original", Now.AddMinutes(3), TimeSpan.FromMinutes(1), out _));
        Assert.True(store.TryClaim("cs", "x", "Original", Now.AddMinutes(5), TimeSpan.FromMinutes(1), out _));
    }

    [Fact]
    public void Source_revision_change_can_replace_prior_claim_without_corrupting_new_owner()
    {
        var store = new InMemoryLocalizationRepairClaims();
        Assert.True(store.TryClaim("cs", "x", "Before", Now, TimeSpan.FromHours(1), out var old));
        Assert.True(store.TryClaim("cs", "x", "After", Now, TimeSpan.FromMinutes(1), out var newer));
        Assert.False(store.TryComplete(old!, Now));
        Assert.True(store.TryComplete(newer!, Now));
    }

    [Fact]
    public void Locale_aliases_share_claim_but_other_locales_and_keys_do_not()
    {
        var store = new InMemoryLocalizationRepairClaims();
        Assert.True(store.TryClaim("pt_BR", "key", "Text", Now, TimeSpan.FromMinutes(1), out _));
        Assert.False(store.TryClaim("PT-br", "key", "Text", Now, TimeSpan.FromMinutes(1), out _));
        Assert.True(store.TryClaim("pt", "key", "Text", Now, TimeSpan.FromMinutes(1), out _));
        Assert.True(store.TryClaim("pt-BR", "Key", "Text", Now, TimeSpan.FromMinutes(1), out _));
    }

    [Fact]
    public void Invalid_lease_duration_is_rejected()
    {
        var store = new InMemoryLocalizationRepairClaims();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            store.TryClaim("cs", "x", "Text", Now, TimeSpan.Zero, out _));
    }
}
