using System.Runtime.Serialization;

namespace HaloUI.Accessibility.Aria;

public enum AriaAutocomplete
{
    [EnumMember(Value = "none")]
    None,

    [EnumMember(Value = "inline")]
    Inline,

    [EnumMember(Value = "list")]
    List,

    [EnumMember(Value = "both")]
    Both
}

public enum AriaCheckedState
{
    [EnumMember(Value = "false")]
    False,

    [EnumMember(Value = "mixed")]
    Mixed,

    [EnumMember(Value = "true")]
    True,

    [EnumMember(Value = "undefined")]
    Undefined
}

public enum AriaCurrentState
{
    [EnumMember(Value = "false")]
    False,

    [EnumMember(Value = "true")]
    True,

    [EnumMember(Value = "page")]
    Page,

    [EnumMember(Value = "step")]
    Step,

    [EnumMember(Value = "location")]
    Location,

    [EnumMember(Value = "date")]
    Date,

    [EnumMember(Value = "time")]
    Time
}

public enum AriaDropEffect
{
    [EnumMember(Value = "none")]
    None,

    [EnumMember(Value = "copy")]
    Copy,

    [EnumMember(Value = "execute")]
    Execute,

    [EnumMember(Value = "link")]
    Link,

    [EnumMember(Value = "move")]
    Move,

    [EnumMember(Value = "popup")]
    Popup
}

public enum AriaExpandedState
{
    [EnumMember(Value = "false")]
    False,

    [EnumMember(Value = "true")]
    True,

    [EnumMember(Value = "undefined")]
    Undefined
}

public enum AriaHasPopup
{
    [EnumMember(Value = "false")]
    False,

    [EnumMember(Value = "true")]
    True,

    [EnumMember(Value = "menu")]
    Menu,

    [EnumMember(Value = "listbox")]
    ListBox,

    [EnumMember(Value = "tree")]
    Tree,

    [EnumMember(Value = "grid")]
    Grid,

    [EnumMember(Value = "dialog")]
    Dialog
}

public enum AriaInvalidState
{
    [EnumMember(Value = "false")]
    False,

    [EnumMember(Value = "grammar")]
    Grammar,

    [EnumMember(Value = "spelling")]
    Spelling,

    [EnumMember(Value = "true")]
    True
}

public enum AriaLivePoliteness
{
    [EnumMember(Value = "off")]
    Off,

    [EnumMember(Value = "polite")]
    Polite,

    [EnumMember(Value = "assertive")]
    Assertive
}

public enum AriaOrientation
{
    [EnumMember(Value = "horizontal")]
    Horizontal,

    [EnumMember(Value = "vertical")]
    Vertical
}

public enum AriaPressedState
{
    [EnumMember(Value = "false")]
    False,

    [EnumMember(Value = "mixed")]
    Mixed,

    [EnumMember(Value = "true")]
    True,

    [EnumMember(Value = "undefined")]
    Undefined
}

public enum AriaRelevantToken
{
    [EnumMember(Value = "additions")]
    Additions,

    [EnumMember(Value = "removals")]
    Removals,

    [EnumMember(Value = "text")]
    Text,

    [EnumMember(Value = "all")]
    All
}

public enum AriaSortDirection
{
    [EnumMember(Value = "none")]
    None,

    [EnumMember(Value = "ascending")]
    Ascending,

    [EnumMember(Value = "descending")]
    Descending,

    [EnumMember(Value = "other")]
    Other
}