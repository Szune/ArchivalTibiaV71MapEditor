using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArchivalTibiaV71MapEditor.Sprites
{
    public class SpritePart
    {
        public Vector2 Offset { get; }
        public Texture2D SpriteSheet { get; }
        public Rectangle SpritePosition { get; }
        public Point RenderSize { get; }
        public Rectangle DrawLocation { get; private set; }

        public void SetDrawLocation(Vector2 drawLocation)
        {
            DrawLocation = new Rectangle((drawLocation + Offset).ToPoint(), RenderSize);
        }

        public SpritePart(Vector2 offset, Texture2D spriteSheet, Rectangle spritePosition, Point renderSize)
        {
            Offset = offset;
            SpriteSheet = spriteSheet;
            SpritePosition = spritePosition;
            RenderSize = renderSize;
            DrawLocation = Rectangle.Empty;
        }
    }
}