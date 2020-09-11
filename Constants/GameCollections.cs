using System.IO;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Readers;

namespace ArchivalTibiaV71MapEditor.Constants
{
    public static class GameCollections
    {
        public static SprSpriteCollection Sprites;
        public static DatItemCollection Items;

        public static void Load(GraphicsDevice device)
        {
            
            var sprReader = new SprReader(File.OpenRead("Tibia.spr"));
            Sprites = sprReader.ReadToSpriteSheets(device);
            var datReader = new DatReader(File.OpenRead("Tibia.dat"));
            Items = datReader.ReadItems();
        }
    }
}