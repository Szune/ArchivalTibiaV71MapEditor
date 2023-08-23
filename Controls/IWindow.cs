using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public interface IWindow : IPanel
    {
        int Width { get; }
        int Height { get; }
        void AddControl(IControl control);
        void Draw(SpriteBatch sb, GameTime gameTime, DrawComponents drawComponents);
        void SetSize(int width, int height);
        void Dirty();
        void Update();
        T GetControl<T>() where T : IControl;
        T[] GetControls<T>() where T : IControl;
        Point Center();
    }
}
