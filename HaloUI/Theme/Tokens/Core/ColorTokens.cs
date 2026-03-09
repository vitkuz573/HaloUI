// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Core;

/// <summary>
/// Core color tokens - primitive color palette foundation.
/// These are the base colors that should not be used directly in components.
/// Use semantic tokens instead for better maintainability and theming.
/// </summary>
public sealed record ColorTokens
{
    // Neutral colors
    public ColorScale White { get; init; } = ColorScale.White;
    public ColorScale Black { get; init; } = ColorScale.Black;
    public ColorScale Gray { get; init; } = ColorScale.Gray;
    public ColorScale Slate { get; init; } = ColorScale.Slate;

    // Brand colors
    public ColorScale Primary { get; init; } = ColorScale.Indigo;
    public ColorScale Secondary { get; init; } = ColorScale.Violet;

    // Semantic colors
    public ColorScale Success { get; init; } = ColorScale.Emerald;
    public ColorScale Warning { get; init; } = ColorScale.Amber;
    public ColorScale Danger { get; init; } = ColorScale.Rose;
    public ColorScale Info { get; init; } = ColorScale.Sky;

    // Additional accent colors
    public ColorScale Accent1 { get; init; } = ColorScale.Cyan;
    public ColorScale Accent2 { get; init; } = ColorScale.Teal;
    public ColorScale Accent3 { get; init; } = ColorScale.Purple;

    public static ColorTokens Default { get; } = new();
}

/// <summary>
/// Represents a complete color scale from 50 to 950.
/// Following industry standards (Tailwind CSS, Material Design, etc.)
/// </summary>
public sealed record ColorScale
{
    public string Scale50 { get; init; } = string.Empty;
    public string Scale100 { get; init; } = string.Empty;
    public string Scale200 { get; init; } = string.Empty;
    public string Scale300 { get; init; } = string.Empty;
    public string Scale400 { get; init; } = string.Empty;
    public string Scale500 { get; init; } = string.Empty;
    public string Scale600 { get; init; } = string.Empty;
    public string Scale700 { get; init; } = string.Empty;
    public string Scale800 { get; init; } = string.Empty;
    public string Scale900 { get; init; } = string.Empty;
    public string Scale950 { get; init; } = string.Empty;

    // Predefined scales based on Tailwind CSS
    public static ColorScale White { get; } = new()
    {
        Scale50 = "#ffffff",
        Scale100 = "#ffffff",
        Scale200 = "#ffffff",
        Scale300 = "#ffffff",
        Scale400 = "#ffffff",
        Scale500 = "#ffffff",
        Scale600 = "#f5f5f5",
        Scale700 = "#e5e5e5",
        Scale800 = "#d4d4d4",
        Scale900 = "#a3a3a3",
        Scale950 = "#737373"
    };

    public static ColorScale Black { get; } = new()
    {
        Scale50 = "#737373",
        Scale100 = "#525252",
        Scale200 = "#404040",
        Scale300 = "#262626",
        Scale400 = "#171717",
        Scale500 = "#000000",
        Scale600 = "#000000",
        Scale700 = "#000000",
        Scale800 = "#000000",
        Scale900 = "#000000",
        Scale950 = "#000000"
    };

    public static ColorScale Gray { get; } = new()
    {
        Scale50 = "#f9fafb",
        Scale100 = "#f3f4f6",
        Scale200 = "#e5e7eb",
        Scale300 = "#d1d5db",
        Scale400 = "#9ca3af",
        Scale500 = "#6b7280",
        Scale600 = "#4b5563",
        Scale700 = "#374151",
        Scale800 = "#1f2937",
        Scale900 = "#111827",
        Scale950 = "#030712"
    };

    public static ColorScale Slate { get; } = new()
    {
        Scale50 = "#f8fafc",
        Scale100 = "#f1f5f9",
        Scale200 = "#e2e8f0",
        Scale300 = "#cbd5e1",
        Scale400 = "#94a3b8",
        Scale500 = "#64748b",
        Scale600 = "#475569",
        Scale700 = "#334155",
        Scale800 = "#1e293b",
        Scale900 = "#0f172a",
        Scale950 = "#020617"
    };

    public static ColorScale Indigo { get; } = new()
    {
        Scale50 = "#eef2ff",
        Scale100 = "#e0e7ff",
        Scale200 = "#c7d2fe",
        Scale300 = "#a5b4fc",
        Scale400 = "#818cf8",
        Scale500 = "#6366f1",
        Scale600 = "#4f46e5",
        Scale700 = "#4338ca",
        Scale800 = "#3730a3",
        Scale900 = "#312e81",
        Scale950 = "#1e1b4b"
    };

    public static ColorScale Violet { get; } = new()
    {
        Scale50 = "#f5f3ff",
        Scale100 = "#ede9fe",
        Scale200 = "#ddd6fe",
        Scale300 = "#c4b5fd",
        Scale400 = "#a78bfa",
        Scale500 = "#8b5cf6",
        Scale600 = "#7c3aed",
        Scale700 = "#6d28d9",
        Scale800 = "#5b21b6",
        Scale900 = "#4c1d95",
        Scale950 = "#2e1065"
    };

    public static ColorScale Emerald { get; } = new()
    {
        Scale50 = "#ecfdf5",
        Scale100 = "#d1fae5",
        Scale200 = "#a7f3d0",
        Scale300 = "#6ee7b7",
        Scale400 = "#34d399",
        Scale500 = "#10b981",
        Scale600 = "#059669",
        Scale700 = "#047857",
        Scale800 = "#065f46",
        Scale900 = "#064e3b",
        Scale950 = "#022c22"
    };

    public static ColorScale Amber { get; } = new()
    {
        Scale50 = "#fffbeb",
        Scale100 = "#fef3c7",
        Scale200 = "#fde68a",
        Scale300 = "#fcd34d",
        Scale400 = "#fbbf24",
        Scale500 = "#f59e0b",
        Scale600 = "#d97706",
        Scale700 = "#b45309",
        Scale800 = "#92400e",
        Scale900 = "#78350f",
        Scale950 = "#451a03"
    };

    public static ColorScale Rose { get; } = new()
    {
        Scale50 = "#fff1f2",
        Scale100 = "#ffe4e6",
        Scale200 = "#fecdd3",
        Scale300 = "#fda4af",
        Scale400 = "#fb7185",
        Scale500 = "#f43f5e",
        Scale600 = "#e11d48",
        Scale700 = "#be123c",
        Scale800 = "#9f1239",
        Scale900 = "#881337",
        Scale950 = "#4c0519"
    };

    public static ColorScale Sky { get; } = new()
    {
        Scale50 = "#f0f9ff",
        Scale100 = "#e0f2fe",
        Scale200 = "#bae6fd",
        Scale300 = "#7dd3fc",
        Scale400 = "#38bdf8",
        Scale500 = "#0ea5e9",
        Scale600 = "#0284c7",
        Scale700 = "#0369a1",
        Scale800 = "#075985",
        Scale900 = "#0c4a6e",
        Scale950 = "#082f49"
    };

    public static ColorScale Cyan { get; } = new()
    {
        Scale50 = "#ecfeff",
        Scale100 = "#cffafe",
        Scale200 = "#a5f3fc",
        Scale300 = "#67e8f9",
        Scale400 = "#22d3ee",
        Scale500 = "#06b6d4",
        Scale600 = "#0891b2",
        Scale700 = "#0e7490",
        Scale800 = "#155e75",
        Scale900 = "#164e63",
        Scale950 = "#083344"
    };

    public static ColorScale Teal { get; } = new()
    {
        Scale50 = "#f0fdfa",
        Scale100 = "#ccfbf1",
        Scale200 = "#99f6e4",
        Scale300 = "#5eead4",
        Scale400 = "#2dd4bf",
        Scale500 = "#14b8a6",
        Scale600 = "#0d9488",
        Scale700 = "#0f766e",
        Scale800 = "#115e59",
        Scale900 = "#134e4a",
        Scale950 = "#042f2e"
    };

    public static ColorScale Purple { get; } = new()
    {
        Scale50 = "#faf5ff",
        Scale100 = "#f3e8ff",
        Scale200 = "#e9d5ff",
        Scale300 = "#d8b4fe",
        Scale400 = "#c084fc",
        Scale500 = "#a855f7",
        Scale600 = "#9333ea",
        Scale700 = "#7e22ce",
        Scale800 = "#6b21a8",
        Scale900 = "#581c87",
        Scale950 = "#3b0764"
    };
}