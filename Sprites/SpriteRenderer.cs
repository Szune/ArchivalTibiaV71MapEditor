using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArchivalTibiaV71MapEditor.Sprites
{
    public class SpriteRenderer
    {
        public void Draw(SpriteBatch sb, SpritePart[] sprite, Color color)
        {
            for (int i = 0; i < sprite.Length; i++)
            {
                sb.Draw(sprite[i].SpriteSheet, sprite[i].DrawLocation, sprite[i].SpritePosition, color);
            }
        }
    }
}