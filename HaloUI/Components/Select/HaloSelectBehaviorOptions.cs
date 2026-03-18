namespace HaloUI.Components.Select;

/// <summary>
/// General behavior options for <see cref="HaloSelect{TValue}" />.
/// </summary>
public sealed record HaloSelectBehaviorOptions
{
    /// <summary>
    /// Shared immutable default instance.
    /// </summary>
    public static HaloSelectBehaviorOptions Default { get; } = new();

    /// <summary>
    /// Controls whether the component renders a custom dropdown or a native <c>&lt;select&gt;</c>.
    /// </summary>
    public HaloSelectPresentation Presentation { get; init; } = HaloSelectPresentation.Custom;

    /// <summary>
    /// Opens the custom dropdown above the trigger instead of below.
    /// </summary>
    public bool OpenUpward { get; init; }

    /// <summary>
    /// Optional max dropdown height in pixels for custom presentation.
    /// </summary>
    public double? MaxDropdownHeightPx { get; init; }
}

/// <summary>
/// Select rendering mode.
/// </summary>
public enum HaloSelectPresentation
{
    /// <summary>
    /// Renders HaloUI custom select trigger and listbox.
    /// </summary>
    Custom = 0,

    /// <summary>
    /// Renders native browser <c>&lt;select&gt;</c>.
    /// </summary>
    Native = 1
}

/// <summary>
/// Enum option generation behavior for <see cref="HaloSelect{TValue}" />.
/// </summary>
/// <typeparam name="TValue">The select value type.</typeparam>
public sealed record HaloSelectEnumBehavior<TValue>
{
    /// <summary>
    /// Shared immutable disabled instance.
    /// </summary>
    public static HaloSelectEnumBehavior<TValue> Disabled { get; } = new();

    /// <summary>
    /// Enables enum option auto-generation.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Includes an explicit null option for nullable enum values.
    /// </summary>
    public bool IncludeNullOption { get; init; }

    /// <summary>
    /// Optional null-option text. Falls back to <c>Placeholder</c>.
    /// </summary>
    public string? NullOptionText { get; init; }

    /// <summary>
    /// Optional enum text selector.
    /// </summary>
    public Func<TValue, string>? TextSelector { get; init; }

    /// <summary>
    /// Optional enum inclusion filter.
    /// </summary>
    public Func<TValue, bool>? Filter { get; init; }

    /// <summary>
    /// Optional enum disabled selector.
    /// </summary>
    public Func<TValue, bool>? DisabledSelector { get; init; }
}
