using Afrowave.Toolbox.UnderstandMe.Localization;
using Xunit;

namespace Afrowave.Toolbox.UnderstandMe.Tests.Localization;

public sealed class LocalizationRepairPlannerTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 20, 18, 0, 0, TimeSpan.Zero);
    private static LocalizationRepairItem Candidate(string key, string text = "Hello {name}") =>
        new(key, text, LocalizationRepairReason.MissingTarget);
    private static LocalizationRepairLease Lease(string culture, string key, string text, bool pending, DateTimeOffset due) =>
        new(culture, key, text, pending, due);

    [Fact]
    public void Pending_and_cooling_repairs_are_not_selected()
    {
        var items = new[] { Candidate("a"), Candidate("b"), Candidate("c") };
        var leases = new[] {
            Lease("cs", "a", "Hello {name}", true, Now.AddMinutes(-1)),
            Lease("cs", "b", "Hello {name}", false, Now.AddMinutes(5)),
            Lease("cs", "c", "Hello {name}", false, Now)
        };
        Assert.Equal(new[] { "c" }, LocalizationRepairPlanner.SelectEligible("cs", items, leases, Now).Select(x => x.Key));
    }

    [Fact]
    public void Changed_source_bypasses_old_pending_or_failed_revision()
    {
        var items = new[] { Candidate("a", "New {name}"), Candidate("b", "Changed {name}") };
        var leases = new[] {
            Lease("cs", "a", "Old {name}", true, Now.AddDays(1)),
            Lease("cs", "b", "Old {name}", false, Now.AddDays(1))
        };
        Assert.Equal(2, LocalizationRepairPlanner.SelectEligible("cs", items, leases, Now).Count);
    }

    [Fact]
    public void Culture_isolation_and_duplicate_candidate_suppression()
    {
        var leases = new[] { Lease("sk", "a", "Hello {name}", true, Now.AddDays(1)) };
        var selected = LocalizationRepairPlanner.SelectEligible(" cs ", new[] { Candidate("a"), Candidate("a") }, leases, Now);
        Assert.Single(selected);
    }

    [Fact]
    public void Duplicate_leases_are_rejected_to_prevent_ambiguous_admission()
    {
        var lease = Lease("cs", "a", "Hello {name}", true, Now);
        Assert.Throws<ArgumentException>(() =>
            LocalizationRepairPlanner.SelectEligible("cs", new[] { Candidate("a") }, new[] { lease, lease }, Now));
    }

    [Fact]
    public void Null_inputs_fail_explicitly()
    {
        Assert.Throws<ArgumentNullException>(() =>
            LocalizationRepairPlanner.SelectEligible("cs", null!, Array.Empty<LocalizationRepairLease>(), Now));
        Assert.Throws<ArgumentNullException>(() =>
            LocalizationRepairPlanner.SelectEligible("cs", Array.Empty<LocalizationRepairItem>(), null!, Now));
        Assert.Throws<ArgumentException>(() =>
            LocalizationRepairPlanner.SelectEligible(" ", Array.Empty<LocalizationRepairItem>(), Array.Empty<LocalizationRepairLease>(), Now));
    }
}
