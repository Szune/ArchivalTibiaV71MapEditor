using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Controls.Addons;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public abstract class ControlBase : IControl
    {
        
        protected ControlBase(IWindow window, IControl parent, bool visible = true)
        {
            Window = window;
            Parent = parent;
            Visible = visible;
        }

        protected bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty == value)
                    return;
                _isDirty = value;
                if(value)
                    PropagatePreRenderDirtiness();
            }
        }

        private void PropagatePreRenderDirtiness()
        {
            if (Parent == null)
                return;
            var parent = Parent;
            while (parent != null)
            {
                if(parent is Panel p)
                    p.Dirty();
                parent = parent.Parent;
            }
        }

        public bool Visible { get; set; }
        private int _x;
        private int _y;
        private int _width;
        private int _height;
        private int _zIndex;
        private IWindow _window;
        protected Rectangle _cleanRect;
        private bool _isDirty;
        public int BorderSize { get; protected set; }

        public Rectangle Bounds => CleanRect;

        public IWindow Window
        {
            get => _window;
            set
            {
                if (ReferenceEquals(_window, value))
                    return;
                IsDirty = true;
                _window = value;
            }
        }

        public int ZIndex
        {
            get => _zIndex;
            set
            {
                if (_zIndex == value)
                    return;
                IsDirty = true;
                _window?.Dirty();
                _zIndex = value;
            }
        }

        public int X
        {
            get => _x;
            set
            {
                if (_x == value)
                    return;
                IsDirty = true;
                _x = value;
            }
        }

        public int Y
        {
            get => _y;
            set
            {
                if (_y == value)
                    return;
                IsDirty = true;
                _y = value;
            }
        }

        public int Width
        {
            get => _width;
            set
            {
                if (_width == value)
                    return;
                IsDirty = true;
                _width = value;
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                if (_height == value)
                    return;
                IsDirty = true;
                _height = value;
            }
        }

        public virtual HitBox HitTest() => HitBox.Miss;

        public void SetRect(Rectangle rect)
        {
            (_x, _y, _width, _height) = rect;
            _cleanRect = rect;
            IsDirty = true;
        }

        protected Rectangle CleanRect
        {
            get => _cleanRect;
            private set => _cleanRect = value;
        }

        public IControl Parent { get; set; }

        public virtual void OffsetChild(ref Rectangle rect)
        {
            Parent?.OffsetChild(ref rect);
            rect.Offset(CleanRect.Location);
        }

        public virtual void Dirty()
        {
            IsDirty = true;
        }

        public abstract void Draw(SpriteBatch sb, DrawComponents drawComponents);

        /// <summary>
        /// If overriding this function, assign CleanRect to Bounds at the end.
        /// </summary>
        public virtual void Recalculate()
        {
            CleanRect = new Rectangle(X, Y, Width, Height);
            Parent?.OffsetChild(ref _cleanRect);
            IsDirty = false;
        }

        protected bool IsVisible()
        {
            if (!Visible)
                return false;
            var parent = Parent;
            while (parent != null)
            {
                if (!parent.Visible)
                    return false;
                parent = parent.Parent;
            }

            return Visible;
        }
    }
}