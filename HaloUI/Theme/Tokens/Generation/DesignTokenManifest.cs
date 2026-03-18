using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;

namespace HaloUI.Theme.Tokens.Generation;

/// <summary>
/// Raw token manifest loaded from the JSON source of truth.
/// Carries untyped JSON fragments that get materialised into strongly-typed tokens.
/// </summary>
internal sealed record DesignTokenManifest
{
    public Dictionary<string, BrandManifest> Brands { get; init; } = new();
    
    public Dictionary<string, ThemeManifest> Themes { get; init; } = new();
    
    public Dictionary<string, HighContrastManifest> HighContrast { get; init; } = new();
}

internal sealed record BrandManifest
{
    public string DisplayName { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? Tagline { get; init; }

    public string? Category { get; init; }

    public string? Icon { get; init; }
    
    public BrandColorManifest Colors { get; init; } = new();
}

internal sealed record BrandColorManifest
{
    public string Primary { get; init; } = "#000000";
    
    public string Secondary { get; init; } = "#000000";
    
    public string Accent { get; init; } = "#000000";
    
    public string Neutral { get; init; } = "#000000";

    public string? Success { get; init; }
    
    public string? Warning { get; init; }
    
    public string? Danger { get; init; }
    
    public string? Info { get; init; }

    public BrandColorPaletteManifest? ExtendedPalette { get; init; }
}

internal sealed record BrandColorPaletteManifest
{
    public string? GradientPrimary { get; init; }
    
    public string? GradientSecondary { get; init; }
    
    public string? GradientAccent { get; init; }
    
    public string? Tertiary { get; init; }
    
    public string? Quaternary { get; init; }
    
    public string? PrimaryLight { get; init; }
    
    public string? PrimaryDark { get; init; }
    
    public string? SecondaryLight { get; init; }
    
    public string? SecondaryDark { get; init; }
}

internal sealed record ThemeManifest
{
    public SemanticManifest Semantic { get; init; } = new();
    
    public ComponentManifest Component { get; init; } = new();
    
    public AccessibilityManifest Accessibility { get; init; } = new();
    
    public MotionManifest Motion { get; init; } = new();
    
    public Dictionary<string, string> CssVariableAliases { get; init; } = new();

    public string Scheme { get; init; } = "Light";
}

internal sealed record SemanticManifest
{
    public JsonObject? Color { get; init; }
    
    public JsonObject? Spacing { get; init; }
    
    public JsonObject? Typography { get; init; }
    
    public JsonObject? Elevation { get; init; }
    
    public JsonObject? Size { get; init; }
}

internal sealed record ComponentManifest
{
    public IReadOnlyDictionary<string, JsonObject> Tokens { get; init; } =
        new ReadOnlyDictionary<string, JsonObject>(new Dictionary<string, JsonObject>(0, StringComparer.OrdinalIgnoreCase));

    public IEnumerable<string> Keys => Tokens.Keys;

    public IEnumerable<KeyValuePair<string, JsonObject>> Items => Tokens;

    public bool TryGet(string key, out JsonObject? value)
    {
        if (Tokens.TryGetValue(key, out var token))
        {
            value = token;
            return true;
        }

        value = null;
        return false;
    }

    public JsonObject? GetOrDefault(string key) => Tokens.TryGetValue(key, out var token) ? token : null;

    public static ComponentManifest Create(IDictionary<string, JsonObject> tokens)
    {
        var map = new Dictionary<string, JsonObject>(tokens.Count, StringComparer.OrdinalIgnoreCase);

        foreach (var pair in tokens)
        {
            map[pair.Key] = pair.Value;
        }

        return new ComponentManifest
        {
            Tokens = new ReadOnlyDictionary<string, JsonObject>(map)
        };
    }
}

internal sealed record AccessibilityManifest
{
    public JsonObject? Focus { get; init; }
    
    public JsonObject? Touch { get; init; }
    
    public JsonObject? Contrast { get; init; }
    
    public JsonObject? Motion { get; init; }
    
    public JsonObject? ScreenReader { get; init; }
}

internal sealed record MotionManifest
{
    public JsonObject? Duration { get; init; }
    
    public JsonObject? Easing { get; init; }
    
    public JsonObject? Presets { get; init; }
    
    public JsonObject? Animation { get; init; }
    
    public JsonObject? Interaction { get; init; }
}

internal sealed record HighContrastManifest
{
    public JsonObject? Semantic { get; init; }
    
    public JsonObject? Component { get; init; }
    
    public JsonObject? Accessibility { get; init; }
    
    public JsonObject? Motion { get; init; }
    
    public Dictionary<string, string> CssVariables { get; init; } = new();
}