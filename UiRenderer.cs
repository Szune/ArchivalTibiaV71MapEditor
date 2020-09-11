using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Extensions;

namespace ArchivalTibiaV71MapEditor
{
    public class UiRenderer
    {
        private readonly Texture2D _spriteSheet;
        private Rectangle _currentRect = Rectangle.Empty;
        private RenderTarget2D _cachedBackground;

        public UiRenderer()
        {
            _spriteSheet = Ui.SpriteSheet;
        }

        public void DrawBackground(SpriteBatch sb)
        {
            sb.Draw(_cachedBackground, _currentRect, Color.White);
        }

        public void PreRender(SpriteBatch sb, Rectangle window)
        {
            if (_currentRect.Size.Equals(window.Size))
                return;
            _currentRect = new Rectangle(Point.Zero, window.Size);
            _cachedBackground?.Dispose();
            _cachedBackground = new RenderTarget2D(sb.GraphicsDevice, window.Width, window.Height, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            sb.GraphicsDevice.SetRenderTarget(_cachedBackground);
            sb.UsualBegin();
            var columns = window.Width / Ui.Background.Width + 1;
            var rows = window.Height / Ui.Background.Height + 1;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    sb.Draw(_spriteSheet,
                        new Vector2(x * Ui.Background.Width, y * Ui.Background.Height),
                        Ui.Background, Color.White);
                }
            }

            sb.UsualEnd();
        }
    }
}