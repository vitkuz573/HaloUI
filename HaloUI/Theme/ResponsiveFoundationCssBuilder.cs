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

        builder.AppendLine(":where([class^=\"halo-\"], [class*=\" halo-\"]) { box-sizing: border-box; min-width: 0; }");
        builder.AppendLine(":where([class^=\"halo-\"], [class*=\" halo-\"]) :where(img, svg, video, canvas) { max-inline-size: 100%; block-size: auto; }");
        builder.AppendLine(":where([class^=\"halo-\"], [class*=\" halo-\"]) :where(input, select, textarea, button) { max-inline-size: 100%; }");
        builder.AppendLine(":where(.halo-layout__header, .halo-layout__toolbar, .halo-layout__content, .halo-container__section, .halo-tabs__panels, .halo-table__mobile-section) { max-inline-size: 100%; }");
        builder.AppendLine(":where(.halo-text__content, .halo-label__text, .halo-badge__label, .halo-tree__label, .halo-tree__description, .halo-expandable-panel__title, .halo-expandable-panel__subtitle, .halo-expandable-panel__description, .halo-select__trigger-text) { overflow-wrap: anywhere; }");
        builder.AppendLine(":where(.halo-flow-y-xs, .halo-flow-y-sm, .halo-flow-y-md, .halo-flow-y-lg, .halo-flow-y-xl, .halo-flow-y-2xl) { display: flex; flex-direction: column; min-width: 0; }");
        builder.AppendLine(":where(.halo-flow-y-xs) { gap: var(--halo-spacing-gap-xs, 0.25rem); }");
        builder.AppendLine(":where(.halo-flow-y-sm) { gap: var(--halo-spacing-gap-sm, 0.5rem); }");
        builder.AppendLine(":where(.halo-flow-y-md) { gap: var(--halo-spacing-gap-md, 0.75rem); }");
        builder.AppendLine(":where(.halo-flow-y-lg) { gap: var(--halo-spacing-gap-lg, 1rem); }");
        builder.AppendLine(":where(.halo-flow-y-xl) { gap: var(--halo-spacing-gap-xl, 1.5rem); }");
        builder.AppendLine(":where(.halo-flow-y-2xl) { gap: var(--halo-spacing-gap-xl2, 2rem); }");
        builder.AppendLine($":where(.halo-layout__content) {{ padding-inline: {fluid.FluidContainerPadding}; }}");
        builder.AppendLine($":where(.halo-table__mobile-section) {{ gap: {spacing.Section.GetValue("xs")}; }}");

        AppendMedia(builder, breakpoints.TouchDevice, """
            :where(.halo-button, .halo-select__trigger, .halo-select__native, .halo-textfield__input, .halo-textarea__input, .halo-datetime__input, .halo-radio-button, .halo-toggle, .halo-tabs__tab, .halo-expandable-panel__header-button, .halo-tree__node) {
                min-height: max(2.75rem, var(--halo-accessibility-touch-minimum-size, 44px));
                touch-action: manipulation;
            }

            :where(.halo-slider) {
                min-height: max(2.75rem, var(--halo-accessibility-touch-minimum-size, 44px));
            }
            """);

        AppendMedia(builder, breakpoints.XsOnly, """
            :where(.halo-layout__header, .halo-layout__toolbar, .halo-layout__content) {
                padding-inline: var(--halo-responsive-spacing-container-padding-xs, 1rem);
            }

            :where(.halo-layout__toolbar, .halo-snackbar__header, .halo-expandable-panel__title-row, .halo-table__toolbar) {
                flex-wrap: wrap;
            }

            :where(.halo-badge) {
                white-space: normal;
            }
            """);

        AppendMedia(builder, breakpoints.ReducedMotion, """
            :where(.halo-button, .halo-radio-button, .halo-select__dropdown, .halo-expandable-panel__content, .halo-snackbar, .halo-dialog__drawer, .halo-dialog__drawer-handle, .halo-toggle__track, .halo-toggle__thumb) {
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
