using System.Linq.Expressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using HaloUI.Components;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloPasswordFieldTests : HaloBunitContext
{
    [Fact]
    public void ToggleVisibilityButton_UpdatesAriaState()
    {
        var model = new PasswordModel
        {
            Value = "secret"
        };

        Expression<Func<string>> valueExpression = () => model.Value;
        EventCallback<string> valueChanged = EventCallback.Factory.Create<string>(this, value => model.Value = value);

#pragma warning disable CS8619
        var cut = Render<HaloPasswordField>(parameters => parameters
            .Add(p => p.Label, "Password")
            .Add(p => p.Value, model.Value)
            .Add(p => p.ValueExpression, valueExpression)
            .Add(p => p.ValueChanged, valueChanged));
#pragma warning restore CS8619

        var toggleButton = cut.Find("button[type='button']");
        Assert.Equal("Show password", toggleButton.GetAttribute("aria-label"));
        Assert.Null(toggleButton.GetAttribute("aria-pressed"));

        toggleButton.Click();

        toggleButton = cut.Find("button[type='button']");
        Assert.Equal("Hide password", toggleButton.GetAttribute("aria-label"));
        Assert.True(toggleButton.HasAttribute("aria-pressed"));
    }

    private sealed class PasswordModel
    {
        public string Value { get; set; } = string.Empty;
    }
}
