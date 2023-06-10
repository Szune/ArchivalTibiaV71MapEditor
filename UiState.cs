using Microsoft.Xna.Framework.Input;

namespace ArchivalTibiaV71MapEditor
{
    public static class UiState
    {
        public static KeyboardState Keyboard;
        public static MouseState Mouse;
        private static int _lastScroll;
        public static int ScrollDelta;
        /// <summary>
        /// Whether the map editor window is active.
        /// </summary>
        public static bool IsActive;

        public static void Update(bool isActive)
        {
            IsActive = isActive;
            Keyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            Mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            ScrollDelta = _lastScroll - Mouse.ScrollWheelValue;
            _lastScroll = Mouse.ScrollWheelValue;
            MouseManager.Update();
        }
    }
}
