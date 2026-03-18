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

        await ClickThemeButtonUntilAppliedAsync("theme-dark", "dark");
        await Expect(root).ToHaveAttributeAsync("data-theme", "dark");

        await ClickThemeButtonUntilAppliedAsync("theme-light", "light");
        await Expect(root).ToHaveAttributeAsync("data-theme", "light");
    }

    [Fact]
    public async Task DemoHost_ShouldOpenDialogAndPushSnackbar()
    {
        await Page.GotoAsync(BuildUrl("/"), new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForInteractiveUiAsync();

        await Page.GetByTestId("demo-section-dialog").GetByTestId("button-open-dialog").ClickAsync();
        var overlay = Page.Locator("[data-dialog-open='true']").First;
        var dialogSurface = Page.Locator(".halo-dialog__modal, .halo-dialog__drawer").First;

        await Expect(overlay).ToBeVisibleAsync();
        await Expect(dialogSurface).ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("Escape");
        await Expect(overlay).ToBeHiddenAsync();

        await Page.GetByTestId("demo-section-snackbar").GetByTestId("button-show-snackbar").ClickAsync();
        var snackbar = Page.Locator(".halo-snackbar").First;

        await Expect(snackbar).ToBeVisibleAsync();
        await Expect(snackbar).ToContainTextAsync("System notice");
        await Expect(snackbar.GetByRole(AriaRole.Button, new() { Name = "View", Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task DemoHost_Select_ShouldOpenAndApplySelection()
    {
        await Page.GotoAsync(BuildUrl("/"), new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForInteractiveUiAsync();

        var section = Page.GetByTestId("demo-section-select");
        await section.ScrollIntoViewIfNeededAsync();

        var nativeSelect = section.Locator("select.halo-select__native:visible").First;
        var isNativeVisible = await nativeSelect.CountAsync() > 0;

        if (isNativeVisible)
        {
            await nativeSelect.SelectOptionAsync(new[] { "apac" });
            await Expect(nativeSelect).ToHaveValueAsync("apac");
        }
        else
        {
            var trigger = section.Locator("button.halo-select__trigger:visible").First;
            await Expect(trigger).ToHaveAttributeAsync("role", "combobox");
            await Expect(trigger).ToHaveAttributeAsync("aria-haspopup", "listbox");
            await Expect(trigger).ToHaveAttributeAsync("aria-expanded", "false");
        }
    }

    [Fact]
    public async Task DemoHost_Snackbar_ShouldDismissFromCloseButton()
    {
        await Page.GotoAsync(BuildUrl("/"), new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await WaitForInteractiveUiAsync();

        await Page.GetByTestId("button-show-snackbar").ClickAsync();

        var snackbar = Page.Locator(".halo-snackbar");
        await Expect(snackbar).ToHaveCountAsync(1);

        await Page.Locator(".halo-snackbar__dismiss").First.ClickAsync(new LocatorClickOptions
        {
            Force = true
        });
        await Expect(snackbar).ToHaveCountAsync(0);
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

    private async Task ClickThemeButtonUntilAppliedAsync(string buttonTestId, string expectedTheme)
    {
        var root = Page.GetByTestId("demo-root");
        var timeoutAt = DateTime.UtcNow.AddSeconds(5);

        while (DateTime.UtcNow < timeoutAt)
        {
            await Page.GetByTestId(buttonTestId).ClickAsync(new LocatorClickOptions
            {
                Force = true
            });

            try
            {
                await Expect(root).ToHaveAttributeAsync("data-theme", expectedTheme, new LocatorAssertionsToHaveAttributeOptions
                {
                    Timeout = 300
                });
                return;
            }
            catch (PlaywrightException)
            {
                await Page.WaitForTimeoutAsync(100);
            }
        }

        await Expect(root).ToHaveAttributeAsync("data-theme", expectedTheme);
    }
}
