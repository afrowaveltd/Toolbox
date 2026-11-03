namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

public enum MdViewSourceKind { Interactive, Local, Url, GitHub, Help }

public sealed record MdViewCommand(MdViewSourceKind Kind, string? Value = null, string? Repository = null)
{
    public static MdViewCommand Parse(params string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length == 0) return new(MdViewSourceKind.Interactive);
        if (args.Length == 1)
        {
            var value = args[0];
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Empty source.");
            if (value is "-h" or "--help" or "/?") return new(MdViewSourceKind.Help);
            if (value.StartsWith("github:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = value["github:".Length..].Split('/');
                if (parts.Length != 2 || parts.Any(string.IsNullOrWhiteSpace))
                    throw new ArgumentException("Use github:OWNER/REPOSITORY.");
                return new(MdViewSourceKind.GitHub, parts[0], parts[1]);
            }
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                uri.Scheme is "http" or "https")
                return new(MdViewSourceKind.Url, value);
            return new(MdViewSourceKind.Local, value);
        }
        if (args.Length == 2)
        {
            if (args[0].Equals("local", StringComparison.OrdinalIgnoreCase))
                return new(MdViewSourceKind.Local, Require(args[1]));
            if (args[0].Equals("url", StringComparison.OrdinalIgnoreCase))
                return new(MdViewSourceKind.Url, Require(args[1]));
            if (args[0].Equals("git", StringComparison.OrdinalIgnoreCase) ||
                args[0].Equals("github", StringComparison.OrdinalIgnoreCase))
            {
                var pieces = Require(args[1]).Split('/');
                if (pieces.Length != 2 || pieces.Any(string.IsNullOrWhiteSpace))
                    throw new ArgumentException("Use git OWNER REPOSITORY or git OWNER/REPOSITORY.");
                return new(MdViewSourceKind.GitHub, pieces[0], pieces[1]);
            }
        }
        if (args.Length == 3 &&
            (args[0].Equals("git", StringComparison.OrdinalIgnoreCase) ||
             args[0].Equals("github", StringComparison.OrdinalIgnoreCase)))
            return new(MdViewSourceKind.GitHub, Require(args[1]), Require(args[2]));
        throw new ArgumentException("Unsupported arguments. Use --help for command syntax.");
    }

    private static string Require(string value) =>
        !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Source cannot be empty.");
}
