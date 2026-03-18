using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using HaloUI.Theme.Sdk.Css;
using HaloUI.Theme.Sdk.Documentation;
using HaloUI.Theme.Sdk.Metadata;
using HaloUI.Theme.Sdk.Lookup;
using HaloUI.ThemeSdk.Internal;

using var serviceProvider = new ServiceCollection()
    .AddSingleton<IFileSystem, FileSystem>()
    .BuildServiceProvider();
var fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
var argsMap = ParseArgs(args);
var outputPath = argsMap.TryGetValue("--output", out var output) && !string.IsNullOrWhiteSpace(output)
    ? output
    : fileSystem.Path.Combine(Environment.CurrentDirectory, "ThemeSdkSnapshot.json");

var snapshot = BuildSnapshot();
fileSystem.Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(outputPath)!);

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

var json = JsonSerializer.Serialize(snapshot, jsonOptions);
fileSystem.File.WriteAllText(outputPath, json + Environment.NewLine, Encoding.UTF8);
Console.WriteLine($"Theme SDK snapshot written to {outputPath}");

return 0;

static Dictionary<string, string?> ParseArgs(string[] commandLineArgs)
{
    var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    for (var i = 0; i < commandLineArgs.Length; i++)
    {
        var current = commandLineArgs[i];
        if (!current.StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        if (i + 1 < commandLineArgs.Length && !commandLineArgs[i + 1].StartsWith("--", StringComparison.Ordinal))
        {
            result[current] = commandLineArgs[i + 1];
            i++;
        }
        else
        {
            result[current] = null;
        }
    }

    return result;
}

static ThemeSdkSnapshot BuildSnapshot()
{
    var metadata = ThemeCssVariables.AllMetadata
        .Select(ToSnapshotEntry)
        .ToList();

    var indexEntries = ThemeVariableIndex.Entries
        .Select(entry => new ThemeVariableIndexSnapshotEntry(
            entry.Variable,
            entry.Accessor,
            entry.IsAlias,
            entry.AliasTarget))
        .ToList();

    var docs = new DocumentationSnapshot(
        ComputeHash(ThemeDocs.CssVariablesMarkdown),
        ThemeDocs.CssVariablesMarkdown.Length,
        ComputeHash(ThemeDocs.CssVariablesJson),
        ThemeDocs.CssVariablesJson.Length);

    var version = GetAssemblyInformationalVersion(typeof(ThemeCssVariables).Assembly);

    return new ThemeSdkSnapshot(
        version,
        DateTimeOffset.UtcNow,
        metadata,
        indexEntries,
        docs);
}

static ThemeSdkSnapshotEntry ToSnapshotEntry(ThemeCssVariableMetadata metadata)
{
    return new ThemeSdkSnapshotEntry(
        metadata.Variable,
        NormalizeAccessor(metadata.Accessor),
        metadata.Category,
        metadata.IsAlias,
        metadata.AliasTarget,
        metadata.AccessorSegments,
        metadata.VariableSegments);
}

static string NormalizeAccessor(string accessor)
{
    var segments = accessor.Split('.');

    if (segments.Length < 3)
    {
        return accessor;
    }

    var builder = new List<string>(segments.Length);
    builder.AddRange(segments[0..3]);

    string? previous = builder.Last();

    for (var i = 3; i < segments.Length; i++)
    {
        var current = segments[i];

        if (string.Equals(current, previous, StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        builder.Add(current);
        previous = current;
    }

    return string.Join('.', builder);
}

static string ComputeHash(string value)
{
    var bytes = Encoding.UTF8.GetBytes(value);
    var hash = SHA256.HashData(bytes);
    return Convert.ToHexString(hash).ToLowerInvariant();
}

static string GetAssemblyInformationalVersion(Assembly assembly)
{
    var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
    if (!string.IsNullOrEmpty(attribute?.InformationalVersion))
    {
        return attribute.InformationalVersion;
    }

    var version = assembly.GetName().Version;
    return version?.ToString() ?? "0.0.0.0";
}
