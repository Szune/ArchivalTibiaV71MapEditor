using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArchivalTibiaV71MapEditor.Fonts
{
    public class FontRenderer
    {
        public void DrawCachedString(SpriteBatch sb, CachedString s, Vector2 position, Color color)
        {
            for (int i = 0; i < s.Chars.Length; i++)
            {
                sb.Draw(s.FontSheet, position + s.Chars[i].Offset,
                    s.Chars[i].SheetPosition, color);
            }
        }
        
        public void DrawCachedString(SpriteBatch sb, CachedString s, Vector2 position)
        {
            DrawCachedString(sb, s, position, Color.White);
        }

        public void DrawString(IFont font, SpriteBatch sb, string str, Vector2 position, Color color)
        {
            float xOffset = 0;
            float yOffset = 0;
            for (int i = 0; i < str.Length; i++)
            {
                
                sb.Draw(font.FontSheet, position + (Vector2.UnitX * xOffset) + (Vector2.UnitY * yOffset),
                    font.GetPosition(str[i]), color);
                var symbolOffset = font.GetOffset(str[i]);
                if (symbolOffset.Y > float.Epsilon)
                {
                    yOffset += symbolOffset.Y;
                    xOffset = 0;
                }
                else
                {
                    xOffset += symbolOffset.X;
                }
            }
        }

        public void DrawString(IFont font, SpriteBatch sb, string str, Vector2 position)
        {
            DrawString(font, sb, str, position, Color.White);
        }
    }
}