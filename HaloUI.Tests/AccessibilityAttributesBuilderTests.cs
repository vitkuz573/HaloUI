// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;
using System.Threading;
using HaloUI.Accessibility;
using HaloUI.Accessibility.Aria;
using HaloUI.Abstractions;
using HaloUI.Services;
using Xunit;

namespace HaloUI.Tests;

public class AccessibilityAttributesBuilderTests
{
    private static readonly TimeSpan DiagnosticsTimeout = TimeSpan.FromSeconds(5);

    [Fact]
    public void Build_ReturnsDefensiveCopy()
    {
        var builder = new AccessibilityAttributesBuilder()
            .WithLabelledBy("primary", "secondary");

        var firstSnapshot = Assert.IsType<Dictionary<string, object>>(builder.Build());
        firstSnapshot["aria-labelledby"] = "external";

        var secondSnapshot = builder.Build();

        Assert.Equal("primary secondary", secondSnapshot["aria-labelledby"]);
    }

    [Fact]
    public void WithAriaBooleans_RespectEmitFalse()
    {
        var builder = new AccessibilityAttributesBuilder()
            .WithInvalid(false)
            .WithPressed(false)
            .WithRequired(false)
            .WithDisabled(false);

        var attributes = builder.Build();

        Assert.Equal("false", attributes["aria-invalid"]);
        Assert.Equal("false", attributes["aria-pressed"]);
        Assert.False(attributes.ContainsKey("aria-required"));
        Assert.False(attributes.ContainsKey("aria-disabled"));
    }

    [Fact]
    public void WithLabelledBy_MaintainsOrderAndUniqueness()
    {
        var builder = new AccessibilityAttributesBuilder()
            .WithLabelledBy("label", "hint", "label")
            .WithLabelledBy("description")
            .WithAttribute("aria-labelledby", "override base")
            .WithLabelledBy("trailing");

        var attributes = builder.Build();

        Assert.Equal("override base trailing", attributes["aria-labelledby"]);
    }

    [Fact]
    public void WithRole_EnumOverload_EmitsLowercaseToken()
    {
        var builder = new AccessibilityAttributesBuilder()
            .WithRole(AriaRole.TreeGrid);

        var attributes = builder.Build();

        Assert.Equal("treegrid", attributes["role"]);
    }

    [Fact]
    public void WithRole_SwitchRequiresCheckedAttribute()
    {
        using var hub = new AriaDiagnosticsHub();

        var builder = new AccessibilityAttributesBuilder()
            .RequireCompliance()
            .WithRole(AriaRole.Switch);

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("aria-checked", exception.Message, StringComparison.OrdinalIgnoreCase);
        var published = WaitForDiagnosticsEvent(hub);
        Assert.Equal(AriaDiagnosticsSeverity.Error, published.Severity);
    }

    [Fact]
    public void WithRoleStrict_DisallowsUnsupportedAriaAttributes()
    {
        using var hub = new AriaDiagnosticsHub();

        var builder = new AccessibilityAttributesBuilder()
            .RequireCompliance()
            .WithRole(AriaRole.Switch, AriaRoleCompliance.Strict)
            .WithAttribute(AriaAttributes.Checked, AriaCheckedState.True)
            .WithAria("foo", "bar");

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("aria-foo", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WithoutRoleCompliance_AllowsCustomAttributes()
    {
        using var hub = new AriaDiagnosticsHub();

        var attributes = new AccessibilityAttributesBuilder()
            .WithRole(AriaRole.Switch)
            .WithoutRoleCompliance()
            .WithAria("foo", "bar")
            .Build();

        Assert.Equal("bar", attributes["aria-foo"]);
    }

    [Fact]
    public void Build_PublishesDiagnosticsEvent()
    {
        using var hub = new AriaDiagnosticsHub();

        var attributes = new AccessibilityAttributesBuilder()
            .WithRole(AriaRole.Button)
            .CaptureSuccessfulInspections()
            .WithAttribute(AriaAttributes.Label, "Submit")
            .Build();

        Assert.NotNull(attributes);
        var published = WaitForDiagnosticsEvent(hub);
        Assert.Equal(AriaDiagnosticsSeverity.Success, published.Severity);
        Assert.Equal("button", published.Role?.ToAttributeValue());
    }

    private static AriaDiagnosticsEvent WaitForDiagnosticsEvent(AriaDiagnosticsHub hub)
    {
        AriaDiagnosticsEvent? @event = null;

        var observed = SpinWait.SpinUntil(() =>
        {
            var snapshot = hub.GetRecentEvents(1);
            if (snapshot.Count > 0)
            {
                @event = snapshot[0];
                return true;
            }

            return false;
        }, DiagnosticsTimeout);

        Assert.True(observed, "Expected diagnostics event.");
        return @event!;
    }

    [Fact]
    public void WithAttribute_TypedBooleanHonorsRenderOnFalse()
    {
        var builder = new AccessibilityAttributesBuilder()
            .WithAttribute(AriaAttributes.Required, false)
            .WithAttribute(AriaAttributes.Disabled, true);

        var attributes = builder.Build();

        Assert.True(attributes.ContainsKey("aria-disabled"));
        Assert.False(attributes.ContainsKey("aria-required"));
    }

    [Fact]
    public void WithAttribute_TokenEnum_AddsDistinctOrderedValues()
    {
        var builder = new AccessibilityAttributesBuilder()
            .WithAttribute(AriaAttributes.DropEffect, AriaDropEffect.Move, AriaDropEffect.Copy, AriaDropEffect.Move)
            .WithAttribute(AriaAttributes.DropEffect, Array.Empty<AriaDropEffect>()); // clears existing tokens
        builder.WithAttribute(AriaAttributes.DropEffect, AriaDropEffect.Move, AriaDropEffect.Copy);

        var attributes = builder.Build();

        Assert.Equal("move copy", attributes["aria-dropeffect"]);
    }

    [Fact]
    public void WithAttribute_IntegerAndNumber_UseInvariantFormatting()
    {
        var builder = new AccessibilityAttributesBuilder()
            .WithAttribute(AriaAttributes.RowCount, 42)
            .WithAttribute(AriaAttributes.ValueNow, 0.75m);

        var attributes = builder.Build();

        Assert.Equal("42", attributes["aria-rowcount"]);
        Assert.Equal("0.75", attributes["aria-valuenow"]);
    }

    [Fact]
    public void WithAttribute_IdReferenceSingle_TrimsValue()
    {
        var builder = new AccessibilityAttributesBuilder()
            .WithAttribute(AriaAttributes.ErrorMessage, " message ");

        var attributes = builder.Build();

        Assert.Equal("message", attributes["aria-errormessage"]);
    }

    [Fact]
    public void WithAttribute_IdReferenceList_SanitisesAndRemovesWhenCleared()
    {
        var builder = new AccessibilityAttributesBuilder()
            .WithAttribute(AriaAttributes.DescribedBy, "summary", "details", " summary ")
            .WithAttribute(AriaAttributes.DescribedBy, Array.Empty<string?>());

        var attributes = builder.Build();

        Assert.False(attributes.ContainsKey("aria-describedby"));
    }

    [Fact]
    public void Merge_CombineTokenAttributesWithoutDuplicates()
    {
        var additional = new Dictionary<string, object>
        {
            ["aria-describedby"] = "summary details"
        };

        var accessibility = new Dictionary<string, object>
        {
            ["aria-describedby"] = "details footer"
        };

        var merged = AccessibilityAttributesBuilder.Merge(additional, accessibility);

        Assert.Equal("summary details footer", merged["aria-describedby"]);
    }

    [Fact]
    public void WithTokenAttribute_ThrowsForUnsupportedAttribute()
    {
        var builder = new AccessibilityAttributesBuilder();

        var exception = Assert.Throws<ArgumentException>(() => builder.WithTokenAttribute("class", "value"));
        Assert.Contains("tokenized attributes", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WithRole_ButtonWithoutAccessibleName_Throws()
    {
        var builder = new AccessibilityAttributesBuilder()
            .RequireCompliance()
            .WithRole(AriaRole.Button);

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("accessible name", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WithAccessibleNameFromContent_WhenSupported_SatisfiesRequirement()
    {
        var attributes = new AccessibilityAttributesBuilder()
            .RequireCompliance()
            .WithRole(AriaRole.Button)
            .WithAccessibleNameFromContent()
            .Build();

        Assert.Equal("button", attributes["role"]);
    }

    [Fact]
    public void WithAccessibleNameFromContent_WhenUnsupported_StillRequiresExplicitName()
    {
        var builder = new AccessibilityAttributesBuilder()
            .RequireCompliance()
            .WithRole(AriaRole.TextBox)
            .WithAccessibleNameFromContent();

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("accessible name", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WithAccessibleNameFromAdditionalAttributes_SatisfiesRequirement()
    {
        var additional = new Dictionary<string, object>
        {
            ["aria-label"] = "Submit form"
        };

        var attributes = new AccessibilityAttributesBuilder()
            .RequireCompliance()
            .WithRole(AriaRole.Button)
            .WithAccessibleNameFromAdditionalAttributes(additional)
            .Build();

        Assert.Equal("button", attributes["role"]);
    }

    [Fact]
    public void WithRole_NameProhibited_DisallowsAccessibleName()
    {
        var builder = new AccessibilityAttributesBuilder()
            .RequireCompliance()
            .WithRole(AriaRole.Presentation)
            .WithAttribute(AriaAttributes.Label, "Decorative element");

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("accessible name", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}