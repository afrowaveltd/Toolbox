using System.Diagnostics;
using Afrowave.Toolbox.Blackboard.SpectreConsole.Markdown;
using Afrowave.Toolbox.ExchangeHouse.Markdown;
using Spectre.Console;

namespace Afrowave.Toolbox.Toolroom.Markdown.Stress;

/// <summary>Opt-in diskless volume, allocation, writer and renderer stress runner.</summary>
internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Contains("--help", StringComparer.Ordinal))
            {
                Console.WriteLine("Usage: dotnet run --project Toolroom/Markdown/Stress -- " +
                    "[--size 10M|100M|1G|10G] [--seed 12345] [--max-heap-mib 256] " +
                    "[--no-render]");
                Console.WriteLine("Binary units: M = MiB, G = GiB. No temporary file is created.");
                return 0;
            }

            var size = ParseSize(GetOption(args, "--size") ?? "10M");
            var seed = int.Parse(GetOption(args, "--seed") ?? "12345",
                System.Globalization.CultureInfo.InvariantCulture);
            var maxHeapMib = int.Parse(GetOption(args, "--max-heap-mib") ?? "256",
                System.Globalization.CultureInfo.InvariantCulture);
            if (maxHeapMib <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxHeapMib));
            var render = !args.Contains("--no-render", StringComparer.Ordinal);
            ValidateOptions(args);

            using var reader = new VirtualMarkdownReader(size, seed);
            var output = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(TextWriter.Null)
            });

            var options = new MarkdownStreamOptions();
            var timer = Stopwatch.StartNew();
            long chunks = 0;
            long parsedChars = 0;
            var baselineHeap = GC.GetTotalMemory(true);
            var sampledMaxHeap = baselineHeap;
            var sampledMaxWorkingSet = Process.GetCurrentProcess().WorkingSet64;

            await foreach (var document in MarkdownStreamingReader.ParseAsync(reader, options))
            {
                chunks++;
                parsedChars += document.Source.Length;

                // Verify the same parsed data that the renderer receives.
                if (!string.Equals(MarkdownWriter.Write(document), document.Source,
                        StringComparison.Ordinal))
                    throw new InvalidDataException("Source-preserving writer mismatch.");

                if (render)
                    MarkdownSpectreRenderer.Render(output, document);

                if (chunks % 32 == 0)
                {
                    var heap = GC.GetTotalMemory(false);
                    sampledMaxHeap = Math.Max(sampledMaxHeap, heap);
                    sampledMaxWorkingSet = Math.Max(sampledMaxWorkingSet,
                        Process.GetCurrentProcess().WorkingSet64);
                    if (heap - baselineHeap > (long)maxHeapMib * 1024 * 1024)
                        throw new InvalidOperationException(
                            "Observed managed heap exceeded the configured growth budget.");
                }
            }

            timer.Stop();
            if (reader.GeneratedUtf8Bytes < size)
                throw new InvalidDataException("Generator ended before its byte target.");
            if (chunks == 0 || parsedChars == 0)
                throw new InvalidDataException("No Markdown was parsed.");

            Console.WriteLine("PASS - virtual Markdown stream stress test");
            Console.WriteLine($"Target UTF-8 bytes:     {size:N0}");
            Console.WriteLine($"Generated UTF-8 bytes:  {reader.GeneratedUtf8Bytes:N0}");
            Console.WriteLine($"Generated blocks:       {reader.BlockCount:N0}");
            Console.WriteLine($"Parsed chunks:          {chunks:N0}");
            Console.WriteLine($"Parsed UTF-16 chars:    {parsedChars:N0}");
            Console.WriteLine($"Seed:                   {seed}");
            Console.WriteLine($"Renderer exercised:     {render} (output -> TextWriter.Null)");
            Console.WriteLine($"Elapsed:                {timer.Elapsed}");
            Console.WriteLine($"Sampled managed heap:   {sampledMaxHeap / 1048576.0:N1} MiB");
            Console.WriteLine($"Sampled process RSS:    {sampledMaxWorkingSet / 1048576.0:N1} MiB");
            Console.WriteLine($"Managed heap growth cap: {maxHeapMib} MiB");
            Console.WriteLine("Heap/RSS values are sampled, not guaranteed absolute peaks.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FAIL - {ex.GetType().Name}: {ex.Message}");
            return 1;
        }
    }

    private static string? GetOption(string[] args, string name)
    {
        var index = Array.IndexOf(args, name);
        if (index < 0)
            return null;
        if (index + 1 >= args.Length)
            throw new ArgumentException($"Missing value after {name}.");
        return args[index + 1];
    }

    private static void ValidateOptions(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--no-render")
                continue;
            if (args[i] is "--size" or "--seed" or "--max-heap-mib")
            {
                i++;
                continue;
            }
            throw new ArgumentException($"Unknown argument: {args[i]}");
        }
    }

    private static long ParseSize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Size must not be empty.");
        var unit = char.ToUpperInvariant(value[^1]);
        var multiplier = unit switch
        {
            'K' => 1024L,
            'M' => 1024L * 1024,
            'G' => 1024L * 1024 * 1024,
            _ => 1L
        };
        var digits = multiplier == 1 ? value : value[..^1];
        var count = long.Parse(digits, System.Globalization.CultureInfo.InvariantCulture);
        var result = checked(count * multiplier);
        if (result <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Size must be positive.");
        return result;
    }
}
