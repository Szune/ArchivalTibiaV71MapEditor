using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls.Addons;
using ArchivalTibiaV71MapEditor.Extensions;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class Panel : ControlBase, IPanel
    {
        private Texture2D _spriteSheet;
        private List<IControl> _controls = new List<IControl>();

        private Vector2 _topLeft;
        private Vector2 _topRight;
        private Vector2 _bottomLeft;
        private Vector2 _bottomRight;
        private Rectangle _middleTop;
        private Rectangle _middleLeft;
        private Rectangle _middleRight;
        private Rectangle _middleBottom;
        private RenderTarget2D _cachedPanel;
        private Rectangle _rectDrawing;

        public Panel(IWindow window, IControl parent, Rectangle rect, int zIndex = 0,
            bool visible = true)
         : base(window, parent, visible)
        {
            Window = window;
            Parent = parent;
            _spriteSheet = Ui.SpriteSheet;
            SetRect(rect);
            _rectDrawing = new Rectangle(0, 0, rect.Width, rect.Height);
            ZIndex = zIndex;
            BorderSize = 4;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime, DrawComponents drawComponents)
        {
            if (!Visible)
                return;
            if(IsDirty)
                Recalculate();
            //sb.Draw(_cachedPanel, CleanRect, Color.White);
            for (int i = 0; i < _controls.Count; i++)
            {
                _controls[i].Draw(sb, gameTime, drawComponents);
            }
            
            sb.Draw(_spriteSheet, _middleTop, Ui.Border.WindowMiddleTop, Color.White);
            sb.Draw(_spriteSheet, _middleLeft, Ui.Border.WindowMiddleLeft, Color.White);
            sb.Draw(_spriteSheet, _middleRight, Ui.Border.WindowMiddleRight, Color.White);
            sb.Draw(_spriteSheet, _middleBottom, Ui.Border.WindowMiddleBottom, Color.White);
            
            sb.Draw(_spriteSheet, _topLeft, Ui.Border.WindowTopLeft, Color.White);
            sb.Draw(_spriteSheet, _topRight, Ui.Border.WindowTopRight, Color.White);
            sb.Draw(_spriteSheet, _bottomLeft, Ui.Border.WindowBottomLeft, Color.White);
            sb.Draw(_spriteSheet, _bottomRight, Ui.Border.WindowBottomRight, Color.White);
        }

        public override HitBox HitTest()
        {
            if (!IsVisible())
                return HitBox.Miss;
            for (var i = _controls.Count - 1; i > -1; i--)
            {
                var test = _controls[i].HitTest();
                if (test.IsHit) 
                    return test;
            }
            return HitBox.Miss;
        }

        public override void Recalculate()
        {
            base.Recalculate();
            _controls.Sort(ZIndexComparer.Instance);
            for (int i = 0; i < _controls.Count; i++)
            {
                _controls[i].Dirty();
            }
            var left = CleanRect.Left;
            var top = CleanRect.Top;
            _topLeft = new Vector2(left, top);
            _topRight = new Vector2(left + Width - BorderSize, top);
            _bottomLeft = new Vector2(left, top + Height - BorderSize);
            _bottomRight = new Vector2(left + Width - BorderSize, top + Height - BorderSize);
            
            _middleTop = new Rectangle(left + BorderSize, top,  Width - (BorderSize * 2), BorderSize);
            _middleLeft = new Rectangle(left, top + BorderSize, BorderSize, Height - (BorderSize * 2));
            _middleRight = new Rectangle(left + Width - BorderSize, top + BorderSize, BorderSize, Height - (BorderSize * 2));
            _middleBottom = new Rectangle(left + BorderSize, top + Height - BorderSize, Width - (BorderSize * 2), BorderSize);
            IsDirty = false;
        }

        public void AddControl(IControl control)
        {
            _controls.Add(control);
            IsDirty = true;
        }

        public void PreRender(SpriteBatch sb, DrawComponents drawComponents)
        {
            // fix later
            return;
            if (!IsDirty)
                return;
            var clean = CleanRect;
            SetRect(_rectDrawing);
            Recalculate();
            
            _cachedPanel?.Dispose();
            _cachedPanel = new RenderTarget2D(sb.GraphicsDevice, Width, Height, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            sb.GraphicsDevice.SetRenderTarget(_cachedPanel);
            sb.UsualBegin();
            _cleanRect = clean;
            sb.UsualEnd();
            IsDirty = false;
        }
    }
}