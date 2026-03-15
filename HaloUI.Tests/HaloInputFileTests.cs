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
    public void InlineMode_RendersLabelAndSummary()
    {
        var cut = Render<HaloInputFile>(parameters => parameters
            .Add(p => p.Label, "Avatar")
            .Add(p => p.Description, "PNG, JPG")
            .Add(p => p.NoSelectionText, "No file yet"));

        var wrapper = cut.Find("div.halo-inputfile");
        Assert.Contains("halo-inputfile--size-md", wrapper.ClassList);
        Assert.Contains("halo-inputfile--mode-inline", wrapper.ClassList);
        Assert.Contains("halo-inputfile--empty", wrapper.ClassList);

        var trigger = cut.Find("label.halo-inputfile__trigger");
        Assert.Equal("Choose file", trigger.TextContent.Trim());

        var summary = cut.Find(".halo-inputfile__summary");
        Assert.Equal("No file yet", summary.TextContent.Trim());
    }

    [Fact]
    public void HiddenMode_DoesNotRenderTriggerOrSummary()
    {
        var cut = Render<HaloInputFile>(parameters => parameters
            .Add(p => p.Mode, HaloInputFileMode.Hidden));

        var wrapper = cut.Find("div.halo-inputfile");
        Assert.Contains("halo-inputfile--mode-hidden", wrapper.ClassList);

        Assert.NotEmpty(cut.FindAll("input[type='file']"));
        Assert.Empty(cut.FindAll("label.halo-inputfile__trigger"));
        Assert.Empty(cut.FindAll(".halo-inputfile__summary"));
    }

    [Fact]
    public void Validation_RejectsFilesBySizeAndExtension()
    {
        HaloInputFileChangeEventArgs? changed = null;

        var cut = Render<HaloInputFile>(parameters => parameters
            .Add(p => p.AllowMultiple, true)
            .Add(p => p.MaxFileSizeBytes, 1024)
            .Add(p => p.AllowedExtensions, new[] { ".log" })
            .Add(p => p.Changed, args => changed = args));

        cut.FindComponent<InputFile>().UploadFiles(
            InputFileContent.CreateFromBinary(new byte[10], "ok.log", contentType: "text/plain"),
            InputFileContent.CreateFromBinary(new byte[2048], "big.log", contentType: "text/plain"),
            InputFileContent.CreateFromBinary(new byte[20], "bad.txt", contentType: "text/plain"));

        Assert.NotNull(changed);
        Assert.Equal(1, changed!.FileCount);
        Assert.Equal(2, changed.Rejections.Count);
        Assert.Contains(changed.Rejections, r => r.Reason == HaloInputFileRejectionReason.FileTooLarge);
        Assert.Contains(changed.Rejections, r => r.Reason == HaloInputFileRejectionReason.InvalidExtension);
    }

    [Fact]
    public void ClearAsync_EmitsClearedEvent()
    {
        HaloInputFileChangeEventArgs? lastArgs = null;

        var cut = Render<HaloInputFile>(parameters => parameters
            .Add(p => p.ShowClearButton, true)
            .Add(p => p.Changed, args => lastArgs = args));

        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromText("avatar", "avatar.png", contentType: "image/png"));

        cut.Find("button.halo-inputfile__clear").Click();

        Assert.NotNull(lastArgs);
        Assert.True(lastArgs!.IsCleared);
        Assert.Equal(0, lastArgs.FileCount);
    }

    [Theory]
    [InlineData(InputFieldSize.Small, "halo-inputfile--size-sm")]
    [InlineData(InputFieldSize.Medium, "halo-inputfile--size-md")]
    [InlineData(InputFieldSize.Large, "halo-inputfile--size-lg")]
    public void Size_AddsExpectedClass(InputFieldSize size, string expectedClass)
    {
        var cut = Render<HaloInputFile>(parameters => parameters
            .Add(p => p.Size, size));

        var wrapper = cut.Find("div.halo-inputfile");
        Assert.Contains(expectedClass, wrapper.ClassList);
    }
}
