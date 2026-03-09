// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Threading.Tasks;
using HaloUI.Accessibility;

namespace HaloUI.Abstractions;

public sealed class DialogReference : IDialogReference
{
    private readonly TaskCompletionSource<DialogResult> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private bool _busy;

    public Task<DialogResult> Result => _tcs.Task;

    public Task Completion => _tcs.Task;

    public string Title { get; private set; } = string.Empty;

    public string? TitleElementId { get; private set; }

    public string? DescriptionElementId { get; private set; }

    public Guid Id { get; private set; }

    public event Action? AccessibilityMetadataChanged;
    public event Action<bool>? BusyChanged;

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
}