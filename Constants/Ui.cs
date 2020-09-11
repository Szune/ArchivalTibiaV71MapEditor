using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Controls;
using ArchivalTibiaV71MapEditor.Sprites;

namespace ArchivalTibiaV71MapEditor.Constants
{
    public static class Ui
    {
        public static Label StatusTextLabel;
        public static Sprite SelectedTile;
        public static Texture2D SpriteSheet;
        public const int CaptionHeight = 20;


        public static readonly Rectangle CaptionBackground = new Rectangle(114, 185, 96, 15);
        public static readonly Rectangle Background = new Rectangle(0, 0, 96, 96);

        public static class Button
        {
            public static readonly Rectangle SmallButtonNormal = new Rectangle(2, 138, 43, 20);
            public static readonly Rectangle SmallButtonPressed = new Rectangle(2, 158, 43, 20);
            public static readonly Rectangle LargeButtonNormal = new Rectangle(45, 138, 86, 20);
            public static readonly Rectangle LargeButtonPressed = new Rectangle(45, 158, 86, 20);
            public static readonly Rectangle CloseButtonNormal = new Rectangle(208, 146, 16, 16);
            public static readonly Rectangle CloseButtonPressed = new Rectangle(208, 162, 16, 16);
        }

        public static class Border
        {
            public static readonly Rectangle WindowTopLeft = new Rectangle(106, 183, 4, 4);
            public static readonly Rectangle WindowTopRight = new Rectangle(110, 183, 4, 4);
            public static readonly Rectangle WindowBottomLeft = new Rectangle(98, 193, 4, 4);
            public static readonly Rectangle WindowBottomRight = new Rectangle(102, 193, 4, 4);
            public static readonly Rectangle Left = new Rectangle(0, 97, 1, 96);
            public static readonly Rectangle Right = new Rectangle(256, 0, 1, 96);
            public static readonly Rectangle Top = new Rectangle(0, 96, 96, 1);
            public static readonly Rectangle Bottom = new Rectangle(210, 198, 96, 1);
            public static readonly Rectangle WindowMiddleTop = new Rectangle(2, 178, 96, 4);
            public static readonly Rectangle WindowMiddleLeft = new Rectangle(256, 0, 4, 96);
            public static readonly Rectangle WindowMiddleRight = new Rectangle(260, 0, 4, 96);
            public static readonly Rectangle WindowMiddleBottom = new Rectangle(2, 193, 96, 4);
        }

        public static class ScrollBar
        {
            public static readonly Rectangle ArrowNormalOverlay = new Rectangle(222, 122, 12, 12);
            public static readonly Rectangle ArrowPressedOverlay = new Rectangle(222, 134, 12, 12);
            public static readonly Rectangle UpArrow = new Rectangle(232, 64, 12, 12);
            public static readonly Rectangle DownArrow = new Rectangle(244, 64, 12, 12);
            public static readonly Rectangle LeftArrow = new Rectangle(232, 76, 12, 12);
            public static readonly Rectangle RightArrow = new Rectangle(244, 76, 12, 12);
            public static readonly Rectangle VerticalBackground = new Rectangle(264, 0, 12, 96);
            public static readonly Rectangle VerticalForegroundStart = new Rectangle(220, 64, 12, 6);
            public static readonly Rectangle VerticalForegroundMiddle = new Rectangle(278, 0, 12, 32);
            public static readonly Rectangle VerticalForegroundEnd = new Rectangle(220, 70, 12, 6);
            public static readonly Rectangle HorizontalBackground = new Rectangle(2, 199, 96, 12);
            public static readonly Rectangle HorizontalForegroundStart = new Rectangle(220, 64, 6, 12);
            public static readonly Rectangle HorizontalForegroundMiddle = new Rectangle(290, 0, 32, 12);
            public static readonly Rectangle HorizontalForegroundEnd = new Rectangle(226, 64, 6, 12);
        }
    }
}