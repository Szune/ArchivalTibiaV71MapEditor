using Microsoft.Xna.Framework.Input;

namespace ArchivalTibiaV71MapEditor
{
    public static class UiState
    {
        public static KeyboardState Keyboard;
        public static MouseState Mouse;
        private static int _lastScroll;
        public static int ScrollDelta;

        public static void Update()
        {
            Keyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            Mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            ScrollDelta = _lastScroll - Mouse.ScrollWheelValue;
            _lastScroll = Mouse.ScrollWheelValue;
            MouseManager.Update();
        }
    }
}