using Microsoft.Xna.Framework;

namespace ArchivalTibiaV71MapEditor.Utilities
{
    public static class Parts
    {
        
        public static Rectangle[] CreateRectangular(Point fullSize, int startX, int startY, Point partSize)
        {
            return CreateRectangular(fullSize.X, fullSize.Y, startX, startY, partSize.X, partSize.Y);
        }

        public static Rectangle[] CreateRectangular(int fullWidth, int fullHeight, int startX, int startY,
            int width, int height)
        {
            var xRects = fullWidth / width;
            var yRects = fullHeight / height;
            if (xRects * width < fullWidth)
                xRects += 1;
            if (yRects * height < fullHeight)
                yRects += 1;
            var rects = new Rectangle[xRects * yRects];
            var rect = 0;
            for (int y = 0; y < yRects; y++)
            {
                for (int x = 0; x < xRects; x++)
                {
                    if (x == xRects - 1 && y == yRects - 1)
                    {
                        var lastWidth = fullWidth - (width * x);
                        var lastHeight = fullHeight - (height * y);
                        rects[rect] = new Rectangle(
                            startX + (width * x),
                            startY + (height * y),
                            lastWidth,
                            lastHeight);
                    }
                    else if (x == xRects - 1)
                    {
                        var lastWidth = fullWidth - (width * x);
                        rects[rect] = new Rectangle(
                            startX + (width * x),
                            startY + (height * y),
                            lastWidth,
                            height);
                    }
                    else if (y == yRects - 1)
                    {
                        var lastHeight = fullHeight - (height * y);
                        rects[rect] = new Rectangle(
                            startX + (width * x),
                            startY + (height * y),
                            width,
                            lastHeight);
                    }
                    else
                    {
                        rects[rect] = new Rectangle(
                            startX + (width * x),
                            startY + (height * y),
                            width,
                            height);
                    }

                    rect++;
                }
            }

            return rects;
        }

        public static Rectangle[] CreateVertical(int fullHeight, int partHeight, int x, int startY, int width)
        {
            var requiredRects = fullHeight / partHeight;
            if (requiredRects * partHeight < fullHeight)
                requiredRects += 1;
            var rects = new Rectangle[requiredRects];
            for (int i = 0; i < requiredRects; i++)
            {
                if (i == requiredRects - 1)
                {
                    var lastHeight = fullHeight - (partHeight * i);
                    rects[i] = new Rectangle(x,
                        startY + (partHeight * i),
                        width, lastHeight);
                }
                else
                {
                    rects[i] = new Rectangle(x,
                        startY + (partHeight * i),
                        width, partHeight);
                }
            }

            return rects;
        }

        public static Rectangle[] CreateHorizontal(int fullWidth, int partWidth, int y, int startX, int height)
        {
            var requiredRects = fullWidth / partWidth;
            if (requiredRects * partWidth < fullWidth)
                requiredRects += 1;
            var rects = new Rectangle[requiredRects];
            for (int i = 0; i < requiredRects; i++)
            {
                if (i == requiredRects - 1)
                {
                    var lastWidth = fullWidth - (partWidth * i);
                    rects[i] = new Rectangle(startX + (partWidth * i), y,
                        lastWidth, height);
                }
                else
                {
                    rects[i] = new Rectangle(
                        startX + (partWidth * i), y,
                        partWidth, height);
                }
            }

            return rects;
        }
    }
}