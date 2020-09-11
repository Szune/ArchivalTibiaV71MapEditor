using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Sprites;

namespace ArchivalTibiaV71MapEditor.Readers
{
    public enum DatCategories
    {
        None,
        NoCategory,
        Tiles,
        Decoration,
        Items,
    }

    public class DatItem
    {
        public DatItem(IEnumerable<string> category, DatCategories type, ushort itemId, ushort[] spriteIds, byte width,
            byte height,
            byte blendFrames,
            byte maxZIndex, byte horizontalParts, byte verticalParts, byte animationLength, ushort minimapColor)
        {
            Categories = category?.ToList() ?? throw new ArgumentNullException(nameof(category));
            Type = type;
            SpriteIds = spriteIds;
            Width = width;
            Height = height;
            BlendFrames = blendFrames;
            MaxZIndex = maxZIndex;
            HorizontalParts = horizontalParts;
            VerticalParts = verticalParts;
            AnimationLength = animationLength;
            MinimapColor = minimapColor;
            ItemId = itemId;
        }

        public List<string> Categories { get; }
        public DatCategories Type { get; }
        public ushort[] SpriteIds { get; }
        public ushort ItemId { get; }
        public byte Width { get; }
        public byte Height { get; }
        public byte BlendFrames { get; }

        /// <summary>
        /// How many sprites are drawn on top of each other
        /// </summary>
        public byte MaxZIndex { get; }

        /// <summary>
        /// Used when the client is connecting tiles
        /// </summary>
        public byte HorizontalParts { get; }

        /// <summary>
        /// Used when the client is connecting tiles
        /// </summary>
        public byte VerticalParts { get; }

        public byte AnimationLength { get; }
        public ushort MinimapColor { get; }

        public Sprite GetSprite()
        {
            return new Sprite(this);
        }
    }

    public class DatItemCollection
    {
        public readonly DatItem[] All;
        public int Length => All.Length;
        public readonly DatItem[] Tiles;
        public readonly DatItem[] Decoration;
        public readonly DatItem[] Items;

        public DatItemCollection(DatItem[] all, DatItem[] tiles, DatItem[] decoration, DatItem[] items)
        {
            All = all;
            Tiles = tiles;
            Decoration = decoration;
            Items = items;
        }

        public DatItem GetItem(uint id)
        {
            return All[id - 100];
        }
    }

    public class DatReader
    {
        private readonly List<Category> _categories;
        private readonly BinaryReader _reader;
        public uint Version { get; }
        public ushort ItemCount { get; }
        public ushort OutfitCount { get; }

        public DatReader(Stream input)
        {
            _categories = Categories.Items;
            _reader = new BinaryReader(input);
            Version = _reader.ReadUInt32();
            ItemCount = _reader.ReadUInt16();
            OutfitCount = _reader.ReadUInt16();
            _reader.ReadUInt16(); // effect count
            _reader.ReadUInt16(); // projectile count
        }

        public DatItemCollection ReadItems()
        {
            var all = new DatItem[ItemCount - 99];
            var tiles = new List<DatItem>();
            var decorations = new List<DatItem>();
            var items = new List<DatItem>();
            for (ushort i = 100; i <= ItemCount; i++)
            {
                var entry = ReadEntry(i);
                all[i - 100] = entry;
                switch (entry.Type)
                {
                    case DatCategories.Tiles:
                        tiles.Add(entry);
                        break;
                    case DatCategories.Decoration:
                        decorations.Add(entry);
                        break;
                    case DatCategories.Items:
                        items.Add(entry);
                        break;
                }
            }


            return new DatItemCollection(all, tiles.ToArray(), decorations.ToArray(), items.ToArray());
        }

        private DatItem ReadEntry(ushort id)
        {
            DatCategories type = DatCategories.NoCategory;
            ushort minimapColor = 0;
            while (true)
            {
                var flag = _reader.ReadByte();
                if (flag == 0xFF) break;
                switch (flag)
                {
                    case 0:
                        if (type == DatCategories.NoCategory)
                            type = DatCategories.Tiles;
                        _reader.ReadUInt16(); // tile speed
                        break;
                    case 1:
                        if (type == DatCategories.NoCategory)
                            type = DatCategories.Decoration;
                        break;
                    case 7:
                    case 8:
                        _reader.ReadUInt16(); // max text length
                        break;
                    case 0xF:
                        type = DatCategories.Items;
                        break;
                    case 0x10:
                        _reader.ReadUInt16(); // light level
                        _reader.ReadUInt16(); // light color
                        break;
                    case 0x13:
                        _reader.ReadUInt16(); // draw height
                        break;
                    case 0x15:
                        _reader.ReadUInt16(); // unused field in tibia 7.1 (maybe draw width)
                        break;
                    case 0x16:
                        minimapColor = _reader.ReadUInt16();
                        break;
                    case 0x1A:
                        _reader.ReadUInt16(); // action
                        break;
                }
            }

            var categories = _categories
                .Where(c => c.Contains(id))
                .Select(c => c.Name);

            //
            // if (id >= 1180 && id <= 1198)
            // {
            //     type = Categories.Fields;
            // }
            //
            // if (id >= 1582 && id <= 1609)
            // {
            //     type = Categories.Trash;
            // }
            //
            // if (id >= 1610 && id <= 1666)
            // {
            //     type = Categories.Runes;
            // }
            //
            // if (id >= 1805 && id <= 1830)
            // {
            //     type = Categories.Tools;
            // }
            //
            // if (id >= 1894 && id <= 1920)
            // {
            //     type = Categories.Food;
            // }
            //
            // if ((id >= 2018 && id <= 2188) || (id >= 2203 && id <= 2205))
            // {
            //     type = Categories.Corpses;
            // }
            //
            // if ((id >= 969 && id <= 1027) || (id >= 407 && id <= 450))
            // {
            //     type = Categories.Houses;
            // }

            var width = _reader.ReadByte(); // width
            var height = _reader.ReadByte(); // height
            byte blendFrames = 0x20;
            if (width >= 2 || height >= 2)
            {
                blendFrames = _reader.ReadByte(); // blend frames
            }

            var maxZIndex = _reader.ReadByte();
            var horizontalParts = _reader.ReadByte();
            var verticalParts = _reader.ReadByte();
            var animationLength = _reader.ReadByte();
            var spriteCount = width * height * maxZIndex * horizontalParts * verticalParts * animationLength;
            var spriteIds = new ushort[spriteCount];

            // with height == 2 and width == 2
            // sprites are stored [right bottom, left bottom, right top, left top]
            // so if you draw them in the order they are stored, they would be 
            //  _____________
            // | (RB)  (LB) |
            // | (RT)  (LT) |
            //  -----------
            // so.. let's not do that
            // instead, because we want to draw them top to bottom, left to right, we reverse the order

            for (int i = 0; i < spriteCount; i++)
            {
                spriteIds[spriteCount - 1 - i] = _reader.ReadUInt16();
            }

            return new DatItem(categories, type, id, spriteIds, width, height, blendFrames, maxZIndex, horizontalParts,
                verticalParts, animationLength, minimapColor);
        }
    }
}