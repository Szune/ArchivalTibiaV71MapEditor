using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls.Addons;
using ArchivalTibiaV71MapEditor.Extensions;
using ArchivalTibiaV71MapEditor.Fonts;
using ArchivalTibiaV71MapEditor.Utilities;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class DialogWindow : ControlBase, IModal
    {
        private readonly Texture2D _spriteSheet;

        private readonly List<IControl> _controls = new List<IControl>();

        private Vector2 _topLeft;
        private Vector2 _topRight;
        private Vector2 _bottomLeft;
        private Vector2 _bottomRight;
        private Rectangle _middleTop;
        private Rectangle _middleLeft;
        private Rectangle _middleRight;
        private Rectangle _middleBottom;
        private IHitTestable _lastHitTest;
        private Rectangle _captionBarRect;
        private Rectangle _mainRect;
        private Rectangle[] _backgroundRects;
        private Rectangle[] _captionBackgroundRects;
        private CachedString _caption;
        private Vector2 _captionPos;
        private SmallButton _closeButton;
        private Draggable _draggable;
        private IWindow _mainWindow = IoC.Get<IWindow>();
        public Guid Id { get; } = Guid.NewGuid();

        public DialogWindow(Point size, string caption) : base(null, null)
        {
            BorderSize = 4;
            _spriteSheet = Ui.SpriteSheet;
            SetRect(new Rectangle(Point.Zero, size));
            _caption = CachedString.Create(IoC.Get<IFont>(), caption);
            _closeButton = new SmallButton(IoC.Get<IWindow>(), this, Ui.Button.CloseButtonNormal, Ui.Button.CloseButtonPressed,
                new Rectangle(), null)
            {
                OnClick = Close,
            };
            var cbWidth = Ui.Button.CloseButtonNormal.Width;
            var cbHeight = Ui.Button.CloseButtonNormal.Height;
            _closeButton.SetRect(new Rectangle(CleanRect.Width - cbWidth - (BorderSize * 2), -Ui.CaptionHeight, cbWidth, cbHeight));
            _draggable = new Draggable(MouseButton.Left, 1);
            IsDirty = true;
        }

        public void Close()
        {
            Modals.RemoveLast();
        }

        public void AddControl(IControl control)
        {
            _controls.Add(control);
        }

        public void Show(Point position)
        {
            SetRect(new Rectangle(position.X, position.Y, _cleanRect.Width, _cleanRect.Height));
            IsDirty = true;
            Modals.Add(this);
        }

        public override void OffsetChild(ref Rectangle rect)
        {
            base.OffsetChild(ref rect);
            // controls shouldn't have to worry about the caption size
            rect.Offset(BorderSize, Ui.CaptionHeight + BorderSize); 
        }

        public override void Draw(SpriteBatch sb, DrawComponents drawComponents)
        {
            if (!Visible)
                return;
            if (IsDirty)
                Recalculate();
            // draw background
            for (var i = 0; i < _backgroundRects.Length; i++)
            {
                sb.Draw(_spriteSheet, _backgroundRects[i], Ui.Background, Color.White);
            }

            // draw caption background
            for (var i = 0; i < _captionBackgroundRects.Length; i++)
            {
                sb.Draw(_spriteSheet, _captionBackgroundRects[i], Ui.CaptionBackground, Color.White);
            }

            for (int i = 0; i < _controls.Count; i++)
            {
                _controls[i].Draw(sb, drawComponents);
            }

            // draw borders
            sb.Draw(_spriteSheet, _middleTop, Ui.Border.WindowMiddleTop, Color.White);
            sb.Draw(_spriteSheet, _middleLeft, Ui.Border.WindowMiddleLeft, Color.White);
            sb.Draw(_spriteSheet, _middleRight, Ui.Border.WindowMiddleRight, Color.White);
            sb.Draw(_spriteSheet, _middleBottom, Ui.Border.WindowMiddleBottom, Color.White);

            // draw caption
            drawComponents.FontRenderer.DrawCachedString(sb, _caption, _captionPos);
            _closeButton.Draw(sb, drawComponents);

            // draw corners
            sb.Draw(_spriteSheet, _topLeft, Ui.Border.WindowTopLeft, Color.White);
            sb.Draw(_spriteSheet, _topRight, Ui.Border.WindowTopRight, Color.White);
            sb.Draw(_spriteSheet, _bottomLeft, Ui.Border.WindowBottomLeft, Color.White);
            sb.Draw(_spriteSheet, _bottomRight, Ui.Border.WindowBottomRight, Color.White);
        }

        public override void Recalculate()
        {
            base.Recalculate();
            _controls.Sort(ZIndexComparer.Instance);
            _captionBarRect =
                new Rectangle(CleanRect.Left, CleanRect.Top + BorderSize, CleanRect.Width, Ui.CaptionHeight);
            _mainRect = new Rectangle(CleanRect.Left, CleanRect.Top + BorderSize + Ui.CaptionHeight, CleanRect.Width,
                CleanRect.Height - Ui.CaptionHeight - BorderSize);
            _closeButton.Dirty();
            for (int i = 0; i < _controls.Count; i++)
            {
                _controls[i].Dirty();
            }

            _topLeft = new Vector2(CleanRect.Left, CleanRect.Top);
            _topRight = new Vector2(CleanRect.Left + Width - BorderSize, CleanRect.Top);
            _bottomLeft = new Vector2(CleanRect.Left, CleanRect.Top + Height - BorderSize);
            _bottomRight = new Vector2(CleanRect.Left + Width - BorderSize, CleanRect.Top + Height - BorderSize);

            _middleTop = new Rectangle(CleanRect.Left + BorderSize, CleanRect.Top, Width - (BorderSize * 2),
                BorderSize);
            _middleLeft = new Rectangle(CleanRect.Left, CleanRect.Top + BorderSize, BorderSize,
                Height - (BorderSize * 2));
            _middleRight = new Rectangle(CleanRect.Left + Width - BorderSize, CleanRect.Top + BorderSize, BorderSize,
                Height - (BorderSize * 2));
            _middleBottom = new Rectangle(CleanRect.Left + BorderSize, CleanRect.Top + Height - BorderSize,
                Width - (BorderSize * 2), BorderSize);

            _captionPos = new Vector2(CleanRect.Left + (_captionBarRect.Width / 2) - (_caption.MeasuredSize.X / 2),
                CleanRect.Top + BorderSize + 2);
            //_closeButton.SetRect(new Rectangle(CleanRect.Right - Ui.CaptionHeight - BorderSize, CleanRect.Top, Ui.CaptionHeight + BorderSize, Ui.CaptionHeight + BorderSize));

            _backgroundRects =
                Parts.CreateRectangular(CleanRect.Size, CleanRect.Left, CleanRect.Top, Ui.Background.Size);
            _captionBackgroundRects = Parts.CreateHorizontal(_captionBarRect.Width,
                Ui.CaptionBackground.Width, _captionBarRect.Top, _captionBarRect.Left, Ui.CaptionHeight);
            _draggable.SetRect(new Rectangle(_captionBarRect.Left, _captionBarRect.Top, _captionBarRect.Width - Ui.CaptionHeight - BorderSize, _captionBarRect.Height));
            IsDirty = false;
        }

        public void Update()
        {
            if (_lastHitTest != null)
            {
                if (MouseManager.IsDown(MouseButton.Left))
                {
                    var test = _lastHitTest.HitTest();
                    if (test.IsHit)
                        return;
                    _lastHitTest = null;
                }

                _lastHitTest = null;
            }
            
            if (_draggable.HitTest().IsHit)
            {
                var delta = _draggable.GetMoveDelta();
                _draggable.InvalidateDelta();
                var x = ((int) (CleanRect.X - delta.X)).Clamp(0, _mainWindow.Width - CleanRect.Width);
                var y = ((int) (CleanRect.Y - delta.Y)).Clamp(0, _mainWindow.Height - CleanRect.Height);
                X = x;
                Y = y;
                _draggable.SetRect(new Rectangle(_captionBarRect.Left, _captionBarRect.Top, _captionBarRect.Width - Ui.CaptionHeight - BorderSize, _captionBarRect.Height));
                SetRect(new Rectangle(X, Y, Width, Height));
                IsDirty = true;
                return;
            }

            _ = _closeButton.HitTest();
            

            for (var i = _controls.Count - 1; i > -1; i--)
            {
                var test = _controls[i].HitTest();
                if (!test.IsHit) continue;
                _lastHitTest = test.Control;
                return;
            }
        }
    }
}