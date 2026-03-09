// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using HaloUI.ThemeSdk.Internal;

using var serviceProvider = new ServiceCollection()
    .AddSingleton<IFileSystem, FileSystem>()
    .BuildServiceProvider();
var fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
var arguments = ParseArguments(args);
if (!arguments.TryGetValue("--old", out var oldPath) || string.IsNullOrWhiteSpace(oldPath))
{
    throw new ArgumentException("Missing required --old <path> argument.");
}

if (!arguments.TryGetValue("--new", out var newPath) || string.IsNullOrWhiteSpace(newPath))
{
    throw new ArgumentException("Missing required --new <path> argument.");
}

var outputPath = arguments.TryGetValue("--output", out var output) && !string.IsNullOrWhiteSpace(output)
    ? output
    : fileSystem.Path.Combine(Environment.CurrentDirectory, "ThemeSdkChanges.txt");

var format = arguments.TryGetValue("--format", out var formatValue) && !string.IsNullOrWhiteSpace(formatValue)
    ? formatValue
    : "text";

var baseline = LoadSnapshot(oldPath);
var current = LoadSnapshot(newPath);
var diff = ThemeSdkDiff.Create(baseline, current);
fileSystem.Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(outputPath)!);

switch (format.ToLowerInvariant())
{
    case "text":
        fileSystem.File.WriteAllText(outputPath, ChangeLogFormatter.Format(diff), Encoding.UTF8);
        break;
    case "json":
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        fileSystem.File.WriteAllText(outputPath, JsonSerializer.Serialize(diff, jsonOptions) + Environment.NewLine, Encoding.UTF8);
        break;
    default:
        throw new InvalidOperationException($"Unsupported format '{format}'.");
}

Console.WriteLine($"Theme SDK diff written to {outputPath}");
return 0;

static Dictionary<string, string?> ParseArguments(string[] commandLineArgs)
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

ThemeSdkSnapshot LoadSnapshot(string path)
{
    if (!fileSystem.File.Exists(path))
    {
        throw new FileNotFoundException($"Snapshot not found: {path}", path);
    }

    var json = fileSystem.File.ReadAllText(path);
    return JsonSerializer.Deserialize<ThemeSdkSnapshot>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    }) ?? throw new InvalidOperationException($"Snapshot '{path}' could not be deserialized.");
}

internal sealed record ThemeSdkDiff(
    ThemeSdkSnapshotMetadata Baseline,
    ThemeSdkSnapshotMetadata Current,
    IReadOnlyList<string> Events)
{
    public static ThemeSdkDiff Create(ThemeSdkSnapshot baseline, ThemeSdkSnapshot current)
    {
        var events = new List<string>();

        if (!string.Equals(baseline.HaloVersion, current.HaloVersion, StringComparison.Ordinal))
        {
            events.Add($"Halo version {baseline.HaloVersion} => {current.HaloVersion}");
        }

        if (baseline.GeneratedAt != current.GeneratedAt)
        {
            events.Add($"Snapshot timestamp {baseline.GeneratedAt:O} => {current.GeneratedAt:O}");
        }

        AppendEntryDiffs(baseline, current, events);
        AppendIndexDiffs(baseline, current, events);
        AppendDocumentationDiffs(baseline.Documentation, current.Documentation, events);

        return new ThemeSdkDiff(
            new ThemeSdkSnapshotMetadata(baseline.HaloVersion, baseline.GeneratedAt),
            new ThemeSdkSnapshotMetadata(current.HaloVersion, current.GeneratedAt),
            events);
    }

    private static void AppendEntryDiffs(ThemeSdkSnapshot baseline, ThemeSdkSnapshot current, List<string> events)
    {
        var baselineMap = baseline.Entries.ToDictionary(entry => entry.Variable, StringComparer.Ordinal);
        var currentMap = current.Entries.ToDictionary(entry => entry.Variable, StringComparer.Ordinal);

        foreach (var entry in current.Entries)
        {
            if (!baselineMap.ContainsKey(entry.Variable))
            {
                events.Add($"{entry.Accessor} added ({entry.Variable})");
            }
        }

        foreach (var entry in baseline.Entries)
        {
            if (!currentMap.TryGetValue(entry.Variable, out var newEntry))
            {
                events.Add($"{entry.Accessor} removed ({entry.Variable})");
                continue;
            }

            CollectEntryDiffs(entry, newEntry, events);
        }
    }

    private static void CollectEntryDiffs(ThemeSdkSnapshotEntry oldEntry, ThemeSdkSnapshotEntry newEntry, List<string> events)
    {
        if (!string.Equals(oldEntry.Accessor, newEntry.Accessor, StringComparison.Ordinal))
        {
            events.Add($"{oldEntry.Accessor} accessor => {newEntry.Accessor}");
        }

        if (!string.Equals(oldEntry.Category, newEntry.Category, StringComparison.Ordinal))
        {
            events.Add($"{newEntry.Accessor} category {oldEntry.Category} => {newEntry.Category}");
        }

        if (oldEntry.IsAlias != newEntry.IsAlias)
        {
            events.Add($"{newEntry.Accessor} alias flag {oldEntry.IsAlias} => {newEntry.IsAlias}");
        }

        if (!string.Equals(oldEntry.AliasTarget, newEntry.AliasTarget, StringComparison.Ordinal))
        {
            events.Add($"{newEntry.Accessor} aliasTarget {FormatNull(oldEntry.AliasTarget)} => {FormatNull(newEntry.AliasTarget)}");
        }

        if (!SegmentsEqual(oldEntry.AccessorSegments, newEntry.AccessorSegments))
        {
            events.Add($"{newEntry.Accessor} accessorSegments {FormatSegments(oldEntry.AccessorSegments)} => {FormatSegments(newEntry.AccessorSegments)}");
        }

        if (!SegmentsEqual(oldEntry.VariableSegments, newEntry.VariableSegments))
        {
            events.Add($"{newEntry.Accessor} variableSegments {FormatSegments(oldEntry.VariableSegments)} => {FormatSegments(newEntry.VariableSegments)}");
        }
    }

    private static void AppendIndexDiffs(ThemeSdkSnapshot baseline, ThemeSdkSnapshot current, List<string> events)
    {
        var baselineMap = baseline.IndexEntries.ToDictionary(entry => entry.Variable, StringComparer.Ordinal);
        var currentMap = current.IndexEntries.ToDictionary(entry => entry.Variable, StringComparer.Ordinal);

        foreach (var entry in current.IndexEntries)
        {
            if (!baselineMap.ContainsKey(entry.Variable))
            {
                events.Add($"ThemeVariableIndex entry {entry.Variable} added (accessor: {entry.Accessor})");
            }
        }

        foreach (var entry in baseline.IndexEntries)
        {
            if (!currentMap.TryGetValue(entry.Variable, out var newEntry))
            {
                events.Add($"ThemeVariableIndex entry {entry.Variable} removed (accessor: {entry.Accessor})");
                continue;
            }

            if (!string.Equals(entry.Accessor, newEntry.Accessor, StringComparison.Ordinal))
            {
                events.Add($"ThemeVariableIndex entry {entry.Variable} accessor {entry.Accessor} => {newEntry.Accessor}");
            }

            if (entry.IsAlias != newEntry.IsAlias)
            {
                events.Add($"ThemeVariableIndex entry {entry.Variable} alias flag {entry.IsAlias} => {newEntry.IsAlias}");
            }

            if (!string.Equals(entry.AliasTarget, newEntry.AliasTarget, StringComparison.Ordinal))
            {
                events.Add($"ThemeVariableIndex entry {entry.Variable} aliasTarget {FormatNull(entry.AliasTarget)} => {FormatNull(newEntry.AliasTarget)}");
            }
        }
    }

    private static void AppendDocumentationDiffs(DocumentationSnapshot baseline, DocumentationSnapshot current, List<string> events)
    {
        if (!string.Equals(baseline.MarkdownHash, current.MarkdownHash, StringComparison.Ordinal))
        {
            events.Add($"ThemeDocs.CssVariablesMarkdown hash {baseline.MarkdownHash} ({baseline.MarkdownLength}) => {current.MarkdownHash} ({current.MarkdownLength})");
        }

        if (!string.Equals(baseline.JsonHash, current.JsonHash, StringComparison.Ordinal))
        {
            events.Add($"ThemeDocs.CssVariablesJson hash {baseline.JsonHash} ({baseline.JsonLength}) => {current.JsonHash} ({current.JsonLength})");
        }
    }

    private static bool SegmentsEqual(IReadOnlyList<string> left, IReadOnlyList<string> right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left.Count != right.Count)
        {
            return false;
        }

        for (var i = 0; i < left.Count; i++)
        {
            if (!string.Equals(left[i], right[i], StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static string FormatNull(string? value) => value ?? "<null>";

    private static string FormatSegments(IReadOnlyList<string> segments)
        => string.Join('.', segments);
}

internal sealed record ThemeSdkSnapshotMetadata(string HaloVersion, DateTimeOffset GeneratedAt);

internal static class ChangeLogFormatter
{
    public static string Format(ThemeSdkDiff diff)
    {
        var builder = new StringBuilder();
        builder.Append("# Theme SDK snapshot ").Append(diff.Baseline.HaloVersion).Append(" -> ").Append(diff.Current.HaloVersion).AppendLine();
        builder.AppendLine();

        if (diff.Events.Count == 0)
        {
            builder.AppendLine("No Theme SDK differences detected.");
        }
        else
        {
            foreach (var entry in diff.Events)
            {
                builder.AppendLine(entry);
            }
        }

        builder.AppendLine();
        return builder.ToString();
    }
}
