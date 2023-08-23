using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls.Addons;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class IconButtonUsingOverlay : ControlBase, IButton
    {
        
        private readonly Texture2D _spriteSheet;
        private readonly Rectangle _spritePosition;
        private readonly Rectangle _spriteNormalOverlay;
        private readonly Rectangle _spritePressedOverlay;
        public Action OnClick { get; set; }
        private ButtonStates _state = ButtonStates.Normal;
        public Color Color { get; set; } = Color.White;

        public IconButtonUsingOverlay(IWindow window, IControl parent, Rectangle spritePosition, Rectangle spriteNormalOverlay, Rectangle spritePressedOverlay, Rectangle rect) : base(window, parent)
        {
            _spriteSheet = Ui.SpriteSheet;
            _spritePosition = spritePosition;
            _spriteNormalOverlay = spriteNormalOverlay;
            _spritePressedOverlay = spritePressedOverlay;
            SetRect(rect);
        }
        
        public override void Draw(SpriteBatch sb, GameTime gameTime, DrawComponents drawComponents)
        {
            sb.Draw(_spriteSheet, CleanRect, _spritePosition, Color);
            var overlay = _state switch
            {
                ButtonStates.Normal => _spriteNormalOverlay,
                ButtonStates.Pressed => _spritePressedOverlay,
            };
            sb.Draw(_spriteSheet, CleanRect, overlay, Color);
        }

        public void SetOnClick(Action onClick)
        {
            OnClick = onClick;
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