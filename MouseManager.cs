using System;
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
        private static bool _leftButton;
        private static bool _rightButton;
        private static bool _middleButton;

        public static bool AreAllUp() =>
            UiState.Mouse.LeftButton != ButtonState.Pressed &&
            UiState.Mouse.RightButton != ButtonState.Pressed &&
            UiState.Mouse.MiddleButton != ButtonState.Pressed;

        public static void Update()
        {
            _leftButton = UiState.Mouse.LeftButton == ButtonState.Pressed;
            _rightButton = UiState.Mouse.RightButton == ButtonState.Pressed;
            _middleButton = UiState.Mouse.MiddleButton == ButtonState.Pressed;
        }

        public static bool IsDown(MouseButton mouse) =>
            mouse switch
            {
                MouseButton.Left => _leftButton,
                MouseButton.Right => _rightButton,
                MouseButton.Middle => _middleButton,
                _ => throw new ArgumentOutOfRangeException(nameof(mouse), mouse, null)
            };

        public static bool IsUp(MouseButton mouse) =>
            !(mouse switch
            {
                MouseButton.Left => _leftButton,
                MouseButton.Right => _rightButton,
                MouseButton.Middle => _middleButton,
                _ => throw new ArgumentOutOfRangeException(nameof(mouse), mouse, null)
            });

        public static bool IsHovering(Rectangle bounds) =>
            !_leftButton &&
            !_rightButton &&
            !_middleButton &&
            bounds.Contains(UiState.Mouse.Position);
    }
}
