// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HaloUI.Iconography;

if (args.Length == 0 || HasSwitch(args, "--help") || HasSwitch(args, "-h"))
{
    PrintUsage();
    return 0;
}

var command = args[0];
var options = ParseOptions(args.Skip(1).ToArray());

switch (command)
{
    case "material-icons":
        return await GenerateMaterialIconsAsync(options);
    default:
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintUsage();
        return 1;
}

static async Task<int> GenerateMaterialIconsAsync(Dictionary<string, string?> options)
{
    var owner = options.GetValueOrDefault("--owner") ?? "google";
    var repo = options.GetValueOrDefault("--repo") ?? "material-design-icons";
    var directory = options.GetValueOrDefault("--directory") ?? "font";
    var output = options.GetValueOrDefault("--output")
        ?? Path.Combine(Environment.CurrentDirectory, "artifacts", "icon-packs", "material-icons");
    var renderMode = ParseRenderMode(options.GetValueOrDefault("--render-mode"));
    var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/contents/{directory}";

    Directory.CreateDirectory(output);

    using var httpClient = CreateHttpClient();
    var files = await FetchCodepointFilesAsync(httpClient, apiUrl);

    if (files.Count == 0)
    {
        Console.Error.WriteLine("No Material Icons codepoint files discovered.");
        return 2;
    }

    var generated = new List<MaterialPackIndexEntry>();

    foreach (var file in files)
    {
        var style = ResolveStyle(file.Name);

        if (style is null)
        {
            continue;
        }

        var providerClass = ResolveProviderClass(style);
        var content = await httpClient.GetStringAsync(file.DownloadUrl);
        var entries = ParseCodepoints(content, renderMode);
        var manifest = new HaloIconPackManifest
        {
            PackId = $"material-icons-{style}",
            RenderMode = renderMode,
            ProviderClass = providerClass,
            DefaultViewBox = "0 0 24 24",
            Icons = entries
        };

        var outputFile = Path.Combine(output, $"{manifest.PackId}.json");
        await File.WriteAllTextAsync(outputFile, manifest.ToJson(indented: true));

        generated.Add(new MaterialPackIndexEntry(
            manifest.PackId,
            style,
            providerClass,
            entries.Count,
            file.Name,
            file.DownloadUrl));

        Console.WriteLine($"[generated] {outputFile} ({entries.Count} icons)");
    }

    var index = new MaterialPackIndex
    {
        GeneratedAtUtc = DateTimeOffset.UtcNow,
        SourceRepository = $"{owner}/{repo}",
        SourceDirectory = directory,
        RenderMode = renderMode,
        Packs = generated
    };

    var indexPath = Path.Combine(output, "material-icons.index.json");
    var indexOptions = CreateJsonOptions();
    indexOptions.WriteIndented = true;
    await File.WriteAllTextAsync(indexPath, JsonSerializer.Serialize(index, indexOptions));

    Console.WriteLine($"[generated] {indexPath}");
    return 0;
}

static HttpClient CreateHttpClient()
{
    var client = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("HaloUI.IconCatalog.Tool", "1.0"));
    return client;
}

static async Task<IReadOnlyList<GitHubContentEntry>> FetchCodepointFilesAsync(HttpClient client, string apiUrl)
{
    var options = CreateJsonOptions();
    var entries = await client.GetFromJsonAsync<List<GitHubContentEntry>>(apiUrl, options)
        ?? [];

    return entries
        .Where(static entry => string.Equals(entry.Type, "file", StringComparison.OrdinalIgnoreCase))
        .Where(static entry => entry.Name.EndsWith(".codepoints", StringComparison.OrdinalIgnoreCase))
        .OrderBy(static entry => entry.Name, StringComparer.OrdinalIgnoreCase)
        .ToArray();
}

static IReadOnlyList<HaloIconPackEntry> ParseCodepoints(string content, HaloIconRenderMode renderMode)
{
    var map = new Dictionary<string, HaloIconPackEntry>(StringComparer.OrdinalIgnoreCase);

    using var reader = new StringReader(content);
    string? line;

    while ((line = reader.ReadLine()) is not null)
    {
        var trimmed = line.Trim();

        if (trimmed.Length == 0 || trimmed.StartsWith('#'))
        {
            continue;
        }

        var parts = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
        {
            continue;
        }

        var name = parts[0];
        var codepoint = parts[1];
        var value = renderMode == HaloIconRenderMode.Glyph
            ? CodepointToGlyph(codepoint)
            : name;

        map[name] = new HaloIconPackEntry
        {
            Name = name,
            RenderMode = renderMode,
            Value = value,
            Codepoint = codepoint
        };
    }

    return map
        .OrderBy(static pair => pair.Key, StringComparer.OrdinalIgnoreCase)
        .Select(static pair => pair.Value)
        .ToArray();
}

static string CodepointToGlyph(string codepoint)
{
    if (!int.TryParse(codepoint, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var scalar))
    {
        throw new InvalidDataException($"Invalid codepoint value: '{codepoint}'.");
    }

    if (!Rune.TryCreate(scalar, out var rune))
    {
        throw new InvalidDataException($"Invalid unicode scalar value: '{codepoint}'.");
    }

    return rune.ToString();
}

static string? ResolveStyle(string fileName)
{
    return fileName switch
    {
        "MaterialIcons-Regular.codepoints" => "regular",
        "MaterialIconsOutlined-Regular.codepoints" => "outlined",
        "MaterialIconsRound-Regular.codepoints" => "round",
        "MaterialIconsSharp-Regular.codepoints" => "sharp",
        "MaterialIconsTwoTone-Regular.codepoints" => "twotone",
        _ => null
    };
}

static string ResolveProviderClass(string style)
{
    return style switch
    {
        "regular" => "material-icons",
        "outlined" => "material-icons-outlined",
        "round" => "material-icons-round",
        "sharp" => "material-icons-sharp",
        "twotone" => "material-icons-two-tone",
        _ => "material-icons"
    };
}

static HaloIconRenderMode ParseRenderMode(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return HaloIconRenderMode.Ligature;
    }

    if (Enum.TryParse<HaloIconRenderMode>(value, ignoreCase: true, out var parsed))
    {
        return parsed;
    }

    throw new ArgumentException($"Unsupported render mode '{value}'. Expected one of: Ligature, Glyph, SvgPath, CssClass.");
}

static Dictionary<string, string?> ParseOptions(string[] args)
{
    var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    for (var i = 0; i < args.Length; i++)
    {
        var current = args[i];

        if (!current.StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
        {
            result[current] = args[i + 1];
            i++;
        }
        else
        {
            result[current] = null;
        }
    }

    return result;
}

static bool HasSwitch(string[] args, string value)
{
    return args.Any(arg => string.Equals(arg, value, StringComparison.OrdinalIgnoreCase));
}

static JsonSerializerOptions CreateJsonOptions()
{
    var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    options.Converters.Add(new JsonStringEnumConverter());
    return options;
}

static void PrintUsage()
{
    Console.WriteLine(
        """
        HaloUI.IconCatalog.Tool

        Commands:
          material-icons   Fetch and generate icon pack manifests from google/material-design-icons codepoints.

        material-icons options:
          --owner <owner>            GitHub owner (default: google)
          --repo <repo>              GitHub repository (default: material-design-icons)
          --directory <path>         Repository directory with .codepoints files (default: font)
          --output <directory>       Output directory for generated manifests
          --render-mode <mode>       Ligature or Glyph (default: Ligature)
        """
    );
}

internal sealed record GitHubContentEntry(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("download_url")] string DownloadUrl);

internal sealed record MaterialPackIndex
{
    public DateTimeOffset GeneratedAtUtc { get; init; }

    public string SourceRepository { get; init; } = string.Empty;

    public string SourceDirectory { get; init; } = string.Empty;

    public HaloIconRenderMode RenderMode { get; init; }

    public IReadOnlyList<MaterialPackIndexEntry> Packs { get; init; } = [];
}

internal sealed record MaterialPackIndexEntry(
    string PackId,
    string Style,
    string ProviderClass,
    int IconCount,
    string SourceFile,
    string SourceUrl);
