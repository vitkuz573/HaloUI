// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Iconography;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloIconPackResolverTests
{
    [Fact]
    public void Resolver_ResolvesManifestEntriesAndAliases()
    {
        var manifest = new HaloIconPackManifest
        {
            PackId = "test-pack",
            RenderMode = HaloIconRenderMode.Ligature,
            ProviderClass = "test-font",
            Icons =
            [
                new HaloIconPackEntry
                {
                    Name = "check"
                },
                new HaloIconPackEntry
                {
                    Name = "checkmark",
                    AliasOf = "check"
                },
                new HaloIconPackEntry
                {
                    Name = "close",
                    RenderMode = HaloIconRenderMode.CssClass,
                    Value = "icon-close"
                }
            ]
        };

        var resolver = new HaloIconPackResolver(manifest);

        Assert.True(resolver.TryResolve("check", out var check));
        Assert.Equal(HaloIconRenderMode.Ligature, check.RenderMode);
        Assert.Equal("check", check.Value);
        Assert.Equal("test-font", check.ProviderClass);

        Assert.True(resolver.TryResolve("checkmark", out var alias));
        Assert.Equal("checkmark", alias.Name);
        Assert.Equal("check", alias.Value);
        Assert.Equal(HaloIconRenderMode.Ligature, alias.RenderMode);

        Assert.True(resolver.TryResolve("close", out var close));
        Assert.Equal(HaloIconRenderMode.CssClass, close.RenderMode);
        Assert.Equal("icon-close", close.Value);
    }

    [Fact]
    public void Resolver_UsesFallbackWhenIconIsMissing()
    {
        var manifest = new HaloIconPackManifest
        {
            PackId = "test-pack",
            RenderMode = HaloIconRenderMode.Ligature,
            Icons = []
        };

        var resolver = new HaloIconPackResolver(manifest, new PassthroughHaloIconResolver("fallback-font"));

        Assert.True(resolver.TryResolve("sync", out var icon));
        Assert.Equal(HaloIconRenderMode.Ligature, icon.RenderMode);
        Assert.Equal("sync", icon.Value);
        Assert.Equal("fallback-font", icon.ProviderClass);
    }

    [Fact]
    public void Manifest_ParseAndSerialize_RoundTrips()
    {
        var source = new HaloIconPackManifest
        {
            PackId = "demo-pack",
            RenderMode = HaloIconRenderMode.Glyph,
            ProviderClass = "demo-font",
            Icons =
            [
                new HaloIconPackEntry
                {
                    Name = "alert",
                    Value = "\u26A0",
                    Codepoint = "26A0"
                }
            ]
        };

        var json = source.ToJson();
        var parsed = HaloIconPackManifest.Parse(json);

        Assert.Equal(source.PackId, parsed.PackId);
        Assert.Equal(source.RenderMode, parsed.RenderMode);
        Assert.Single(parsed.Icons);
        Assert.Equal("alert", parsed.Icons[0].Name);
        Assert.Equal("\u26A0", parsed.Icons[0].Value);
        Assert.Equal("26A0", parsed.Icons[0].Codepoint);
    }
}
