using System.Collections.Generic;
using System.IO;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Readers;
using ArchivalTibiaV71MapEditor.Sprites;

namespace ArchivalTibiaV71MapEditor.World
{
    public class MapReader
    {
        private BinaryReader _reader;

        public MapReader(Stream stream)
        {
            _reader = new BinaryReader(stream);
        }

        public (Tile t, bool done, string err) Read()
        {
            if (_reader.BaseStream.Position >= _reader.BaseStream.Length)
                return (null, true, null);
            var x = _reader.ReadUInt16();
            var y = _reader.ReadUInt16();
            var flags = (TileFlags)_reader.ReadByte();
            var z = (byte)(flags & (TileFlags)15); // Z goes 0 to 15
            var itemId = _reader.ReadUInt16();
            var items = new List<Sprite>(3);
            if (flags.HasFlag(TileFlags.HasItems))
            {
                var count = _reader.ReadByte();
                for (int a = 0; a < count; a++)
                {
                    items.Add(GameCollections.Items.GetItem(_reader.ReadUInt16()).GetSprite());
                }
            }
            var item = GameCollections.Items.GetItem(itemId);
            string err = null;
            for (var i = items.Count - 1; i > -1; i--)
            {
                if (items[i].Item.Type != DatCategories.Tiles) continue;
                if (err == null)
                    err = $"Removed item at ({x},{y},{z}) from zindex {i+1} with item id {items[i].Item.ItemId} because it is a ground tile id.";
                else
                    err = $"\nRemoved item at ({x},{y},{z}) from zindex {i+1} with item id {items[i].Item.ItemId} because it is a ground tile id.";
                items.RemoveAt(i);
            }
            if (item.Type != DatCategories.Tiles)
                return (null, false, $"Removed ground tile at ({x},{y},{z}) with item id {itemId} because it is not a ground tile id.");
            if (x > Controls.Map.MaxX || y > Controls.Map.MaxY || z > Controls.Map.MaxZ)
                return (null, false, $"Removed tile with item id {itemId} because it was out of bounds ({x},{y},{z}).");
            return (new Tile(item.GetSprite(), new Position(x, y, z), items), false, err);
        }
    }
}