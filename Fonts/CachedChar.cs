using Microsoft.Xna.Framework;

namespace ArchivalTibiaV71MapEditor.Fonts
{
    public class CachedChar
    {
        public Vector2 Offset;
        public Rectangle SheetPosition;

        public CachedChar(Vector2 offset, Rectangle sheetPosition)
        {
            Offset = offset;
            SheetPosition = sheetPosition;
        }
    }
}