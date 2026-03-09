// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HaloUI.ThemeSdk.Internal;

public sealed record ThemeSdkSnapshot(
    [property: JsonPropertyName("uiKitVersion")] string HaloVersion,
    [property: JsonPropertyName("generatedAt")] DateTimeOffset GeneratedAt,
    [property: JsonPropertyName("entries")] IReadOnlyList<ThemeSdkSnapshotEntry> Entries,
    [property: JsonPropertyName("indexEntries")] IReadOnlyList<ThemeVariableIndexSnapshotEntry> IndexEntries,
    [property: JsonPropertyName("documentation")] DocumentationSnapshot Documentation);

public sealed record ThemeSdkSnapshotEntry(
    [property: JsonPropertyName("variable")] string Variable,
    [property: JsonPropertyName("accessor")] string Accessor,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("isAlias")] bool IsAlias,
    [property: JsonPropertyName("aliasTarget")] string? AliasTarget,
    [property: JsonPropertyName("accessorSegments")] IReadOnlyList<string> AccessorSegments,
    [property: JsonPropertyName("variableSegments")] IReadOnlyList<string> VariableSegments);

public sealed record ThemeVariableIndexSnapshotEntry(
    [property: JsonPropertyName("variable")] string Variable,
    [property: JsonPropertyName("accessor")] string Accessor,
    [property: JsonPropertyName("isAlias")] bool IsAlias,
    [property: JsonPropertyName("aliasTarget")] string? AliasTarget);

public sealed record DocumentationSnapshot(
    [property: JsonPropertyName("markdownHash")] string MarkdownHash,
    [property: JsonPropertyName("markdownLength")] int MarkdownLength,
    [property: JsonPropertyName("jsonHash")] string JsonHash,
    [property: JsonPropertyName("jsonLength")] int JsonLength);