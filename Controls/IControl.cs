using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Controls.Addons;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public interface IControl : IHitTestable
    {
        bool Visible { get; set; }
        Rectangle Bounds { get; }
        IWindow Window { get; set; }
        int ZIndex { get; }
        IControl Parent { get; set; }
        void SetRect(Rectangle rect);
        void OffsetChild(ref Rectangle rect);
        void Dirty();
        void Draw(SpriteBatch sb, GameTime gameTime, DrawComponents drawComponents);
        int BorderSize { get; }
        int X { get; set; }
        int Y { get; set; }
        int Width { get; set; }
        int Height { get; set; }
    }
}
