using System;
using Microsoft.Xna.Framework;

namespace ArchivalTibiaV71MapEditor.Controls;

public enum ButtonStates
{
    Normal,
    Pressed
}
public interface IButton : IControl
{
    Color Color { get; set; }
    Action OnClick { get; set; }
}
