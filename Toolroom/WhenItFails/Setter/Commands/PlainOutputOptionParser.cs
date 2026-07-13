namespace Afrowave.Toolbox.Toolroom.WhenItFails.Setter.Commands;

/// <summary>
/// Parses commands that support one optional <c>--plain</c> switch.
/// </summary>
internal static class PlainOutputOptionParser
{
    /// <summary>
    /// Parses arguments beginning at the supplied option index.
    /// </summary>
    /// <param name="args">The full command-line arguments.</param>
    /// <param name="optionStartIndex">The index where optional switches begin.</param>
    /// <param name="usePlainOutput">True when <c>--plain</c> was supplied.</param>
    /// <returns>True when the optional arguments are valid.</returns>
    public static bool TryParse(
        string[] args,
        int optionStartIndex,
        out bool usePlainOutput)
    {
        usePlainOutput = false;

        for (int index = optionStartIndex; index < args.Length; index++)
        {
            if (!string.Equals(
                    args[index],
                    "--plain",
                    StringComparison.OrdinalIgnoreCase)
                || usePlainOutput)
            {
                return false;
            }

            usePlainOutput = true;
        }

        return true;
    }
}
