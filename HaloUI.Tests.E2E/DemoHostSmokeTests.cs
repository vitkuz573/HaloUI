// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace HaloUI.Tests.E2E;

[Collection(PlaywrightE2ECollection.Name)]
public sealed class DemoHostSmokeTests(PlaywrightEnvironmentFixture environmentFixture) : PageTest
{
    [Fact]
    public async Task DemoHost_ShouldRenderCoreSectionsAndAllowThemeSwitch()
    {
        await Page.GotoAsync(BuildUrl("/"), new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForInteractiveUiAsync();

        var root = Page.GetByTestId("demo-root");
        await Expect(root).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("demo-hero")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("demo-section-buttons")).ToBeVisibleAsync();
        await Expect(Page.GetByTestId("demo-section-table")).ToBeVisibleAsync();

        await Page.GetByTestId("theme-dark").ClickAsync();
        await Expect(root).ToHaveAttributeAsync("data-theme", "dark");

        await Page.GetByTestId("theme-light").ClickAsync();
        await Expect(root).ToHaveAttributeAsync("data-theme", "light");
    }

    [Fact]
    public async Task DemoHost_ShouldOpenDialogAndPushSnackbar()
    {
        await Page.GotoAsync(BuildUrl("/"), new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForInteractiveUiAsync();

        var dialogSection = Page.GetByTestId("demo-section-dialog");
        await dialogSection.ScrollIntoViewIfNeededAsync();
        await dialogSection.GetByTestId("button-open-dialog").ClickAsync();
        var overlay = Page.Locator("[data-dialog-open='true']").First;
        var dialogSurface = Page.Locator(".ui-dialog__modal, .ui-dialog__drawer").First;

        await Expect(overlay).ToBeVisibleAsync();
        await Expect(dialogSurface).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");
        await Expect(overlay).ToBeHiddenAsync();

        var snackbarSection = Page.GetByTestId("demo-section-snackbar");
        await snackbarSection.ScrollIntoViewIfNeededAsync();
        await snackbarSection.GetByTestId("button-show-snackbar").ClickAsync();
        var snackbar = Page.Locator(".ui-snackbar").First;

        await Expect(snackbar).ToBeVisibleAsync();
        await Expect(snackbar).ToContainTextAsync("System notice");
        await Expect(snackbar.GetByRole(AriaRole.Button, new() { Name = "View", Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task DemoHost_DarkThemeQuery_ShouldApplyDarkScheme()
    {
        await Page.GotoAsync(BuildUrl("/?theme=dark"), new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForInteractiveUiAsync();

        await Expect(Page.GetByTestId("demo-root")).ToHaveAttributeAsync("data-theme", "dark");
    }

    private string BuildUrl(string relativePath)
    {
        return $"{environmentFixture.BaseUrl}{relativePath}";
    }

    private async Task WaitForInteractiveUiAsync()
    {
        await Page.WaitForSelectorAsync("html[data-circuit-ready='true']", new PageWaitForSelectorOptions
        {
            Timeout = 60_000
        });
        await Expect(Page.GetByTestId("demo-root")).ToBeVisibleAsync();
    }
}
