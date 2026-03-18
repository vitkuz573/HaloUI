using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaloUI.Iconography;

/// <summary>
/// Serializable icon pack manifest that can be used to construct an <see cref="IHaloIconResolver" />.
/// </summary>
public sealed record HaloIconPackManifest
{
    public string PackId { get; init; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HaloIconRenderMode RenderMode { get; init; } = HaloIconRenderMode.Ligature;

    public string? ProviderClass { get; init; }

    public string? DefaultViewBox { get; init; }

    public IReadOnlyList<HaloIconPackEntry> Icons { get; init; } = [];

    public static HaloIconPackManifest Parse(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var options = CreateSerializerOptions();
        var manifest = JsonSerializer.Deserialize<HaloIconPackManifest>(json, options);

        return manifest ?? throw new InvalidDataException("Icon pack manifest could not be deserialized.");
    }

    public static async Task<HaloIconPackManifest> ParseAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var options = CreateSerializerOptions();
        var manifest = await JsonSerializer.DeserializeAsync<HaloIconPackManifest>(stream, options, cancellationToken).ConfigureAwait(false);

        return manifest ?? throw new InvalidDataException("Icon pack manifest could not be deserialized.");
    }

    public string ToJson(bool indented = true)
    {
        var options = CreateSerializerOptions();
        options.WriteIndented = indented;
        return JsonSerializer.Serialize(this, options);
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}

public sealed record HaloIconPackEntry
{
    public string Name { get; init; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HaloIconRenderMode? RenderMode { get; init; }

    public string? Value { get; init; }

    public string? ProviderClass { get; init; }

    public string? ViewBox { get; init; }

    public string? AliasOf { get; init; }

    /// <summary>
    /// Optional source codepoint metadata (for example Material icon codepoint).
    /// </summary>
    public string? Codepoint { get; init; }
}
