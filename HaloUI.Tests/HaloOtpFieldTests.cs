// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Bunit;
using HaloUI.Components;
using HaloUI.Enums;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloOtpFieldTests : HaloBunitContext
{
    [Fact]
    public void RendersSegmentedInputsAndOtpAttributes()
    {
        var model = new OtpModel();
        var cut = RenderOtpField(model, parameters => parameters
            .Add(p => p.Label, "Authenticator code")
            .Add(p => p.Length, 6));

        var inputs = cut.FindAll("input.halo-otpfield__segment");
        Assert.Equal(6, inputs.Count);

        Assert.Equal("numeric", inputs[0].GetAttribute("inputmode"));
        Assert.Equal("one-time-code", inputs[0].GetAttribute("autocomplete"));
        Assert.Equal("off", inputs[1].GetAttribute("autocomplete"));
        Assert.Equal("1", inputs[0].GetAttribute("maxlength"));
        Assert.Equal("[0-9]*", inputs[0].GetAttribute("pattern"));
    }

    [Fact]
    public void Input_DistributesDigitsAcrossSegments()
    {
        var model = new OtpModel();
        var cut = RenderOtpField(model, parameters => parameters
            .Add(p => p.Length, 6));

        var inputs = cut.FindAll("input.halo-otpfield__segment");
        inputs[0].Input("12a-3 4x56y");

        inputs = cut.FindAll("input.halo-otpfield__segment");
        Assert.Equal("1", inputs[0].GetAttribute("value"));
        Assert.Equal("2", inputs[1].GetAttribute("value"));
        Assert.Equal("3", inputs[2].GetAttribute("value"));
        Assert.Equal("4", inputs[3].GetAttribute("value"));
        Assert.Equal("5", inputs[4].GetAttribute("value"));
        Assert.Equal("6", inputs[5].GetAttribute("value"));
        Assert.Equal("123456", model.Value);
    }

    [Fact]
    public void ValueInput_ReceivesNormalizedValue()
    {
        var model = new OtpModel();
        string? capturedValue = null;

        var cut = RenderOtpField(model, parameters => parameters
            .Add(p => p.Length, 6)
            .Add(p => p.ValueInput, value => capturedValue = value));

        cut.FindAll("input.halo-otpfield__segment")[0].Input("a1 2b3-4");

        Assert.Equal("1234", capturedValue);
    }

    [Fact]
    public void KeyPress_NonDigit_DoesNotMutateValue()
    {
        var model = new OtpModel();
        var cut = RenderOtpField(model, parameters => parameters
            .Add(p => p.Length, 6));

        cut.FindAll("input.halo-otpfield__segment")[0].KeyPress("A");

        Assert.Equal(string.Empty, model.Value);
        Assert.Equal(string.Empty, cut.FindAll("input.halo-otpfield__segment")[0].GetAttribute("value"));
    }

    [Fact]
    public void KeyPress_Digit_UpdatesValue()
    {
        var model = new OtpModel();
        var cut = RenderOtpField(model, parameters => parameters
            .Add(p => p.Length, 6));

        cut.FindAll("input.halo-otpfield__segment")[0].KeyPress("1");

        Assert.Equal("1", model.Value);

        var inputs = cut.FindAll("input.halo-otpfield__segment");
        Assert.Equal("1", inputs[0].GetAttribute("value"));
        Assert.Equal(string.Empty, inputs[1].GetAttribute("value"));
    }

    [Fact]
    public void RequiredAndSize_AreApplied()
    {
        var model = new OtpModel();
        var cut = RenderOtpField(model, parameters => parameters
            .Add(p => p.Label, "Code")
            .Add(p => p.Required, true)
            .Add(p => p.Size, InputFieldSize.Small));

        var wrapper = cut.Find("div.halo-otpfield");
        Assert.Contains("halo-otpfield--size-sm", wrapper.ClassList);
        Assert.NotNull(cut.Find(".halo-label__indicator--required"));

        var inputs = cut.FindAll("input.halo-otpfield__segment");
        Assert.True(inputs[0].HasAttribute("required"));
    }

    [Fact]
    public void BackspaceOnEmptySegment_ClearsPreviousDigit()
    {
        var model = new OtpModel
        {
            Value = "12"
        };

        var cut = RenderOtpField(model, parameters => parameters
            .Add(p => p.Length, 6));

        var inputs = cut.FindAll("input.halo-otpfield__segment");
        inputs[2].KeyDown("Backspace");

        Assert.Equal("1", model.Value);
    }

    [Fact]
    public void LengthOutsideRange_Throws()
    {
        var model = new OtpModel();

        Assert.Throws<ArgumentOutOfRangeException>(() => RenderOtpField(model, parameters => parameters
            .Add(p => p.Length, 0)));
    }

    private IRenderedComponent<HaloOtpField> RenderOtpField(
        OtpModel model,
        Action<ComponentParameterCollectionBuilder<HaloOtpField>> configure)
    {
        Expression<Func<string>> valueExpression = () => model.Value;
        EventCallback<string> valueChanged = EventCallback.Factory.Create<string>(this, value => model.Value = value);

#pragma warning disable CS8619
        return Render<HaloOtpField>(parameters =>
        {
            parameters.Add(p => p.Value, model.Value)
                .Add(p => p.ValueExpression, valueExpression)
                .Add(p => p.ValueChanged, valueChanged);
            configure(parameters);
        });
#pragma warning restore CS8619
    }

    private sealed class OtpModel
    {
        public string Value { get; set; } = string.Empty;
    }
}
