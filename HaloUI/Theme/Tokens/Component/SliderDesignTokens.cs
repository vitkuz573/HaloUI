// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Component;

public sealed partial record SliderDesignTokens
{
    public string TrackHeight { get; init; } = string.Empty;
    public string TrackBorderRadius { get; init; } = string.Empty;
    public string TrackBackground { get; init; } = string.Empty;
    public string TrackFillColor { get; init; } = string.Empty;
    
    public string ThumbSize { get; init; } = string.Empty;
    public string ThumbBackground { get; init; } = string.Empty;
    public string ThumbBorder { get; init; } = string.Empty;
    public string ThumbShadow { get; init; } = string.Empty;
    public string ThumbTransition { get; init; } = string.Empty;
    public string ThumbActiveScale { get; init; } = string.Empty;
    
    public string FocusRing { get; init; } = string.Empty;
}