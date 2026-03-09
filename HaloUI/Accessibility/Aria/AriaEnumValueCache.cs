// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;

namespace HaloUI.Accessibility.Aria;

internal static class AriaEnumValueCache<TEnum> where TEnum : struct, Enum
{
    private static readonly ConcurrentDictionary<TEnum, string> Cache = new();

    public static string ToAttributeValue(TEnum value)
    {
        return Cache.GetOrAdd(value, static key =>
        {
            var member = typeof(TEnum).GetMember(key.ToString());

            if (member.Length <= 0)
            {
                return key.ToString().ToLowerInvariant();
            }

            var attribute = member[0].GetCustomAttribute<EnumMemberAttribute>();

            if (attribute is not null && !string.IsNullOrWhiteSpace(attribute.Value))
            {
                return attribute.Value;
            }

            return key.ToString().ToLowerInvariant();
        });
    }
}