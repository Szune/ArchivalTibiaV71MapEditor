using System.Collections.Generic;
using ArchivalTibiaV71MapEditor.Sprites;

namespace ArchivalTibiaV71MapEditor.World
{
    public class Tile
    {
        public Position Position { get; }
        public Sprite Ground { get; set; }
        public List<Sprite> OnTop { get; } = new List<Sprite>(3);

        public Tile(Sprite ground, Position position)
        {
            Position = position;
            Ground = ground;
        }

        public Tile(Sprite ground, Position position, List<Sprite> items)
        {
            Ground = ground;
            Position = position;
            OnTop = items;
        }

        public void AddItem(Sprite item)
        {
            OnTop.Add(item);
        }

        public Sprite GetTopItem()
        {
            if (OnTop.Count != 0)
                return OnTop[^1];
            return Ground;
        }

        public TileFlags GetFlags()
        {
            var flags = TileFlags.None;
            if (OnTop.Count > 0)
                flags |= TileFlags.HasItems;
            return flags;
        }
    }
}