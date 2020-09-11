using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArchivalTibiaV71MapEditor.Fonts
{
    public class CachedString
    {
        private static CachedString _cachedString;

        public static CachedString Create(IFont font, string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return _cachedString ??= new CachedString(new CachedChar[0], Vector2.Zero, font.FontSheet);
            }

            float maxX = 0;
            float maxY = 0;
            float currentX = 0;
            var chars = new CachedChar[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                chars[i] = new CachedChar(new Vector2(currentX, maxY), font.GetPosition(s[i]));
                var symbolOffset = font.GetOffset(s[i]);
                if (symbolOffset.Y > float.Epsilon)
                {
                    maxX = Math.Max(currentX, maxX);
                    maxY += symbolOffset.Y;
                    currentX = 0;
                }
                else
                {
                    currentX += symbolOffset.X;
                    maxX = Math.Max(currentX, maxX);
                }
            }

            return new CachedString(chars, new Vector2(maxX, maxY + font.SymbolMaxSize.Y), font.FontSheet);
        }

        public Texture2D FontSheet;
        public CachedChar[] Chars;
        public Vector2 MeasuredSize;

        public CachedString(CachedChar[] chars, Vector2 measuredSize, Texture2D fontSheet)
        {
            FontSheet = fontSheet;
            Chars = chars;
            MeasuredSize = measuredSize;
        }
    }
}