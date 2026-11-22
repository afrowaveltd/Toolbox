using System.Diagnostics;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Internal, opt-in stages of scanner-backed event-journal construction.
/// These are logical asynchronous operations, NOT physical disk IO events.
/// </summary>
internal enum MdViewJournalScanStage
{
    ScannerAdvance,
    SerializeEvent,
    EventLengthWrite,
    EventPayloadWrite,
    OffsetWrite
}

/// <summary>Immutable statistics for one timed journal operation in a progress window.</summary>
internal readonly record struct MdViewJournalStageWindow(
    long Calls,
    double TotalMs,
    double MaxMs,
    long CallsAtLeast100Ms,
    long MaxSourceLine,
    long MaxJournalOffset);

/// <summary>Optional journal-construction profile sampled and reset per progress window.</summary>
internal readonly record struct MdViewJournalTimingWindow(
    MdViewJournalStageWindow ScannerAdvance,
    MdViewJournalStageWindow SerializeEvent,
    MdViewJournalStageWindow EventLengthWrite,
    MdViewJournalStageWindow EventPayloadWrite,
    MdViewJournalStageWindow OffsetWrite)
{
    /// <summary>Sum of instrumented stage timings, not the cursor's entire wall time.</summary>
    public double AccountedMs =>
        ScannerAdvance.TotalMs + SerializeEvent.TotalMs +
        EventLengthWrite.TotalMs + EventPayloadWrite.TotalMs + OffsetWrite.TotalMs;
}

/// <summary>
/// Single-caller, bounded-memory timing accumulator for an optional HDD
/// diagnostic run. No log output, disk writes, timers or per-event
/// allocations. Production cursors leave it null.
/// </summary>
internal sealed class MdViewJournalScanTimings
{
    private readonly StageCounts[] _counts = new StageCounts[5];

    /// <summary>Records one completed logical operation on the scanning caller.</summary>
    internal void Record(
        MdViewJournalScanStage stage,
        long startedAt,
        long sourceLine,
        long journalOffset)
    {
        var ticks = Math.Max(0L, Stopwatch.GetTimestamp() - startedAt);
        ref var stats = ref _counts[(int)stage];
        stats.Calls++;
        stats.TotalTicks = checked(stats.TotalTicks + ticks);
        if (ticks * 10.0 >= Stopwatch.Frequency)
            stats.CallsAtLeast100Ms++;
        if (ticks > stats.MaxTicks)
        {
            stats.MaxTicks = ticks;
            stats.MaxSourceLine = sourceLine;
            stats.MaxJournalOffset = journalOffset;
        }
    }

    /// <summary>
    /// Takes a completed progress-window snapshot and starts a fresh window.
    /// Only call from the same sequential scanning task between ReadAsync calls.
    /// </summary>
    internal MdViewJournalTimingWindow TakeWindowAndReset()
    {
        var result = new MdViewJournalTimingWindow(
            ToWindow(_counts[0]),
            ToWindow(_counts[1]),
            ToWindow(_counts[2]),
            ToWindow(_counts[3]),
            ToWindow(_counts[4]));
        Array.Clear(_counts);
        return result;
    }

    private static MdViewJournalStageWindow ToWindow(StageCounts stats)
    {
        static double Milliseconds(long ticks) =>
            1000.0 * ticks / Stopwatch.Frequency;

        return new MdViewJournalStageWindow(
            stats.Calls, Milliseconds(stats.TotalTicks),
            Milliseconds(stats.MaxTicks), stats.CallsAtLeast100Ms,
            stats.Calls == 0 ? -1 : stats.MaxSourceLine,
            stats.Calls == 0 ? -1 : stats.MaxJournalOffset);
    }

    private struct StageCounts
    {
        internal long Calls;
        internal long TotalTicks;
        internal long MaxTicks;
        internal long CallsAtLeast100Ms;
        internal long MaxSourceLine;
        internal long MaxJournalOffset;
    }
}
