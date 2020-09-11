using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Sprites;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class ImageBox : ControlBase
    {
        private readonly Point _renderSize;
        public readonly Sprite Sprite;
        private SpritePart[] _spriteParts;

        public ImageBox(Sprite sprite, Point renderSize) 
            : this(null, null, sprite, renderSize, Rectangle.Empty)
        {
        }

        public ImageBox(Window window, IControl parent, Sprite sprite, Point renderSize, Rectangle rect,
            int zIndex = 0, bool visible = true) : base(window, parent, visible)
        {
            Window = window;
            Parent = parent;
            Sprite = sprite;
            _renderSize = renderSize;
            SetRect(rect);
            ZIndex = zIndex;
            _color = Color.White;
        }

        private Color _color;
        public Color Color
        {
            get => _color;
            set
            {
                if (_color == value)
                    return;
                _color = value;
                IsDirty = true;
            }
        }

        public override void Draw(SpriteBatch sb, DrawComponents drawComponents)
        {
            if (!Visible)
                return;
            if (IsDirty)
                Recalculate();
            drawComponents.SpriteRenderer.Draw(sb, _spriteParts, _color);
        }

        public override void Recalculate()
        {
            base.Recalculate();
            _spriteParts = Sprite.GetParts(CleanRect.Location.ToVector2(), _renderSize);
        }
    }
}