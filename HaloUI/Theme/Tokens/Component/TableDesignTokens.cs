// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Component;

/// <summary>
/// Component-specific design tokens for tables.
/// </summary>
public sealed partial record TableDesignTokens
{
    // Container
    public string BorderRadius { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string BorderColor { get; init; } = string.Empty;
    public string Background { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
    public string BackdropBlur { get; init; } = string.Empty;

    // Header
    public TableHeaderTokens Header { get; init; } = new();
    public TableTextToken Title { get; init; } = new();
    public TableTextToken SubtleText { get; init; } = new();
    public TableTextToken TertiaryText { get; init; } = new();

    // Row
    public TableRowTokens Row { get; init; } = new();

    // Cell
    public string CellPaddingX { get; init; } = string.Empty;
    public string CellPaddingY { get; init; } = string.Empty;
    public string CellFontSize { get; init; } = string.Empty;
    public string CellLineHeight { get; init; } = string.Empty;

    // Selection
    public string SelectionCheckboxSize { get; init; } = string.Empty;
    public string SelectionBackground { get; init; } = string.Empty;
    public string SelectionBorder { get; init; } = string.Empty;

    // Hover
    public string HoverBackground { get; init; } = string.Empty;
    public string HoverTransition { get; init; } = string.Empty;

    // Sorting
    public string SortIconSize { get; init; } = string.Empty;
    public string SortIconColor { get; init; } = string.Empty;
    public string SortIconActiveColor { get; init; } = string.Empty;

    // Toolbar
    public TableToolbarTokens Toolbar { get; init; } = new();

    // Filters
    public TableFilterTokens Filter { get; init; } = new();

    // Pagination
    public TablePaginationTokens Pagination { get; init; } = new();

    // Empty state
    public string EmptyStatePaddingY { get; init; } = string.Empty;
    public string EmptyStateTextColor { get; init; } = string.Empty;
    public string EmptyStateIconSize { get; init; } = string.Empty;
    public string EmptyStateIconColor { get; init; } = string.Empty;

    // Mobile
    public TableMobileTokens Mobile { get; init; } = new();

    // Density
    public TableDensityTokens Density { get; init; } = new();
}

public sealed partial record TableHeaderTokens
{
    public string Background { get; init; } = string.Empty;
    public string TextColor { get; init; } = string.Empty;
    public string FontWeight { get; init; } = string.Empty;
    public string FontSize { get; init; } = string.Empty;
    public string BorderBottom { get; init; } = string.Empty;
    public string TextTransform { get; init; } = string.Empty;
    public string LetterSpacing { get; init; } = string.Empty;
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
}

public sealed partial record TableRowTokens
{
    public string Background { get; init; } = string.Empty;
    public string BackgroundAlt { get; init; } = string.Empty;
    public string TextColor { get; init; } = string.Empty;
    public string BorderBottom { get; init; } = string.Empty;
}

public sealed partial record TablePaginationTokens
{
    public string Background { get; init; } = string.Empty;
    public string BorderTop { get; init; } = string.Empty;
    public string TextColor { get; init; } = string.Empty;
    public string TextSecondary { get; init; } = string.Empty;
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public TablePaginationControlTokens Control { get; init; } = new();
    public TablePaginationSelectTokens Select { get; init; } = new();
}

public sealed partial record TablePaginationControlTokens
{
    public string Gap { get; init; } = string.Empty;
}

public sealed partial record TablePaginationSelectTokens
{
    public string Background { get; init; } = string.Empty;
    public string Border { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string BorderRadius { get; init; } = string.Empty;
}

public sealed partial record TableToolbarTokens
{
    public string Gap { get; init; } = string.Empty;
    public TableToolbarIconTokens Icon { get; init; } = new();
}

public sealed partial record TableToolbarIconTokens
{
    public string Color { get; init; } = string.Empty;
}

public sealed partial record TableFilterTokens
{
    public TableFilterPanelTokens Panel { get; init; } = new();
    public TableFilterButtonTokens Button { get; init; } = new();
    public TableFilterChipTokens Chip { get; init; } = new();
    public TableFilterBadgeTokens Badge { get; init; } = new();
    public TableFilterInputTokens Input { get; init; } = new();
}

public sealed partial record TableFilterPanelTokens
{
    public string Background { get; init; } = string.Empty;
    public string Border { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public TableFilterPanelIconTokens Icon { get; init; } = new();
}

public sealed partial record TableFilterPanelIconTokens
{
    public string Color { get; init; } = string.Empty;
}

public sealed partial record TableFilterButtonTokens
{
    public string Text { get; init; } = string.Empty;
    public string Border { get; init; } = string.Empty;
    public string Background { get; init; } = string.Empty;
    public string Hover { get; init; } = string.Empty;
    public string Focus { get; init; } = string.Empty;
}

public sealed partial record TableFilterChipTokens
{
    public string Background { get; init; } = string.Empty;
    public string Border { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
}

public sealed partial record TableFilterBadgeTokens
{
    public string Background { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}

public sealed partial record TableFilterInputTokens
{
    public string Background { get; init; } = string.Empty;
    public string Border { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string Placeholder { get; init; } = string.Empty;
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string BorderRadius { get; init; } = string.Empty;
    public TableFilterInputIconTokens Icon { get; init; } = new();
}

public sealed partial record TableFilterInputIconTokens
{
    public string Color { get; init; } = string.Empty;
}

public sealed partial record TableMobileTokens
{
    public TableMobileCardTokens Card { get; init; } = new();
    public string Divider { get; init; } = string.Empty;
    public TableMobileLabelTokens Label { get; init; } = new();
    public TableMobileValueTokens Value { get; init; } = new();
    public TableMobileEmptyStateTokens EmptyState { get; init; } = new();
}

public sealed partial record TableMobileCardTokens
{
    public string Background { get; init; } = string.Empty;
    public string Border { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
    public string SelectedBorder { get; init; } = string.Empty;
    public string SelectedShadow { get; init; } = string.Empty;
    public string Padding { get; init; } = string.Empty;
    public string BorderRadius { get; init; } = string.Empty;
}

public sealed partial record TableMobileLabelTokens
{
    public string Color { get; init; } = string.Empty;
}

public sealed partial record TableMobileValueTokens
{
    public string Color { get; init; } = string.Empty;
}

public sealed partial record TableMobileEmptyStateTokens
{
    public string TextColor { get; init; } = string.Empty;
}

public sealed partial record TableDensityTokens
{
    public string Comfortable { get; init; } = string.Empty;
    public string Dense { get; init; } = string.Empty;
}

public sealed partial record TableTextToken
{
    public string Color { get; init; } = string.Empty;
}