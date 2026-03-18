using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HaloUI.Accessibility;

namespace HaloUI.Abstractions;

public sealed class DialogReference : IDialogReference
{
    private readonly TaskCompletionSource<DialogResult> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly object _parametersSync = new();

    private bool _busy;
    private DialogParameters? _parameters;

    public Task<DialogResult> Result => _tcs.Task;

    public Task Completion => _tcs.Task;

    public string Title { get; private set; } = string.Empty;

    public string? TitleElementId { get; private set; }

    public string? DescriptionElementId { get; private set; }

    public Guid Id { get; private set; }

    public event Action? AccessibilityMetadataChanged;
    public event Action<bool>? BusyChanged;
    public event Action? RenderRequested;

    public void Close(DialogResult result)
    {
        _tcs.TrySetResult(result);
    }

    public void CloseSuccess(object? value = null)
    {
        Close(DialogResult.Success(value));
    }

    public void Cancel()
    {
        _tcs.TrySetResult(DialogResult.Cancel());
    }

    public void RegisterAccessibilityElement(DialogAccessibilityRole role, string elementId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(elementId);

        var changed = false;

        switch (role)
        {
            case DialogAccessibilityRole.Title:
                if (!string.Equals(TitleElementId, elementId, StringComparison.Ordinal))
                {
                    TitleElementId = elementId;
                    changed = true;
                }
                break;
            case DialogAccessibilityRole.Description:
                if (!string.Equals(DescriptionElementId, elementId, StringComparison.Ordinal))
                {
                    DescriptionElementId = elementId;
                    changed = true;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown dialog accessibility role.");
        }

        if (changed)
        {
            AccessibilityMetadataChanged?.Invoke();
        }
    }

    public void SetBusy(bool busy)
    {
        if (_busy == busy)
        {
            return;
        }

        _busy = busy;
        BusyChanged?.Invoke(_busy);
    }

    public bool IsBusy => _busy;

    public void RequestRender()
    {
        RenderRequested?.Invoke();
    }

    public bool TrySetParameter(string parameterName, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        var updated = false;

        lock (_parametersSync)
        {
            if (_parameters is null)
            {
                return false;
            }

            if (_parameters.TryGetValue(parameterName, out var existingValue) &&
                Equals(existingValue, value))
            {
                return true;
            }

            _parameters[parameterName] = value;
            updated = true;
        }

        if (updated)
        {
            RequestRender();
        }

        return true;
    }

    public bool TrySetParameters(IReadOnlyDictionary<string, object?> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var updated = false;

        lock (_parametersSync)
        {
            if (_parameters is null)
            {
                return false;
            }

            foreach (var (parameterName, value) in parameters)
            {
                if (string.IsNullOrWhiteSpace(parameterName))
                {
                    throw new ArgumentException("Parameter name cannot be null or whitespace.", nameof(parameters));
                }

                if (_parameters.TryGetValue(parameterName, out var existingValue) &&
                    Equals(existingValue, value))
                {
                    continue;
                }

                _parameters[parameterName] = value;
                updated = true;
            }
        }

        if (updated)
        {
            RequestRender();
        }

        return true;
    }

    internal void ResetMetadata()
    {
        var changed = false;

        if (!string.IsNullOrEmpty(TitleElementId) || !string.IsNullOrEmpty(DescriptionElementId))
        {
            changed = true;
        }

        Title = string.Empty;
        TitleElementId = null;
        DescriptionElementId = null;
        Id = Guid.Empty;

        if (changed)
        {
            AccessibilityMetadataChanged?.Invoke();
        }
    }

    internal void SetTitle(string title)
    {
        Title = title;
    }

    internal void SetId(Guid id)
    {
        Id = id;
    }

    internal void BindParameters(DialogParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        lock (_parametersSync)
        {
            _parameters = parameters;
        }
    }

    internal IReadOnlyList<KeyValuePair<string, object>> SnapshotParameters()
    {
        lock (_parametersSync)
        {
            if (_parameters is null || _parameters.Count == 0)
            {
                return Array.Empty<KeyValuePair<string, object>>();
            }

            var snapshot = new List<KeyValuePair<string, object>>(_parameters.Count);

            foreach (var (key, value) in _parameters)
            {
                snapshot.Add(new KeyValuePair<string, object>(key, value!));
            }

            return snapshot;
        }
    }
}
