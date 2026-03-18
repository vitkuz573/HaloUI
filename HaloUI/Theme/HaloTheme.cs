using HaloUI.Theme.Tokens;

namespace HaloUI.Theme;

public sealed class HaloTheme
{
    /// <summary>
    /// Enterprise-grade design token system.
    /// Provides access to core, semantic, and component-level design tokens.
    /// </summary>
    public DesignTokenSystem Tokens { get; init; } = DesignTokenSystem.Light;

    public HaloTheme Clone() => new()
    {
        Tokens = Tokens,
    };
}