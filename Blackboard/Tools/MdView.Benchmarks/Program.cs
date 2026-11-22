using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Afrowave.Toolbox.Blackboard.TextFiles;
using Afrowave.Toolbox.Blackboard.Tools.MdView;
using Spectre.Console.Testing;

return await MdViewDiskBenchmark.RunAsync(args);

/// <summary>
/// Manually invoked, read-only-after-generation test harness for actual HDD
/// throughput. Never changes a system cache, a production setting or the
/// user's original source files.
/// </summary>
internal static class MdViewDiskBenchmark
{
    private const long MiB = 1024L * 1024;
    private const long GiB = 1024L * MiB;
    private const long BenchmarkJournalLimit = 8 * GiB;
    private const long BenchmarkOffsetLimit = 512 * MiB;

    private sealed record Settings(
        string Mode, string Root, int SizeMiB = 2048, int Seeks = 250,
        int Passes = 1, int Width = 120, int Rows = 48,
        int MaxWidth = 100, int Seed = 20261115,
        bool LongTables = false, int ProgressStep = 10)
    {
        public long TargetBytes => checked(SizeMiB * MiB);
        public string SourcePath => Path.Combine(
            Root, "data", $"mdview-synthetic-{SizeMiB}MiB.md");
    }

    /// <summary>Executes a requested benchmark phase and writes diagnostics.</summary>
    internal static async Task<int> RunAsync(string[] args)
    {
        // Safe, isolated logger-only regression: no HDD, parser, snapshot
        // or physical-disk preflight. Used by the two-platform CI matrix.
        if (args.Length == 1 &&
            args[0].Equals("--self-test-log-buffer", StringComparison.Ordinal))
            return await VerifyRamBufferedLogAsync();

        BenchLog? log = null;
        try
        {
            var options = Parse(args);
            var tmp = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(Path.GetTempPath()));
            var desiredTmp = Path.TrimEndingDirectorySeparator(
                Path.GetFullPath(Path.Combine(options.Root, "tmp")));
            var comparison = OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            if (!string.Equals(tmp, desiredTmp, comparison))
                throw new InvalidOperationException(
                    $"Process temporary directory does not match the test disk. Actual: {tmp}; expected: {desiredTmp}. " +
                    "Use run.ps1 on Windows or run.sh on Linux; do not start the benchmark DLL directly.");

            // Only the Markdown source/snapshot/journals/indexes touch
            // the test disk during benchmark phases. Diagnostic records
            // remain in managed RAM until completion or failure.
            Directory.CreateDirectory(Path.Combine(options.Root, "data"));
            log = new BenchLog(Path.Combine(options.Root, "logs"));
            log.Write("session_start", new
            {
                options.Mode, options.Root, options.SourcePath,
                options.SizeMiB, target_bytes = options.TargetBytes,
                options.Seeks, options.Passes, options.Width, options.Rows,
                options.MaxWidth, options.Seed, options.LongTables,
                options.ProgressStep,
                temp_path = tmp,
                os = RuntimeInformation.OSDescription,
                runtime = RuntimeInformation.FrameworkDescription,
                architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                cpu_count = Environment.ProcessorCount,
                total_gc_available_bytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
                process_id = Environment.ProcessId,
                first_pass_is_not_proven_cold_cache = true,
                diagnostic_output = "memory_buffered_then_persisted_after_test",
                diagnostic_buffer_limit_bytes = BenchLog.MaximumBufferedUtf8Bytes
            });

            if (options.Mode == "prepare")
            {
                // Explicit opt-in only: create the full reusable fixture set.
                // Existing files are verified and reused, never overwritten.
                foreach (var sizeMiB in new[] { 8, 128, 1024, 2048 })
                    await GenerateAsync(options with { SizeMiB = sizeMiB }, log);
                log.Write("fixture_set_completed", new
                {
                    sizes_mib = new[] { 8, 128, 1024, 2048 },
                    total_mib = 3208
                });
            }
            if (options.Mode is "generate" or "all")
                await GenerateAsync(options, log);
            if (options.Mode is "measure" or "all")
            {
                RequireSource(options);
                for (var pass = 1; pass <= options.Passes; pass++)
                    await MeasureSourceAsync(options, log, pass);
            }
            if (options.Mode == "index")
            {
                RequireSource(options);
                await MeasureMixedIndexAsync(options, log);
            }

            log.Write("session_completed", new
            {
                result = "success",
                log_jsonl = log.JsonPath,
                log_text = log.TextPath
            });
            // The benchmark interval ends before any diagnostic disk I/O.
            await log.PersistAsync();
            Console.WriteLine();
            Console.WriteLine("Diagnostic logs buffered in RAM during the run.");
            Console.WriteLine("Please send both log files:");
            Console.WriteLine(log.JsonPath);
            Console.WriteLine(log.TextPath);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Benchmark failed: " + ex);
            if (log is not null)
            {
                // Normal failures still receive a complete RAM-buffered
                // diagnostic export. An abrupt process termination cannot
                // be recovered by this opt-in benchmark.
                try
                {
                    log.Write("session_failed", new { error = ex.ToString() });
                }
                catch (Exception appendError)
                {
                    Console.Error.WriteLine(
                        "Could not append failure to RAM log: " + appendError);
                }

                try
                {
                    await log.PersistAsync();
                    Console.Error.WriteLine(
                        "Saved diagnostic log after failure: " + log.JsonPath);
                    Console.Error.WriteLine("Text log: " + log.TextPath);
                }
                catch (Exception persistError)
                {
                    Console.Error.WriteLine(
                        "Could not persist RAM-buffered diagnostics: " + persistError);
                }
            }
            return 1;
        }
    }

    /// <summary>
    /// Validates delayed writes and both output encodings using a unique
    /// temporary directory on the CI runner, never on a test HDD.
    /// This is NOT a performance benchmark.
    /// </summary>
    private static async Task<int> VerifyRamBufferedLogAsync()
    {
        var directory = Path.Combine(Path.GetTempPath(),
            "afrowave-mdview-log-selftest-" + Guid.NewGuid().ToString("N"));
        try
        {
            var log = new BenchLog(directory);
            log.Write("self_test_progress", new { percent = 50 });
            log.Write("self_test_completed", new { ok = true });
            if (Directory.Exists(directory) ||
                File.Exists(log.JsonPath) || File.Exists(log.TextPath))
                throw new InvalidOperationException(
                    "Buffered logger touched disk before PersistAsync.");

            await log.PersistAsync();
            var jsonLines = await File.ReadAllLinesAsync(log.JsonPath);
            var textLines = await File.ReadAllLinesAsync(log.TextPath);
            if (jsonLines.Length != 2 || textLines.Length != 2 ||
                !jsonLines[0].Contains("self_test_progress", StringComparison.Ordinal) ||
                !textLines[1].Contains("self_test_completed", StringComparison.Ordinal))
                throw new InvalidDataException(
                    "Persisted diagnostic records are missing or malformed.");

            using var first = JsonDocument.Parse(jsonLines[0]);
            using var second = JsonDocument.Parse(jsonLines[1]);
            if (first.RootElement.GetProperty("event").GetString()
                    != "self_test_progress" ||
                second.RootElement.GetProperty("event").GetString()
                    != "self_test_completed")
                throw new InvalidDataException(
                    "Buffered diagnostic JSON event names did not survive export.");

            try
            {
                log.Write("too_late", new { error = true });
                throw new InvalidOperationException(
                    "Logger allowed another event after export.");
            }
            catch (InvalidOperationException e) when (
                e.Message == "Diagnostic log has already been saved.")
            {
                // Expected safety guard.
            }

            Console.WriteLine(
                "PASS: diagnostics stayed in RAM until explicit export; " +
                "JSONL and TXT files preserved events on disk afterward.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("RAM logger self-test FAILED: " + ex);
            return 1;
        }
        finally
        {
            // Only the unique directory created by this self-test.
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    private static Settings Parse(string[] args)
    {
        if (args.Length < 2)
            throw new ArgumentException(
                "Usage: run.sh ROOT all|prepare|generate|measure|index, or run.ps1 -Mode MODE " +
                "[--size-mib 2048] [--seeks 250] [--passes 1] " +
                "[--width 120] [--rows 48] [--max-width 100] " +
                "[--seed N] [--progress-step 1..10] [--long-tables].");

        var mode = args[0].ToLowerInvariant();
        if (mode is not ("all" or "prepare" or "generate" or "measure" or "index"))
            throw new ArgumentException("Unsupported mode: " + args[0]);

        var root = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(args[1]));
        if (root == Path.GetPathRoot(root) || !Directory.Exists(root))
            throw new ArgumentException("A separately mounted test directory is required.");

        var opt = new Settings(mode, root);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 2; i < args.Length; i++)
        {
            var flag = args[i];
            if (!seen.Add(flag))
                throw new ArgumentException("Duplicate benchmark option: " + flag);
            if (flag.Equals("--long-tables", StringComparison.OrdinalIgnoreCase))
            {
                opt = opt with { LongTables = true };
                continue;
            }
            if (++i >= args.Length ||
                !int.TryParse(args[i], out var number))
                throw new ArgumentException("Expected an integer after " + flag);
            opt = flag.ToLowerInvariant() switch
            {
                "--size-mib" => opt with { SizeMiB = number },
                "--seeks" => opt with { Seeks = number },
                "--passes" => opt with { Passes = number },
                "--width" => opt with { Width = number },
                "--rows" => opt with { Rows = number },
                "--max-width" => opt with { MaxWidth = number },
                "--seed" => opt with { Seed = number },
                "--progress-step" => opt with { ProgressStep = number },
                _ => throw new ArgumentException("Unknown benchmark flag: " + flag)
            };
        }

        if (opt.SizeMiB is < 8 or > 8192 ||
            opt.Seeks is < 1 or > 2000 ||
            opt.Passes is < 1 or > 4 ||
            opt.Width is < 80 or > 4096 ||
            opt.Rows is < 8 or > 128 ||
            opt.ProgressStep is < 1 or > 10 ||
            opt.MaxWidth < 40 || opt.MaxWidth > opt.Width)
            throw new ArgumentOutOfRangeException(
                nameof(args), "Size, geometry or iteration limits are invalid.");
        return opt;
    }

    private static void RequireSource(Settings settings)
    {
        if (!File.Exists(settings.SourcePath))
            throw new FileNotFoundException(
                "The requested synthetic source must first be generated using 'generate', 'prepare', or 'all'.",
                settings.SourcePath);
        if (new FileInfo(settings.SourcePath).Length != settings.TargetBytes)
            throw new InvalidDataException(
                "The synthetic source size does not match --size-mib.");
    }

    private static byte[] MakeRepeatedMarkdownBlock()
    {
        var text = new StringBuilder(120_000);
        text.AppendLine("# MdView benchmark synthetic fixture v1");
        text.AppendLine("## Repeated section — disk journal, paging and Unicode");
        text.AppendLine(
            "Source [documentation](https://example.org/manual) " +
            "and [index](https://example.org/index): stable repeatable content.");
        for (var i = 0; i < 16; i++)
        {
            text.Append("Paragraph ").Append(i).Append(": ");
            text.Append('x', 4096);
            text.AppendLine();
        }
        text.AppendLine("| Name | Count | Description |");
        text.AppendLine("| :--- | ---: | :--- |");
        for (var i = 0; i < 24; i++)
            text.Append("| Item-").Append(i.ToString("00"))
                .Append(" | ").Append(i * 17)
                .Append(" | Literal text with escaped [brackets] and data |")
                .AppendLine();

        text.AppendLine("~~~csharp");
        text.AppendLine("int value = 42; // literal fence, not a table");
        text.AppendLine("Console.WriteLine(value);");
        text.AppendLine("~~~");
        text.AppendLine("Čeština: příliš žluťoučký kůň; Português; Français; 🌍");
        text.AppendLine();
        return Encoding.UTF8.GetBytes(text.ToString());
    }

    private static async Task GenerateAsync(Settings settings, BenchLog log)
    {
        if (File.Exists(settings.SourcePath))
        {
            RequireSource(settings);
            // Avoid silently replacing anybody's file of the same name.
            // A matching size alone is not sufficient proof of provenance.
            var marker = Encoding.UTF8.GetBytes(
                "# MdView benchmark synthetic fixture v1");
            await using var reader = File.OpenRead(settings.SourcePath);
            var prefix = new byte[marker.Length];
            await reader.ReadExactlyAsync(prefix);
            if (!prefix.AsSpan().SequenceEqual(marker))
                throw new InvalidDataException(
                    "Existing source is not our synthetic fixture; refusing overwrite.");
            log.Write("generation_reused", new
            {
                source = settings.SourcePath,
                bytes = settings.TargetBytes,
                file_reused_without_full_hash = true
            });
            return;
        }

        var free = GetFreeBytes(settings.Root);
        var minimum = checked(settings.TargetBytes * 6 + GiB);
        log.Write("disk_preflight", new
        {
            free_bytes = free,
            free_space_source = OperatingSystem.IsWindows()
                ? "live_windows_driveinfo_bounded_by_launcher"
                : "launcher_initial_linux_df",
            required_reserve_bytes = minimum,
            source_bytes = settings.TargetBytes
        });
        if (free < minimum)
            throw new IOException(
                "Insufficient free space for the 2-GiB source plus snapshot, journals, " +
                "indexes and safety reserve. No file was created.");

        var block = MakeRepeatedMarkdownBlock();
        var stopwatch = Stopwatch.StartNew();
        // FileMode.CreateNew never truncates an existing user file.
        await using var output = new FileStream(
            settings.SourcePath, FileMode.CreateNew, FileAccess.Write,
            FileShare.None, 1024 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        try
        {
            var nextLog = 256 * MiB;
            while (output.Position + block.Length <= settings.TargetBytes)
            {
                await output.WriteAsync(block);
                if (output.Position >= nextLog)
                {
                    log.Write("generation_progress", new
                    {
                        bytes_written = output.Position,
                        target_bytes = settings.TargetBytes
                    });
                    nextLog += 256 * MiB;
                }
            }

            var tailBytes = checked((int)(settings.TargetBytes - output.Position));
            if (tailBytes > 0)
            {
                // ASCII-only tail preserves strict UTF-8 even when the
                // exact byte target cuts the next repeated block.
                var tail = new byte[tailBytes];
                Array.Fill(tail, (byte)'x');
                await output.WriteAsync(tail);
            }
            await output.FlushAsync();
            stopwatch.Stop();

            if (output.Length != settings.TargetBytes)
                throw new InvalidDataException("Generator produced the wrong byte count.");
            log.Write("generation_completed", new
            {
                source = settings.SourcePath,
                bytes = output.Length,
                elapsed_ms = stopwatch.Elapsed.TotalMilliseconds,
                throughput_mib_s = settings.SizeMiB /
                    Math.Max(stopwatch.Elapsed.TotalSeconds, 0.001),
                fixture_version = 1,
                block_bytes = block.Length
            });
        }
        catch
        {
            // Deliberately do not delete a partial source: preserve
            // evidence and require the maintainer to inspect it.
            log.Write("generation_interrupted", new
            {
                file = settings.SourcePath,
                current_bytes = output.Length
            });
            throw;
        }
    }

    private static async Task MeasureSourceAsync(
        Settings settings, BenchLog log, int pass)
    {
        log.Write("pass_start", new { pass, mode = "journal" });
        var snapshot = await TimedAsync(log, $"snapshot_pass_{pass}",
            settings.TargetBytes, () => Utf8ValidatedSnapshot.CreateAsync(
                settings.SourcePath,
                maximumSnapshotBytes: settings.TargetBytes + MiB,
                temporaryRoot: Path.Combine(settings.Root, "tmp")));
        await using (snapshot)
        {
            log.Write("snapshot_info", new
            {
                pass, snapshot_bytes = snapshot.ByteLength,
                source_lines = snapshot.Lines.LineCount,
                offset_index_bytes = snapshot.Lines.IndexBytes
            });
            await using var cursor = new MdViewContainerPageCursor(
                snapshot,
                maximumJournalBytes: BenchmarkJournalLimit,
                maximumOffsetBytes: BenchmarkOffsetLimit);

            await TimedAsync(log, $"journal_sequential_pass_{pass}",
                settings.TargetBytes, async () =>
                {
                    long first = 0;
                    var nextProgress = (long)settings.ProgressStep;

                    // Progress windows measure the relative cost of scanning
                    // versus benchmark diagnostic logging. The previous
                    // progress Write may be included in the next wall interval.
                    // Instrument ReadAsync at each 128-line request without
                    // producing extra per-request disk log writes.
                    using var intervalProcess = Process.GetCurrentProcess();
                    var intervalTimestamp = Stopwatch.GetTimestamp();
                    var intervalAllocatedBytes = GC.GetTotalAllocatedBytes(false);
                    var intervalGc0 = GC.CollectionCount(0);
                    var intervalGc1 = GC.CollectionCount(1);
                    var intervalGc2 = GC.CollectionCount(2);
                    var intervalGcPause = GC.GetTotalPauseDuration();
                    var intervalCpu = intervalProcess.TotalProcessorTime;
                    long intervalJournalBytes = 0;
                    long intervalCursorCalls = 0;
                    long intervalCursorAbove100Ms = 0;
                    double intervalCursorElapsedMs = 0;
                    double intervalCursorMaxMs = 0;
                    long intervalCursorMaxLine = -1;
                    long intervalCursorMaxJournalBefore = 0;
                    long intervalCursorMaxJournalAfter = 0;
                    double previousProgressLogWriteMs = 0;
                    while (first < snapshot.Lines.LineCount)
                    {
                        var requestLine = first;
                        var requestJournalBefore = cursor.JournalBytes;
                        var requestStart = Stopwatch.GetTimestamp();
                        var lines = await cursor.ReadAsync(first, 128);
                        var requestElapsedMs = Stopwatch.GetElapsedTime(
                            requestStart).TotalMilliseconds;

                        intervalCursorCalls++;
                        intervalCursorElapsedMs += requestElapsedMs;
                        if (requestElapsedMs >= 100)
                            intervalCursorAbove100Ms++;
                        if (requestElapsedMs > intervalCursorMaxMs)
                        {
                            intervalCursorMaxMs = requestElapsedMs;
                            intervalCursorMaxLine = requestLine;
                            intervalCursorMaxJournalBefore = requestJournalBefore;
                            intervalCursorMaxJournalAfter = cursor.JournalBytes;
                        }

                        if (lines.Count == 0 ||
                            lines[0].Source.LineNumber != first)
                            throw new InvalidDataException(
                                "Journal scanning stopped or lost its source position.");
                        first += lines.Count;
                        var percent = 100L * first /
                            Math.Max(1L, snapshot.Lines.LineCount);
                        if (percent >= nextProgress)
                        {
                            var nowTimestamp = Stopwatch.GetTimestamp();
                            var nowAllocatedBytes = GC.GetTotalAllocatedBytes(false);
                            var nowGc0 = GC.CollectionCount(0);
                            var nowGc1 = GC.CollectionCount(1);
                            var nowGc2 = GC.CollectionCount(2);
                            var nowGcPause = GC.GetTotalPauseDuration();
                            var nowCpu = intervalProcess.TotalProcessorTime;
                            var nowJournalBytes = cursor.JournalBytes;
                            var intervalWallMs = Stopwatch.GetElapsedTime(
                                intervalTimestamp, nowTimestamp).TotalMilliseconds;
                            var progressWriteStart = Stopwatch.GetTimestamp();
                            log.Write("journal_progress", new
                            {
                                pass, percent, source_lines_scanned = first,
                                journal_bytes = nowJournalBytes,
                                interval_elapsed_ms = intervalWallMs,
                                interval_allocated_bytes = nowAllocatedBytes - intervalAllocatedBytes,
                                interval_gc_gen0 = nowGc0 - intervalGc0,
                                interval_gc_gen1 = nowGc1 - intervalGc1,
                                interval_gc_gen2 = nowGc2 - intervalGc2,
                                interval_gc_pause_ms =
                                    (nowGcPause - intervalGcPause).TotalMilliseconds,
                                interval_cpu_ms = (nowCpu - intervalCpu).TotalMilliseconds,
                                interval_journal_bytes = nowJournalBytes - intervalJournalBytes,
                                working_set_bytes = intervalProcess.WorkingSet64,
                                interval_cursor_read_count = intervalCursorCalls,
                                interval_cursor_read_total_ms = intervalCursorElapsedMs,
                                interval_cursor_read_max_ms = intervalCursorMaxMs,
                                interval_cursor_read_above_100ms = intervalCursorAbove100Ms,
                                interval_cursor_max_start_line = intervalCursorMaxLine,
                                interval_cursor_max_journal_before = intervalCursorMaxJournalBefore,
                                interval_cursor_max_journal_after = intervalCursorMaxJournalAfter,
                                previous_progress_log_write_ms = previousProgressLogWriteMs
                            });
                            previousProgressLogWriteMs =
                                Stopwatch.GetElapsedTime(progressWriteStart).TotalMilliseconds;

                            // The new interval begins at the original
                            // pre-log timestamp so wall time still includes
                            // the previous progress Write; its duration is
                            // now measured separately for attribution.
                            intervalTimestamp = nowTimestamp;
                            intervalAllocatedBytes = nowAllocatedBytes;
                            intervalGc0 = nowGc0;
                            intervalGc1 = nowGc1;
                            intervalGc2 = nowGc2;
                            intervalGcPause = nowGcPause;
                            intervalCpu = nowCpu;
                            intervalJournalBytes = nowJournalBytes;
                            intervalCursorCalls = 0;
                            intervalCursorAbove100Ms = 0;
                            intervalCursorElapsedMs = 0;
                            intervalCursorMaxMs = 0;
                            intervalCursorMaxLine = -1;
                            intervalCursorMaxJournalBefore = 0;
                            intervalCursorMaxJournalAfter = 0;
                            // 128-line requests may skip multiple percentage
                            // thresholds in very small fixtures.
                            nextProgress =
                                (percent / settings.ProgressStep + 1) *
                                settings.ProgressStep;
                        }
                    }
                    if (first != snapshot.Lines.LineCount)
                        throw new InvalidDataException(
                            "Journal scan ended at an incorrect physical line.");
                    return new
                    {
                        pass, source_lines = first,
                        journal_bytes = cursor.JournalBytes,
                        offset_bytes = checked(first * sizeof(long)),
                        source_requests = cursor.ReadRequestCount,
                        event_stream_reads = cursor.EventReadRequestCount
                    };
                });

            var random = new Random(settings.Seed);
            var targetLines = new long[settings.Seeks];
            for (var i = 0; i < targetLines.Length; i++)
                targetLines[i] = random.NextInt64(snapshot.Lines.LineCount);
            await MeasureSeeksAsync(cursor, targetLines, log, pass, "first");
            await MeasureSeeksAsync(cursor, targetLines, log, pass, "repeat");
            log.Write("pass_completed", new
            {
                pass, snapshot_bytes = snapshot.ByteLength,
                journal_bytes = cursor.JournalBytes,
                source_requests = cursor.ReadRequestCount,
                event_stream_reads = cursor.EventReadRequestCount
            });
        }
    }

    private static async Task MeasureSeeksAsync(
        MdViewContainerPageCursor cursor, long[] targetLines,
        BenchLog log, int pass, string stage)
    {
        var samples = new List<SeekSample>(targetLines.Length);
        var stopwatch = Stopwatch.StartNew();
        var sourceRequests = cursor.ReadRequestCount;
        var eventReads = cursor.EventReadRequestCount;
        var startIo = LinuxIo.Read();
        foreach (var line in targetLines)
        {
            var step = Stopwatch.StartNew();
            var lines = await cursor.ReadAsync(line, 7);
            step.Stop();
            if (lines.Count == 0 || lines[0].Source.LineNumber != line)
                throw new InvalidDataException(
                    "Random seek returned the wrong physical source position.");
            samples.Add(new SeekSample(line, step.Elapsed.TotalMilliseconds,
                lines.Count));
        }
        stopwatch.Stop();
        var elapsed = samples.Select(x => x.ElapsedMs)
            .OrderBy(x => x).ToArray();
        log.Write("random_seek_summary", new
        {
            pass, stage, sample_count = elapsed.Length,
            total_elapsed_ms = stopwatch.Elapsed.TotalMilliseconds,
            min_ms = elapsed[0],
            median_ms = Percentile(elapsed, 0.50),
            p95_ms = Percentile(elapsed, 0.95),
            p99_ms = Percentile(elapsed, 0.99),
            max_ms = elapsed[^1],
            cursor_requests = cursor.ReadRequestCount - sourceRequests,
            event_stream_reads = cursor.EventReadRequestCount - eventReads,
            proc_io_delta = LinuxIo.Delta(startIo, LinuxIo.Read()),
            // Preserves every requested seek and its measured duration.
            samples
        });
    }

    private static double Percentile(double[] sorted, double rank)
    {
        var index = Math.Clamp(
            (int)Math.Ceiling(rank * sorted.Length) - 1,
            0, sorted.Length - 1);
        return sorted[index];
    }

    private sealed record SeekSample(long SourceLine, double ElapsedMs, int ReturnedLines);

    private static async Task MeasureMixedIndexAsync(Settings settings, BenchLog log)
    {
        log.Write("index_start", new
        {
            selected_rows = settings.Rows,
            selected_width = settings.Width,
            maximum_content_width = settings.MaxWidth,
            long_native_tables = settings.LongTables,
            warning = "This scans the complete file and may be slow on a 5400 RPM HDD."
        });
        var snapshot = await TimedAsync(log, "index_snapshot",
            settings.TargetBytes, () => Utf8ValidatedSnapshot.CreateAsync(
                settings.SourcePath,
                maximumSnapshotBytes: settings.TargetBytes + MiB,
                temporaryRoot: Path.Combine(settings.Root, "tmp")));
        await using (snapshot)
        {
            await using var cursor = new MdViewContainerPageCursor(
                snapshot,
                maximumJournalBytes: BenchmarkJournalLimit,
                maximumOffsetBytes: BenchmarkOffsetLimit);
            var console = new TestConsole();
            console.Profile.Width = settings.Width;
            console.Profile.Height = Math.Max(settings.Rows + 12, 80);
            console.Profile.Capabilities.Unicode = true;
            console.Profile.Capabilities.Interactive = false;

            var index = await TimedAsync(log, "mixed_page_index",
                settings.TargetBytes,
                () => MdViewMixedVisualPageIndex.CreateAsync(
                    cursor, console, settings.Rows,
                    maximumIndexBytes: 256 * MiB,
                    maximumContentColumns: settings.MaxWidth,
                    longNativeTables: settings.LongTables));
            await using (index)
            {
                log.Write("index_info", new
                {
                    source_lines = index.SourceLineCount,
                    pages = index.PageCount,
                    bytes_on_disk = index.PageCount *
                        MdViewMixedVisualPageIndex.RecordBytes,
                    journal_bytes = cursor.JournalBytes,
                    source_requests = cursor.ReadRequestCount,
                    event_stream_reads = cursor.EventReadRequestCount
                });

                var pages = new[]
                {
                    0L, index.PageCount / 2, index.PageCount - 1
                }.Distinct().ToArray();
                foreach (var pageNo in pages)
                {
                    var record = await TimedAsync(log,
                        $"index_get_page_{pageNo}", 0,
                        () => index.GetPageAsync(pageNo));
                    log.Write("index_page", new
                    {
                        page = pageNo,
                        record.Start, record.Next,
                        record.VisualRows, record.AtEnd
                    });
                }

                var lines = new[]
                {
                    0L, snapshot.Lines.LineCount / 2,
                    Math.Max(0, snapshot.Lines.LineCount - 1)
                }.Distinct();
                foreach (var line in lines)
                {
                    var pageNo = await TimedAsync(log,
                        $"source_lookup_{line}", 0,
                        () => index.FindPageAsync(line));
                    log.Write("source_lookup",
                        new { source_line = line, page = pageNo });
                }
            }
        }
    }

    private static async Task<T> TimedAsync<T>(
        BenchLog log, string phase, long logicalBytes, Func<Task<T>> work)
    {
        var before = LinuxIo.Read();
        var allocStart = GC.GetTotalAllocatedBytes(false);
        // Retain the existing cumulative counters for old log consumers,
        // but also record per-phase deltas for meaningful GC comparisons.
        var gc0Start = GC.CollectionCount(0);
        var gc1Start = GC.CollectionCount(1);
        var gc2Start = GC.CollectionCount(2);
        var gcPauseStart = GC.GetTotalPauseDuration();
        var cpuBefore = Process.GetCurrentProcess().TotalProcessorTime;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await work();
            stopwatch.Stop();
            using var process = Process.GetCurrentProcess();
            log.Write("timed_phase", new
            {
                phase, ok = true,
                elapsed_ms = stopwatch.Elapsed.TotalMilliseconds,
                logical_bytes = logicalBytes,
                throughput_mib_s = logicalBytes > 0
                    ? logicalBytes / (double)MiB /
                      Math.Max(stopwatch.Elapsed.TotalSeconds, 0.001)
                    : (double?)null,
                cpu_ms = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds,
                gc_allocated_bytes = GC.GetTotalAllocatedBytes(false) - allocStart,
                working_set_bytes = process.WorkingSet64,
                peak_working_set_bytes = process.PeakWorkingSet64,
                managed_heap_bytes = GC.GetGCMemoryInfo().HeapSizeBytes,
                gc_gen0 = GC.CollectionCount(0),
                gc_gen1 = GC.CollectionCount(1),
                gc_gen2 = GC.CollectionCount(2),
                gc_gen0_delta = GC.CollectionCount(0) - gc0Start,
                gc_gen1_delta = GC.CollectionCount(1) - gc1Start,
                gc_gen2_delta = GC.CollectionCount(2) - gc2Start,
                gc_pause_ms_delta =
                    (GC.GetTotalPauseDuration() - gcPauseStart).TotalMilliseconds,
                proc_io_delta = LinuxIo.Delta(before, LinuxIo.Read()),
                result_type = typeof(T).Name
            });
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            log.Write("timed_phase", new
            {
                phase, ok = false,
                elapsed_ms = stopwatch.Elapsed.TotalMilliseconds,
                error = ex.ToString(),
                gc_gen0_delta = GC.CollectionCount(0) - gc0Start,
                gc_gen1_delta = GC.CollectionCount(1) - gc1Start,
                gc_gen2_delta = GC.CollectionCount(2) - gc2Start,
                gc_pause_ms_delta =
                    (GC.GetTotalPauseDuration() - gcPauseStart).TotalMilliseconds,
                proc_io_delta = LinuxIo.Delta(before, LinuxIo.Read())
            });
            throw;
        }
    }

    private static long GetFreeBytes(string root)
    {
        // Always require the platform launcher's verified initial volume.
        // Windows can then refresh its free-space reading before each
        // prepare fixture; keeping the smaller of the two readings is
        // conservative even if unrelated files are removed in between.
        // The Linux df preflight remains an initial conservative estimate:
        // DriveInfo(root) may resolve the WRONG filesystem for a nested mount.
        var value = Environment.GetEnvironmentVariable(
            "MDVIEW_BENCH_AVAILABLE_BYTES");
        if (!long.TryParse(value, out var initialFree) || initialFree < 0)
            throw new InvalidOperationException(
                "Missing test-volume free-space check; launch through run.ps1 or run.sh. " +
                "Selected benchmark root: " + root);
        if (!OperatingSystem.IsWindows())
            return initialFree;

        var driveRoot = Path.GetPathRoot(root);
        if (string.IsNullOrEmpty(driveRoot))
            throw new InvalidOperationException("Missing Windows test drive root.");
        var currentFree = new DriveInfo(driveRoot).AvailableFreeSpace;
        return Math.Min(initialFree, currentFree);
    }

    /// <summary>
    /// Collects diagnostics entirely in managed RAM during measured work.
    /// The only file writes occur explicitly after the benchmark finishes,
    /// including after a handled failure. No progress writes go to the
    /// console, JSONL or TXT while disk I/O is being timed.
    /// </summary>
    private sealed class BenchLog
    {
        // Small enough for low-RAM benchmark hosts; even the maximum
        // 2,000-seek / four-pass workload normally remains far below this.
        // Counts both UTF-8 outputs, not the source or MD journals.
        public const long MaximumBufferedUtf8Bytes = 32L * 1024 * 1024;

        private readonly string _directory;
        private readonly List<string> _jsonLines = [];
        private readonly List<string> _textLines = [];
        private long _bufferedUtf8Bytes;
        private bool _persisted;

        public BenchLog(string directory)
        {
            _directory = directory;
            var suffix = DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ") +
                "-" + Guid.NewGuid().ToString("N")[..8];
            JsonPath = Path.Combine(directory, $"mdview-hdd-{suffix}.jsonl");
            TextPath = Path.Combine(directory, $"mdview-hdd-{suffix}.txt");
        }

        public string JsonPath { get; }
        public string TextPath { get; }

        public void Write(string eventName, object values)
        {
            if (_persisted)
                throw new InvalidOperationException("Diagnostic log has already been saved.");

            var timestamp = DateTimeOffset.UtcNow;
            var payload = JsonSerializer.Serialize(values);
            var jsonLine = JsonSerializer.Serialize(new
            {
                timestamp_utc = timestamp,
                @event = eventName,
                values
            });
            var textLine = $"{timestamp:O} [{eventName}] {payload}";
            var size = (long)Encoding.UTF8.GetByteCount(jsonLine) +
                Encoding.UTF8.GetByteCount(textLine) + 2;
            if (size > MaximumBufferedUtf8Bytes - _bufferedUtf8Bytes)
                throw new InvalidOperationException(
                    "Diagnostic RAM buffer limit reached; stopping instead of " +
                    "writing benchmark logs to the measured disk.");

            _bufferedUtf8Bytes += size;
            _jsonLines.Add(jsonLine);
            _textLines.Add(textLine);
        }

        /// <summary>
        /// Persists collected records only after measured workload ends.
        /// Uses new unique filenames and never overwrites prior results.
        /// </summary>
        public async Task PersistAsync()
        {
            if (_persisted)
                return;
            Directory.CreateDirectory(_directory);
            await SaveLinesAsync(JsonPath, _jsonLines);
            await SaveLinesAsync(TextPath, _textLines);
            _persisted = true;
            Console.WriteLine(
                $"Saved {_jsonLines.Count} buffered diagnostic events " +
                $"({_bufferedUtf8Bytes} UTF-8 bytes across both formats).");
        }

        private static async Task SaveLinesAsync(
            string path, IReadOnlyList<string> lines)
        {
            await using var file = new FileStream(
                path, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                65536, FileOptions.Asynchronous);
            await using var writer = new StreamWriter(
                file, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            foreach (var line in lines)
                await writer.WriteLineAsync(line);
        }
    }

    /// <summary>
    /// Linux per-process I/O counters. read_bytes/write_bytes reflect
    /// storage-accounted I/O but not guaranteed rotational-device reads.
    /// syscr/syscw count syscalls; rchar/wchar include cached I/O.
    /// </summary>
    private sealed record LinuxIo(
        long ReadBytes, long WriteBytes, long Syscr, long Syscw,
        long Rchar, long Wchar)
    {
        public static LinuxIo Read()
        {
            if (!File.Exists("/proc/self/io"))
                return new LinuxIo(0, 0, 0, 0, 0, 0);
            var values = new Dictionary<string, long>(StringComparer.Ordinal);
            foreach (var line in File.ReadLines("/proc/self/io"))
            {
                var pair = line.Split(':', 2);
                if (pair.Length == 2 &&
                    long.TryParse(pair[1].Trim(), out var number))
                    values[pair[0]] = number;
            }
            long Get(string name) =>
                values.TryGetValue(name, out var value) ? value : 0;
            return new LinuxIo(Get("read_bytes"), Get("write_bytes"),
                Get("syscr"), Get("syscw"),
                Get("rchar"), Get("wchar"));
        }

        public static LinuxIo Delta(LinuxIo start, LinuxIo end) =>
            new(end.ReadBytes - start.ReadBytes,
                end.WriteBytes - start.WriteBytes,
                end.Syscr - start.Syscr,
                end.Syscw - start.Syscw,
                end.Rchar - start.Rchar,
                end.Wchar - start.Wchar);
    }
}
