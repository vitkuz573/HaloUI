// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Text.Json;

namespace HaloUI.Tests.Contracts;

internal sealed class ComponentContractManifest
{
    public IReadOnlyList<ComponentContractDescriptor> Components { get; init; } = [];

    public IReadOnlyList<DemoSectionContractDescriptor> DemoSections { get; init; } = [];

    public static ComponentContractManifest Load(string repositoryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);

        var manifestPath = Path.Combine(repositoryRoot, "contracts", "component-contracts.json");

        if (!File.Exists(manifestPath))
        {
            throw new InvalidOperationException($"Component contract manifest was not found: {manifestPath}");
        }

        var json = File.ReadAllText(manifestPath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var manifest = JsonSerializer.Deserialize<ComponentContractManifest>(json, options);

        if (manifest is null)
        {
            throw new InvalidOperationException("Failed to deserialize component contract manifest.");
        }

        if (manifest.Components.Count == 0)
        {
            throw new InvalidOperationException("Component contract manifest does not contain any components.");
        }

        return manifest;
    }
}

internal sealed record ComponentContractDescriptor
{
    public required string Name { get; init; }

    public required string AccessibilityKind { get; init; }

    public required string ResponsiveKind { get; init; }

    public bool RequireStylesheetInspection { get; init; } = true;

    public string[] EvidenceFiles { get; init; } = [];

    public string[] FocusIndicators { get; init; } = [];

    public string[] RequiredStates { get; init; } = [];
}

internal sealed record DemoSectionContractDescriptor
{
    public required string Id { get; init; }

    public required string Heading { get; init; }

    public required string PresenceSelector { get; init; }

    public required string FocusSelector { get; init; }

    public string[] RequiredStateSelectors { get; init; } = [];
}
