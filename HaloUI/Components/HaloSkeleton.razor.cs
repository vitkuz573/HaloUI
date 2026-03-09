// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Enums;
using HaloUI.Theme.Tokens.Component;

namespace HaloUI.Components;

public partial class HaloSkeleton
{
    [Parameter]
    public SkeletonShape Shape { get; set; } = SkeletonShape.Rounded;
   
    [Parameter]
    public bool Pulse { get; set; } = true;
    
    [Parameter]
    public string? Class { get; set; }
    
    [Parameter]
    public string? Width { get; set; }
    
    [Parameter]
    public string? Height { get; set; }
    
    [Parameter]
    public string? MinWidth { get; set; }
    
    [Parameter]
    public string? MinHeight { get; set; }
    
    [Parameter]
    public string? MaxWidth { get; set; }
    
    [Parameter]
    public string? MaxHeight { get; set; }
    
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    private SkeletonDesignTokens Tokens => ThemeContext?.Theme.Tokens.Component.Get<SkeletonDesignTokens>() ?? new SkeletonDesignTokens();

    private string BuildStyle()
    {
        var styles = new List<string>
        {
            $"background:{Tokens.Background}",
            $"border-radius:{GetBorderRadius()}",
            "display:block",
            "overflow:hidden",
            $"width:{Width ?? Tokens.DefaultWidth}",
            $"height:{Height ?? Tokens.DefaultHeight}"
        };

        if (!string.IsNullOrWhiteSpace(MinWidth))
        {
            styles.Add($"min-width:{MinWidth}");
        }

        if (!string.IsNullOrWhiteSpace(MinHeight))
        {
            styles.Add($"min-height:{MinHeight}");
        }

        if (!string.IsNullOrWhiteSpace(MaxWidth))
        {
            styles.Add($"max-width:{MaxWidth}");
        }

        if (!string.IsNullOrWhiteSpace(MaxHeight))
        {
            styles.Add($"max-height:{MaxHeight}");
        }

        if (Pulse)
        {
            styles.Add($"animation:skeleton-pulse {Tokens.AnimationDuration} {Tokens.AnimationTimingFunction} {Tokens.AnimationIterationCount}");
        }

        if (Shape == SkeletonShape.Circle)
        {
            styles.Add("aspect-ratio:1");
        }

        return string.Join(';', styles);
    }

    private string GetBorderRadius() => Shape switch
    {
        SkeletonShape.Circle => Tokens.BorderRadiusCircle,
        SkeletonShape.Sharp => Tokens.BorderRadiusSharp,
        _ => Tokens.BorderRadius
    };
}