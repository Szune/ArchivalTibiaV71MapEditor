using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls.Addons;
using ArchivalTibiaV71MapEditor.Extensions;
using ArchivalTibiaV71MapEditor.Sprites;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class PaletteBox<T> : ControlBase, ICanHaveVerticalScrollBar
    {
        private class PaletteBoxItem
        {
            public T Value;
            public ImageBox ImageBox;
            public Rectangle Bounds;

            public bool HitTest()
            {
                if (MouseManager.IsUp(MouseButton.Left))
                    return false;
                return Bounds.Contains(UiState.Mouse.Position);
            }

            public void Draw(SpriteBatch sb, GameTime gameTime, DrawComponents drawComponents, bool selected)
            {
                if (selected)
                    sb.Draw(Pixel.White, Bounds, Color.LightGray);
                else if (MouseManager.IsHovering(Bounds))
                {
                    sb.Draw(Pixel.White, Bounds, Color.LightBlue);
                }
                var color =
                    selected
                    ? Color.Red
                    : MouseManager.IsHovering(Bounds)
                        ? Color.LightBlue
                        : Color.White;
                ImageBox.Color = color;
                ImageBox.Draw(sb, gameTime, drawComponents);
            }
        }

        public int VerticalItemCount => _items.Count / HorizontalMaxVisibleItems;
        public int VerticalScrollIndex { get; private set; }
        private const int ItemWidth = 32;
        private const int ItemHeight = 32;
        public int HorizontalMaxVisibleItems => Width / ItemWidth;
        public int VerticalMaxVisibleItems => Height / ItemHeight;
        public int VerticalMaxScrollIndex => Math.Max(VerticalItemCount - VerticalMaxVisibleItems, 0);
        private int MaxVisibleItemsTotal => HorizontalMaxVisibleItems * VerticalMaxVisibleItems;
        private readonly Texture2D _spriteSheet;
        private readonly List<PaletteBoxItem> _items = new List<PaletteBoxItem>();
        private Rectangle _destRectTop;
        private Rectangle _destRectLeft;
        private Rectangle _destRectRight;
        private Rectangle _destRectBottom;
        private readonly VerticalScrollBar _scrollBar;
        private PaletteBoxItem _selectedItem;
        public Action<T> OnSelect { get; set; }
        public T SelectedValue => _selectedItem == null ? default : _selectedItem.Value;
        private readonly Clickable _clickable = new Clickable();

        public PaletteBox(IWindow window, IControl parent, Rectangle rect,
            int zIndex = 0,
            bool visible = true)
            : base(window, parent, visible)
        {
            Window = window;
            Parent = parent;
            _spriteSheet = Ui.SpriteSheet;
            SetRect(rect);
            _clickable.SetRect(rect);
            ZIndex = zIndex;
            BorderSize = 1;
            _scrollBar = new VerticalScrollBar(window, this, Border.Right);
        }

        public void AddItem(ImageBox image, T value)
        {
            image.Parent = this;
            image.Window = Window;
            var item = new PaletteBoxItem
            {
                Value = value,
                ImageBox = image
            };
            _items.Add(item);
            IsDirty = true;
        }

        public void AddRange(IEnumerable<ImageBox> images, Func<Sprite, T> valueFunc)
        {
            foreach (var img in images)
            {
                img.Parent = this;
                img.Window = Window;
                var item = new PaletteBoxItem
                {
                    Value = valueFunc(img.Sprite),
                    ImageBox = img,
                };
                _items.Add(item);
            }

            IsDirty = true;
        }

        public override void Draw(SpriteBatch sb, GameTime gameTime, DrawComponents drawComponents)
        {
            if (!Visible)
                return;
            if (IsDirty)
                Recalculate();
            // draw background
            sb.Draw(Pixel.ListBackground, CleanRect, Color.White);
            // draw border
            sb.Draw(_spriteSheet, _destRectTop, Ui.Border.Top, Color.White);
            sb.Draw(_spriteSheet, _destRectLeft, Ui.Border.Left, Color.White);
            sb.Draw(_spriteSheet, _destRectRight, Ui.Border.Right, Color.White);
            sb.Draw(_spriteSheet, _destRectBottom, Ui.Border.Bottom, Color.White);

            // draw items
            for (int i = VerticalScrollIndex * HorizontalMaxVisibleItems; i < _items.Count && i - (VerticalScrollIndex * HorizontalMaxVisibleItems) < MaxVisibleItemsTotal; i++)
            {
                _items[i].Draw(sb, gameTime, drawComponents, _selectedItem?.Value.Equals(_items[i].Value) ?? false);
            }

            if (VerticalItemCount > VerticalMaxVisibleItems)
                 _scrollBar.Draw(sb, gameTime, drawComponents);
        }

        public override HitBox HitTest()
        {
            if (!IsVisible())
                return HitBox.Miss;
            HitBox hit;
            if ((hit = _scrollBar.HitTest()).IsHit)
            {
                return hit;
            }

            for (int i = VerticalScrollIndex * HorizontalMaxVisibleItems;
                i < _items.Count && i - (VerticalScrollIndex * HorizontalMaxVisibleItems) < MaxVisibleItemsTotal;
                i++)
            {
                if (!_items[i].HitTest()) continue;
                _selectedItem = _items[i];
                OnSelect?.Invoke(_items[i].Value);
                return HitBox.Hit(this);
            }

            _clickable.HitTest(MouseButton.Left);
            if (_clickable.IsDown(MouseButton.Left))
                return HitBox.Hit(this);

            return HitBox.Miss;
        }

        public override void OffsetChild(ref Rectangle rect)
        {
            rect.Offset(CleanRect.Location);
        }

        public void Select(T value)
        {
            var index = _items.FindIndex(it => it.Value.Equals(value));
            _selectedItem = _items[index];
            OnSelect?.Invoke(value);
            VerticalScrollTo(index.Clamp(0, VerticalMaxScrollIndex));
        }

        public void VerticalScroll(int delta)
        {
            VerticalScrollIndex = (VerticalScrollIndex + delta).Clamp(0, VerticalMaxScrollIndex);
            IsDirty = true;
        }

        public void VerticalScrollTo(int index)
        {
            VerticalScrollIndex = index;
            IsDirty = true;
        }

        public override void Recalculate()
        {
            base.Recalculate();
            _destRectTop = new Rectangle(CleanRect.Left, CleanRect.Top, CleanRect.Width, BorderSize);
            _destRectLeft = new Rectangle(CleanRect.Left, CleanRect.Top, BorderSize, CleanRect.Height);
            _destRectRight = new Rectangle(CleanRect.Right - BorderSize, CleanRect.Top, BorderSize, CleanRect.Height);
            _destRectBottom = new Rectangle(CleanRect.Left, CleanRect.Bottom, CleanRect.Width, BorderSize);

            for (int i = VerticalScrollIndex * HorizontalMaxVisibleItems; i < _items.Count && i - (VerticalScrollIndex * HorizontalMaxVisibleItems) < MaxVisibleItemsTotal; i++)
            {
                var rect = new Rectangle(
                    ((i - VerticalScrollIndex * HorizontalMaxVisibleItems) % HorizontalMaxVisibleItems) * ItemWidth,
                    ((i - VerticalScrollIndex * HorizontalMaxVisibleItems) / HorizontalMaxVisibleItems) * ItemHeight,
                    ItemWidth,
                    ItemHeight);
                _items[i].ImageBox.SetRect(rect);
                _items[i].ImageBox.Recalculate();
                _items[i].Bounds = _items[i].ImageBox.Bounds;
            }

            _scrollBar.Recalculate();
            _clickable.SetRect(CleanRect);
            IsDirty = false;
        }
    }
}
