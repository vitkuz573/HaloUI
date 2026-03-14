// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using HaloUI.Enums;

namespace HaloUI.Components;

public partial class HaloTextField
{
    [Parameter]
    public RenderFragment? StartAdornment { get; set; }
    
    [Parameter]
    public RenderFragment? EndAdornment { get; set; }
    
    [Parameter]
    public string? Class { get; set; }
    
    [Parameter]
    public string? Placeholder { get; set; }
    
    [Parameter]
    public TextFieldType Type { get; set; } = TextFieldType.Text;
    
    [Parameter]
    public TextFieldInputMode? InputMode { get; set; }
    
    [Parameter]
    public TextFieldAutocomplete? Autocomplete { get; set; }
    
    [Parameter]
    public bool? Spellcheck { get; set; }
    
    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public InputFieldSize Size { get; set; } = InputFieldSize.Medium;
    
    [Parameter]
    public TextFieldUpdateBehavior UpdateBehavior { get; set; } = TextFieldUpdateBehavior.OnChange;
    
    [Parameter]
    public string? AriaLabel { get; set; }
    
    [Parameter]
    public string? AriaLabelledBy { get; set; }
    
    [Parameter]
    public string? AriaDescribedBy { get; set; }
    
    [Parameter]
    public bool ReadOnly { get; set; }
    
    [Parameter]
    public bool HasError { get; set; }
    
    [Parameter]
    public EventCallback<string> InputChanged { get; set; }

    private bool IsInvalid => HasError || (EditContext is not null && EditContext.GetValidationMessages(FieldIdentifier).Any());

    protected override bool TryParseValueFromString(string? value, out string result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value ?? string.Empty;
        validationErrorMessage = null;

        return true;
    }

    private string BuildWrapperClass()
    {
        var classes = new List<string>
        {
            "halo-textfield",
            GetSizeClass(Size)
        };

        if (IsInvalid)
        {
            classes.Add("halo-textfield--error");
        }

        if (Disabled)
        {
            classes.Add("halo-textfield--disabled");
        }

        if (ReadOnly)
        {
            classes.Add("halo-textfield--readonly");
        }

        if (StartAdornment is not null)
        {
            classes.Add("halo-textfield--adorned-start");
        }

        if (EndAdornment is not null)
        {
            classes.Add("halo-textfield--adorned-end");
        }

        return string.Join(' ', classes);
    }

    private static string GetSizeClass(InputFieldSize size)
    {
        return size switch
        {
            InputFieldSize.Small => "halo-textfield--size-sm",
            InputFieldSize.Large => "halo-textfield--size-lg",
            _ => "halo-textfield--size-md"
        };
    }

    private LabelVariant ResolveLabelVariant()
    {
        if (IsInvalid)
        {
            return LabelVariant.Danger;
        }

        if (Disabled || ReadOnly)
        {
            return LabelVariant.Disabled;
        }

        return LabelVariant.Primary;
    }

    private string BuildInputClass()
    {
        var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "halo-textfield__input"
        };

        if (Disabled)
        {
            classes.Add("halo-is-disabled");
        }

        if (ReadOnly)
        {
            classes.Add("halo-is-readonly");
        }

        if (IsInvalid)
        {
            classes.Add("halo-is-error");
        }

        if (StartAdornment is not null)
        {
            classes.Add("halo-textfield__input--adorned-start");
        }

        if (EndAdornment is not null)
        {
            classes.Add("halo-textfield__input--adorned-end");
        }

        AddClasses(Class);
        AddClasses(CssClass);

        return string.Join(' ', classes);

        void AddClasses(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            foreach (var token in value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                classes.Add(token);
            }
        }
    }

    private async Task HandleInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (UpdateBehavior == TextFieldUpdateBehavior.OnInput)
        {
            CurrentValueAsString = value;
        }

        if (InputChanged.HasDelegate)
        {
            await InputChanged.InvokeAsync(value);
        }
    }

    private async Task HandleChangeAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        CurrentValueAsString = value;

        if (UpdateBehavior != TextFieldUpdateBehavior.OnInput && InputChanged.HasDelegate)
        {
            await InputChanged.InvokeAsync(value);
        }
    }

    private Dictionary<string, object>? BuildInputAttributes()
    {
        var attributes = BuildBaseInputAttributes(builder =>
        {
            builder.WithInvalid(IsInvalid);
            builder.WithDisabled(Disabled);

            if (!string.IsNullOrWhiteSpace(AriaLabel))
            {
                builder.WithAria("label", AriaLabel);
            }

            foreach (var labelledBy in SplitIds(AriaLabelledBy))
            {
                builder.WithLabelledBy(labelledBy);
            }

            foreach (var describedBy in SplitIds(AriaDescribedBy))
            {
                builder.WithDescribedBy(describedBy);
            }

            if (ReadOnly)
            {
                builder.WithAria("readonly", "true");
            }
        }, "class", "value", "oninput", "onchange", "style");

        if (!attributes.ContainsKey("type"))
        {
            attributes["type"] = ResolveTypeAttribute();
        }

        if (!string.IsNullOrWhiteSpace(Placeholder) && !attributes.ContainsKey("placeholder"))
        {
            attributes["placeholder"] = Placeholder!;
        }

        if (InputMode.HasValue && !attributes.ContainsKey("inputmode"))
        {
            attributes["inputmode"] = GetInputModeValue(InputMode.Value);
        }

        if (Autocomplete.HasValue && !attributes.ContainsKey("autocomplete"))
        {
            attributes["autocomplete"] = GetAutocompleteValue(Autocomplete.Value);
        }

        if (Spellcheck.HasValue && !attributes.ContainsKey("spellcheck"))
        {
            attributes["spellcheck"] = Spellcheck.Value ? "true" : "false";
        }

        if (Disabled)
        {
            attributes["disabled"] = "disabled";
        }

        if (ReadOnly)
        {
            attributes["readonly"] = "readonly";
            attributes["aria-readonly"] = "true";
        }

        if (IsRequired)
        {
            attributes.TryAdd("required", "required");
        }

        if (IsRequired)
        {
            attributes.TryAdd("aria-required", "true");
        }

        return attributes.Count > 0 ? attributes : null;
    }

    private static IEnumerable<string> SplitIds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield break;
        }

        foreach (var token in value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return token;
        }
    }

    private string ResolveTypeAttribute()
    {
        return Type switch
        {
            TextFieldType.Email => "email",
            TextFieldType.Password => "password",
            TextFieldType.Search => "search",
            TextFieldType.Telephone => "tel",
            TextFieldType.Url => "url",
            TextFieldType.Number => "number",
            _ => "text"
        };
    }

    private static string GetInputModeValue(TextFieldInputMode value)
    {
        return value switch
        {
            TextFieldInputMode.None => "none",
            TextFieldInputMode.Text => "text",
            TextFieldInputMode.Decimal => "decimal",
            TextFieldInputMode.Numeric => "numeric",
            TextFieldInputMode.Tel => "tel",
            TextFieldInputMode.Search => "search",
            TextFieldInputMode.Email => "email",
            TextFieldInputMode.Url => "url",
            TextFieldInputMode.Latin => "latin",
            TextFieldInputMode.LatinName => "latin-name",
            TextFieldInputMode.LatinProse => "latin-prose",
            TextFieldInputMode.FullWidthLatin => "full-width-latin",
            TextFieldInputMode.Kana => "kana",
            TextFieldInputMode.Katakana => "katakana",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }

    private static string GetAutocompleteValue(TextFieldAutocomplete value)
    {
        return value switch
        {
            TextFieldAutocomplete.Off => "off",
            TextFieldAutocomplete.On => "on",
            TextFieldAutocomplete.Name => "name",
            TextFieldAutocomplete.HonorificPrefix => "honorific-prefix",
            TextFieldAutocomplete.GivenName => "given-name",
            TextFieldAutocomplete.AdditionalName => "additional-name",
            TextFieldAutocomplete.FamilyName => "family-name",
            TextFieldAutocomplete.Nickname => "nickname",
            TextFieldAutocomplete.Email => "email",
            TextFieldAutocomplete.Username => "username",
            TextFieldAutocomplete.NewPassword => "new-password",
            TextFieldAutocomplete.CurrentPassword => "current-password",
            TextFieldAutocomplete.OneTimeCode => "one-time-code",
            TextFieldAutocomplete.OrganizationTitle => "organization-title",
            TextFieldAutocomplete.Organization => "organization",
            TextFieldAutocomplete.StreetAddress => "street-address",
            TextFieldAutocomplete.AddressLine1 => "address-line1",
            TextFieldAutocomplete.AddressLine2 => "address-line2",
            TextFieldAutocomplete.AddressLine3 => "address-line3",
            TextFieldAutocomplete.AddressLevel1 => "address-level1",
            TextFieldAutocomplete.AddressLevel2 => "address-level2",
            TextFieldAutocomplete.AddressLevel3 => "address-level3",
            TextFieldAutocomplete.AddressLevel4 => "address-level4",
            TextFieldAutocomplete.Country => "country",
            TextFieldAutocomplete.CountryName => "country-name",
            TextFieldAutocomplete.PostalCode => "postal-code",
            TextFieldAutocomplete.CreditCardName => "cc-name",
            TextFieldAutocomplete.CreditCardGivenName => "cc-given-name",
            TextFieldAutocomplete.CreditCardAdditionalName => "cc-additional-name",
            TextFieldAutocomplete.CreditCardFamilyName => "cc-family-name",
            TextFieldAutocomplete.CreditCardNumber => "cc-number",
            TextFieldAutocomplete.CreditCardExpiration => "cc-exp",
            TextFieldAutocomplete.CreditCardExpirationMonth => "cc-exp-month",
            TextFieldAutocomplete.CreditCardExpirationYear => "cc-exp-year",
            TextFieldAutocomplete.CreditCardSecurityCode => "cc-csc",
            TextFieldAutocomplete.CreditCardType => "cc-type",
            TextFieldAutocomplete.TransactionCurrency => "transaction-currency",
            TextFieldAutocomplete.TransactionAmount => "transaction-amount",
            TextFieldAutocomplete.Language => "language",
            TextFieldAutocomplete.Birthday => "bday",
            TextFieldAutocomplete.BirthdayDay => "bday-day",
            TextFieldAutocomplete.BirthdayMonth => "bday-month",
            TextFieldAutocomplete.BirthdayYear => "bday-year",
            TextFieldAutocomplete.Sex => "sex",
            TextFieldAutocomplete.Url => "url",
            TextFieldAutocomplete.Photo => "photo",
            TextFieldAutocomplete.Telephone => "tel",
            TextFieldAutocomplete.TelephoneCountryCode => "tel-country-code",
            TextFieldAutocomplete.TelephoneNational => "tel-national",
            TextFieldAutocomplete.TelephoneAreaCode => "tel-area-code",
            TextFieldAutocomplete.TelephoneLocal => "tel-local",
            TextFieldAutocomplete.TelephoneExtension => "tel-extension",
            TextFieldAutocomplete.Impp => "impp",
            TextFieldAutocomplete.Search => "search",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
    }
}
