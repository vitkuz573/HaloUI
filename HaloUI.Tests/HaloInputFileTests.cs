// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using HaloUI.Components;
using HaloUI.Enums;
using Microsoft.AspNetCore.Components.Forms;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloInputFileTests : HaloBunitContext
{
    [Fact]
    public void RendersLabelDescriptionAndDefaultSummary()
    {
        var cut = Render<HaloInputFile>(parameters => parameters
            .Add(p => p.Label, "Avatar")
            .Add(p => p.Description, "PNG, JPG, WEBP")
            .Add(p => p.Placeholder, "No files uploaded"));

        var wrapper = cut.Find("div.halo-inputfile");
        Assert.Contains("halo-inputfile--size-md", wrapper.ClassList);
        Assert.Contains("halo-inputfile--empty", wrapper.ClassList);

        var input = cut.Find("input[type='file']");
        Assert.Equal("file", input.GetAttribute("type"));

        var trigger = cut.Find("label.halo-inputfile__trigger");
        Assert.Equal("Choose file", trigger.TextContent.Trim());

        var summary = cut.Find(".halo-inputfile__summary");
        Assert.Equal("No files uploaded", summary.TextContent.Trim());

        var description = cut.Find(".halo-inputfile__description");
        Assert.Equal("PNG, JPG, WEBP", description.TextContent.Trim());

        var label = cut.Find("label.halo-label");
        Assert.NotNull(label);
    }

    [Fact]
    public void AppliesDisabledRequiredErrorAndLargeSizeState()
    {
        var cut = Render<HaloInputFile>(parameters => parameters
            .Add(p => p.Label, "Evidence")
            .Add(p => p.ButtonText, "Upload evidence")
            .Add(p => p.Disabled, true)
            .Add(p => p.Required, true)
            .Add(p => p.HasError, true)
            .Add(p => p.Size, InputFieldSize.Large));

        var wrapper = cut.Find("div.halo-inputfile");
        Assert.Contains("halo-inputfile--disabled", wrapper.ClassList);
        Assert.Contains("halo-inputfile--error", wrapper.ClassList);
        Assert.Contains("halo-inputfile--size-lg", wrapper.ClassList);

        var input = cut.Find("input[type='file']");
        Assert.Equal("disabled", input.GetAttribute("disabled"));
        Assert.Equal("required", input.GetAttribute("required"));
        Assert.Equal("true", input.GetAttribute("aria-required"));
        Assert.Equal("true", input.GetAttribute("aria-invalid"));

        var trigger = cut.Find("label.halo-inputfile__trigger");
        Assert.Equal("true", trigger.GetAttribute("aria-disabled"));
    }

    [Fact]
    public void UploadFilesUpdatesSummaryListAndInvokesCallbacks()
    {
        var onChangeInvoked = false;
        HaloInputFileChangedEventArgs? changedEvent = null;

        var cut = Render<HaloInputFile>(parameters => parameters
            .Add(p => p.Multiple, true)
            .Add(p => p.ShowSelectedFiles, true)
            .Add(p => p.SelectionChanged, _ => onChangeInvoked = true)
            .Add(p => p.FilesChanged, args => changedEvent = args));

        var inputFile = cut.FindComponent<InputFile>();

        inputFile.UploadFiles(
            InputFileContent.CreateFromText("first", "alpha.txt", contentType: "text/plain"),
            InputFileContent.CreateFromText("second", "beta.log", contentType: "text/plain"));

        var summary = cut.Find(".halo-inputfile__summary");
        Assert.Equal("2 files selected", summary.TextContent.Trim());

        var items = cut.FindAll(".halo-inputfile__item");
        Assert.Equal(2, items.Count);
        Assert.Contains(items, item => item.TextContent.Contains("alpha.txt", StringComparison.Ordinal));
        Assert.Contains(items, item => item.TextContent.Contains("beta.log", StringComparison.Ordinal));

        Assert.True(onChangeInvoked);
        Assert.NotNull(changedEvent);
        Assert.Equal(2, changedEvent!.FileCount);
    }

    [Fact]
    public void ClearButtonClearsSelectedFilesAndRestoresPlaceholder()
    {
        var cut = Render<HaloInputFile>(parameters => parameters
            .Add(p => p.ShowClearButton, true)
            .Add(p => p.Placeholder, "Nothing selected"));

        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromText("avatar", "avatar.png", contentType: "image/png"));

        var clearButton = cut.Find("button.halo-inputfile__clear");
        clearButton.Click();

        var summary = cut.Find(".halo-inputfile__summary");
        Assert.Equal("Nothing selected", summary.TextContent.Trim());
        Assert.Empty(cut.FindAll(".halo-inputfile__item"));
    }
}
