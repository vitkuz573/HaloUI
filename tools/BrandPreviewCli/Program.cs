// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;

using var serviceProvider = new ServiceCollection()
    .AddSingleton<IFileSystem, FileSystem>()
    .BuildServiceProvider();
var fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
var arguments = ParseArgs(args);
var manifestPath = ResolveManifestPath(arguments.GetValueOrDefault("--manifest"));
var outputDirectory = ResolveOutputPath(arguments.GetValueOrDefault("--output"));

if (!fileSystem.File.Exists(manifestPath))
{
    Console.Error.WriteLine($"Manifest not found: {manifestPath}");
    return 1;
}

fileSystem.Directory.CreateDirectory(outputDirectory);

var manifest = JsonNode.Parse(fileSystem.File.ReadAllText(manifestPath))?.AsObject()
    ?? throw new InvalidOperationException("Unable to parse design token manifest.");

var themeKeys = GetRequestedKeys(arguments.GetValueOrDefault("--themes"), manifest["themes"]?.AsObject());
var brandKeys = GetRequestedKeys(arguments.GetValueOrDefault("--brands"), manifest["brands"]?.AsObject());

GenerateBrandSnapshots(manifest, themeKeys, brandKeys, outputDirectory);

Console.WriteLine($"Brand previews generated under {outputDirectory}");
return 0;

static Dictionary<string, string?> ParseArgs(string[] commandLineArgs)
{
    var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    for (var i = 0; i < commandLineArgs.Length; i++)
    {
        var current = commandLineArgs[i];
        if (!current.StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        if (i + 1 < commandLineArgs.Length && !commandLineArgs[i + 1].StartsWith("--", StringComparison.Ordinal))
        {
            dict[current] = commandLineArgs[i + 1];
            i++;
        }
        else
        {
            dict[current] = null;
        }
    }

    return dict;
}

string ResolveManifestPath(string? manifestArg)
{
    if (!string.IsNullOrWhiteSpace(manifestArg))
    {
        return fileSystem.Path.GetFullPath(manifestArg);
    }

    var repoCandidate = fileSystem.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "HaloUI", "Theme", "Tokens", "design-tokens.json");
    repoCandidate = fileSystem.Path.GetFullPath(repoCandidate);
    if (fileSystem.File.Exists(repoCandidate))
    {
        return repoCandidate;
    }

    var siblingCandidate = fileSystem.Path.Combine(AppContext.BaseDirectory, "..", "HaloUI", "Theme", "Tokens", "design-tokens.json");
    siblingCandidate = fileSystem.Path.GetFullPath(siblingCandidate);
    return siblingCandidate;
}

string ResolveOutputPath(string? outputArg)
{
    if (!string.IsNullOrWhiteSpace(outputArg))
    {
        return fileSystem.Path.GetFullPath(outputArg);
    }

    var defaultPath = fileSystem.Path.Combine(Environment.CurrentDirectory, "brand-previews");
    return fileSystem.Path.GetFullPath(defaultPath);
}

static IReadOnlyList<string> GetRequestedKeys(string? argument, JsonObject? source)
{
    var available = source?.Select(p => p.Key).ToList() ?? new List<string>();

    if (string.IsNullOrWhiteSpace(argument))
    {
        return available;
    }

    var requested = argument
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToList();

    return requested.Count == 0 ? available : requested;
}

void GenerateBrandSnapshots(JsonObject manifest, IReadOnlyList<string> themes, IReadOnlyList<string> brands, string outputDirectory)
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = null
    };

    WriteBrandPalettes(manifest["brands"]?.AsObject(), brands, outputDirectory, options);
    var aggregatedThemes = WriteThemeSnapshots(manifest["themes"]?.AsObject(), themes, outputDirectory, options);

    if (aggregatedThemes.Count > 0)
    {
        var aggregatedPath = fileSystem.Path.Combine(outputDirectory, "themes.json");
        fileSystem.File.WriteAllText(aggregatedPath, aggregatedThemes.ToJsonString(options) + Environment.NewLine, Encoding.UTF8);
        Console.WriteLine($"[brand-preview] Aggregated themes exported to {aggregatedPath}");
    }
}

void WriteBrandPalettes(JsonObject? brandsNode, IReadOnlyList<string> brands, string outputDirectory, JsonSerializerOptions options)
{
    if (brandsNode is null || brands.Count == 0)
    {
        return;
    }

    var output = new JsonObject();

    foreach (var brandKey in brands)
    {
        if (brandsNode.TryGetPropertyValue(brandKey, out var brandNode) && brandNode is not null)
        {
            output[brandKey] = brandNode.DeepClone();
        }
        else
        {
            Console.Error.WriteLine($"[brand-preview] Brand '{brandKey}' not found in manifest.");
        }
    }

    if (output.Count > 0)
    {
        var brandsPath = fileSystem.Path.Combine(outputDirectory, "brands.json");
        fileSystem.File.WriteAllText(brandsPath, output.ToJsonString(options) + Environment.NewLine, Encoding.UTF8);
        Console.WriteLine($"[brand-preview] Brand palette exported to {brandsPath}");
    }
}

JsonObject WriteThemeSnapshots(JsonObject? themesNode, IReadOnlyList<string> themes, string outputDirectory, JsonSerializerOptions options)
{
    var aggregated = new JsonObject();

    if (themesNode is null || themes.Count == 0)
    {
        Console.Error.WriteLine("[brand-preview] No themes found in manifest.");
        return aggregated;
    }

    foreach (var themeKey in themes)
    {
        if (!themesNode.TryGetPropertyValue(themeKey, out var themeNode) || themeNode is null)
        {
            Console.Error.WriteLine($"[brand-preview] Theme '{themeKey}' not found.");
            continue;
        }

        var themeDir = fileSystem.Path.Combine(outputDirectory, Sanitize(themeKey));
        fileSystem.Directory.CreateDirectory(themeDir);

        var snapshot = new JsonObject
        {
            ["theme"] = themeKey,
            ["generatedAt"] = DateTimeOffset.UtcNow,
            ["semantic"] = themeNode["semantic"]?.DeepClone(),
            ["component"] = themeNode["component"]?.DeepClone(),
            ["motion"] = themeNode["motion"]?.DeepClone(),
            ["accessibility"] = themeNode["accessibility"]?.DeepClone()
        };

        var jsonPath = fileSystem.Path.Combine(themeDir, $"{Sanitize(themeKey)}.tokens.json");
        fileSystem.File.WriteAllText(jsonPath, snapshot.ToJsonString(options) + Environment.NewLine, Encoding.UTF8);

        var markdownPath = fileSystem.Path.Combine(themeDir, $"{Sanitize(themeKey)}.summary.md");
        fileSystem.File.WriteAllText(markdownPath, BuildMarkdown(themeKey, themeNode.AsObject()), Encoding.UTF8);

        aggregated[themeKey] = snapshot.DeepClone();
        Console.WriteLine($"[brand-preview] Theme '{themeKey}' exported to {themeDir}");
    }

    return aggregated;
}

string Sanitize(string value)
{
    var invalidChars = fileSystem.Path.GetInvalidFileNameChars();
    var builder = new StringBuilder(value.Length);

    foreach (var ch in value)
    {
        builder.Append(invalidChars.Contains(ch) ? '-' : char.ToLowerInvariant(ch));
    }

    return builder.ToString();
}

static string BuildMarkdown(string themeKey, JsonObject themeNode)
{
    var sb = new StringBuilder();
    sb.AppendLine($"# {themeKey} Token Preview");
    sb.AppendLine();

    AppendSemanticColors(sb, themeNode["semantic"] as JsonObject);
    AppendButtonSummary(sb, themeNode["component"] as JsonObject);
    AppendSelectSummary(sb, themeNode["component"] as JsonObject);
    AppendTabSummary(sb, themeNode["component"] as JsonObject);

    sb.AppendLine();
    sb.AppendLine("_Generated by BrandPreviewCli_");

    return sb.ToString();
}

static void AppendSemanticColors(StringBuilder sb, JsonObject? semanticNode)
{
    if (semanticNode?["color"] is not JsonObject colors || colors.Count == 0)
    {
        return;
    }

    sb.AppendLine("## Semantic Colors");
    sb.AppendLine("| Token | Value |");
    sb.AppendLine("|-------|-------|");

    foreach (var entry in colors)
    {
        var value = entry.Value?.ToString() ?? string.Empty;
        sb.AppendLine($"| {entry.Key} | `{value}` |");
    }

    sb.AppendLine();
}

static void AppendButtonSummary(StringBuilder sb, JsonObject? componentNode)
{
    if (componentNode?["Button"] is not JsonObject buttonNode)
    {
        return;
    }

    sb.AppendLine("## Button Variants");
    AppendVariantTable(sb, "Primary", buttonNode["Primary"] as JsonObject, new[] { "Background", "Text", "Border" });
    AppendVariantTable(sb, "Danger", buttonNode["Danger"] as JsonObject, new[] { "Background", "Text", "Border" });
    AppendVariantTable(sb, "Warning", buttonNode["Warning"] as JsonObject, new[] { "Background", "Text", "Border" });
}

static void AppendSelectSummary(StringBuilder sb, JsonObject? componentNode)
{
    if (componentNode?["Select"] is not JsonObject selectNode)
    {
        return;
    }

    var optionDefault = selectNode["Option"]?["Default"] as JsonObject;
    if (optionDefault is null)
    {
        return;
    }

    sb.AppendLine("## Select Option (Default)");
    sb.AppendLine("| Property | Value |");
    sb.AppendLine("|----------|-------|");

    AppendKey(optionDefault, sb, "Background");
    AppendKey(optionDefault, sb, "Text");

    sb.AppendLine();
}

static void AppendTabSummary(StringBuilder sb, JsonObject? componentNode)
{
    if (componentNode?["Tab"] is not JsonObject tabNode)
    {
        return;
    }

    var inactive = tabNode["Inactive"] as JsonObject;
    if (inactive is null)
    {
        return;
    }

    sb.AppendLine("## Tab Inactive State");
    sb.AppendLine("| Property | Value |");
    sb.AppendLine("|----------|-------|");

    AppendKey(inactive, sb, "Background");
    AppendKey(inactive, sb, "TextColor");
    AppendKey(inactive, sb, "IconColor");

    sb.AppendLine();
}

static void AppendVariantTable(StringBuilder sb, string title, JsonObject? variant, IEnumerable<string> fields)
{
    if (variant is null)
    {
        return;
    }

    sb.AppendLine($"### {title}");
    sb.AppendLine("| Property | Value |");
    sb.AppendLine("|----------|-------|");

    foreach (var field in fields)
    {
        AppendKey(variant, sb, field);
    }

    sb.AppendLine();
}

static void AppendKey(JsonObject obj, StringBuilder sb, string key)
{
    if (obj.TryGetPropertyValue(key, out var node) && node is not null)
    {
        var value = node.ToString().Replace("|", "\\|", StringComparison.Ordinal);
        sb.AppendLine($"| {key} | `{value}` |");
    }
}
