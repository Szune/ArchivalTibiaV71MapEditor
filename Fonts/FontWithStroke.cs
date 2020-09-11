using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArchivalTibiaV71MapEditor.Fonts
{
    public class FontWithStroke : IFont
    {
        private Dictionary<char, Rectangle> _fontSheetPositions;
        private Dictionary<char, Vector2> _symbolOffsetTranslation;
        public Texture2D FontSheet { get; }
        public Point SymbolBoxSize { get; }
        public Point SymbolMaxSize { get; }

        public Vector2 GetOffset(char c)
        {
            return _symbolOffsetTranslation[c];
        }

        public Rectangle GetPosition(char c)
        {
            return _fontSheetPositions[c];
        }

        public Vector2 MeasureString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Vector2.Zero;
            float maxX = 0;
            float maxY = SymbolMaxSize.Y;
            float currentX = 0;
            for (int i = 0; i < text.Length; i++)
            {
                var nextSize = GetOffset(text[i]);
                if (nextSize.Y > float.Epsilon)
                {
                    maxX = Math.Max(currentX, maxX);
                    maxY += nextSize.Y;
                    currentX = 0;
                }
                else
                {
                    currentX += nextSize.X;
                    maxX = Math.Max(currentX, maxX);
                }
            }

            return new Vector2(maxX, maxY); // add Y if implementing overflow
        }

        public FontWithStroke(Texture2D fontSheet)
        {
            SymbolBoxSize = new Point(16,16);
            SymbolMaxSize = new Point(12,12);
            FontSheet = fontSheet;
            Setup();
        }

        private void Setup()
        {
            _symbolOffsetTranslation = new Dictionary<char, Vector2>
            {
                {' ', new Vector2(8, 0)},
                {'!', new Vector2(4, 0)},
                {'"', new Vector2(7, 0)},
                {'#', new Vector2(9, 0)},
                {'$', new Vector2(8, 0)},
                {'%', new Vector2(10, 0)},
                {'&', new Vector2(10, 0)},
                {'\'', new Vector2(4, 0)},
                {'(', new Vector2(6, 0)},
                {')', new Vector2(6, 0)},
                {'*', new Vector2(9, 0)},
                {'+', new Vector2(9, 0)},
                {',', new Vector2(4, 0)},
                {'-', new Vector2(9, 0)},
                {'.', new Vector2(4, 0)},
                {'/', new Vector2(8, 0)},

                {'0', new Vector2(8, 0)},
                {'1', new Vector2(6, 0)},
                {'2', new Vector2(8, 0)},
                {'3', new Vector2(8, 0)},
                {'4', new Vector2(8, 0)},
                {'5', new Vector2(8, 0)},
                {'6', new Vector2(8, 0)},
                {'7', new Vector2(8, 0)},
                {'8', new Vector2(8, 0)},
                {'9', new Vector2(8, 0)},
                {':', new Vector2(4, 0)},
                {';', new Vector2(4, 0)},
                
                {'?', new Vector2(7, 0)},

                {'A', new Vector2(9, 0)},
                {'B', new Vector2(8, 0)},
                {'C', new Vector2(8, 0)},
                {'D', new Vector2(9, 0)},
                {'E', new Vector2(8, 0)},
                {'F', new Vector2(8, 0)},
                {'G', new Vector2(9, 0)},
                {'H', new Vector2(9, 0)},
                {'I', new Vector2(6, 0)},
                {'J', new Vector2(7, 0)},
                {'K', new Vector2(8, 0)},
                {'L', new Vector2(8, 0)},
                {'M', new Vector2(10, 0)},
                {'N', new Vector2(9, 0)},
                {'O', new Vector2(9, 0)},
                {'P', new Vector2(8, 0)},
                {'Q', new Vector2(9, 0)},
                {'R', new Vector2(9, 0)},
                {'S', new Vector2(8, 0)},
                {'T', new Vector2(10, 0)},
                {'U', new Vector2(9, 0)},
                {'V', new Vector2(8, 0)},
                {'W', new Vector2(10, 0)},
                {'X', new Vector2(8, 0)},
                {'Y', new Vector2(8, 0)},
                {'Z', new Vector2(8, 0)},
                
                {'a', new Vector2(8, 0)},
                {'b', new Vector2(8, 0)},
                {'c', new Vector2(7, 0)},
                {'d', new Vector2(8, 0)},
                {'e', new Vector2(8, 0)},
                {'f', new Vector2(7, 0)},
                {'g', new Vector2(8, 0)},
                {'h', new Vector2(8, 0)},
                {'i', new Vector2(4, 0)},
                {'j', new Vector2(6, 0)},
                {'k', new Vector2(8, 0)},
                {'l', new Vector2(4, 0)},
                {'m', new Vector2(10, 0)},
                {'n', new Vector2(8, 0)},
                {'o', new Vector2(8, 0)},
                {'p', new Vector2(8, 0)},
                {'q', new Vector2(8, 0)},
                {'r', new Vector2(7, 0)},
                {'s', new Vector2(7, 0)},
                {'t', new Vector2(7, 0)},
                {'u', new Vector2(8, 0)},
                {'v', new Vector2(8, 0)},
                {'w', new Vector2(10, 0)},
                {'x', new Vector2(8, 0)},
                {'y', new Vector2(8, 0)},
                {'z', new Vector2(7, 0)},
                {'{', new Vector2(8, 0)},
                {'|', new Vector2(4, 0)},
                {'}', new Vector2(8, 0)},
                
                {'\n', new Vector2(0, SymbolMaxSize.Y)},
                {'\t', new Vector2(32, 0)},
            };
            
            
            _fontSheetPositions = new Dictionary<char, Rectangle>();
            var columns = FontSheet.Width / SymbolBoxSize.X;
            for (int i = 32; i < 127; i++)
            {
                var boxX = ((i - 32) % columns) * SymbolBoxSize.X;
                var boxY = ((i - 32) / columns) * SymbolBoxSize.Y;
                _fontSheetPositions[(char)i] = new Rectangle(boxX, boxY, SymbolBoxSize.X, SymbolBoxSize.Y);
            }

            _fontSheetPositions['\n'] = _fontSheetPositions[' '];
            _fontSheetPositions['\t'] = _fontSheetPositions[' '];
        }
    }
}