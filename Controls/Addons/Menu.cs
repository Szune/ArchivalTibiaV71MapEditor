using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Fonts;

namespace ArchivalTibiaV71MapEditor.Controls.Addons
{
    public class Menu : ControlBase
    {
        private class RootMenuItem
        {
            public RootMenuItem(Menu m, string text, ContextMenu menu, Point position, int margin)
            {
                Button = new TextButton(IoC.Get<IWindow>(), m, position, text, margin);
                Menu = menu;
                Button.OnClick = () => 
                    Menu.Show(new Point(Button.Bounds.X, Button.Bounds.Y + Button.Height));
            }
            
            public TextButton Button { get; }
            public ContextMenu Menu { get; }
        }
        
        private readonly List<RootMenuItem> _items = new List<RootMenuItem>();

        private const int Margin = 8;

        private int NextX => _items.Count == 0 ? -Margin : _items.Last().Button.Width + _items.Last().Button.X; //_items.Sum(i => i.Button.Width) + _items.Count * Margin;
        
        public Menu(IWindow window, IControl parent, Point location, bool visible = true) : base(window, parent, visible)
        {
            SetRect(new Rectangle(location.X, location.Y, NextX, IoC.Get<IFont>().SymbolMaxSize.Y));
        }

        public void AddItem(string text, ContextMenu menu)
        {
            // add a button with a hover color
            // when the button is clicked, open the menu
            // add following menu items just to the right of this one
            _items.Add(new RootMenuItem(this, text, menu, new Point(NextX, CleanRect.Top), Margin));
        }

        public override void Draw(SpriteBatch sb, DrawComponents drawComponents)
        {
            if (!IsVisible())
                return;
            if(IsDirty)
                Recalculate();
            for (var i = 0; i < _items.Count; i++)
            {
                _items[i].Button.Draw(sb, drawComponents);
            }
        }

        public override HitBox HitTest()
        {
            for (var i = 0; i < _items.Count; i++)
            {
                var hitTest = _items[i].Button.HitTest();
                if (hitTest.IsHit)
                    return hitTest;
                hitTest = _items[i].Menu.HitTest();
                if (hitTest.IsHit)
                    return hitTest;
            }

            return HitBox.Miss;
        }

        public override void Recalculate()
        {
            SetRect(new Rectangle(X, Y, NextX, Height));
            for (int i = 0; i < _items.Count; i++)
            {
                _items[i].Button.Dirty();
                _items[i].Menu.Dirty();
            }
        }
    }
}