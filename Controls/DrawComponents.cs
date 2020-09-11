using ArchivalTibiaV71MapEditor.Fonts;
using ArchivalTibiaV71MapEditor.Sprites;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class DrawComponents
    {
        public FontRenderer FontRenderer;
        public SpriteRenderer SpriteRenderer;
        public DrawComponents(FontRenderer fontRenderer, SpriteRenderer spriteRenderer)
        {
            FontRenderer = fontRenderer;
            SpriteRenderer = spriteRenderer;
        }
    }
}