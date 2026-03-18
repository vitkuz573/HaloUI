namespace HaloUI.Theme.Tokens.Core;

/// <summary>
/// Core border tokens - border widths and radius values.
/// Provides consistent border styling across components.
/// </summary>
public sealed record BorderTokens
{
    public BorderWidthTokens Width { get; init; } = BorderWidthTokens.Default;
    public BorderRadiusTokens Radius { get; init; } = BorderRadiusTokens.Default;

    public static BorderTokens Default { get; } = new();
}

public sealed record BorderWidthTokens
{
    public string None { get; init; } = "0";
    public string DefaultWidth { get; init; } = "1px";
    public string Thin { get; init; } = "1px";
    public string Medium { get; init; } = "2px";
    public string Thick { get; init; } = "4px";
    public string Xl { get; init; } = "8px";

    public static BorderWidthTokens Default { get; } = new();
}

public sealed record BorderRadiusTokens
{
    public string None { get; init; } = "0";
    public string Sm { get; init; } = "0.125rem";    // 2px
    public string Base { get; init; } = "0.25rem";   // 4px
    public string Md { get; init; } = "0.375rem";    // 6px
    public string Lg { get; init; } = "0.5rem";      // 8px
    public string Xl { get; init; } = "0.75rem";     // 12px
    public string Xl2 { get; init; } = "1rem";       // 16px
    public string Xl3 { get; init; } = "1.5rem";     // 24px
    public string Full { get; init; } = "9999px";

    public static BorderRadiusTokens Default { get; } = new();
}