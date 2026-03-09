// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Text;
using HaloUI.Theme.Tokens;
using HaloUI.Theme.Tokens.Responsive;

namespace HaloUI.Theme;

internal static class ResponsiveFoundationCssBuilder
{
    public static string Build(DesignTokenSystem tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        var responsive = tokens.Responsive ?? ResponsiveTokens.Default;
        var breakpoints = responsive.Breakpoints ?? Breakpoints.Default;
        var spacing = responsive.Spacing ?? ResponsiveSpacing.Default;
        var fluid = responsive.Fluid ?? FluidScale.Default;

        var builder = new StringBuilder();

        builder.AppendLine(":where([class^=\"ui-\"], [class*=\" ui-\"]) { box-sizing: border-box; min-width: 0; }");
        builder.AppendLine(":where([class^=\"ui-\"], [class*=\" ui-\"]) :where(img, svg, video, canvas) { max-inline-size: 100%; block-size: auto; }");
        builder.AppendLine(":where([class^=\"ui-\"], [class*=\" ui-\"]) :where(input, select, textarea, button) { max-inline-size: 100%; }");
        builder.AppendLine(":where(.ui-layout__header, .ui-layout__toolbar, .ui-layout__content, .ui-container__section, .ui-tabs__panels, .ui-table__mobile-section) { max-inline-size: 100%; }");
        builder.AppendLine(":where(.ui-text__content, .ui-label__text, .ui-badge__label, .ui-tree__label, .ui-tree__description, .ui-expandable-panel__title, .ui-expandable-panel__subtitle, .ui-expandable-panel__description, .ui-select__trigger-text) { overflow-wrap: anywhere; }");
        builder.AppendLine($":where(.ui-layout__content) {{ padding-inline: {fluid.FluidContainerPadding}; }}");
        builder.AppendLine($":where(.ui-table__mobile-section) {{ gap: {spacing.Section.GetValue("xs")}; }}");

        AppendMedia(builder, breakpoints.TouchDevice, """
            :where(.ui-button, .ui-select__trigger, .ui-select__native, .ui-textfield__input, .ui-textarea__input, .ui-datetime__input, .ui-radio-button, .ui-toggle, .ui-tabs__tab, .ui-expandable-panel__header-button, .ui-tree__node) {
                min-height: max(2.75rem, var(--ui-accessibility-touch-minimum-size, 44px));
                touch-action: manipulation;
            }

            :where(.ui-slider) {
                min-height: max(2.75rem, var(--ui-accessibility-touch-minimum-size, 44px));
            }
            """);

        AppendMedia(builder, breakpoints.XsOnly, """
            :where(.ui-layout__header, .ui-layout__toolbar, .ui-layout__content) {
                padding-inline: var(--ui-responsive-spacing-container-padding-xs, 1rem);
            }

            :where(.ui-layout__toolbar, .ui-snackbar__header, .ui-expandable-panel__title-row, .ui-table__toolbar) {
                flex-wrap: wrap;
            }

            :where(.ui-badge) {
                white-space: normal;
            }
            """);

        AppendMedia(builder, breakpoints.ReducedMotion, """
            :where(.ui-button, .ui-radio-button, .ui-select__dropdown, .ui-expandable-panel__content, .ui-snackbar, .ui-dialog__drawer, .ui-dialog__drawer-handle, .ui-toggle__track, .ui-toggle__thumb) {
                transition-duration: 1ms !important;
                animation-duration: 1ms !important;
                animation-iteration-count: 1 !important;
            }
            """);

        return builder.ToString();
    }

    private static void AppendMedia(StringBuilder builder, string? query, string rules)
    {
        if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(rules))
        {
            return;
        }

        builder.Append("@media ").Append(query).AppendLine(" {");
        builder.AppendLine(rules);
        builder.AppendLine("}");
    }
}
