using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls.Addons;
using ArchivalTibiaV71MapEditor.Extensions;
using ArchivalTibiaV71MapEditor.Fonts;
using ArchivalTibiaV71MapEditor.Utilities;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class ConfirmationBox : ControlBase, IModal
    {
        private const int ButtonMargin = 5;
        private const int Margin = 25;
        private static Texture2D _spriteSheet;
        private readonly CachedString _text;
        private readonly CachedString _caption;
        private readonly Draggable _draggable;
        private const int CaptionHeight = 20;
        private const int ButtonHeight = 24;
        private Vector2 _topLeft;
        private Vector2 _topRight;
        private Vector2 _bottomLeft;
        private Vector2 _bottomRight;
        private Rectangle _middleTop;
        private Rectangle _middleLeft;
        private Rectangle _middleRight;
        private Rectangle _middleBottom;
        private static IWindow _mBoxWindow;
        private static IFont _font;
        private Rectangle _mBoxCaptionBarRect;
        private Rectangle _mBoxMainRect;
        private Vector2 _captionPos;
        private Vector2 _textPos;
        private Rectangle[] _backgroundRects;
        private Rectangle[] _captionBackgroundRects;
        private readonly SmallButton _buttonNo;
        private readonly SmallButton _buttonYes;

        public Guid Id { get; } = Guid.NewGuid();

        private ConfirmationBox(Rectangle rect, string caption, string text, Action onYes, bool visible = true)
            : base(null, null, visible)
        {
            BorderSize = 4;
            _mBoxCaptionBarRect = new Rectangle(rect.X, rect.Y, rect.Width, CaptionHeight);
            _mBoxMainRect = new Rectangle(rect.X, rect.Y + CaptionHeight, rect.Width, rect.Height - CaptionHeight);
            _text = CachedString.Create(_font, text);
            _caption = CachedString.Create(_font, caption);
            SetRect(rect);
            _draggable = new Draggable(MouseButton.Left, 1);
            _draggable.SetRect(_mBoxCaptionBarRect);
            IsDirty = true;
            _buttonYes = new SmallButton(_mBoxWindow, null,
                new Rectangle(0, 0, 40, ButtonHeight - 2), "Yes");
            _buttonYes.OnClick = () =>
            {
                Close();
                onYes?.Invoke();
            };
            _buttonNo = new SmallButton(_mBoxWindow, null,
                new Rectangle(0, 0, 40, ButtonHeight - 2), "No");
            _buttonNo.OnClick = Close;
        }

        private void Close()
        {
            Modals.RemoveThis(Id);
        }

        public static void Setup()
        {
            _mBoxWindow = IoC.Get<IWindow>();
            _font = IoC.Get<IFont>();
            _spriteSheet = Ui.SpriteSheet;
        }

        public static void Show(Point location, string text, Action onYes, string caption = "Are you sure?")
        {
            var strSize = _font.MeasureString(text);
            Modals.Add(new ConfirmationBox(
                new Rectangle(
                    location.X,
                    location.Y,
                    Math.Max((int) strSize.X + Margin * 2, 150),
                    Math.Max((int) strSize.Y + Ui.CaptionHeight + ButtonHeight + 8 + Margin,
                        Ui.CaptionHeight + ButtonHeight + Margin * 2)), caption, text, onYes));
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime, DrawComponents drawComponents)
        {
            if (!Visible)
                return;
            if (IsDirty)
                Recalculate();
            for (var i = 0; i < _backgroundRects.Length; i++)
            {
                sb.Draw(_spriteSheet, _backgroundRects[i], Ui.Background, Color.White);
            }

            for (var i = 0; i < _captionBackgroundRects.Length; i++)
            {
                sb.Draw(_spriteSheet, _captionBackgroundRects[i], Ui.CaptionBackground, Color.White);
            }

            sb.Draw(_spriteSheet, _middleTop, Ui.Border.WindowMiddleTop, Color.White);
            sb.Draw(_spriteSheet, _middleLeft, Ui.Border.WindowMiddleLeft, Color.White);
            sb.Draw(_spriteSheet, _middleRight, Ui.Border.WindowMiddleRight, Color.White);
            sb.Draw(_spriteSheet, _middleBottom, Ui.Border.WindowMiddleBottom, Color.White);


            drawComponents.FontRenderer.DrawCachedString(sb, _caption, _captionPos);
            drawComponents.FontRenderer.DrawCachedString(sb, _text, _textPos);
            _buttonYes.Draw(sb, gameTime, drawComponents);
            _buttonNo.Draw(sb, gameTime, drawComponents);

            sb.Draw(_spriteSheet, _topLeft, Ui.Border.WindowTopLeft, Color.White);
            sb.Draw(_spriteSheet, _topRight, Ui.Border.WindowTopRight, Color.White);
            sb.Draw(_spriteSheet, _bottomLeft, Ui.Border.WindowBottomLeft, Color.White);
            sb.Draw(_spriteSheet, _bottomRight, Ui.Border.WindowBottomRight, Color.White);
        }

        public override void Recalculate()
        {
            base.Recalculate();
            _mBoxCaptionBarRect =
                new Rectangle(CleanRect.Left, CleanRect.Top + BorderSize, CleanRect.Width, CaptionHeight);
            _mBoxMainRect = new Rectangle(CleanRect.Left, CleanRect.Top + BorderSize + CaptionHeight, CleanRect.Width,
                CleanRect.Height - CaptionHeight - BorderSize);
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

            _captionPos = new Vector2(CleanRect.Left + (_mBoxCaptionBarRect.Width / 2) - (_caption.MeasuredSize.X / 2),
                CleanRect.Top + BorderSize + 2);
            _textPos = new Vector2(
                _mBoxMainRect.Left + (_mBoxMainRect.Width / 2) - (_text.MeasuredSize.X / 2),
                _mBoxMainRect.Top + ((_mBoxMainRect.Height - ButtonHeight) / 2) - (_text.MeasuredSize.Y / 2)
            );
            _backgroundRects =
                Parts.CreateRectangular(CleanRect.Size, CleanRect.Left, CleanRect.Top, Ui.Background.Size);
            _captionBackgroundRects = Parts.CreateHorizontal(_mBoxCaptionBarRect.Width,
                Ui.CaptionBackground.Width, _mBoxCaptionBarRect.Top, _mBoxCaptionBarRect.Left, CaptionHeight);
            _buttonYes.X = _mBoxMainRect.Left + _mBoxMainRect.Width - _buttonYes.Width - _buttonNo.Width - (ButtonMargin * 3);
            _buttonYes.Y = _mBoxMainRect.Top + _mBoxMainRect.Height - ButtonHeight - BorderSize;
            _buttonNo.X = _mBoxMainRect.Left + _mBoxMainRect.Width - _buttonNo.Width - (ButtonMargin * 2);
            _buttonNo.Y = _mBoxMainRect.Top + _mBoxMainRect.Height - ButtonHeight - BorderSize;
            IsDirty = false;
        }

        public void Update()
        {
            if (!Visible)
                return;
            if (_draggable.HitTest().IsHit)
            {
                var delta = _draggable.GetMoveDelta();
                _draggable.InvalidateDelta();
                var x = ((int) (CleanRect.X - delta.X)).Clamp(0, _mBoxWindow.Width - CleanRect.Width);
                var y = ((int) (CleanRect.Y - delta.Y)).Clamp(0, _mBoxWindow.Height - CleanRect.Height);
                X = x;
                Y = y;
                _draggable.SetRect(_mBoxCaptionBarRect);
                IsDirty = true;
                return;
            }

            if (_buttonNo.HitTest().IsHit)
            {
                IsDirty = true;
            }
            else if (_buttonYes.HitTest().IsHit)
            {
                IsDirty = true;
            }
        }
    }
}
