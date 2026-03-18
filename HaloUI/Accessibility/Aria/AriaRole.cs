using System.Runtime.CompilerServices;

namespace HaloUI.Accessibility.Aria;

/// <summary>
/// Represents the set of valid values for the <c>role</c> attribute defined by the WAI-ARIA 1.2 specification.
/// </summary>
public enum AriaRole
{
    Alert,
    AlertDialog,
    Application,
    Article,
    Banner,
    BlockQuote,
    Button,
    Caption,
    Cell,
    Checkbox,
    Code,
    ColumnHeader,
    Combobox,
    Command,
    Composite,
    Complementary,
    ContentInfo,
    Definition,
    Deletion,
    Dialog,
    Directory,
    Document,
    Emphasis,
    Feed,
    Figure,
    Form,
    Generic,
    Grid,
    GridCell,
    Group,
    Heading,
    Img,
    Input,
    Insertion,
    Landmark,
    Link,
    List,
    ListBox,
    ListItem,
    Log,
    Main,
    Marquee,
    Math,
    Menu,
    MenuBar,
    MenuItem,
    MenuItemCheckbox,
    MenuItemRadio,
    Meter,
    Navigation,
    None,
    Note,
    Option,
    Paragraph,
    Presentation,
    ProgressBar,
    Radio,
    RadioGroup,
    Range,
    Region,
    Row,
    RowGroup,
    RowHeader,
    ScrollBar,
    Search,
    SearchBox,
    SectionHead,
    Select,
    Separator,
    Slider,
    SpinButton,
    Status,
    Strong,
    Subscript,
    Superscript,
    Switch,
    Tab,
    Table,
    TabList,
    TabPanel,
    Term,
    TextBox,
    Time,
    Timer,
    ToolBar,
    ToolTip,
    Tree,
    TreeGrid,
    TreeItem,
    Window
}

/// <summary>
/// Provides helpers for working with <see cref="AriaRole"/> values.
/// </summary>
public static class AriaRoleExtensions
{
    /// <summary>
    /// Converts a <see cref="AriaRole"/> value to the lowercase token used in the <c>role</c> attribute.
    /// </summary>
    public static string ToAttributeValue(this AriaRole role)
    {
        return role switch
        {
            AriaRole.Alert => "alert",
            AriaRole.AlertDialog => "alertdialog",
            AriaRole.Application => "application",
            AriaRole.Article => "article",
            AriaRole.Banner => "banner",
            AriaRole.BlockQuote => "blockquote",
            AriaRole.Button => "button",
            AriaRole.Caption => "caption",
            AriaRole.Cell => "cell",
            AriaRole.Checkbox => "checkbox",
            AriaRole.Code => "code",
            AriaRole.ColumnHeader => "columnheader",
            AriaRole.Combobox => "combobox",
            AriaRole.Command => "command",
            AriaRole.Composite => "composite",
            AriaRole.Complementary => "complementary",
            AriaRole.ContentInfo => "contentinfo",
            AriaRole.Definition => "definition",
            AriaRole.Deletion => "deletion",
            AriaRole.Dialog => "dialog",
            AriaRole.Directory => "directory",
            AriaRole.Document => "document",
            AriaRole.Emphasis => "emphasis",
            AriaRole.Feed => "feed",
            AriaRole.Figure => "figure",
            AriaRole.Form => "form",
            AriaRole.Generic => "generic",
            AriaRole.Grid => "grid",
            AriaRole.GridCell => "gridcell",
            AriaRole.Group => "group",
            AriaRole.Heading => "heading",
            AriaRole.Img => "img",
            AriaRole.Input => "input",
            AriaRole.Insertion => "insertion",
            AriaRole.Landmark => "landmark",
            AriaRole.Link => "link",
            AriaRole.List => "list",
            AriaRole.ListBox => "listbox",
            AriaRole.ListItem => "listitem",
            AriaRole.Log => "log",
            AriaRole.Main => "main",
            AriaRole.Marquee => "marquee",
            AriaRole.Math => "math",
            AriaRole.Menu => "menu",
            AriaRole.MenuBar => "menubar",
            AriaRole.MenuItem => "menuitem",
            AriaRole.MenuItemCheckbox => "menuitemcheckbox",
            AriaRole.MenuItemRadio => "menuitemradio",
            AriaRole.Meter => "meter",
            AriaRole.Navigation => "navigation",
            AriaRole.None => "none",
            AriaRole.Note => "note",
            AriaRole.Option => "option",
            AriaRole.Paragraph => "paragraph",
            AriaRole.Presentation => "presentation",
            AriaRole.ProgressBar => "progressbar",
            AriaRole.Radio => "radio",
            AriaRole.RadioGroup => "radiogroup",
            AriaRole.Range => "range",
            AriaRole.Region => "region",
            AriaRole.Row => "row",
            AriaRole.RowGroup => "rowgroup",
            AriaRole.RowHeader => "rowheader",
            AriaRole.ScrollBar => "scrollbar",
            AriaRole.Search => "search",
            AriaRole.SearchBox => "searchbox",
            AriaRole.SectionHead => "sectionhead",
            AriaRole.Select => "select",
            AriaRole.Separator => "separator",
            AriaRole.Slider => "slider",
            AriaRole.SpinButton => "spinbutton",
            AriaRole.Status => "status",
            AriaRole.Strong => "strong",
            AriaRole.Subscript => "subscript",
            AriaRole.Superscript => "superscript",
            AriaRole.Switch => "switch",
            AriaRole.Tab => "tab",
            AriaRole.Table => "table",
            AriaRole.TabList => "tablist",
            AriaRole.TabPanel => "tabpanel",
            AriaRole.Term => "term",
            AriaRole.TextBox => "textbox",
            AriaRole.Time => "time",
            AriaRole.Timer => "timer",
            AriaRole.ToolBar => "toolbar",
            AriaRole.ToolTip => "tooltip",
            AriaRole.Tree => "tree",
            AriaRole.TreeGrid => "treegrid",
            AriaRole.TreeItem => "treeitem",
            AriaRole.Window => "window",
            _ => ThrowInvalidRole(role)
        };
    }

    /// <summary>
    /// Attempts to parse an attribute token into a known <see cref="AriaRole"/> value.
    /// </summary>
    public static bool TryParse(string? value, out AriaRole role)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return NameLookup.TryGetValue(value.Trim(), out role);
        }

        role = default;
            
        return false;

    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string ThrowInvalidRole(AriaRole role)
    {
        throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown ARIA role value.");
    }

    private static readonly Dictionary<string, AriaRole> NameLookup = Enum
        .GetValues<AriaRole>()
        .ToDictionary(static role => role.ToAttributeValue(), StringComparer.OrdinalIgnoreCase);
}