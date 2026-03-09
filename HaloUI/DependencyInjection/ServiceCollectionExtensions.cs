// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using HaloUI.Abstractions;
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
    /// Registers all HaloUI services (theme state, dialog/snackbar infrastructure, inspectors).
    /// </summary>
    public static IServiceCollection AddHaloUI(this IServiceCollection services, IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHaloUIThemeProvider(configuration);
        services.AddHaloUIDialogHost();
        services.AddHaloUISnackbarHost();
        services.AddHaloUIDialogInspector();
        services.AddHaloUIAriaInspector(configuration);

        return services;
    }

    /// <summary>
    /// Registers theme-related services (ThemeState + cascading context).
    /// </summary>
    public static IServiceCollection AddHaloUIThemeProvider(this IServiceCollection services, IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.TryAddSingleton<IThemeCatalog>(_ => GeneratedThemeCatalog.Instance);

        services.AddScoped<ThemeState>(sp =>
        {
            var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor.HttpContext;
            var catalog = sp.GetRequiredService<IThemeCatalog>();

            var defaultKey = catalog.DefaultThemeKey;
            var themeKey = defaultKey;
            var hasExplicit = false;
            HaloTheme theme;

            string? requestedKey = null;
            var fromQuery = false;

            if (httpContext is not null)
            {
                if (httpContext.Request.Query.TryGetValue(ThemeState.CookieName, out var queryValue) &&
                    !string.IsNullOrWhiteSpace(queryValue))
                {
                    requestedKey = queryValue.ToString();
                    fromQuery = true;
                }
                else if (httpContext.Request.Cookies.TryGetValue(ThemeState.CookieName, out var storedKey) &&
                         !string.IsNullOrWhiteSpace(storedKey))
                {
                    requestedKey = storedKey;
                }
            }

            if (!string.IsNullOrWhiteSpace(requestedKey) &&
                catalog.TryCreateThemeSystem(requestedKey, out var requestedSystem) &&
                catalog.TryGetDescriptor(requestedKey, out var descriptor))
            {
                themeKey = descriptor.Key;
                theme = new HaloTheme { Tokens = requestedSystem };
                hasExplicit = true;

                if (fromQuery && httpContext?.Response is { } response)
                {
                    response.Cookies.Append(
                        ThemeState.CookieName,
                        themeKey,
                        new CookieOptions
                        {
                            Path = "/",
                            MaxAge = TimeSpan.FromDays(365)
                        });
                }
            }
            else
            {
                theme = new HaloTheme { Tokens = catalog.CreateThemeSystem(defaultKey) };
            }

            return new ThemeState(catalog, themeKey, theme, hasExplicit);
        });

        services.AddCascadingValue(sp => sp.GetRequiredService<ThemeState>().Context);

        return services;
    }

    /// <summary>
    /// Registers dialog host infrastructure (diagnostics + dialog service).
    /// </summary>
    public static IServiceCollection AddHaloUIDialogHost(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

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
    public static IServiceCollection AddHaloUIDialogInspector(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<DialogInspectorState>();

        return services;
    }

    /// <summary>
    /// Registers ARIA inspector diagnostics provider.
    /// </summary>
    public static IServiceCollection AddHaloUIAriaInspector(this IServiceCollection services, IConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (configuration is not null)
        {
            var ariaInspectorSection = configuration.GetSection("AriaInspector");
            services.Configure<AriaInspectorOptions>(ariaInspectorSection);
            var ariaInspectorOptions = ariaInspectorSection.Get<AriaInspectorOptions>()?.Normalize() ?? AriaInspectorOptions.Default;

            if (ariaInspectorOptions.IsEnabled)
            {
                services.TryAddSingleton<IAriaDiagnosticsHub, AriaDiagnosticsHub>();
                services.TryAddSingleton<AriaInspectorState>();
                
                return services;
            }
        }

        services.TryAddSingleton<IAriaDiagnosticsHub, NoOpAriaDiagnosticsHub>();
        
        return services;
    }
}