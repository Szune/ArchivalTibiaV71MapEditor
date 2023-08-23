using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls.Addons;
using ArchivalTibiaV71MapEditor.Fonts;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class LargeTextButton : ControlBase, IButton
    {
        private readonly Texture2D _spriteSheet;
        private readonly IFont _font;
        private static readonly Rectangle SpriteNormal = Ui.Button.LargeButtonNormal;
        private static readonly Rectangle SpritePressed = Ui.Button.LargeButtonPressed;
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;
                IsDirty = true;
                _text = value;
            }
        }
        
        public Action OnClick { get; set; }
        private ButtonStates _state = ButtonStates.Normal;
        private Vector2 _textPos;
        public Color Color { get; set; } = Color.White;

        public LargeTextButton(IWindow window, IControl parent, Rectangle rect, string text)
            : base(window, parent)
        {
            SetRect(rect);
            _spriteSheet = Ui.SpriteSheet;
            _font = IoC.Get<IFont>();
            _text = text;
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
                ButtonStates.Normal => SpriteNormal,
                ButtonStates.Pressed => SpritePressed,
            };
            sb.Draw(_spriteSheet, CleanRect, sprite, Color);
            drawComponents.FontRenderer.DrawString(_font, sb, _text, _textPos);
        }

        public override void Recalculate()
        {
            base.Recalculate();
            var strSize = _font.MeasureString(_text);
            _textPos = new Vector2(
                CleanRect.Left + (Width / 2) - (strSize.X / 2),
                CleanRect.Top + (CleanRect.Height / 2 - strSize.Y / 2)); 
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