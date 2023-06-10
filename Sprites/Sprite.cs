using Microsoft.Xna.Framework;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Readers;

namespace ArchivalTibiaV71MapEditor.Sprites
{
    public class Sprite
    {
        public DatItem Item { get; }

        public Sprite(DatItem item)
        {
            Item = item;
        }

        /// <summary>
        /// Sprites are returned in top to bottom and left to right order, e.g. for a 2x2 part sprite:
        /// <para>[top left, top right, bottom left, bottom right]</para>
        /// </summary>
        public SpritePart[] GetParts(Vector2 drawLocation, Point renderSizePerTile)
        {
            var spriteCount = Item.Height * Item.Width * Item.MaxZIndex;
            if (Item is {Height: > 1, Width: > 1})
            {
                drawLocation -= new Vector2(
                    renderSizePerTile.X * (Item.Width - 1),
                    renderSizePerTile.Y * (Item.Height - 1)
                );
            }
            else if (Item.Width > 1)
            {
                drawLocation -= new Vector2(
                    renderSizePerTile.X * (Item.Width - 1),
                    0
                );
            }
            else if (Item.Height > 1)
            {
                drawLocation -= new Vector2(
                    0,
                    renderSizePerTile.Y * (Item.Height - 1)
                );
            }

            //var pixelOffset = Item.BlendFrames - Constants.SpriteWidth;
            var parts = new SpritePart[spriteCount];
            if (Item.MaxZIndex > 1)
            {
                GetMultiZIndexParts(drawLocation, spriteCount, renderSizePerTile, parts);
            }
            else
            {
                GetOneZIndexParts(drawLocation, spriteCount, renderSizePerTile, parts);
            }

            return parts;
        }

        private void GetMultiZIndexParts(Vector2 drawLocation, int spriteCount, Point renderSizePerSprite, SpritePart[] parts)
        {
            var itemsPerZIndex = Item.Width * Item.Height;
            var c = spriteCount - 1;
            var s = 0;
            for (int z = 0; z < Item.MaxZIndex; z++)
            {
                for (int i = 0; i < itemsPerZIndex; i++)
                {
                    var x = i % Item.Width;
                    var y = i / Item.Height;
                    var spriteId = Item.SpriteIds[s];
                    var sprite = GameCollections.Sprites.GetSprite(spriteId);
                    var partOffset = new Vector2(renderSizePerSprite.X * x, renderSizePerSprite.Y * y);
                    parts[c] = new SpritePart(partOffset, sprite.SpriteSheet, sprite.Position, renderSizePerSprite);
                    parts[c].SetDrawLocation(drawLocation);
                    c--;
                    s++;
                }
            }
        }

        private void GetOneZIndexParts(Vector2 drawLocation, int spriteCount, Point renderSizePerSprite, SpritePart[] parts)
        {
            for (int i = 0; i < spriteCount; i++)
            {
                var x = i % Item.Width;
                var y = i / Item.Height;
                var spriteId = Item.SpriteIds[i];
                var sprite = GameCollections.Sprites.GetSprite(spriteId);
                var partOffset = new Vector2(renderSizePerSprite.X * x, renderSizePerSprite.Y * y);
                parts[i] = new SpritePart(partOffset, sprite.SpriteSheet, sprite.Position, renderSizePerSprite); //renderSize);
                parts[i].SetDrawLocation(drawLocation);
            }
        }

        /// <summary>
        /// Sprites are returned in top to bottom and left to right order, e.g. for a 2x2 part sprite:
        /// <para>[top left, top right, bottom left, bottom right]</para>
        /// </summary>
        public SpritePart[] GetSingleTileParts(Vector2 drawLocation, Point renderSize)
        {
            var spriteCount = Item.Height * Item.Width * Item.MaxZIndex;
            //var pixelOffset = Item.BlendFrames - Constants.SpriteWidth;
            var renderSizePerSprite = new Point(renderSize.X / Item.Width, renderSize.Y / Item.Height);
            var parts = new SpritePart[spriteCount];
            if (Item.MaxZIndex > 1)
            {
                GetMultiZIndexSingleTileParts(drawLocation, spriteCount, renderSizePerSprite, parts);
            }
            else
            {
                GetOneZIndexSingleTileParts(drawLocation, spriteCount, renderSizePerSprite, parts);
            }

            return parts;
        }

        private void GetMultiZIndexSingleTileParts(Vector2 drawLocation, int spriteCount, Point renderSizePerSprite, SpritePart[] parts)
        {
            var itemsPerZIndex = Item.Width * Item.Height;
            var c = spriteCount - 1;
            var s = 0;
            for (int z = 0; z < Item.MaxZIndex; z++)
            {
                for (int i = 0; i < itemsPerZIndex; i++)
                {
                    var x = i % Item.Width;
                    var y = i / Item.Height;
                    var spriteId = Item.SpriteIds[s];
                    var sprite = GameCollections.Sprites.GetSprite(spriteId);
                    var partOffset = new Vector2(renderSizePerSprite.X * x, renderSizePerSprite.Y * y);
                    parts[c] = new SpritePart(partOffset, sprite.SpriteSheet, sprite.Position, renderSizePerSprite);
                    parts[c].SetDrawLocation(drawLocation);
                    c--;
                    s++;
                }
            }
        }

        private void GetOneZIndexSingleTileParts(Vector2 drawLocation, int spriteCount, Point renderSizePerSprite, SpritePart[] parts)
        {
            for (int i = 0; i < spriteCount; i++)
            {
                var x = i % Item.Width;
                var y = i / Item.Height;
                var spriteId = Item.SpriteIds[i];
                var sprite = GameCollections.Sprites.GetSprite(spriteId);
                var partOffset = new Vector2(renderSizePerSprite.X * x, renderSizePerSprite.Y * y);
                parts[i] = new SpritePart(partOffset, sprite.SpriteSheet, sprite.Position, renderSizePerSprite); //renderSize);
                parts[i].SetDrawLocation(drawLocation);
            }
        }
    }
}
