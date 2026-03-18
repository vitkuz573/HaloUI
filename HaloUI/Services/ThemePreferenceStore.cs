using HaloUI.Abstractions;
using Microsoft.AspNetCore.Http;

namespace HaloUI.Services;

internal sealed class ThemePreferenceStore : IThemePreferenceStore
{
    public ThemePreferenceResolution Resolve(string defaultThemeKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultThemeKey);

        return new ThemePreferenceResolution(defaultThemeKey, false);
    }
}

internal sealed class HttpThemePreferenceStore(IHttpContextAccessor httpContextAccessor) : IThemePreferenceStore
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    public ThemePreferenceResolution Resolve(string defaultThemeKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultThemeKey);

        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return new ThemePreferenceResolution(defaultThemeKey, false);
        }

        string? requestedTheme = null;
        var fromQuery = false;

        if (httpContext.Request.Query.TryGetValue(ThemeState.CookieName, out var queryValue) &&
            !string.IsNullOrWhiteSpace(queryValue))
        {
            requestedTheme = queryValue.ToString();
            fromQuery = true;
        }
        else if (httpContext.Request.Cookies.TryGetValue(ThemeState.CookieName, out var storedTheme) &&
                 !string.IsNullOrWhiteSpace(storedTheme))
        {
            requestedTheme = storedTheme;
        }

        if (string.IsNullOrWhiteSpace(requestedTheme))
        {
            return new ThemePreferenceResolution(defaultThemeKey, false);
        }

        var resolvedTheme = requestedTheme.Trim();

        if (fromQuery && !httpContext.Response.HasStarted)
        {
            httpContext.Response.Cookies.Append(
                ThemeState.CookieName,
                resolvedTheme,
                new CookieOptions
                {
                    Path = "/",
                    MaxAge = TimeSpan.FromDays(365)
                });
        }

        return new ThemePreferenceResolution(resolvedTheme, true);
    }
}
