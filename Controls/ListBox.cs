using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls.Addons;
using ArchivalTibiaV71MapEditor.Extensions;
using ArchivalTibiaV71MapEditor.Fonts;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class ListBox<T> : ControlBase, ICanHaveVerticalScrollBar
    {
        private readonly bool _sunken;

        private class ListBoxItem
        {
            public T Value;
            public Label Label;
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
                Label.Color = selected ? Color.Red : Color.White;
                Label.Draw(sb, gameTime, drawComponents);
            }
        }


        public int VerticalItemCount => _items.Count;
        public int VerticalScrollIndex { get; private set; }
        public const int ItemHeight = 18;
        public int VerticalMaxVisibleItems => Height / ItemHeight;
        public int VerticalMaxScrollIndex => Math.Max(VerticalItemCount - VerticalMaxVisibleItems, 0);
        private readonly int _margin;
        private readonly Texture2D _spriteSheet;
        private readonly IFont _font;
        private readonly List<ListBoxItem> _items = new List<ListBoxItem>();
        private Rectangle _destRectTop;
        private Rectangle _destRectLeft;
        private Rectangle _destRectRight;
        private Rectangle _destRectBottom;
        private readonly VerticalScrollBar _scrollBar;
        private ListBoxItem _selectedItem;
        public T SelectedValue => _selectedItem == null ? default : _selectedItem.Value;
        public Action<T> OnSelect { get; set; }
        private readonly Clickable _clickable = new Clickable();
        private const int HeightMargin = 2;

        public ListBox(IWindow window, IControl parent, Rectangle rect, bool sunken = true,
            int zIndex = 0, bool visible = true, int margin = 4)
            : base(window, parent, visible)
        {
            _sunken = sunken;
            Window = window;
            Parent = parent;
            _spriteSheet = Ui.SpriteSheet;
            _font = IoC.Get<IFont>();
            SetRect(rect);
            _clickable.SetRect(rect);
            ZIndex = zIndex;
            BorderSize = 1;
            _margin = margin + BorderSize;
            _scrollBar = new VerticalScrollBar(window, this, Border.Right);
        }

        public void AddItem(string text, T value)
        {
            var item = new ListBoxItem
            {
                Value = value,
                Label = new Label(Window, this, Rectangle.Empty, text, ZIndex + 1)
            };
            _items.Add(item);
            IsDirty = true;
        }

        public void AddRange(IEnumerable<T> items, Func<T, string> textFunc)
        {
            foreach (var item in items)
            {
                _items.Add(new ListBoxItem
                {
                    Value = item,
                    Label = new Label(Window, this, Rectangle.Empty, textFunc(item), ZIndex + 1),
                });
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

            if(VerticalItemCount > VerticalMaxVisibleItems)
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
            if (_sunken)
            {
                _destRectTop = new Rectangle(CleanRect.Left, CleanRect.Top, CleanRect.Width, 1);
                _destRectLeft = new Rectangle(CleanRect.Left, CleanRect.Top, 1, CleanRect.Height);
                _destRectRight = new Rectangle(CleanRect.Right - 1, CleanRect.Top, 1, CleanRect.Height);
                _destRectBottom = new Rectangle(CleanRect.Left, CleanRect.Bottom, CleanRect.Width, 1);
            }
            else
            {
                _destRectBottom = new Rectangle(CleanRect.Left, CleanRect.Top, CleanRect.Width, 1);
                _destRectRight = new Rectangle(CleanRect.Left, CleanRect.Top, 1, CleanRect.Height);
                _destRectLeft = new Rectangle(CleanRect.Right - 1, CleanRect.Top, 1, CleanRect.Height);
                _destRectTop = new Rectangle(CleanRect.Left, CleanRect.Bottom, CleanRect.Width, 1);
            }

            var scrollWidth = VerticalItemCount > VerticalMaxVisibleItems ? Scroll.Width : 0;

            for (int i = VerticalScrollIndex;
                i < _items.Count && i - VerticalScrollIndex < VerticalMaxVisibleItems;
                i++)
            {
                var yOffset = (_font.SymbolBoxSize.Y / 2);
                var rect = new Rectangle(_margin,
                    HeightMargin + ((i - VerticalScrollIndex) * ItemHeight) + yOffset / 2, Width - (_margin * 2),
                    ItemHeight);
                _items[i].Label.SetRect(rect);
                _items[i].Label.Recalculate();
                _items[i].Bounds = new Rectangle(_items[i].Label.Bounds.X - (_margin / 2),
                    _items[i].Label.Bounds.Y - yOffset / 2,
                    Width - _margin - _margin / 2 - scrollWidth, ItemHeight);
            }

            _scrollBar.Dirty();
            _clickable.SetRect(CleanRect);
            IsDirty = false;
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

        public void Unselect()
        {
            _selectedItem = null;
        }
    }
}
