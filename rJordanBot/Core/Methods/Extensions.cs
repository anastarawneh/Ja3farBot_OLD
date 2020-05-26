using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace rJordanBot.Core.Methods
{
    public static class Extensions
    {
        public static string CapitalizeFirst(this string input)
        {
            string output;

            char first = input[0];
            output = char.ToUpper(first) + input.Substring(1);

            return output;
        }

        public static string GetRGB(this Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
