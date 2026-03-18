namespace HaloUI.Accessibility.Aria;

/// <summary>
/// Provides strongly typed descriptors for all WAI-ARIA attributes.
/// </summary>
public static class AriaAttributes
{
    // Global states and properties
    public static readonly AriaBooleanAttribute Atomic = new("aria-atomic", RenderOnFalse: true);
    public static readonly AriaBooleanAttribute Busy = new("aria-busy");
    public static readonly AriaIdReferenceAttribute Controls = new("aria-controls", AllowMultiple: true);
    public static readonly AriaEnumAttribute<AriaCurrentState> Current = new("aria-current");
    public static readonly AriaIdReferenceAttribute DescribedBy = new("aria-describedby", AllowMultiple: true);
    public static readonly AriaStringAttribute Description = new("aria-description");
    public static readonly AriaIdReferenceAttribute Details = new("aria-details");
    public static readonly AriaBooleanAttribute Disabled = new("aria-disabled");
    public static readonly AriaTokenEnumAttribute<AriaDropEffect> DropEffect = new("aria-dropeffect");
    public static readonly AriaIdReferenceAttribute ErrorMessage = new("aria-errormessage");
    public static readonly AriaEnumAttribute<AriaExpandedState> Expanded = new("aria-expanded");
    public static readonly AriaIdReferenceAttribute FlowTo = new("aria-flowto", AllowMultiple: true);
    public static readonly AriaBooleanAttribute Grabbed = new("aria-grabbed", RenderOnFalse: true);
    public static readonly AriaEnumAttribute<AriaHasPopup> HasPopup = new("aria-haspopup");
    public static readonly AriaBooleanAttribute Hidden = new("aria-hidden", RenderOnFalse: true);
    public static readonly AriaEnumAttribute<AriaInvalidState> Invalid = new("aria-invalid");
    public static readonly AriaStringAttribute KeyShortcuts = new("aria-keyshortcuts");
    public static readonly AriaStringAttribute Label = new("aria-label");
    public static readonly AriaIdReferenceAttribute LabelledBy = new("aria-labelledby", AllowMultiple: true);
    public static readonly AriaEnumAttribute<AriaLivePoliteness> Live = new("aria-live");
    public static readonly AriaIdReferenceAttribute Owns = new("aria-owns", AllowMultiple: true);
    public static readonly AriaTokenEnumAttribute<AriaRelevantToken> Relevant = new("aria-relevant");
    public static readonly AriaStringAttribute RoleDescription = new("aria-roledescription");
    public static readonly AriaStringAttribute BrailleLabel = new("aria-braillelabel");
    public static readonly AriaStringAttribute BrailleRoleDescription = new("aria-brailleroledescription");

    // Widget attributes
    public static readonly AriaIdReferenceAttribute ActiveDescendant = new("aria-activedescendant");
    public static readonly AriaEnumAttribute<AriaAutocomplete> Autocomplete = new("aria-autocomplete");
    public static readonly AriaEnumAttribute<AriaCheckedState> Checked = new("aria-checked");
    public static readonly AriaIntegerAttribute ColCount = new("aria-colcount");
    public static readonly AriaIntegerAttribute ColIndex = new("aria-colindex");
    public static readonly AriaStringAttribute ColIndexText = new("aria-colindextext");
    public static readonly AriaIntegerAttribute ColSpan = new("aria-colspan");
    public static readonly AriaIntegerAttribute Level = new("aria-level");
    public static readonly AriaBooleanAttribute Modal = new("aria-modal");
    public static readonly AriaBooleanAttribute Multiline = new("aria-multiline");
    public static readonly AriaBooleanAttribute Multiselectable = new("aria-multiselectable");
    public static readonly AriaEnumAttribute<AriaOrientation> Orientation = new("aria-orientation");
    public static readonly AriaStringAttribute Placeholder = new("aria-placeholder");
    public static readonly AriaIntegerAttribute PosInSet = new("aria-posinset");
    public static readonly AriaEnumAttribute<AriaPressedState> Pressed = new("aria-pressed");
    public static readonly AriaBooleanAttribute ReadOnly = new("aria-readonly");
    public static readonly AriaBooleanAttribute Required = new("aria-required");
    public static readonly AriaIntegerAttribute RowCount = new("aria-rowcount");
    public static readonly AriaIntegerAttribute RowIndex = new("aria-rowindex");
    public static readonly AriaStringAttribute RowIndexText = new("aria-rowindextext");
    public static readonly AriaIntegerAttribute RowSpan = new("aria-rowspan");
    public static readonly AriaBooleanAttribute Selected = new("aria-selected");
    public static readonly AriaIntegerAttribute SetSize = new("aria-setsize");
    public static readonly AriaEnumAttribute<AriaSortDirection> Sort = new("aria-sort");
    public static readonly AriaNumberAttribute ValueMax = new("aria-valuemax");
    public static readonly AriaNumberAttribute ValueMin = new("aria-valuemin");
    public static readonly AriaNumberAttribute ValueNow = new("aria-valuenow");
    public static readonly AriaStringAttribute ValueText = new("aria-valuetext");
}