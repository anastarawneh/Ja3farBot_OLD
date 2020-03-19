using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace rJordanBot.Core.Methods
{
    public interface Constants
    {
        public interface Colors
        {
            public static readonly Color Blurple = new Color(66, 134, 244);
            public static readonly Color Red = new Color(221, 95, 83);
            public static readonly Color Green = new Color(83, 221, 172);
        }

        public interface Emojis
        {
            public static readonly Emoji Tick = new Emoji("✅");
        }
    }
}
