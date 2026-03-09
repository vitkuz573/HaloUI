// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Accessibility.Aria;

/// <summary>
/// Central registry containing ARIA role definitions used for validation and IntelliSense-style guidance.
/// </summary>
public static class AriaRoleRegistry
{
    private static readonly Dictionary<AriaRole, AriaRoleDefinition> Definitions = new()
    {
        [AriaRole.Button] = new(
            AriaRole.Button,
            allowed: AttributeSet(AriaAttributes.Disabled, AriaAttributes.Expanded, AriaAttributes.HasPopup, AriaAttributes.Pressed),
            description: "Represents a form control that triggers an action without maintaining a state.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.Switch] = new(
            AriaRole.Switch,
            required: AttributeSet(AriaAttributes.Checked),
            allowed: AttributeSet(AriaAttributes.Disabled, AriaAttributes.Required, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy, AriaAttributes.Label, AriaAttributes.Busy, AriaAttributes.ReadOnly),
            description: "A type of checkbox that represents an on/off control.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.Slider] = new(
            AriaRole.Slider,
            required: AttributeSet(AriaAttributes.ValueNow),
            allowed: AttributeSet(AriaAttributes.ValueMin, AriaAttributes.ValueMax, AriaAttributes.ValueText, AriaAttributes.Orientation, AriaAttributes.Disabled, AriaAttributes.ReadOnly, AriaAttributes.Required, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy, AriaAttributes.Label),
            recommended: AttributeSet(AriaAttributes.ValueMin, AriaAttributes.ValueMax, AriaAttributes.Orientation),
            description: "Allows the user to select a value from a given range.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.Checkbox] = new(
            AriaRole.Checkbox,
            required: AttributeSet(AriaAttributes.Checked),
            allowed: AttributeSet(AriaAttributes.Required, AriaAttributes.ReadOnly, AriaAttributes.Disabled, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "Represents a checkable input that can be checked, unchecked, or mixed.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.Radio] = new(
            AriaRole.Radio,
            required: AttributeSet(AriaAttributes.Checked),
            allowed: AttributeSet(AriaAttributes.Disabled, AriaAttributes.Required, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy, AriaAttributes.PosInSet, AriaAttributes.SetSize),
            description: "A selectable item within a group where only one item can be selected at a time.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.RadioGroup] = new(
            AriaRole.RadioGroup,
            allowed: AttributeSet(AriaAttributes.Required, AriaAttributes.Disabled, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "Groups related radio buttons.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.Dialog] = new(
            AriaRole.Dialog,
            allowed: AttributeSet(AriaAttributes.Modal, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "A window separate from the main content that requires user interaction.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.AlertDialog] = new(
            AriaRole.AlertDialog,
            allowed: AttributeSet(AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "A dialog that is designed to alert the user to something important.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.Tab] = new(
            AriaRole.Tab,
            allowed: AttributeSet(AriaAttributes.Selected, AriaAttributes.Controls, AriaAttributes.PosInSet, AriaAttributes.SetSize),
            description: "An item in a tab set that selects a tabpanel.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.TabPanel] = new(
            AriaRole.TabPanel,
            allowed: AttributeSet(AriaAttributes.LabelledBy, AriaAttributes.DescribedBy),
            description: "The container for the resources associated with a tab."),

        [AriaRole.ToolTip] = new(
            AriaRole.ToolTip,
            allowed: AttributeSet(AriaAttributes.DescribedBy, AriaAttributes.Label),
            description: "Text that appears in relation to another element when that element receives focus or hover.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.Menu] = new(
            AriaRole.Menu,
            allowed: AttributeSet(AriaAttributes.ActiveDescendant, AriaAttributes.Orientation, AriaAttributes.LabelledBy, AriaAttributes.DescribedBy),
            description: "A widget offering a list of choices to the user."),

        [AriaRole.MenuItem] = new(
            AriaRole.MenuItem,
            allowed: AttributeSet(AriaAttributes.Checked, AriaAttributes.HasPopup, AriaAttributes.PosInSet, AriaAttributes.SetSize, AriaAttributes.Disabled, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "An option within a menu that performs an action.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.SpinButton] = new(
            AriaRole.SpinButton,
            required: AttributeSet(AriaAttributes.ValueNow),
            allowed: AttributeSet(AriaAttributes.ValueMin, AriaAttributes.ValueMax, AriaAttributes.ValueText, AriaAttributes.Required, AriaAttributes.ReadOnly, AriaAttributes.Disabled, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "Allows the user to increment or decrement a value.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.TextBox] = new(
            AriaRole.TextBox,
            allowed: AttributeSet(AriaAttributes.ActiveDescendant, AriaAttributes.Autocomplete, AriaAttributes.Multiline, AriaAttributes.Placeholder, AriaAttributes.ReadOnly, AriaAttributes.Required, AriaAttributes.Invalid, AriaAttributes.Disabled, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "Allows free-form input of text content.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.Link] = new(
            AriaRole.Link,
            allowed: AttributeSet(AriaAttributes.Expanded, AriaAttributes.HasPopup, AriaAttributes.Disabled, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "Navigates the user to another resource.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.ProgressBar] = new(
            AriaRole.ProgressBar,
            required: AttributeSet(AriaAttributes.ValueNow),
            allowed: AttributeSet(AriaAttributes.ValueMin, AriaAttributes.ValueMax, AriaAttributes.ValueText, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "Displays the progress status for a task.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.Application] = new(
            AriaRole.Application,
            allowed: AttributeSet(AriaAttributes.Label, AriaAttributes.LabelledBy, AriaAttributes.DescribedBy, AriaAttributes.RoleDescription),
            description: "Top-level region that contains an interactive web application.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.ColumnHeader] = new(
            AriaRole.ColumnHeader,
            allowed: AttributeSet(AriaAttributes.Sort, AriaAttributes.ColIndex, AriaAttributes.ColIndexText, AriaAttributes.ColSpan, AriaAttributes.RowSpan, AriaAttributes.RowIndex, AriaAttributes.RowIndexText, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "Header cell for a column of tabular data.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.Combobox] = new(
            AriaRole.Combobox,
            allowed: AttributeSet(AriaAttributes.ActiveDescendant, AriaAttributes.Autocomplete, AriaAttributes.Expanded, AriaAttributes.HasPopup, AriaAttributes.Required, AriaAttributes.ReadOnly, AriaAttributes.Disabled, AriaAttributes.Invalid, AriaAttributes.Label, AriaAttributes.LabelledBy, AriaAttributes.DescribedBy, AriaAttributes.Controls),
            description: "Input that controls another element that can pop up to assist in selection of values.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.Grid] = new(
            AriaRole.Grid,
            allowed: AttributeSet(AriaAttributes.ActiveDescendant, AriaAttributes.ColCount, AriaAttributes.RowCount, AriaAttributes.Multiselectable, AriaAttributes.ReadOnly, AriaAttributes.Level, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "A widget with cells organized into rows and columns, like a spreadsheet.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.Heading] = new(
            AriaRole.Heading,
            allowed: AttributeSet(AriaAttributes.Level, AriaAttributes.Label, AriaAttributes.LabelledBy, AriaAttributes.DescribedBy),
            description: "Heading for a section of content.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.Img] = new(
            AriaRole.Img,
            allowed: AttributeSet(AriaAttributes.Label, AriaAttributes.LabelledBy, AriaAttributes.DescribedBy),
            description: "A graphic or symbolic image that conveys meaning.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.ListBox] = new(
            AriaRole.ListBox,
            allowed: AttributeSet(AriaAttributes.ActiveDescendant, AriaAttributes.Expanded, AriaAttributes.Multiselectable, AriaAttributes.Required, AriaAttributes.Orientation, AriaAttributes.Disabled, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "Widget that allows the user to select one or more static options.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.MenuItemCheckbox] = new(
            AriaRole.MenuItemCheckbox,
            allowed: AttributeSet(AriaAttributes.Checked, AriaAttributes.HasPopup, AriaAttributes.Disabled, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy, AriaAttributes.PosInSet, AriaAttributes.SetSize),
            description: "Menu item with a checkable state.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.MenuItemRadio] = new(
            AriaRole.MenuItemRadio,
            allowed: AttributeSet(AriaAttributes.Checked, AriaAttributes.HasPopup, AriaAttributes.Disabled, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy, AriaAttributes.PosInSet, AriaAttributes.SetSize),
            description: "Menu item with a mutually exclusive selectable state.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.Option] = new(
            AriaRole.Option,
            allowed: AttributeSet(AriaAttributes.Selected, AriaAttributes.Checked, AriaAttributes.Disabled, AriaAttributes.PosInSet, AriaAttributes.SetSize, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "Selectable item within a listbox or combobox.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.RowHeader] = new(
            AriaRole.RowHeader,
            allowed: AttributeSet(AriaAttributes.Sort, AriaAttributes.RowIndex, AriaAttributes.RowIndexText, AriaAttributes.RowSpan, AriaAttributes.ColIndex, AriaAttributes.ColIndexText, AriaAttributes.ColSpan, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "Header cell for a row of tabular data.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.Table] = new(
            AriaRole.Table,
            allowed: AttributeSet(AriaAttributes.RowCount, AriaAttributes.ColCount, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "A section of rows and columns forming a tabular structure.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.Tree] = new(
            AriaRole.Tree,
            allowed: AttributeSet(AriaAttributes.ActiveDescendant, AriaAttributes.Multiselectable, AriaAttributes.Required, AriaAttributes.Expanded, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "Hierarchical list with parent and child nodes.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.TreeGrid] = new(
            AriaRole.TreeGrid,
            allowed: AttributeSet(AriaAttributes.ActiveDescendant, AriaAttributes.Level, AriaAttributes.Multiselectable, AriaAttributes.ReadOnly, AriaAttributes.RowCount, AriaAttributes.ColCount, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "Grid whose rows can be expanded and collapsed in a hierarchical fashion.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.TreeItem] = new(
            AriaRole.TreeItem,
            allowed: AttributeSet(AriaAttributes.Checked, AriaAttributes.Disabled, AriaAttributes.Expanded, AriaAttributes.Selected, AriaAttributes.PosInSet, AriaAttributes.SetSize, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "An item in a tree structure that may have child nodes.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required,
            supportsNameFromContent: true),

        [AriaRole.SearchBox] = new(
            AriaRole.SearchBox,
            allowed: AttributeSet(AriaAttributes.ActiveDescendant, AriaAttributes.Autocomplete, AriaAttributes.Required, AriaAttributes.ReadOnly, AriaAttributes.Invalid, AriaAttributes.Disabled, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "Text box intended specifically for search.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.Marquee] = new(
            AriaRole.Marquee,
            allowed: AttributeSet(AriaAttributes.Live, AriaAttributes.Atomic, AriaAttributes.Relevant, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "A type of live region where new information is continuously added.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.Meter] = new(
            AriaRole.Meter,
            required: AttributeSet(AriaAttributes.ValueNow),
            allowed: AttributeSet(AriaAttributes.ValueMin, AriaAttributes.ValueMax, AriaAttributes.ValueText, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "Represents a scalar measurement within a known range, such as temperature.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Required),

        [AriaRole.Generic] = new(
            AriaRole.Generic,
            description: "An element without an explicit semantic meaning.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Prohibited),

        [AriaRole.Presentation] = new(
            AriaRole.Presentation,
            description: "Indicates that the element should be removed from the accessibility tree.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Prohibited),

        [AriaRole.None] = new(
            AriaRole.None,
            description: "Synonym for presentation that removes semantics from the element.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Prohibited),

        [AriaRole.Caption] = new(
            AriaRole.Caption,
            description: "Caption for a figure or other component.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Prohibited),

        [AriaRole.Code] = new(
            AriaRole.Code,
            description: "Content that represents computer code.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Prohibited),

        [AriaRole.Deletion] = new(
            AriaRole.Deletion,
            description: "Content representing deleted text.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Prohibited),

        [AriaRole.Emphasis] = new(
            AriaRole.Emphasis,
            description: "Content with stress emphasis.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Prohibited),

        [AriaRole.Insertion] = new(
            AriaRole.Insertion,
            description: "Content representing inserted text.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Prohibited),

        [AriaRole.Paragraph] = new(
            AriaRole.Paragraph,
            description: "Paragraph of text.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Prohibited),

        [AriaRole.Strong] = new(
            AriaRole.Strong,
            description: "Content of strong importance.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Prohibited),

        [AriaRole.Subscript] = new(
            AriaRole.Subscript,
            description: "Content presented as subscript.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Prohibited),

        [AriaRole.Superscript] = new(
            AriaRole.Superscript,
            description: "Content presented as superscript.",
            accessibleNameRequirement: AriaAccessibleNameRequirement.Prohibited),

        [AriaRole.Status] = new(
            AriaRole.Status,
            allowed: AttributeSet(AriaAttributes.Live, AriaAttributes.Atomic, AriaAttributes.Relevant, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "A live region containing advisory information."),

        [AriaRole.Alert] = new(
            AriaRole.Alert,
            allowed: AttributeSet(AriaAttributes.Live, AriaAttributes.Atomic, AriaAttributes.Relevant, AriaAttributes.DescribedBy, AriaAttributes.LabelledBy),
            description: "A type of status, often critical, that requires immediate attention."),
    };

    public static bool TryGetDefinition(AriaRole role, out AriaRoleDefinition definition)
    {
        return Definitions.TryGetValue(role, out definition!);
    }

    public static AriaRoleDefinition GetDefinition(AriaRole role)
    {
        if (!Definitions.TryGetValue(role, out var definition))
        {
            throw new KeyNotFoundException($"ARIA role definition for '{role}' is not registered.");
        }

        return definition;
    }

    private static AriaAttributeDefinition[] AttributeSet(params AriaAttributeDefinition[] attributes) => attributes;
}
