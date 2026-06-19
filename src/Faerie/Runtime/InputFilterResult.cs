namespace Faerie.Runtime;

/// <summary>
/// Result of an input filter registered via <see cref="Building.GameBuilder.FilterInput"/>.
/// </summary>
public readonly struct InputFilterResult
{
    private readonly string? _message;

    private InputFilterResult(bool continueProcessing, string? message)
    {
        ShouldContinue = continueProcessing;
        _message = message;
    }

    /// <summary>When false, parsing and turn execution are skipped for this line.</summary>
    public bool ShouldContinue { get; }

    /// <summary>
    /// When <see cref="ShouldContinue"/> is false and this is non-null, the message is printed.
    /// </summary>
    public string? Message => _message;

    /// <summary>Continue with normal parse and command execution.</summary>
    public static InputFilterResult Continue => new(true, null);

    /// <summary>Stop processing this line without advancing a turn.</summary>
    public static InputFilterResult Reject(string? message = null) => new(false, message);
}
