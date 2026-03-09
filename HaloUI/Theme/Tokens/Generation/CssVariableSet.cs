// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Text;

namespace HaloUI.Theme.Tokens.Generation;

/// <summary>
/// Represents a resolved set of CSS variables generated for a theme snapshot.
/// </summary>
internal sealed class CssVariableSet
{
    private readonly Dictionary<string, string> _variables = new();

    public IReadOnlyDictionary<string, string> Variables => _variables;

    public void Set(string name, string value)
    {
        if (!name.StartsWith("--"))
        {
            name = $"--{name}";
        }

        _variables[name] = value;
    }

    public string ToCss(string scopeSelector = ":root")
    {
        var builder = new StringBuilder();
        builder.Append(scopeSelector);
        builder.Append('{');

        var first = true;
        
        foreach (var pair in _variables)
        {
            if (!first)
            {
                builder.Append(' ');
            }
            
            builder.Append(pair.Key);
            builder.Append(':');
            builder.Append(pair.Value);
            builder.Append(';');
            
            first = false;
        }

        builder.Append('}');
        
        return builder.ToString();
    }
}