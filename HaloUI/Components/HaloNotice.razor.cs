// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Enums;

namespace HaloUI.Components;

public partial class HaloNotice
{
    private const string DefaultLivePoliteness = "polite";

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string Icon { get; set; } = "info";

    [Parameter]
    public NoticeVariant Variant { get; set; } = NoticeVariant.Info;

    [Parameter]
    public bool AnnounceChanges { get; set; }

    [Parameter]
    public string AriaLive { get; set; } = DefaultLivePoliteness;

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? AriaRole { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string BuildCssClass()
    {
        var classes = new List<string>
        {
            "ui-notice",
            GetVariantClass()
        };

        return string.Join(' ', classes);
    }

    private string GetVariantClass() => Variant switch
    {
        NoticeVariant.Warning => "ui-notice--warning",
        NoticeVariant.Danger => "ui-notice--danger",
        NoticeVariant.Success => "ui-notice--success",
        _ => "ui-notice--info"
    };

    private Dictionary<string, object>? BuildNoticeAttributes()
    {
        var attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var role = ResolveRole();

        if (!string.IsNullOrWhiteSpace(role))
        {
            attributes["role"] = role;

            if (AnnounceChanges)
            {
                attributes["aria-live"] = role == "alert"
                    ? "assertive"
                    : NormalizeLivePoliteness(AriaLive);
            }
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

    private string? ResolveRole()
    {
        var explicitRole = NormalizeRole(AriaRole);

        if (!string.IsNullOrWhiteSpace(explicitRole))
        {
            return explicitRole;
        }

        if (!AnnounceChanges)
        {
            return null;
        }

        return Variant == NoticeVariant.Danger ? "alert" : "status";
    }

    private static string? NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        return role.Trim().ToLowerInvariant() switch
        {
            "status" => "status",
            "alert" => "alert",
            "log" => "log",
            _ => null
        };
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
}
