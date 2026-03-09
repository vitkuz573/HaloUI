// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Iconography;
using HaloUI.Iconography.Packs.Material;
using Microsoft.Extensions.DependencyInjection;

namespace HaloUI.DependencyInjection;

/// <summary>
/// Dependency injection helpers for the embedded Material icon packs.
/// </summary>
public static class HaloMaterialIconPackServiceCollectionExtensions
{
    /// <summary>
    /// Replaces the active icon resolver with an embedded Material icon pack resolver.
    /// </summary>
    public static IServiceCollection AddHaloUIMaterialIconPack(
        this IServiceCollection services,
        HaloMaterialIconStyle style = HaloMaterialIconStyle.Regular,
        IHaloIconResolver? fallback = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var resolver = HaloMaterialIconPack.CreateResolver(style, fallback);
        return services.AddHaloUIIconResolver(resolver);
    }
}
