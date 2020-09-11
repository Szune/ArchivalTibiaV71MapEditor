using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArchivalTibiaV71MapEditor.Fonts
{
    public interface IFont
    {
        Texture2D FontSheet { get; }
        Point SymbolBoxSize { get; }
        Point SymbolMaxSize { get; }
        Vector2 GetOffset(char c);
        Rectangle GetPosition(char c);
        Vector2 MeasureString(string text);
    }
}