// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Components.Select;

/// <summary>
/// General behavior options for <see cref="HaloSelect{TValue}" />.
/// </summary>
public sealed record HaloSelectBehaviorOptions
{
    /// <summary>
    /// Shared immutable default instance.
    /// </summary>
    public static HaloSelectBehaviorOptions Default { get; } = new();

    /// <summary>
    /// Enables viewport-aware dropdown placement.
    /// </summary>
    public bool UseViewportPlacement { get; init; }

    /// <summary>
    /// Enables native select rendering on compact/mobile viewports.
    /// </summary>
    public bool UseNativeSelectOnMobile { get; init; } = true;
}

/// <summary>
/// Enum option generation behavior for <see cref="HaloSelect{TValue}" />.
/// </summary>
/// <typeparam name="TValue">The select value type.</typeparam>
public sealed record HaloSelectEnumBehavior<TValue>
{
    /// <summary>
    /// Shared immutable disabled instance.
    /// </summary>
    public static HaloSelectEnumBehavior<TValue> Disabled { get; } = new();

    /// <summary>
    /// Enables enum option auto-generation.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Includes an explicit null option for nullable enum values.
    /// </summary>
    public bool IncludeNullOption { get; init; }

    /// <summary>
    /// Optional null-option text. Falls back to <c>Placeholder</c>.
    /// </summary>
    public string? NullOptionText { get; init; }

    /// <summary>
    /// Optional enum text selector.
    /// </summary>
    public Func<TValue, string>? TextSelector { get; init; }

    /// <summary>
    /// Optional enum inclusion filter.
    /// </summary>
    public Func<TValue, bool>? Filter { get; init; }

    /// <summary>
    /// Optional enum disabled selector.
    /// </summary>
    public Func<TValue, bool>? DisabledSelector { get; init; }
}
