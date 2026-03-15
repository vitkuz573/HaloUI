// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using HaloUI.Accessibility;
using HaloUI.Enums;

namespace HaloUI.Components;

public partial class HaloInputFile
{
    private readonly string _generatedInputId = AccessibilityIdGenerator.Create("halo-inputfile");
    private readonly List<HaloInputFileItem> _selectedFiles = [];
    private InputFile? _inputFile;
    private string? _labelElementId;
    private string? _descriptionElementId;
    private string? _summaryElementId;
    private int _inputRenderKey;

    [Parameter]
    public EventCallback<InputFileChangeEventArgs> SelectionChanged { get; set; }

    [Parameter]
    public EventCallback<HaloInputFileChangedEventArgs> FilesChanged { get; set; }

    [Parameter]
    public EventCallback SelectionCleared { get; set; }

    [Parameter]
    public string? Accept { get; set; }

    [Parameter]
    public bool Multiple { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Required { get; set; }

    [Parameter]
    public bool HasError { get; set; }

    [Parameter]
    public InputFieldSize Size { get; set; } = InputFieldSize.Medium;

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public string? Placeholder { get; set; } = "No file selected";

    [Parameter]
    public string? ButtonText { get; set; }

    [Parameter]
    public string? ClearButtonText { get; set; } = "Clear";

    [Parameter]
    public string? MultipleSelectionTextFormat { get; set; } = "{0} files selected";

    [Parameter]
    public bool ShowSummary { get; set; } = true;

    [Parameter]
    public bool ShowSelectedFiles { get; set; } = true;

    [Parameter]
    public bool ShowFileSize { get; set; } = true;

    [Parameter]
    public bool ShowClearButton { get; set; }

    [Parameter]
    public int MaxVisibleFiles { get; set; } = 3;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? ControlClass { get; set; }

    [Parameter]
    public string? InputClass { get; set; }

    [Parameter]
    public string? ButtonClass { get; set; }

    [Parameter]
    public string? SummaryClass { get; set; }

    [Parameter]
    public string? ListClass { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? AriaLabelledBy { get; set; }

    [Parameter]
    public string? AriaDescribedBy { get; set; }

    [Parameter]
    public RenderFragment? TriggerContent { get; set; }

    [Parameter]
    public RenderFragment<HaloInputFileItem>? ItemTemplate { get; set; }

    [Parameter]
    public IReadOnlyDictionary<string, object>? InputAttributes { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    public ElementReference Element => _inputFile?.Element ?? default;

    public IReadOnlyList<HaloInputFileItem> SelectedFiles => _selectedFiles;

    public bool HasSelectedFiles => _selectedFiles.Count > 0;

    public int SelectedFileCount => _selectedFiles.Count;

    private string InputId => string.IsNullOrWhiteSpace(Id) ? _generatedInputId : Id!;

    private string? LabelElementId => string.IsNullOrWhiteSpace(Label)
        ? null
        : _labelElementId ??= AccessibilityIdGenerator.Create("halo-inputfile-label");

    private string? DescriptionElementId => string.IsNullOrWhiteSpace(Description)
        ? null
        : _descriptionElementId ??= AccessibilityIdGenerator.Create("halo-inputfile-description");

    private string? SummaryElementId => !ShowSummary
        ? null
        : _summaryElementId ??= AccessibilityIdGenerator.Create("halo-inputfile-summary");

    private int HiddenFileCount => Math.Max(0, _selectedFiles.Count - MaxVisibleFiles);

    protected override void OnParametersSet()
    {
        if (MaxVisibleFiles < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxVisibleFiles), MaxVisibleFiles, "MaxVisibleFiles must be greater than zero.");
        }
    }

    public async Task ClearAsync()
    {
        if (_selectedFiles.Count == 0)
        {
            return;
        }

        _selectedFiles.Clear();
        _inputRenderKey++;

        if (SelectionCleared.HasDelegate)
        {
            await SelectionCleared.InvokeAsync();
        }

        StateHasChanged();
    }

    private async Task HandleChangeAsync(InputFileChangeEventArgs args)
    {
        _selectedFiles.Clear();

        foreach (var file in args.GetMultipleFiles(int.MaxValue))
        {
            _selectedFiles.Add(new HaloInputFileItem(file.Name, file.Size, file.ContentType));
        }

        if (SelectionChanged.HasDelegate)
        {
            await SelectionChanged.InvokeAsync(args);
        }

        if (FilesChanged.HasDelegate)
        {
            await FilesChanged.InvokeAsync(new HaloInputFileChangedEventArgs(_selectedFiles.ToArray(), args));
        }
    }

    private IReadOnlyDictionary<string, object>? BuildRootAttributes()
    {
        if (AdditionalAttributes is null || AdditionalAttributes.Count == 0)
        {
            return null;
        }

        var attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value) in AdditionalAttributes)
        {
            if (string.Equals(key, "class", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            attributes[key] = value;
        }

        return attributes.Count > 0 ? attributes : null;
    }

    private IReadOnlyDictionary<string, object>? BuildInputAttributes()
    {
        var builder = new AccessibilityAttributesBuilder()
            .ForComponent(typeof(HaloInputFile))
            .WithInspectorElementId(InputId)
            .WithAttribute("id", InputId)
            .WithDisabled(Disabled)
            .WithAccessibleNameFromAdditionalAttributes(InputAttributes);

        if (Required)
        {
            builder.WithRequired(true);
        }

        if (HasError)
        {
            builder.WithInvalid(true);
        }

        if (!string.IsNullOrWhiteSpace(AriaLabel))
        {
            builder.WithAria("label", AriaLabel);
        }

        foreach (var labelReference in GetLabelReferences())
        {
            builder.WithLabelledBy(labelReference);
        }

        foreach (var descriptionReference in GetDescriptionReferences())
        {
            builder.WithDescribedBy(descriptionReference);
        }

        var attributes = AccessibilityAttributesBuilder.Merge(
            InputAttributes,
            builder.Build(),
            "id",
            "class",
            "accept",
            "multiple",
            "disabled",
            "required",
            "onchange",
            "style",
            "type");

        attributes["id"] = InputId;
        attributes["type"] = "file";

        if (!string.IsNullOrWhiteSpace(Accept))
        {
            attributes["accept"] = Accept!;
        }

        if (Multiple)
        {
            attributes["multiple"] = "multiple";
        }

        if (Disabled)
        {
            attributes["disabled"] = "disabled";
        }

        if (Required)
        {
            attributes["required"] = "required";
        }

        return attributes.Count > 0 ? attributes : null;
    }

    private LabelVariant ResolveLabelVariant()
    {
        if (HasError)
        {
            return LabelVariant.Danger;
        }

        if (Disabled)
        {
            return LabelVariant.Disabled;
        }

        return LabelVariant.Primary;
    }

    private string BuildWrapperClass()
    {
        var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "halo-inputfile",
            GetSizeClass(Size)
        };

        if (Disabled)
        {
            classes.Add("halo-inputfile--disabled");
        }

        if (HasError)
        {
            classes.Add("halo-inputfile--error");
        }

        if (HasSelectedFiles)
        {
            classes.Add("halo-inputfile--has-files");
        }
        else
        {
            classes.Add("halo-inputfile--empty");
        }

        AddClasses(classes, Class);

        if (AdditionalAttributes is not null && AdditionalAttributes.TryGetValue("class", out var additionalClass) && additionalClass is not null)
        {
            AddClasses(classes, additionalClass.ToString());
        }

        return string.Join(' ', classes);
    }

    private string BuildControlClass()
    {
        var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "halo-inputfile__control"
        };

        AddClasses(classes, ControlClass);

        return string.Join(' ', classes);
    }

    private string BuildInputClass()
    {
        var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "halo-inputfile__input"
        };

        if (Disabled)
        {
            classes.Add("halo-is-disabled");
        }

        if (HasError)
        {
            classes.Add("halo-is-error");
        }

        AddClasses(classes, InputClass);

        if (InputAttributes is not null && InputAttributes.TryGetValue("class", out var additionalClass) && additionalClass is not null)
        {
            AddClasses(classes, additionalClass.ToString());
        }

        return string.Join(' ', classes);
    }

    private string BuildTriggerClass()
    {
        var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "halo-inputfile__trigger"
        };

        if (Disabled)
        {
            classes.Add("halo-is-disabled");
        }

        AddClasses(classes, ButtonClass);

        return string.Join(' ', classes);
    }

    private string BuildSummaryClass()
    {
        var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "halo-inputfile__summary"
        };

        if (!HasSelectedFiles)
        {
            classes.Add("halo-inputfile__summary--empty");
        }

        AddClasses(classes, SummaryClass);

        return string.Join(' ', classes);
    }

    private string BuildListClass()
    {
        var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "halo-inputfile__files"
        };

        AddClasses(classes, ListClass);

        return string.Join(' ', classes);
    }

    private string ResolveButtonText()
    {
        if (!string.IsNullOrWhiteSpace(ButtonText))
        {
            return ButtonText!;
        }

        return Multiple ? "Choose files" : "Choose file";
    }

    private string BuildSummaryText()
    {
        if (_selectedFiles.Count == 0)
        {
            return string.IsNullOrWhiteSpace(Placeholder)
                ? "No file selected"
                : Placeholder!;
        }

        if (_selectedFiles.Count == 1)
        {
            return _selectedFiles[0].Name;
        }

        var format = string.IsNullOrWhiteSpace(MultipleSelectionTextFormat)
            ? "{0} files selected"
            : MultipleSelectionTextFormat!;

        try
        {
            return string.Format(CultureInfo.CurrentCulture, format, _selectedFiles.Count);
        }
        catch (FormatException)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} files selected", _selectedFiles.Count);
        }
    }

    private IReadOnlyList<HaloInputFileItem> GetVisibleFiles()
    {
        if (_selectedFiles.Count <= MaxVisibleFiles)
        {
            return _selectedFiles;
        }

        return _selectedFiles.Take(MaxVisibleFiles).ToArray();
    }

    private static string FormatFileSize(long size)
    {
        if (size < 1024)
        {
            return $"{size} B";
        }

        var units = new[] { "KB", "MB", "GB", "TB" };
        var value = size / 1024d;
        var unitIndex = 0;

        while (value >= 1024d && unitIndex < units.Length - 1)
        {
            value /= 1024d;
            unitIndex++;
        }

        return string.Format(CultureInfo.CurrentCulture, "{0:0.#} {1}", value, units[unitIndex]);
    }

    private string[] GetLabelReferences()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);

        if (LabelElementId is not null)
        {
            ids.Add(LabelElementId);
        }

        foreach (var labelReference in SplitIds(AriaLabelledBy))
        {
            ids.Add(labelReference);
        }

        return ids.Count == 0 ? [] : [.. ids];
    }

    private string[] GetDescriptionReferences()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);

        if (DescriptionElementId is not null)
        {
            ids.Add(DescriptionElementId);
        }

        if (SummaryElementId is not null)
        {
            ids.Add(SummaryElementId);
        }

        foreach (var descriptionReference in SplitIds(AriaDescribedBy))
        {
            ids.Add(descriptionReference);
        }

        return ids.Count == 0 ? [] : [.. ids];
    }

    private static string GetSizeClass(InputFieldSize size)
    {
        return size switch
        {
            InputFieldSize.Small => "halo-inputfile--size-sm",
            InputFieldSize.Large => "halo-inputfile--size-lg",
            _ => "halo-inputfile--size-md"
        };
    }

    private static IEnumerable<string> SplitIds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield break;
        }

        foreach (var token in value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return token;
        }
    }

    private static void AddClasses(HashSet<string> destination, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        foreach (var token in value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            destination.Add(token);
        }
    }
}
