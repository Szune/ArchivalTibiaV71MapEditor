using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace ArchivalTibiaV71MapEditor
{
    [Flags]
    public enum ModifierKeys
    {
        None = 0,
        Ctrl = 1,
        Alt = 2,
        Shift = 4,
        CtrlAlt = Ctrl | Alt,
        CtrlShift = Ctrl | Shift,
        ShiftAlt = Shift | Alt,
        CtrlShiftAlt = Ctrl | Shift | Alt,
    }

    public static class KeyboardManager
    {
        private static readonly Dictionary<Keys, bool> LastState = new Dictionary<Keys, bool>();


        public static bool IsDown(Keys key)
        {
            return LastState[key];
        }

        public static bool IsUp(Keys key)
        {
            return !LastState[key];
        }

        public static bool Pressed(Keys key)
        {
            var pressed = UiState.Keyboard.IsKeyDown(key);
            if (LastState.ContainsKey(key))
            {
                if (LastState[key] == pressed)
                {
                    return false;
                }

                LastState.Remove(key);
                return false;
            }

            if (!pressed) return false;
            LastState[key] = true;
            return true;
        }

        public static bool IsModifierDown(ModifierKeys modifier)
        {
            return modifier switch
            {
                ModifierKeys.None => !IsCtrlDown() && !IsAltDown() && !IsShiftDown(),
                ModifierKeys.Ctrl => IsCtrlDown() && !IsAltDown() && !IsShiftDown(),
                ModifierKeys.Alt => IsAltDown() && !IsCtrlDown() && !IsShiftDown(),
                ModifierKeys.Shift => IsShiftDown() && !IsCtrlDown() && !IsAltDown(),
                ModifierKeys.CtrlAlt => IsCtrlDown() && IsAltDown() && !IsShiftDown(),
                ModifierKeys.CtrlShift => IsCtrlDown() && IsShiftDown() && !IsAltDown(),
                ModifierKeys.ShiftAlt => IsShiftDown() && IsAltDown() && !IsCtrlDown(),
                ModifierKeys.CtrlShiftAlt => IsCtrlDown() && IsShiftDown() && IsAltDown(),
                _ => false,
            };
        }

        private static bool IsCtrlDown()
        {
            return UiState.Keyboard.IsKeyDown(Keys.LeftControl) 
                   || UiState.Keyboard.IsKeyDown(Keys.RightControl);
        }
        
        private static bool IsShiftDown()
        {
            return UiState.Keyboard.IsKeyDown(Keys.LeftShift) 
                   || UiState.Keyboard.IsKeyDown(Keys.RightShift);
        }
        
        private static bool IsAltDown()
        {
            return UiState.Keyboard.IsKeyDown(Keys.LeftAlt) 
                   || UiState.Keyboard.IsKeyDown(Keys.RightAlt);
        }
    }
}