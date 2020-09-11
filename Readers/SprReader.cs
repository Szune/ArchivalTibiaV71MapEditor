using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;

namespace ArchivalTibiaV71MapEditor.Readers
{
    public class SprSpriteSheet
    {
        public Texture2D SpriteSheet;
    }

    public class SprSpriteCollection
    {
        private readonly SprSpriteSheet[] _sheets;

        public SprSpriteCollection(SprSpriteSheet[] sheets)
        {
            _sheets = sheets;
        }

        public (Texture2D SpriteSheet, Rectangle Position) GetSprite(int spriteId)
        {
            var rightId = (spriteId - 1);
            var sheet = rightId / SpriteConst.PerSheet;
            var x = (rightId % SpriteConst.PerSheet) * SpriteConst.Width;

            return (_sheets[sheet].SpriteSheet, new Rectangle(x, 0, SpriteConst.Width, SpriteConst.Height));
        }
    }
    public class SprReader
    {
        // Read all sprites into one huge sprite sheet
        private readonly BinaryReader _reader;
        public uint Version { get; }
        public ushort SpriteCount { get; }

        public SprReader(Stream input)
        {
            _reader = new BinaryReader(input);
            Version = _reader.ReadUInt32();
            SpriteCount = _reader.ReadUInt16();
        }

        public SprSpriteCollection ReadToSpriteSheets(GraphicsDevice device)
        {
            var bytePositions = new uint[SpriteCount];
            for (var i = 0; i < SpriteCount; i++)
            {
                bytePositions[i] = _reader.ReadUInt32();
            }

            // this is a single row sprite sheet
            // first sprite has id 2, so to get the position of the sprite,
            // just do (id - 2) * Constants.SpriteWidth
            var sheetCount = SpriteCount / SpriteConst.PerSheet;
            if (sheetCount * SpriteConst.PerSheet < SpriteCount)
                sheetCount += 1;

            var sheets = new SprSpriteSheet[sheetCount];
            for (int s = 0; s < sheetCount; s++)
            {
                if (s == sheetCount - 1)
                {
                    var endId = SpriteCount;
                    var count = SpriteCount - (SpriteConst.PerSheet * s) - 1;
                    sheets[s] = new SprSpriteSheet
                    {
                         SpriteSheet = ReadToSpriteSheetInner(s * SpriteConst.PerSheet, count)
                    };
                }
                else
                {
                    sheets[s] = new SprSpriteSheet
                    {
                        SpriteSheet = ReadToSpriteSheetInner(s * SpriteConst.PerSheet, SpriteConst.PerSheet)
                    };
                }
            }
            
            // var inLow = SpriteCount / 2;
            // var inHigh = SpriteCount - inLow;
            // var sheetLow = ReadToSpriteSheetInner(0, inLow);
            // var sheetHigh = ReadToSpriteSheetInner(inLow, inHigh);

            Texture2D ReadToSpriteSheetInner(int start, int count)
            {
                // var sqrt = (int)Math.Sqrt(count * Constants.SpriteWidth * Constants.SpriteHeight);
                // var fullWidth = sqrt * Constants.SpriteWidth;
                // var fullHeight = sqrt * Constants.SpriteHeight;
                var fullWidth = count * SpriteConst.Width;
                var fullHeight = SpriteConst.Height;
                var sheet = new Texture2D(device, fullWidth, fullHeight);
                int[] pixels = new int[fullWidth * fullHeight];

                for (var i = 0; i < count; i++)
                {
                    var pos = bytePositions[start + i];
                    if (pos < 1) continue;
                    // var transparentR = _reader.ReadByte(); // in case we want to draw the transparent colors as well
                    // var transparentG = _reader.ReadByte();
                    // var transparentB = _reader.ReadByte();

                    // var boxX = (i % sqrt) * Constants.SpriteWidth;
                    // var boxY = (i / sqrt) * Constants.SpriteHeight;
                    // var boxXOffset = boxX;
                    // var boxYOffset = boxY * fullWidth;
                    var boxXOffset = i * SpriteConst.Width;
                    _reader.BaseStream.Seek(pos + 3, SeekOrigin.Begin);
                    var spriteEnd = _reader.BaseStream.Position + _reader.ReadUInt16();
                    var cPixel = 0;
                    while (_reader.BaseStream.Position < spriteEnd)
                    {
                        var transparentPixels = _reader.ReadUInt16();
                        var colorfulPixels = _reader.ReadUInt16();
                        cPixel += transparentPixels;


                        for (var p = 0; p < colorfulPixels; p++)
                        {
                            var x = cPixel % SpriteConst.Width;
                            var y = cPixel / SpriteConst.Height;
                            var xOffset = x;
                            var yOffset = y * fullWidth;
                            // go to the specific scanline and offset into it for the current pixel
                            var index = boxXOffset + xOffset + yOffset;
                            cPixel += 1;

                            byte red = _reader.ReadByte(), green = _reader.ReadByte(), blue = _reader.ReadByte();
                            pixels[index] = red + (green << 8) + (blue << 16) + (255 << 24);
                        }
                    }
                }

                sheet.SetData(pixels);
                return sheet;
            }

            _reader.Dispose();
            return new SprSpriteCollection(sheets);
        }
    }
}