using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls.Addons;
using ArchivalTibiaV71MapEditor.Fonts;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class TextButton : ControlBase, IButton
    {
        private readonly int _margin;
        private readonly IFont _font;
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
        private Rectangle _backgroundRect;

        public TextButton(IWindow window, IControl parent, Point location, string text, int margin)
            : base(window, parent)
        {
            _margin = margin;
            _font = IoC.Get<IFont>();
            Text = text ?? throw new ArgumentNullException(nameof(text));
            SetRect(new Rectangle(location.X + margin, location.Y, (int)_cached.MeasuredSize.X, (int)_cached.MeasuredSize.Y));
            IsDirty = true;
        }
        
        private bool Hovering()
        {
            return MouseManager.IsHovering(_backgroundRect);
        }
        
        public override void Draw(SpriteBatch sb, DrawComponents drawComponents)
        {
            if (!Visible)
                return;
            if (IsDirty)
                Recalculate();
            if(Hovering())
                sb.Draw(Pixel.White, _backgroundRect, Color.LightBlue);
            else if (_state == ButtonStates.Pressed)
                sb.Draw(Pixel.White, _backgroundRect, Color.DarkGray);
            drawComponents.FontRenderer.DrawCachedString(sb, _cached, _textPos, Color);
        }

        public override void Recalculate()
        {
            base.Recalculate();
            if (_textDirty)
            {
                _cached = CachedString.Create(_font, _text);
                //SetRect(new Rectangle(X, Y, (int)_cached.MeasuredSize.X, (int)_cached.MeasuredSize.Y));
                _textDirty = false;
            }
            _backgroundRect = new Rectangle(
                CleanRect.Left - _margin / 2,
                CleanRect.Top - _margin / 2, Width + _margin, Height + _margin);
            _textPos = new Vector2(
                CleanRect.Left + (Width / 2) - (_cached.MeasuredSize.X / 2),
                CleanRect.Top + (CleanRect.Height / 2 - _cached.MeasuredSize.Y / 2));
            if (_state == ButtonStates.Pressed)
                _textPos -= Vector2.One;
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
                    if (_backgroundRect.Contains(UiState.Mouse.Position))
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