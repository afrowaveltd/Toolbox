using System.Text;

namespace Afrowave.Toolbox.Toolroom.Markdown.Stress;

/// <summary>
/// Repeatable, on-demand virtual UTF-8 Markdown corpus. Only the current
/// prebuilt Markdown block is exposed; no large file or string is created.
/// A block is always completed even if it crosses the requested byte target.
/// </summary>
public sealed class VirtualMarkdownReader : TextReader
{
    private readonly string[] _blocks;
    private readonly long _requestedBytes;
    private uint _randomState;
    private string? _currentBlock;
    private int _position;

    public VirtualMarkdownReader(long requestedBytes, int seed)
    {
        if (requestedBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(requestedBytes));

        _requestedBytes = requestedBytes;
        _randomState = unchecked((uint)seed);
        if (_randomState == 0)
            _randomState = 0x9e3779b9;

        _blocks =
        [
            Repeat("# Sample heading\nThis is **strong**, *emphasis*, and inline code.\n\n", 18),
            Repeat("- First item\n  Continued paragraph\n  2. Ordered child\n- Second item\n\n", 16),
            Repeat("> Quoted introduction\n> - A quoted item\n> - Another quoted item\n\n", 18),
            Repeat("~~~csharp\nusing System;\nclass Demo { static void Main() { Console.WriteLine(42); } }\n~~~\n\n", 12),
            Repeat("~~~bash\nset -eu\nprintf 'Hello from a code block\\n'\n~~~\n\n", 18),
            Repeat("Plain paragraph with [a link](https://example.org/docs) and ![logo](../img.png).\n\n", 24),
            Repeat("International text: Příliš žluťoučký kůň, Ελληνικά, 日本語, Україна.\n\n", 18),
            Repeat("| Key | Value |\n| --- | --- |\n| A | 1 |\n| B | 2 |\n\n", 24),
            "# Large but bounded line\n" + new string('x', 10 * 1024) + "\n\n",
            Repeat("1. Ordered first\n2. Ordered second\n   - Nested item\n\n", 24),
            Repeat("> ~~~csharp\n> Console.WriteLine(\"quoted code\");\n> ~~~\n\n", 18),
            Repeat("## Heading\n\nA paragraph before a thematic separator.\n\n---\n\n", 16)
        ];
    }

    /// <summary>Requested minimum logical UTF-8 payload size.</summary>
    public long RequestedBytes => _requestedBytes;

    /// <summary>Full-block UTF-8 bytes scheduled so far. Equals output bytes at EOF.</summary>
    public long GeneratedUtf8Bytes { get; private set; }

    /// <summary>Number of pseudorandom template blocks scheduled so far.</summary>
    public long BlockCount { get; private set; }

    public override ValueTask<int> ReadAsync(
        Memory<char> buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var written = 0;
        while (written < buffer.Length)
        {
            if (_currentBlock is null || _position == _currentBlock.Length)
            {
                if (GeneratedUtf8Bytes >= _requestedBytes)
                    break;

                // Xorshift32: explicit deterministic algorithm independent of
                // System.Random changes between runtime implementations.
                _randomState ^= _randomState << 13;
                _randomState ^= _randomState >> 17;
                _randomState ^= _randomState << 5;
                _currentBlock = _blocks[(int)(_randomState % (uint)_blocks.Length)];
                _position = 0;
                GeneratedUtf8Bytes = checked(GeneratedUtf8Bytes +
                    Encoding.UTF8.GetByteCount(_currentBlock));
                BlockCount++;
            }

            var copy = Math.Min(buffer.Length - written, _currentBlock.Length - _position);
            _currentBlock.AsMemory(_position, copy).CopyTo(buffer[written..]);
            _position += copy;
            written += copy;
        }

        return ValueTask.FromResult(written);
    }

    private static string Repeat(string text, int count) =>
        string.Concat(Enumerable.Repeat(text, count));
}
