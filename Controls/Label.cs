using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Fonts;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class Label : ControlBase
    {
        private readonly IFont _font;
        private string _text;
        private Vector2 _location;
        private CachedString _cached;
        private bool _textDirty;

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

        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;
                _text = value;
                _cached = CachedString.Create(_font, _text);
                _textDirty = true;
                IsDirty = true;
            }
        }

        public Label(IWindow window, IControl parent, Rectangle rect, string text = "", int zIndex = 0, bool visible = true)
        : this(window, parent, Color.White, rect, text, zIndex, visible)
        {
        }
        
        public Label(IWindow window, IControl parent, Color color, Rectangle rect, string text = "", int zIndex = 0, bool visible = true)
        : base(window, parent, visible)
        {
            _font = IoC.Get<IFont>();
            SetRect(rect);
            Color = color;
            Text = text;
            ZIndex = zIndex;
        }


        public override void Draw(SpriteBatch sb, DrawComponents drawComponents)
        {
            if (!Visible)
                return;
            if (IsDirty)
                Recalculate();
            drawComponents.FontRenderer.DrawCachedString(sb, _cached, _location, _color);
        }

        public override void Recalculate()
        {
            base.Recalculate();
            _location = CleanRect.Location.ToVector2();
            if (!_textDirty) return;
            _cached = CachedString.Create(_font, _text);
            _textDirty = false;
            IsDirty = false;
        }

    }
}