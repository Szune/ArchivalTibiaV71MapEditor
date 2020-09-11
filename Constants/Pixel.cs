using Microsoft.Xna.Framework.Graphics;

namespace ArchivalTibiaV71MapEditor.Constants
{
    public static class Pixel
    {
        public static void Setup(GraphicsDevice device)
        {
            ListBackground = new Texture2D(device, 1, 1);
            ListBackground.SetData(new byte[]{64,64,64,255}); // grey background
            Black = new Texture2D(device, 1, 1);
            Black.SetData(new byte[]{0,0,0,255});
            White = new Texture2D(device, 1, 1);
            White.SetData(new byte[]{255,255,255,255});
        }
        
        public static Texture2D Black;
        public static Texture2D White;
        public static Texture2D ListBackground;
    }
}