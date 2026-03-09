// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using HaloUI.Enums;

namespace HaloUI.Abstractions;

public abstract record DialogVariantOptions(DialogPresentation Presentation)
{
    public static DialogModalOptions Modal { get; } = DialogModalOptions.Default;

    public static DialogDrawerOptions Drawer(DialogDrawerPlacement placement = DialogDrawerPlacement.End, string? width = null) =>
        new(placement, width);

    public static DialogDrawerOptions DrawerNarrow(DialogDrawerPlacement placement = DialogDrawerPlacement.End) =>
        DialogDrawerOptions.Narrow(placement);

    public static DialogDrawerOptions DrawerMedium(DialogDrawerPlacement placement = DialogDrawerPlacement.End) =>
        DialogDrawerOptions.Medium(placement);

    public static DialogDrawerOptions DrawerWide(DialogDrawerPlacement placement = DialogDrawerPlacement.End) =>
        DialogDrawerOptions.Wide(placement);
}

public sealed record DialogModalOptions : DialogVariantOptions
{
    private DialogModalOptions() : base(DialogPresentation.Modal)
    {
    }

    public static DialogModalOptions Default { get; } = new();
}

public sealed record DialogDrawerOptions(DialogDrawerPlacement Placement = DialogDrawerPlacement.End, string? Width = null)
    : DialogVariantOptions(DialogPresentation.Drawer)
{
    private const string NarrowWidth = "24rem";
    private const string MediumWidth = "32rem";
    private const string WideWidth = "40rem";

    public static DialogDrawerOptions Narrow(DialogDrawerPlacement placement = DialogDrawerPlacement.End) =>
        new(placement, NarrowWidth);

    public static DialogDrawerOptions Medium(DialogDrawerPlacement placement = DialogDrawerPlacement.End) =>
        new(placement, MediumWidth);

    public static DialogDrawerOptions Wide(DialogDrawerPlacement placement = DialogDrawerPlacement.End) =>
        new(placement, WideWidth);
}

public sealed record DialogAccessPolicy
{
    private static readonly DialogAccessPolicy DefaultPolicy = new(true, ImmutableHashSet<string>.Empty);
    private static readonly StringComparer RoleComparer = StringComparer.OrdinalIgnoreCase;

    public static DialogAccessPolicy AllowAll { get; } = DefaultPolicy;

    public DialogAccessPolicy(bool allowAnonymous = true, IEnumerable<string>? requiredRoles = null)
    {
        AllowAnonymous = allowAnonymous;

        if (requiredRoles is null)
        {
            RequiredRoles = ImmutableHashSet<string>.Empty;
        }
        else
        {
            RequiredRoles = requiredRoles.Where(static role => !string.IsNullOrWhiteSpace(role))
                .Select(static role => role.Trim())
                .Distinct(RoleComparer)
                .ToImmutableHashSet(RoleComparer);
        }
    }

    public bool AllowAnonymous { get; }

    public IReadOnlySet<string> RequiredRoles { get; }

    public DialogAccessPolicy RequireRoles(params string[] roles) => new(AllowAnonymous, roles);

    public static DialogAccessPolicy RequireRole(string role) => new(false, new[] { role });

    public DialogAccessPolicy MergeWith(DialogAccessPolicy other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var combinedRoles = RequiredRoles.Count == 0
            ? other.RequiredRoles
            : RequiredRoles.Union(other.RequiredRoles, RoleComparer).ToImmutableHashSet(RoleComparer);

        var allowAnonymous = AllowAnonymous && other.AllowAnonymous;

        return new DialogAccessPolicy(allowAnonymous, combinedRoles);
    }
}

public sealed record DialogOptions
{
    public bool CloseOnOverlayClick { get; init; }

    public bool CloseOnEscape { get; init; }

    public TimeSpan? AutoDismissAfter { get; init; }

    public DialogResult? AutoDismissResult { get; init; }

    public DialogSize Size { get; init; } = DialogSize.Small;

    public string? CssClass { get; init; }

    public string? InitialFocusElementId { get; init; }

    public DialogAriaRole AriaRole { get; init; } = DialogAriaRole.Dialog;

    public DialogVariantOptions Variant { get; init; } = DialogModalOptions.Default;

    public DialogAccessPolicy AccessPolicy { get; init; } = DialogAccessPolicy.AllowAll;

    public bool StickyHeader { get; init; }

    public bool StickyFooter { get; init; }

    public bool Busy { get; init; }

    public bool ShowDrawerHandle { get; init; }

    public string DrawerHandleAriaLabel { get; init; } = "Close drawer";

    public static DialogOptions Default { get; } = new();

    public static DialogOptions CreateModal() => Default;

    public static DialogOptions CreateDrawer(DialogDrawerPlacement placement = DialogDrawerPlacement.End, string? width = null) =>
        Default with { Variant = DialogVariantOptions.Drawer(placement, width) };

    public DialogOptions WithVariant(DialogVariantOptions variant)
    {
        ArgumentNullException.ThrowIfNull(variant);

        return this with { Variant = variant };
    }

    public DialogOptions WithAutoDismiss(TimeSpan timeout, DialogResult? result = null)
    {
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "Auto-dismiss timeout must be positive.");
        }

        return this with
        {
            AutoDismissAfter = timeout,
            AutoDismissResult = result
        };
    }

    public DialogOptions WithAccessPolicy(DialogAccessPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        return this with { AccessPolicy = policy };
    }

    internal DialogOptions Normalize()
    {
        var normalized = this;

        if (normalized.Variant is null)
        {
            normalized = normalized with { Variant = DialogModalOptions.Default };
        }

        if (normalized.AccessPolicy is null)
        {
            normalized = normalized with { AccessPolicy = DialogAccessPolicy.AllowAll };
        }

        if (normalized.AutoDismissAfter is { } timeout && timeout <= TimeSpan.Zero)
        {
            normalized = normalized with { AutoDismissAfter = null, AutoDismissResult = null };
        }

        return normalized;
    }
}
