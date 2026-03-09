// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Enums;

namespace HaloUI.Components;

public partial class HaloBadge
{
    private const string DefaultLivePoliteness = "polite";

    [Parameter]
    public string? Text { get; set; }
    
    [Parameter]
    public string? Icon { get; set; }
    
    [Parameter]
    public BadgeVariant Variant { get; set; } = BadgeVariant.Neutral;
    
    [Parameter]
    public string? Class { get; set; }
    
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool AnnounceChanges { get; set; }

    [Parameter]
    public string AriaLive { get; set; } = DefaultLivePoliteness;

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override bool ShouldRender() => true;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (HasVisibleTextContent())
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(AriaLabel))
        {
            return;
        }

        if (AdditionalAttributes is not null &&
            AdditionalAttributes.TryGetValue("aria-label", out var rawAriaLabel) &&
            rawAriaLabel is string ariaLabel &&
            !string.IsNullOrWhiteSpace(ariaLabel))
        {
            return;
        }

        throw new InvalidOperationException("HaloBadge without visible text must define an accessible name via AriaLabel.");
    }

    private string BuildCssClass()
    {
        var classes = new List<string>
        {
            "ui-badge",
            GetVariantClass(Variant)
        };

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class!);
        }

        return string.Join(' ', classes);
    }

    private static string GetVariantClass(BadgeVariant variant)
    {
        return variant switch
        {
            BadgeVariant.Success => "ui-badge--success",
            BadgeVariant.Warning => "ui-badge--warning",
            BadgeVariant.Danger => "ui-badge--danger",
            BadgeVariant.Info => "ui-badge--info",
            _ => "ui-badge--neutral"
        };
    }

    private Dictionary<string, object>? BuildAccessibilityAttributes()
    {
        var attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (AnnounceChanges)
        {
            attributes["role"] = "status";
            attributes["aria-live"] = NormalizeLivePoliteness(AriaLive);
        }

        if (!string.IsNullOrWhiteSpace(AriaLabel))
        {
            attributes["aria-label"] = AriaLabel!;
        }

        if (AdditionalAttributes is not null)
        {
            foreach (var (key, value) in AdditionalAttributes)
            {
                attributes[key] = value;
            }
        }

        return attributes.Count == 0 ? null : attributes;
    }

    private static string NormalizeLivePoliteness(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DefaultLivePoliteness;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "off" => "off",
            "assertive" => "assertive",
            _ => DefaultLivePoliteness
        };
    }

    private bool HasVisibleTextContent()
    {
        if (ChildContent is not null)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(Text);
    }
}
