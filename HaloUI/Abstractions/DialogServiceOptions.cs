using System;

namespace HaloUI.Abstractions;

public sealed record DialogServiceOptions
{
    private int _maxActiveDialogs;

    public static DialogServiceOptions Default { get; } = new();

    public int MaxActiveDialogs
    {
        get => _maxActiveDialogs;
        init => _maxActiveDialogs = value < 0
            ? throw new ArgumentOutOfRangeException(nameof(value), value, "MaxActiveDialogs cannot be negative.")
            : value;
    }

    public bool ThrowIfNoDialogHostRegistered { get; init; }

    public bool EnforceOverlayCloseForRestrictedDialogs { get; init; } = true;

    public bool MarkBusyDuringActivation { get; init; } = true;
}