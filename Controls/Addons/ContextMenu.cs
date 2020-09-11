using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Fonts;

namespace ArchivalTibiaV71MapEditor.Controls.Addons
{
    public class MenuItem
    {
        public MenuItem(string text, Action onClick = default)
        {
            Text = text;
            OnClick = onClick;
        }

        public string Text { get; }
        public Action OnClick { get; }
    }

    public class ContextMenu : ControlBase, IModal
    {
        private const int MaxItems = 15;
        private static IFont _font;
        private readonly ListBox<MenuItem> _listBox;
        public Guid Id { get; } = Guid.NewGuid();
        private const int Margin = 5;

        public ContextMenu(IEnumerable<MenuItem> items, bool visible = true)
            : base(null, null, visible)
        {
            BorderSize = 1;
            var menuItems = items as MenuItem[] ?? items.ToArray();
            var maxX = (int)menuItems.Max(m => _font.MeasureString(m.Text).X);
            var yPerItem = ListBox<MenuItem>.ItemHeight;
            var visibleItems = Math.Min(MaxItems, menuItems.Length);
            var rect = new Rectangle(0,0, maxX + Margin * 2 + Scroll.Width, visibleItems * yPerItem + Margin * 2);
            SetRect(rect);
            _listBox = new ListBox<MenuItem>(null, null, rect, false, 1);
            _listBox.AddRange(menuItems, item => item.Text);
            _listBox.OnSelect = item =>
            {
                if (item.OnClick != null)
                {
                    Shortcuts.SetToolSelect();
                    Close();
                }
                _listBox.Unselect();
                item.OnClick?.Invoke();
            };
        }

        private void Close()
        {
            Modals.RemoveLast();
        }

        public static void Setup()
        {
            _font = IoC.Get<IFont>();
        }

        public void Show(Point position)
        {
            SetRect(new Rectangle(position.X, position.Y, _cleanRect.Width, _cleanRect.Height));
            IsDirty = true;
            Modals.Add(this);
        }

        public override void Draw(SpriteBatch sb, DrawComponents drawComponents)
        {
            if (!Visible)
                return;
            if (IsDirty)
                Recalculate();
            _listBox.Draw(sb, drawComponents);
        }

        public override void Recalculate()
        {
            base.Recalculate();
            _listBox.SetRect(new Rectangle(X, Y, Width, Height));
            IsDirty = false;
        }

        public void Update()
        {
            if (!Visible)
                return;

            if (_listBox.HitTest().IsHit)
            {
                IsDirty = true;
                return;
            }

            // hit test first, then check mouse is down outside of context menu
            if (MouseManager.IsDown(MouseButton.Left) && !CleanRect.Contains(UiState.Mouse.Position))
            {
                Shortcuts.SetToolSelect();
                Close();
            }
        }
    }
}