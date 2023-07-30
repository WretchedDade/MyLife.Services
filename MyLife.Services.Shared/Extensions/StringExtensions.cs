using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLife.Services.Shared.Extensions;
public static class StringExtensions
{
    public static bool HasValue([NotNullWhen(true)]this string? value)
    {
        return !string.IsNullOrEmpty(value);
    }
}
