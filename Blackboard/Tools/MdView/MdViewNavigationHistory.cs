namespace Afrowave.Toolbox.Blackboard.Tools.MdView;

/// <summary>
/// Bounded document-and-fragment navigation history, independent of source IO.
/// Callers should record a visit only after the target has opened successfully.
/// </summary>
public sealed class MdViewNavigationHistory
{
    /// <summary>Default maximum retained locations, including current (128).</summary>
    public const int DefaultCapacity = 128;

    private readonly List<MdViewResolvedLink> _locations = [];
    private int _index = -1;

    /// <summary>Creates a bounded history without an initial location.</summary>
    /// <param name="capacity">Maximum retained locations, 1 through 4096.</param>
    public MdViewNavigationHistory(int capacity = DefaultCapacity)
    {
        if (capacity is < 1 or > 4096)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        Capacity = capacity;
    }

    /// <summary>Maximum number of retained locations.</summary>
    public int Capacity { get; }

    /// <summary>Current location or null before the first visit.</summary>
    public MdViewResolvedLink? Current => _index < 0 ? null : _locations[_index];

    /// <summary>Whether a previous location exists.</summary>
    public bool CanGoBack => _index > 0;

    /// <summary>Whether a forward location exists.</summary>
    public bool CanGoForward => _index >= 0 && _index < _locations.Count - 1;

    /// <summary>Number of retained history locations.</summary>
    public int Count => _locations.Count;

    /// <summary>Next location if moving backward, without changing history.</summary>
    public MdViewResolvedLink? BackTarget => CanGoBack ? _locations[_index - 1] : null;

    /// <summary>Next location if moving forward, without changing history.</summary>
    public MdViewResolvedLink? ForwardTarget => CanGoForward ? _locations[_index + 1] : null;

    /// <summary>Records one successfully opened document/fragment location.</summary>
    /// <param name="target">New resolved location.</param>
    public void Visit(MdViewResolvedLink target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (target == Current)
            return;
        if (CanGoForward)
            _locations.RemoveRange(_index + 1, _locations.Count - _index - 1);
        _locations.Add(target);
        _index = _locations.Count - 1;
        if (_locations.Count > Capacity)
        {
            _locations.RemoveAt(0);
            _index--;
        }
    }

    /// <summary>Moves to the previously visited location; returns false at the beginning.</summary>
    /// <returns>True if the history position changed.</returns>
    public bool GoBack()
    {
        if (!CanGoBack)
            return false;
        _index--;
        return true;
    }

    /// <summary>Moves to the next location; returns false at the end.</summary>
    /// <returns>True if the history position changed.</returns>
    public bool GoForward()
    {
        if (!CanGoForward)
            return false;
        _index++;
        return true;
    }
}
