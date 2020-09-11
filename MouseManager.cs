using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ArchivalTibiaV71MapEditor
{
    public enum MouseButton
    {
        Left,
        Right,
        Middle
    }

    public static class MouseManager
    {
        // true == button down
        // false == button up
        private static readonly Dictionary<MouseButton, bool> LastState = new Dictionary<MouseButton, bool>
        {
            {MouseButton.Left, false},
            {MouseButton.Right, false},
            {MouseButton.Middle, false},
        };

        public static bool AreAllUp()
        {
            return UiState.Mouse.LeftButton != ButtonState.Pressed
            && UiState.Mouse.RightButton != ButtonState.Pressed
            && UiState.Mouse.MiddleButton != ButtonState.Pressed;
        }

        public static void Update()
        {
            LastState[MouseButton.Left] = UiState.Mouse.LeftButton == ButtonState.Pressed;
            LastState[MouseButton.Right] = UiState.Mouse.RightButton == ButtonState.Pressed;
            LastState[MouseButton.Middle] = UiState.Mouse.MiddleButton == ButtonState.Pressed;
        }
        public static bool IsDown(MouseButton mouse)
        {
            return LastState[mouse];
        }
        
        public static bool IsUp(MouseButton mouse)
        {
            return !LastState[mouse];
        }

        public static bool IsHovering(Rectangle bounds)
        {
            return IsUp(MouseButton.Left) && IsUp(MouseButton.Middle) && IsUp(MouseButton.Right) &&
                   bounds.Contains(UiState.Mouse.Position);
        }
    }
}