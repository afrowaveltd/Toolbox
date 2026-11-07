using System.Buffers;

namespace Afrowave.Toolbox.Blackboard.TextFiles;

/// <summary>
/// Owns a private, fully validated temporary UTF-8 copy of a source file.
/// Later reads use the copy rather than the possibly changing original path.
/// </summary>
/// <remarks>
/// Only content successfully copied and then validated by
/// <see cref="Utf8DiskLineIndex.OpenAsync"/> is exposed. Temporary files are
/// removed on disposal. The private path is not intended as a protection from
/// malicious processes with the same OS identity or elevated privileges.
/// </remarks>
public sealed class Utf8ValidatedSnapshot : IAsyncDisposable
{
    private const int CopyBufferBytes = 64 * 1024;
    /// <summary>Default maximum bytes copied to a temporary snapshot (16 GiB).</summary>
    public const long DefaultMaximumSnapshotBytes = 16L * 1024 * 1024 * 1024;

    private readonly string _directory;
    private readonly FileStream _readGuard;
    private bool _disposed;

    private Utf8ValidatedSnapshot(
        string directory, FileStream readGuard, Utf8DiskLineIndex index)
    {
        _directory = directory;
        _readGuard = readGuard;
        Lines = index;
    }

    /// <summary>
    /// Validated read-only line index over the temporary copy.
    /// The snapshot owns and disposes it; callers must not dispose it separately.
    /// </summary>
    public Utf8DiskLineIndex Lines { get; }

    /// <summary>Byte length of the validated copy, including any UTF-8 BOM.</summary>
    public long ByteLength => Lines.ByteLength;

    /// <summary>
    /// Copies a source into private temporary storage with a byte budget and
    /// validates the finished copy before exposing any read access.
    /// </summary>
    /// <param name="path">Original local file; never written by this method.</param>
    /// <param name="maximumSnapshotBytes">Maximum bytes permitted in the temporary copy.</param>
    /// <param name="maximumLineBytes">Per-line read limit passed to the UTF-8 index.</param>
    /// <param name="maximumIndexBytes">Disk budget for the physical-line index.</param>
    /// <param name="cancellationToken">Cancels copy and validation and cleans up.</param>
    /// <param name="temporaryRoot">Existing directory for temporary snapshot storage,
    /// or null to use the OS temporary directory. The directory itself is never removed.</param>
    /// <returns>Validated private copy and its owned physical-line index.</returns>
    public static async Task<Utf8ValidatedSnapshot> CreateAsync(
        string path,
        long maximumSnapshotBytes = DefaultMaximumSnapshotBytes,
        int maximumLineBytes = 1024 * 1024,
        long maximumIndexBytes = Utf8DiskLineIndex.DefaultMaximumIndexBytes,
        CancellationToken cancellationToken = default,
        string? temporaryRoot = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (maximumSnapshotBytes < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumSnapshotBytes));
        cancellationToken.ThrowIfCancellationRequested();

        // Use a uniquely named private subdirectory even when the caller
        // chooses a shared work volume. Never delete the caller's root.
        DirectoryInfo directory;
        if (temporaryRoot is null)
        {
            directory = Directory.CreateTempSubdirectory("afrowave-utf8-snapshot-");
        }
        else
        {
            if (!Directory.Exists(temporaryRoot))
                throw new DirectoryNotFoundException(
                    "The configured snapshot temporary root does not exist.");
            directory = Directory.CreateDirectory(Path.Combine(
                temporaryRoot, "afrowave-utf8-snapshot-" +
                    Guid.NewGuid().ToString("N")));
        }
        var destination = Path.Combine(directory.FullName, "validated.utf8");
        Utf8DiskLineIndex? index = null;
        FileStream? readGuard = null;
        try
        {
            await using (var input = new FileStream(
                path, FileMode.Open, FileAccess.Read, FileShare.Read,
                CopyBufferBytes, FileOptions.Asynchronous | FileOptions.SequentialScan))
            await using (var output = new FileStream(
                destination, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                CopyBufferBytes, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                if (input.Length > maximumSnapshotBytes)
                    throw new InvalidDataException(
                        "Source is larger than the temporary snapshot byte budget.");

                var bytes = ArrayPool<byte>.Shared.Rent(CopyBufferBytes);
                try
                {
                    long total = 0;
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var read = await input.ReadAsync(
                            bytes.AsMemory(0, CopyBufferBytes), cancellationToken)
                            .ConfigureAwait(false);
                        if (read == 0)
                            break;
                        if (read > maximumSnapshotBytes - total)
                            throw new InvalidDataException(
                                "Source exceeds the temporary snapshot byte budget.");
                        await output.WriteAsync(bytes.AsMemory(0, read), cancellationToken)
                            .ConfigureAwait(false);
                        total = checked(total + read);
                    }
                    if (input.Length != total)
                        throw new IOException("Source size changed during copying.");
                    await output.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(bytes);
                }
            }

            // Validate the private copy, not the original file. No partially
            // copied or unchecked data becomes visible to callers.
            index = await Utf8DiskLineIndex.OpenAsync(
                destination, maximumLineBytes, maximumIndexBytes, cancellationToken)
                .ConfigureAwait(false);

            // On Windows, keep a read-only handle sharing reads but not writes
            // for the lifetime of the snapshot. Private-directory isolation
            // limits accidental modification on other platforms as well.
            readGuard = new FileStream(
                destination, FileMode.Open, FileAccess.Read, FileShare.Read,
                CopyBufferBytes, FileOptions.Asynchronous);
            return new Utf8ValidatedSnapshot(directory.FullName, readGuard, index);
        }
        catch
        {
            if (readGuard is not null)
                await readGuard.DisposeAsync().ConfigureAwait(false);
            if (index is not null)
                await index.DisposeAsync().ConfigureAwait(false);
            Directory.Delete(directory.FullName, recursive: true);
            throw;
        }
    }

    /// <summary>
    /// Closes the indexed copy and deletes the private temporary directory.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        try
        {
            await Lines.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            await _readGuard.DisposeAsync().ConfigureAwait(false);
            Directory.Delete(_directory, recursive: true);
        }
    }
}
