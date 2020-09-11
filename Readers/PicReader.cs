using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;

namespace ArchivalTibiaV71MapEditor.Readers
{
    public class PicReader
    {
        private readonly BinaryReader _reader;
        public uint Version { get; }
        public ushort SpriteSheetCount { get; }

        public PicReader(Stream input)
        {
            _reader = new BinaryReader(input);
            Version = _reader.ReadUInt32();
            SpriteSheetCount = _reader.ReadUInt16();
        }

        public Texture2D[] ReadSpriteSheets(GraphicsDevice device)
        {
            var sheetInfo = ReadSpriteSheetInfo();
            var sheets = new Texture2D[SpriteSheetCount];
            for (int a = 0; a < sheetInfo.Length; a++)
            {
                var fullWidth = sheetInfo[a].Width * SpriteConst.Width;
                var fullHeight = sheetInfo[a].Height * SpriteConst.Height;

                int[] pixels = new int[fullWidth * fullHeight];
                // loop through all the parts of the sprite sheet
                for (int b = 0; b < sheetInfo[a].SpriteBytePositions.Count; b++)
                {
                    // go to the byte position in the file where the part starts
                    _reader.BaseStream.Seek(sheetInfo[a].SpriteBytePositions[b], SeekOrigin.Begin);
                    // calculate offsets (doesn't have to be done this way, it's just how I prefer to do it)
                    var boxX = (b % sheetInfo[a].Width) * SpriteConst.Width;
                    var boxY = (b / sheetInfo[a].Width) * SpriteConst.Height;
                    var boxXOffset = boxX;
                    var boxYOffset = boxY * fullWidth;
                    // go to the scanline of the sprite part where we are going to start assigning pixels
                    var endOfPic = _reader.BaseStream.Position + _reader.ReadUInt16();
                    var cPixel = 0;
                    while (_reader.BaseStream.Position < endOfPic)
                    {
                        var transparentPixels = _reader.ReadUInt16();
                        var colorfulPixels = _reader.ReadUInt16();

                        cPixel += transparentPixels;
                        // instead of just skipping the transparent pixels, we could use the TransparentR/G/B
                        // and actually write the pixels out in the format the client expects
                        // that way, we could edit the .pic file
                        for (var p = 0; p < colorfulPixels; p++)
                        {
                            var x = cPixel % SpriteConst.Width;
                            var y = cPixel / SpriteConst.Height;
                            var xOffset = x;
                            var yOffset = y * fullWidth;
                            // go to the specific scanline and offset into it for the current pixel
                            var index = boxXOffset + boxYOffset + xOffset + yOffset;
                            cPixel += 1;

                            byte red = _reader.ReadByte(), green = _reader.ReadByte(), blue = _reader.ReadByte();
                            pixels[index] = red + (green << 8) + (blue << 16) + (255 << 24);
                        }
                    }
                }

                var texture = new Texture2D(device, fullWidth, fullHeight);
                texture.SetData(pixels);
                sheets[a] = texture;
            }
            
            _reader.Dispose();
            return sheets;
        }

        private SpriteSheet[] ReadSpriteSheetInfo()
        {
            var sheets = new SpriteSheet[SpriteSheetCount];
            for (int i = 0; i < SpriteSheetCount; i++)
            {
                var sheet = new SpriteSheet();
                sheet.Width = _reader.ReadByte();
                sheet.Height = _reader.ReadByte();
                sheet.TransparentR = _reader.ReadByte();
                sheet.TransparentG = _reader.ReadByte();
                sheet.TransparentB = _reader.ReadByte();
                sheet.SpriteCount = (ushort) (sheet.Width * sheet.Height);
                for (int p = 0; p < sheet.SpriteCount; p++)
                {
                    sheet.SpriteBytePositions.Add(_reader.ReadUInt32());
                }

                sheets[i] = sheet;
            }

            return sheets;
        }
    }

    public class SpriteSheet
    {
        public byte Width;
        public byte Height;
        public byte TransparentR;
        public byte TransparentG;
        public byte TransparentB;
        public ushort SpriteCount;
        public List<uint> SpriteBytePositions = new List<uint>();
    }
}