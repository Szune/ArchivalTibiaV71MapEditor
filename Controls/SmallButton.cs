using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls.Addons;
using ArchivalTibiaV71MapEditor.Fonts;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class SmallButton : ControlBase, IButton
    {
        private readonly Texture2D _spriteSheet;
        private readonly IFont _font;
        private readonly Rectangle _spriteNormal = Ui.Button.SmallButtonNormal;
        private readonly Rectangle _spritePressed = Ui.Button.SmallButtonPressed;
        private string _text;
        private CachedString _cached;
        private bool _textDirty;
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;
                _text = value;
                _cached = _text == null ? null : CachedString.Create(_font, _text);
                _textDirty = true;
                IsDirty = true;
            }
        }

        public Action OnClick { get; set; }

        public Color Color
        {
            get => _color;
            set
            {
                if (_color.Equals(value))
                    return;
                _color = value;
                IsDirty = true;
            }
        }

        private ButtonStates _state = ButtonStates.Normal;
        private Vector2 _textPos;
        private Color _color = Color.White;

        public SmallButton(IWindow window, IControl parent, Rectangle spriteNormal, Rectangle spritePressed,  Rectangle rect, string text)
            : base(window, parent)
        {
            SetRect(rect);
            _spriteSheet = Ui.SpriteSheet;
            _font = IoC.Get<IFont>();
            Text = text;
            _spriteNormal = spriteNormal;
            _spritePressed = spritePressed;
            IsDirty = true;
        }

        public SmallButton(IWindow window, IControl parent,  Rectangle rect, string text)
            : base(window, parent)
        {
            SetRect(rect);
            _spriteSheet = Ui.SpriteSheet;
            _font = IoC.Get<IFont>();
            Text = text;
            IsDirty = true;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime, DrawComponents drawComponents)
        {
            if (!Visible)
                return;
            if (IsDirty)
                Recalculate();
            var sprite = _state switch
            {
                ButtonStates.Normal => _spriteNormal,
                ButtonStates.Pressed => _spritePressed,
            };
            sb.Draw(_spriteSheet, CleanRect, sprite, Color);
            if(_cached != null)
                drawComponents.FontRenderer.DrawCachedString(sb, _cached, _textPos);
        }

        public override void Recalculate()
        {
            base.Recalculate();
            if (_textDirty)
            {
                if (_text == null)
                {
                    _cached = null;
                }
                else
                {
                    _cached = CachedString.Create(_font, _text);
                }
                _textDirty = false;
            }
            if(_cached != null)
                _textPos = new Vector2(
                    CleanRect.Left + (Width / 2) - (_cached.MeasuredSize.X / 2),
                    CleanRect.Top + (CleanRect.Height / 2 - _cached.MeasuredSize.Y / 2));
            IsDirty = false;
        }

        public override HitBox HitTest()
        {
            if (!IsVisible())
                return HitBox.Miss;
            switch (_state)
            {
                case ButtonStates.Normal:
                    if (MouseManager.IsUp(MouseButton.Left))
                        return HitBox.Miss;
                    if (Bounds.Contains(UiState.Mouse.Position))
                    {
                        _state = ButtonStates.Pressed;
                        IsDirty = true;
                        return HitBox.Hit(this);
                    }
                    break;
                case ButtonStates.Pressed:
                    if (MouseManager.IsUp(MouseButton.Left))
                    {
                        OnClick?.Invoke();
                        _state = ButtonStates.Normal;
                        IsDirty = true;
                    }
                    else
                    {
                        return HitBox.Hit(this);
                    }
                    break;
            }
            return HitBox.Miss;
        }
    }
}
