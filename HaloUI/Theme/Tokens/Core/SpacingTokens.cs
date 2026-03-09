// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Core;

/// <summary>
/// Core spacing tokens - base spacing scale for layout and component spacing.
/// Based on 4px/0.25rem base unit following 8-point grid system.
/// </summary>
public sealed record SpacingTokens
{
    public string Space0 { get; init; } = "0";
    public string SpacePx { get; init; } = "1px";
    public string Space0_5 { get; init; } = "0.125rem";  // 2px
    public string Space1 { get; init; } = "0.25rem";     // 4px
    public string Space1_5 { get; init; } = "0.375rem";  // 6px
    public string Space2 { get; init; } = "0.5rem";      // 8px
    public string Space2_5 { get; init; } = "0.625rem";  // 10px
    public string Space3 { get; init; } = "0.75rem";     // 12px
    public string Space3_5 { get; init; } = "0.875rem";  // 14px
    public string Space4 { get; init; } = "1rem";        // 16px
    public string Space5 { get; init; } = "1.25rem";     // 20px
    public string Space6 { get; init; } = "1.5rem";      // 24px
    public string Space7 { get; init; } = "1.75rem";     // 28px
    public string Space8 { get; init; } = "2rem";        // 32px
    public string Space9 { get; init; } = "2.25rem";     // 36px
    public string Space10 { get; init; } = "2.5rem";     // 40px
    public string Space11 { get; init; } = "2.75rem";    // 44px
    public string Space12 { get; init; } = "3rem";       // 48px
    public string Space14 { get; init; } = "3.5rem";     // 56px
    public string Space16 { get; init; } = "4rem";       // 64px
    public string Space20 { get; init; } = "5rem";       // 80px
    public string Space24 { get; init; } = "6rem";       // 96px
    public string Space28 { get; init; } = "7rem";       // 112px
    public string Space32 { get; init; } = "8rem";       // 128px
    public string Space36 { get; init; } = "9rem";       // 144px
    public string Space40 { get; init; } = "10rem";      // 160px
    public string Space44 { get; init; } = "11rem";      // 176px
    public string Space48 { get; init; } = "12rem";      // 192px
    public string Space52 { get; init; } = "13rem";      // 208px
    public string Space56 { get; init; } = "14rem";      // 224px
    public string Space60 { get; init; } = "15rem";      // 240px
    public string Space64 { get; init; } = "16rem";      // 256px
    public string Space72 { get; init; } = "18rem";      // 288px
    public string Space80 { get; init; } = "20rem";      // 320px
    public string Space96 { get; init; } = "24rem";      // 384px

    public static SpacingTokens Default { get; } = new();
}