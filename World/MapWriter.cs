using System.Collections.Generic;
using System.IO;
using ArchivalTibiaV71MapEditor.Sprites;

namespace ArchivalTibiaV71MapEditor.World
{
    public class MapWriter
    {
        private BinaryWriter _writer;

        public MapWriter(Stream stream)
        {
            _writer = new BinaryWriter(stream);
        }

        public void Write(Tile tile)
        {
            var flags = tile.GetFlags();
            _writer.Write(tile.Position.X);
            _writer.Write(tile.Position.Y);
            _writer.Write((byte)((TileFlags)tile.Position.Z | flags)); // everything up to 15 is Z, rest is tile flags
            _writer.Write(tile.Ground.Item.ItemId);
            if (flags.HasFlag(TileFlags.HasItems))
            {
                WriteItems(tile.OnTop);
            }
        }

        private void WriteItems(List<Sprite> items)
        {
            _writer.Write((byte) items.Count); // item count
            for (var a = 0; a < items.Count; a++)
            {
                _writer.Write(items[a].Item.ItemId);
            }
        }
    }
}