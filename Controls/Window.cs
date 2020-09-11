using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls.Addons;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class Window : IWindow
    {
        public const int BorderSize = 4;
        private readonly Texture2D _spriteSheet;
        private int _width;
        private int _height;
        private Point _center;

        public int Width
        {
            get => _width;
            private set
            {
                if (_width == value)
                    return;
                _isDirty = true;
                _width = value;
            }
        }

        public int Height
        {
            get => _height;
            private set
            {
                if (_height == value)
                    return;
                _isDirty = true;
                _height = value;
            }
        }

        private readonly List<IControl> _controls = new List<IControl>();

        private bool _isDirty = true;
        private Vector2 _topLeft;
        private Vector2 _topRight;
        private Vector2 _bottomLeft;
        private Vector2 _bottomRight;
        private Rectangle _middleTop;
        private Rectangle _middleLeft;
        private Rectangle _middleRight;
        private Rectangle _middleBottom;

        private Rectangle _bounds;
        public Rectangle Bounds
        {
            get => _bounds;
            private set
            {
                if (_bounds == value)
                    return;
                _isDirty = true;
                _bounds = value;
            }
        }

        public Window(int width, int height)
        {
            _spriteSheet = Ui.SpriteSheet;
            SetSize(width, height);
        }

        public void AddControl(IControl control)
        {
            _controls.Add(control);
        }

        public void Draw(SpriteBatch sb, DrawComponents drawComponents)
        {
            if (_isDirty)
                Recalculate();
            for (int i = 0; i < _controls.Count; i++)
            {
                _controls[i].Draw(sb, drawComponents);
            }

            sb.Draw(_spriteSheet, _middleTop, Ui.Border.WindowMiddleTop, Color.White);
            sb.Draw(_spriteSheet, _middleLeft, Ui.Border.WindowMiddleLeft, Color.White);
            sb.Draw(_spriteSheet, _middleRight, Ui.Border.WindowMiddleRight, Color.White);
            sb.Draw(_spriteSheet, _middleBottom, Ui.Border.WindowMiddleBottom, Color.White);

            sb.Draw(_spriteSheet, _topLeft, Ui.Border.WindowTopLeft, Color.White);
            sb.Draw(_spriteSheet, _topRight, Ui.Border.WindowTopRight, Color.White);
            sb.Draw(_spriteSheet, _bottomLeft, Ui.Border.WindowBottomLeft, Color.White);
            sb.Draw(_spriteSheet, _bottomRight, Ui.Border.WindowBottomRight, Color.White);

            // for (var i = _modals.Count - 1; i > -1; i--)
            // {
            //     _modals[i].Draw(sb, drawComponents);
            // }
        }

        private void Recalculate()
        {
            _controls.Sort(ZIndexComparer.Instance);
            for (int i = 0; i < _controls.Count; i++)
            {
                _controls[i].Dirty();
            }

            _topLeft = new Vector2(0, 0);
            _topRight = new Vector2(Width - BorderSize, 0);
            _bottomLeft = new Vector2(0, Height - BorderSize);
            _bottomRight = new Vector2(Width - BorderSize, Height - BorderSize);

            _middleTop = new Rectangle(BorderSize, 0, Width - (BorderSize * 2), BorderSize);
            _middleLeft = new Rectangle(0, BorderSize, BorderSize, Height - (BorderSize * 2));
            _middleRight = new Rectangle(Width - BorderSize, BorderSize, BorderSize, Height - (BorderSize * 2));
            _middleBottom = new Rectangle(BorderSize, Height - BorderSize, Width - (BorderSize * 2), BorderSize);
            _center = new Point(Width / 2, Height / 2);
            _isDirty = false;
        }

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
            Bounds = new Rectangle(0, 0, width, height);
            _center = new Point(Width / 2, Height / 2);
            Recalculate();
        }

        public void Dirty()
        {
            _isDirty = true;
        }

        public IHitTestable LastHitTest;


        public void Update()
        {
            if (LastHitTest != null)
            {
                if (MouseManager.IsDown(MouseButton.Left))
                {
                    var test = LastHitTest.HitTest();
                    if(test.IsHit)
                        return;
                    LastHitTest = null;
                }
                LastHitTest = null;
            }
            for (var i = _controls.Count - 1; i > -1; i--)
            {
                var test = _controls[i].HitTest();
                if (!test.IsHit) continue;
                LastHitTest = test.Control;
                return;
            }
        }

        public T GetControl<T>() where T : IControl
        {
            return _controls.OfType<T>().Single();
        }

        public T[] GetControls<T>() where T : IControl
        {
            return _controls.OfType<T>().ToArray();
        }

        public Point Center() => _center;
    }
}