using System.Text;

namespace HaloUI.ThemeSdk.Internal;

internal static class CssVariableNaming
{
    public static string ToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var builder = new StringBuilder(value.Length * 2);
        var lastWasSeparator = false;
        var lastWasDigit = false;

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (c == '_' || c == ' ')
            {
                if (!lastWasSeparator && builder.Length > 0)
                {
                    builder.Append('-');
                }

                lastWasSeparator = true;
                lastWasDigit = false;
                continue;
            }

            if (char.IsDigit(c))
            {
                if (builder.Length > 0 && !lastWasSeparator && !lastWasDigit)
                {
                    builder.Append('-');
                    lastWasSeparator = true;
                }

                builder.Append(c);
                lastWasSeparator = false;
                lastWasDigit = true;
                continue;
            }

            if (char.IsUpper(c))
            {
                var previous = i > 0 ? value[i - 1] : '\0';
                var nextIsLower = i + 1 < value.Length && char.IsLower(value[i + 1]);

                if (i > 0 && !lastWasSeparator && (char.IsLower(previous) || char.IsDigit(previous) || nextIsLower))
                {
                    builder.Append('-');
                }

                builder.Append(char.ToLowerInvariant(c));
                lastWasSeparator = false;
                lastWasDigit = false;
                continue;
            }

            builder.Append(c);
            lastWasSeparator = c == '-';
            lastWasDigit = false;
        }

        return builder.ToString();
    }
}