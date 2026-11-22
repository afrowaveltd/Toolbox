using System.Diagnostics;

namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Internal, opt-in stages of scanning and reading the event journal.
/// These are logical asynchronous operations, NOT physical disk IO events.
/// </summary>
internal enum MdViewJournalScanStage
{
    ScannerAdvance,
    SerializeEvent,
    EventLengthWrite,
    EventPayloadWrite,
    OffsetWrite,
    OffsetTableRead,
    EventWindowRead,
    DeserializeEvent,
    EventWritePosition,
    OffsetWritePosition,
    OffsetReadPosition,
    EventReadPosition
}

/// <summary>Immutable statistics for one timed journal operation in a progress window.</summary>
internal readonly record struct MdViewJournalStageWindow(
    long Calls,
    double TotalMs,
    double MaxMs,
    long CallsAtLeast100Ms,
    long MaxSourceLine,
    long MaxJournalOffset);

/// <summary>Optional journal build and readback timings per progress window.</summary>
internal readonly record struct MdViewJournalTimingWindow(
    MdViewJournalStageWindow ScannerAdvance,
    MdViewJournalStageWindow SerializeEvent,
    MdViewJournalStageWindow EventLengthWrite,
    MdViewJournalStageWindow EventPayloadWrite,
    MdViewJournalStageWindow OffsetWrite,
    MdViewJournalStageWindow OffsetTableRead,
    MdViewJournalStageWindow EventWindowRead,
    MdViewJournalStageWindow DeserializeEvent,
    MdViewJournalStageWindow EventWritePosition,
    MdViewJournalStageWindow OffsetWritePosition,
    MdViewJournalStageWindow OffsetReadPosition,
    MdViewJournalStageWindow EventReadPosition)
{
    /// <summary>Sum of instrumented stages including the journal readback path.</summary>
    public double AccountedMs =>
        ScannerAdvance.TotalMs + SerializeEvent.TotalMs +
        EventLengthWrite.TotalMs + EventPayloadWrite.TotalMs +
        OffsetWrite.TotalMs + OffsetTableRead.TotalMs +
        EventWindowRead.TotalMs + DeserializeEvent.TotalMs +
        EventWritePosition.TotalMs + OffsetWritePosition.TotalMs +
        OffsetReadPosition.TotalMs + EventReadPosition.TotalMs;
}

/// <summary>
/// Single-caller, bounded-memory timing accumulator for an optional HDD
/// diagnostic run. No log output, disk writes, timers or per-event
/// allocations. Production cursors leave it null.
/// </summary>
internal sealed class MdViewJournalScanTimings
{
    private readonly StageCounts[] _counts = new StageCounts[12];

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
            ToWindow(_counts[4]),
            ToWindow(_counts[5]),
            ToWindow(_counts[6]),
            ToWindow(_counts[7]),
            ToWindow(_counts[8]),
            ToWindow(_counts[9]),
            ToWindow(_counts[10]),
            ToWindow(_counts[11]));
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
