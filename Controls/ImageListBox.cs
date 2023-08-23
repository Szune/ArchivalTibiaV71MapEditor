using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls.Addons;
using ArchivalTibiaV71MapEditor.Extensions;
using ArchivalTibiaV71MapEditor.Fonts;
using ArchivalTibiaV71MapEditor.Sprites;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class ImageListBox<T> : ControlBase, ICanHaveVerticalScrollBar
    {
        const int ImageMargin = 8;

        private class ImageListBoxItem
        {
            public T Value;
            public Label Label;
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
                    sb.Draw(Pixel.White, Bounds, Color.LightBlue);
                Label.Color = selected ? Color.Red : Color.White;
                ImageBox.Draw(sb, gameTime, drawComponents);
                Label.Draw(sb, gameTime, drawComponents);
            }
        }

        public int VerticalItemCount => _items.Count;
        public int VerticalScrollIndex { get; private set; }
        private const int VerticalItemHeight = 40;
        public int VerticalMaxVisibleItems => Height / VerticalItemHeight;
        public int VerticalMaxScrollIndex => Math.Max(VerticalItemCount - VerticalMaxVisibleItems, 0);
        private readonly int _margin;
        private readonly Texture2D _spriteSheet;
        private readonly IFont _font;
        private readonly List<ImageListBoxItem> _items = new List<ImageListBoxItem>();
        private Rectangle _destRectTop;
        private Rectangle _destRectLeft;
        private Rectangle _destRectRight;
        private Rectangle _destRectBottom;
        private readonly VerticalScrollBar _scrollBar;
        private ImageListBoxItem _selectedItem;
        public Action<T> OnSelect { get; set; }
        public T SelectedValue => _selectedItem == null ? default : _selectedItem.Value;
        private readonly Clickable _clickable = new Clickable();

        public ImageListBox(IWindow window, IControl parent, Rectangle rect,
            int zIndex = 0,
            bool visible = true)
            : base(window, parent, visible)
        {
            Window = window;
            Parent = parent;
            _spriteSheet = Ui.SpriteSheet;
            _font = IoC.Get<IFont>();
            SetRect(rect);
            _clickable.SetRect(rect);
            ZIndex = zIndex;
            BorderSize = 1;
            _margin = 5 + BorderSize;
            _scrollBar = new VerticalScrollBar(window, this, Border.Right);
        }

        public void AddItem(ImageBox image, string text, T value)
        {
            image.Parent = this;
            image.Window = Window;
            var item = new ImageListBoxItem
            {
                Value = value,
                Label = new Label(Window, this, Rectangle.Empty, text, ZIndex + 1),
                ImageBox = image
            };
            _items.Add(item);
            IsDirty = true;
        }

        public void AddRange(IEnumerable<ImageBox> images, Func<Sprite, string> textFunc, Func<Sprite, T> valueFunc)
        {
            foreach (var img in images)
            {
                img.Parent = this;
                img.Window = Window;
                var item = new ImageListBoxItem
                {
                    Value = valueFunc(img.Sprite),
                    Label = new Label(Window, this, Rectangle.Empty, textFunc(img.Sprite), ZIndex + 1),
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
            for (int i = VerticalScrollIndex;
                i < _items.Count && i - VerticalScrollIndex < VerticalMaxVisibleItems;
                i++)
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

            for (int i = VerticalScrollIndex;
                i < _items.Count && i - VerticalScrollIndex < VerticalMaxVisibleItems;
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

            var scrollWidth = VerticalItemCount > VerticalMaxVisibleItems ? Scroll.Width : 0;

            for (int i = VerticalScrollIndex;
                i < _items.Count && i - VerticalScrollIndex < VerticalMaxVisibleItems;
                i++)
            {
                var rect = new Rectangle(SpriteConst.Width + ImageMargin + _margin,
                    8 + _margin + ((i - VerticalScrollIndex) * VerticalItemHeight),
                    Width - (SpriteConst.Width - ImageMargin) - (_margin * 2), VerticalItemHeight);
                var imageRect = new Rectangle(rect.X - SpriteConst.Width - ImageMargin, rect.Y - 8, rect.Width,
                    rect.Height);
                _items[i].Label.SetRect(rect);
                _items[i].ImageBox.SetRect(imageRect);
                _items[i].ImageBox.Recalculate();
                _items[i].Label.Recalculate();
                _items[i].Bounds = new Rectangle(_items[i].ImageBox.Bounds.Left, _items[i].ImageBox.Bounds.Top,
                    Width - (_margin * 2) - scrollWidth, VerticalItemHeight);
            }

            _scrollBar.Recalculate();
            _clickable.SetRect(CleanRect);
            IsDirty = false;
        }
    }
}
