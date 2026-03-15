// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using HaloUI.Abstractions;
using HaloUI.Accessibility;
using HaloUI.Enums;

namespace HaloUI.Components;

public partial class HaloInputFile
{
    private readonly string _generatedInputId = AccessibilityIdGenerator.Create("halo-inputfile");
    private readonly List<HaloInputFileSelection> _selectedFiles = [];
    private readonly List<HaloInputFileRejection> _rejections = [];
    private HaloInputFileRules _effectiveRules = HaloInputFileRules.Default;
    private HashSet<string>? _normalizedAllowedExtensions;
    private InputFile? _inputFile;
    private string? _labelElementId;
    private string? _descriptionElementId;
    private string? _summaryElementId;
    private int _inputRenderKey;

    [Parameter]
    public EventCallback<HaloInputFileChangeEventArgs> Changed { get; set; }

    [Parameter]
    public string? Accept { get; set; }

    [Parameter]
    public bool AllowMultiple { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Required { get; set; }

    [Parameter]
    public bool HasError { get; set; }

    [Parameter]
    public HaloInputFileMode Mode { get; set; } = HaloInputFileMode.Inline;

    [Parameter]
    public InputFieldSize Size { get; set; } = InputFieldSize.Medium;

    [Parameter]
    public HaloInputFileRules? Rules { get; set; } = HaloInputFileRules.Default;

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public string? BrowseText { get; set; }

    [Parameter]
    public string? NoSelectionText { get; set; } = "No file selected";

    [Parameter]
    public string? MultiSelectionTextFormat { get; set; } = "{0} files selected";

    [Parameter]
    public string? ClearText { get; set; } = "Clear";

    [Parameter]
    public bool ShowSummary { get; set; } = true;

    [Parameter]
    public bool ShowFileList { get; set; } = true;

    [Parameter]
    public bool ShowFileSize { get; set; } = true;

    [Parameter]
    public bool ShowClearButton { get; set; }

    [Parameter]
    public bool ShowRejections { get; set; }

    [Parameter]
    public int MaxVisibleFiles { get; set; } = 3;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? AriaLabelledBy { get; set; }

    [Parameter]
    public string? AriaDescribedBy { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [Inject]
    private IInputFileRuntime? InputFileRuntime { get; set; }

    public ElementReference Element => _inputFile?.Element ?? default;

    public IReadOnlyList<HaloInputFileSelection> Files => _selectedFiles;

    public IReadOnlyList<HaloInputFileRejection> Rejections => _rejections;

    public bool HasFiles => _selectedFiles.Count > 0;

    public bool HasRejections => _rejections.Count > 0;

    private string InputId => string.IsNullOrWhiteSpace(Id) ? _generatedInputId : Id!;

    private string? LabelElementId => string.IsNullOrWhiteSpace(Label)
        ? null
        : _labelElementId ??= AccessibilityIdGenerator.Create("halo-inputfile-label");

    private string? DescriptionElementId => string.IsNullOrWhiteSpace(Description)
        ? null
        : _descriptionElementId ??= AccessibilityIdGenerator.Create("halo-inputfile-description");

    private string? SummaryElementId => !ShowSummary || Mode == HaloInputFileMode.Hidden
        ? null
        : _summaryElementId ??= AccessibilityIdGenerator.Create("halo-inputfile-summary");

    private int HiddenFileCount => Math.Max(0, _selectedFiles.Count - MaxVisibleFiles);

    protected override void OnParametersSet()
    {
        if (MaxVisibleFiles < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxVisibleFiles), MaxVisibleFiles, "MaxVisibleFiles must be greater than zero.");
        }

        _effectiveRules = Rules ?? HaloInputFileRules.Default;

        if (_effectiveRules.MaxFiles is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Rules), _effectiveRules.MaxFiles, "Rules.MaxFiles must be greater than zero when specified.");
        }

        if (_effectiveRules.MaxFileSizeBytes is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Rules), _effectiveRules.MaxFileSizeBytes, "Rules.MaxFileSizeBytes must be greater than zero when specified.");
        }

        if (!AllowMultiple && _effectiveRules.MaxFiles is > 1)
        {
            throw new InvalidOperationException("Rules.MaxFiles cannot be greater than one when AllowMultiple is false.");
        }

        _normalizedAllowedExtensions = BuildAllowedExtensionsSet();
    }

    public async Task ClearAsync()
    {
        if (_selectedFiles.Count == 0 && _rejections.Count == 0)
        {
            return;
        }

        _selectedFiles.Clear();
        _rejections.Clear();
        _inputRenderKey++;

        await NotifyChangedAsync(HaloInputFileChangeKind.Cleared);

        StateHasChanged();
    }

    public async Task<bool> OpenAsync(CancellationToken cancellationToken = default)
    {
        if (Disabled || _inputFile is null || InputFileRuntime is null)
        {
            return false;
        }

        if (_inputFile.Element is not { } inputElement)
        {
            return false;
        }

        return await InputFileRuntime.OpenAsync(inputElement, cancellationToken);
    }

    private async Task HandleChangeAsync(InputFileChangeEventArgs args)
    {
        _selectedFiles.Clear();
        _rejections.Clear();

        var effectiveMaxFiles = ResolveMaxFiles();
        var files = args.GetMultipleFiles(int.MaxValue);

        foreach (var file in files)
        {
            if (_selectedFiles.Count >= effectiveMaxFiles)
            {
                _rejections.Add(new HaloInputFileRejection(
                    file.Name,
                    file.Size,
                    file.ContentType,
                    HaloInputFileRejectionReason.TooManyFiles,
                    $"Only {effectiveMaxFiles} file(s) can be selected."));
                continue;
            }

            if (_effectiveRules.MaxFileSizeBytes is { } maxFileSizeBytes && file.Size > maxFileSizeBytes)
            {
                _rejections.Add(new HaloInputFileRejection(
                    file.Name,
                    file.Size,
                    file.ContentType,
                    HaloInputFileRejectionReason.FileTooLarge,
                    $"{file.Name} exceeds the maximum size of {FormatFileSize(maxFileSizeBytes)}."));
                continue;
            }

            if (!IsExtensionAllowed(file.Name))
            {
                var extension = Path.GetExtension(file.Name);
                _rejections.Add(new HaloInputFileRejection(
                    file.Name,
                    file.Size,
                    file.ContentType,
                    HaloInputFileRejectionReason.InvalidExtension,
                    $"{file.Name} has an unsupported extension '{extension}'."));
                continue;
            }

            _selectedFiles.Add(new HaloInputFileSelection(file));
        }

        await NotifyChangedAsync(ResolveChangeKind());
    }

    private int ResolveMaxFiles()
    {
        if (!AllowMultiple)
        {
            return 1;
        }

        return _effectiveRules.MaxFiles ?? int.MaxValue;
    }

    private HashSet<string>? BuildAllowedExtensionsSet()
    {
        if (_effectiveRules.AllowedExtensions.Count == 0)
        {
            return null;
        }

        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var extension in _effectiveRules.AllowedExtensions)
        {
            var normalized = NormalizeExtension(extension);

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                set.Add(normalized);
            }
        }

        return set.Count == 0 ? null : set;
    }

    private bool IsExtensionAllowed(string fileName)
    {
        if (_normalizedAllowedExtensions is null)
        {
            return true;
        }

        var extension = Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(extension))
        {
            return false;
        }

        return _normalizedAllowedExtensions.Contains(NormalizeExtension(extension));
    }

    private static string NormalizeExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return string.Empty;
        }

        var normalized = extension.Trim();

        if (!normalized.StartsWith(".", StringComparison.Ordinal))
        {
            normalized = $".{normalized}";
        }

        return normalized.ToLowerInvariant();
    }

    private HaloInputFileChangeKind ResolveChangeKind()
    {
        if (_selectedFiles.Count > 0)
        {
            return HaloInputFileChangeKind.Selected;
        }

        if (_rejections.Count > 0)
        {
            return HaloInputFileChangeKind.Rejected;
        }

        return HaloInputFileChangeKind.Cleared;
    }

    private async Task NotifyChangedAsync(HaloInputFileChangeKind kind)
    {
        if (!Changed.HasDelegate)
        {
            return;
        }

        await Changed.InvokeAsync(new HaloInputFileChangeEventArgs(_selectedFiles.ToArray(), _rejections.ToArray(), kind));
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
            .WithDisabled(Disabled);

        if (Required)
        {
            builder.WithRequired(true);
        }

        if (HasError || HasRejections)
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

        var attributes = builder.Build();
        var bag = new Dictionary<string, object>(attributes, StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = InputId,
            ["type"] = "file"
        };

        if (!string.IsNullOrWhiteSpace(Accept))
        {
            bag["accept"] = Accept!;
        }

        if (AllowMultiple)
        {
            bag["multiple"] = "multiple";
        }

        if (Disabled)
        {
            bag["disabled"] = "disabled";
        }

        if (Required)
        {
            bag["required"] = "required";
        }

        return bag;
    }

    private LabelVariant ResolveLabelVariant()
    {
        if (HasError || HasRejections)
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
            GetSizeClass(Size),
            GetModeClass(Mode)
        };

        if (Disabled)
        {
            classes.Add("halo-inputfile--disabled");
        }

        if (HasError || HasRejections)
        {
            classes.Add("halo-inputfile--error");
        }

        if (HasFiles)
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

        if (Mode == HaloInputFileMode.Hidden)
        {
            classes.Add("halo-inputfile__control--hidden");
        }

        if (Mode == HaloInputFileMode.Dropzone)
        {
            classes.Add("halo-inputfile__control--dropzone");
        }

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

        if (HasError || HasRejections)
        {
            classes.Add("halo-is-error");
        }

        return string.Join(' ', classes);
    }

    private string BuildTriggerClass()
    {
        var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "halo-inputfile__trigger"
        };

        if (Mode == HaloInputFileMode.Dropzone)
        {
            classes.Add("halo-inputfile__trigger--dropzone");
        }

        if (Disabled)
        {
            classes.Add("halo-is-disabled");
        }

        return string.Join(' ', classes);
    }

    private string BuildSummaryClass()
    {
        var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "halo-inputfile__summary"
        };

        if (!HasFiles)
        {
            classes.Add("halo-inputfile__summary--empty");
        }

        return string.Join(' ', classes);
    }

    private string BuildListClass()
    {
        return "halo-inputfile__files";
    }

    private string ResolveBrowseText()
    {
        if (!string.IsNullOrWhiteSpace(BrowseText))
        {
            return BrowseText!;
        }

        return AllowMultiple ? "Choose files" : "Choose file";
    }

    private string ResolveDropzoneTitleText()
    {
        if (!string.IsNullOrWhiteSpace(BrowseText))
        {
            return BrowseText!;
        }

        return AllowMultiple ? "Drop files here" : "Drop a file here";
    }

    private string ResolveDropzoneHintText()
    {
        return AllowMultiple
            ? "or click to choose files"
            : "or click to choose a file";
    }

    private string BuildSummaryText()
    {
        if (_selectedFiles.Count == 0)
        {
            if (_rejections.Count > 0)
            {
                return _rejections[0].Message;
            }

            return string.IsNullOrWhiteSpace(NoSelectionText)
                ? "No file selected"
                : NoSelectionText!;
        }

        if (_selectedFiles.Count == 1)
        {
            return _selectedFiles[0].Name;
        }

        var format = string.IsNullOrWhiteSpace(MultiSelectionTextFormat)
            ? "{0} files selected"
            : MultiSelectionTextFormat!;

        try
        {
            return string.Format(CultureInfo.CurrentCulture, format, _selectedFiles.Count);
        }
        catch (FormatException)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} files selected", _selectedFiles.Count);
        }
    }

    private IReadOnlyList<HaloInputFileSelection> GetVisibleFiles()
    {
        if (_selectedFiles.Count <= MaxVisibleFiles)
        {
            return _selectedFiles;
        }

        return _selectedFiles.Take(MaxVisibleFiles).ToArray();
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

    private static string GetModeClass(HaloInputFileMode mode)
    {
        return mode switch
        {
            HaloInputFileMode.Dropzone => "halo-inputfile--mode-dropzone",
            HaloInputFileMode.Hidden => "halo-inputfile--mode-hidden",
            _ => "halo-inputfile--mode-inline"
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

}
