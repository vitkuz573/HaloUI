// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HaloUI.Abstractions;
using HaloUI.Iconography;
using HaloUI.Services;
using HaloUI.Theme.Sdk.Runtime;
using HaloUI.Theme;

namespace HaloUI.DependencyInjection;

/// <summary>
/// Dependency injection helpers for HaloUI.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the HaloUI runtime core (theme state, dialogs, snackbars).
    /// </summary>
    public static IServiceCollection AddHaloUICore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHaloUIThemeProvider();
        services.AddHaloUIDialogHost();
        services.AddHaloUISnackbarHost();
        services.TryAddSingleton<IHaloIconResolver>(_ => new PassthroughHaloIconResolver());
        services.TryAddSingleton<IAriaDiagnosticsHub, NoOpAriaDiagnosticsHub>();

        return services;
    }

    /// <summary>
    /// Registers diagnostics overlays and telemetry hubs (dialog/ARIA inspectors).
    /// </summary>
    public static IServiceCollection AddHaloUIDiagnostics(this IServiceCollection services, IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHaloUIDialogInspector(configuration);
        services.AddHaloUIAriaInspector(configuration);

        return services;
    }

    /// <summary>
    /// Registers theme-related services (ThemeState + cascading context).
    /// </summary>
    public static IServiceCollection AddHaloUIThemeProvider(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IThemeCatalog>(_ => GeneratedThemeCatalog.Instance);
        services.TryAddScoped<IThemePreferenceStore, ThemePreferenceStore>();

        services.AddScoped<ThemeState>(sp =>
        {
            var catalog = sp.GetRequiredService<IThemeCatalog>();
            var themePreferenceStore = sp.GetRequiredService<IThemePreferenceStore>();
            var logger = sp.GetService<ILogger<ThemeState>>();

            var defaultKey = catalog.DefaultThemeKey;
            var preference = themePreferenceStore.Resolve(defaultKey);
            var requestedKey = preference.ThemeKey;

            if (!string.IsNullOrWhiteSpace(requestedKey) &&
                catalog.TryCreateThemeSystem(requestedKey, out var requestedSystem) &&
                catalog.TryGetDescriptor(requestedKey, out var descriptor))
            {
                var theme = new HaloTheme { Tokens = requestedSystem };
                return new ThemeState(catalog, descriptor.Key, theme, preference.HasExplicitTheme, logger);
            }

            var fallbackTheme = new HaloTheme { Tokens = catalog.CreateThemeSystem(defaultKey) };
            return new ThemeState(catalog, defaultKey, fallbackTheme, false, logger);
        });

        services.AddCascadingValue(sp => sp.GetRequiredService<ThemeState>().Context);

        return services;
    }

    /// <summary>
    /// Enables HTTP query/cookie-backed theme preference resolution.
    /// </summary>
    public static IServiceCollection AddHaloUIHttpThemePreferenceStore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.Replace(ServiceDescriptor.Scoped<IThemePreferenceStore, HttpThemePreferenceStore>());

        return services;
    }

    /// <summary>
    /// Registers dialog host infrastructure (diagnostics + dialog service).
    /// </summary>
    public static IServiceCollection AddHaloUIDialogHost(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IOverlayRuntime, OverlayRuntime>();
        services.TryAddSingleton<IDialogDiagnosticsHub, DialogDiagnosticsHub>();
        services.TryAddScoped<IDialogService, DialogService>();

        return services;
    }

    /// <summary>
    /// Registers snackbar service.
    /// </summary>
    public static IServiceCollection AddHaloUISnackbarHost(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ISnackbarService, SnackbarService>();

        return services;
    }

    /// <summary>
    /// Registers dialog inspector state.
    /// </summary>
    public static IServiceCollection AddHaloUIDialogInspector(this IServiceCollection services, IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<DialogInspectorOptions>();

        if (configuration is not null)
        {
            services.Configure<DialogInspectorOptions>(configuration.GetSection("DialogInspector"));
        }

        services.TryAddSingleton<DialogInspectorState>();

        return services;
    }

    /// <summary>
    /// Registers ARIA inspector diagnostics provider.
    /// </summary>
    public static IServiceCollection AddHaloUIAriaInspector(this IServiceCollection services, IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<AriaInspectorOptions>();

        if (configuration is not null)
        {
            var ariaInspectorSection = configuration.GetSection("AriaInspector");
            services.Configure<AriaInspectorOptions>(ariaInspectorSection);
        }

        services.TryAddSingleton<AriaInspectorState>();
        services.Replace(ServiceDescriptor.Singleton<IAriaDiagnosticsHub>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AriaInspectorOptions>>().Value.Normalize();

            if (!options.IsEnabled)
            {
                return new NoOpAriaDiagnosticsHub();
            }

            return ActivatorUtilities.CreateInstance<AriaDiagnosticsHub>(sp);
        }));

        return services;
    }

    /// <summary>
    /// Replaces the active icon resolver.
    /// </summary>
    public static IServiceCollection AddHaloUIIconResolver(this IServiceCollection services, IHaloIconResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(resolver);

        services.Replace(ServiceDescriptor.Singleton<IHaloIconResolver>(resolver));

        return services;
    }

    /// <summary>
    /// Replaces the active icon resolver with a passthrough ligature resolver.
    /// </summary>
    public static IServiceCollection AddHaloUIPassthroughLigatureIcons(this IServiceCollection services, string? providerClass = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Replace(ServiceDescriptor.Singleton<IHaloIconResolver>(_ => new PassthroughHaloIconResolver(providerClass)));

        return services;
    }

    /// <summary>
    /// Replaces the active icon resolver with a manifest-backed resolver.
    /// </summary>
    public static IServiceCollection AddHaloUIIconPack(this IServiceCollection services, HaloIconPackManifest manifest, IHaloIconResolver? fallback = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(manifest);

        services.Replace(ServiceDescriptor.Singleton<IHaloIconResolver>(_ => new HaloIconPackResolver(manifest, fallback)));

        return services;
    }
}
