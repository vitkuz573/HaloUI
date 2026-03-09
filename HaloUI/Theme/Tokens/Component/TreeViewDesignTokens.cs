// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Component;

public sealed partial record TreeViewDesignTokens
{
    public string IndentStep { get; init; } = string.Empty;

    public TreeViewContainerTokens Container { get; init; } = new();

    public TreeViewListTokens List { get; init; } = new();

    public TreeViewToggleTokens Toggle { get; init; } = new();

    public TreeViewNodeTokens Node { get; init; } = new();

    public TreeViewNodeStateTokens NodeSelected { get; init; } = new();

    public TreeViewNodeStateTokens NodeDisabled { get; init; } = new();

    public TreeViewLabelTokens Label { get; init; } = new();

    public TreeViewLabelTokens LabelSelected { get; init; } = new();

    public TreeViewLabelTokens LabelDisabled { get; init; } = new();

    public TreeViewDescriptionTokens Description { get; init; } = new();

    public TreeViewBadgeTokens Badge { get; init; } = new();

    public TreeViewBadgeTokens BadgeSelected { get; init; } = new();

    public TreeViewIconTokens Icon { get; init; } = new();

    public TreeViewIconTokens IconSelected { get; init; } = new();

    public TreeViewIconTokens IconDisabled { get; init; } = new();

    public TreeViewGroupTokens Group { get; init; } = new();
}

public sealed record TreeViewContainerTokens
{
    public string Background { get; init; } = string.Empty;
    public string BorderColor { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string BorderRadius { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;
    public string Padding { get; init; } = string.Empty;
}

public sealed record TreeViewListTokens
{
    public string Gap { get; init; } = string.Empty;
}

public sealed record TreeViewToggleTokens
{
    public string Size { get; init; } = string.Empty;
    public string BorderRadius { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string BorderColor { get; init; } = string.Empty;
    public string Background { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public string HoverBackground { get; init; } = string.Empty;
    public string HoverBorderColor { get; init; } = string.Empty;
    public string HoverColor { get; init; } = string.Empty;
    public string Transition { get; init; } = string.Empty;
    public string DisabledOpacity { get; init; } = string.Empty;
}

public sealed record TreeViewNodeTokens
{
    public string MinHeight { get; init; } = string.Empty;
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;
    public string BorderRadius { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string BorderColor { get; init; } = string.Empty;
    public string Background { get; init; } = string.Empty;
    public string TextColor { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
    public string Transition { get; init; } = string.Empty;
}

public sealed record TreeViewNodeStateTokens
{
    public string Background { get; init; } = string.Empty;
    public string TextColor { get; init; } = string.Empty;
    public string BorderColor { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
    public string Opacity { get; init; } = string.Empty;
}

public sealed record TreeViewLabelTokens
{
    public string Color { get; init; } = string.Empty;
    public string FontWeight { get; init; } = string.Empty;
}

public sealed record TreeViewDescriptionTokens
{
    public string Color { get; init; } = string.Empty;
    public string FontSize { get; init; } = string.Empty;
}

public sealed record TreeViewBadgeTokens
{
    public string Background { get; init; } = string.Empty;
    public string TextColor { get; init; } = string.Empty;
}

public sealed record TreeViewIconTokens
{
    public string Background { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
}

public sealed record TreeViewGroupTokens
{
    public string BorderColor { get; init; } = string.Empty;
    public string PaddingLeft { get; init; } = string.Empty;
    public string MarginTop { get; init; } = string.Empty;
}