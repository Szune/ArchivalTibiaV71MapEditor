using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls;

namespace ArchivalTibiaV71MapEditor
{
    public static class Shortcuts
    {
        public static readonly List<(Keys key, ModifierKeys modifier, Action action, string HelpText)> Keybindings = new();

        public static void Update()
        {
            foreach (var binding in Keybindings)
            {
                if (KeyboardManager.Pressed(binding.key) && KeyboardManager.IsModifierDown(binding.modifier))
                    binding.action();
            }
        }

        public static string KeyToString(this Keys key, bool shift = true) =>
            key switch
            {
                Keys.D1 => "1",
                Keys.D2 => "2",
                Keys.D3 => "3",
                Keys.D4 => "4",
                Keys.D5 => "5",
                Keys.D6 => "6",
                Keys.D7 => "7",
                Keys.D8 => "8",
                Keys.D9 => "9",
                Keys.D0 => "0",
                Keys.OemPeriod => ".",
                Keys.OemComma => ",",
                Keys.OemMinus => "-",
                Keys.OemPlus => "+",
                var k => shift ? k.ToString() : k.ToString().ToLowerInvariant()
            };

        public static string ModifierToString(this ModifierKeys key) =>
            key switch
            {
                ModifierKeys.None => "",
                ModifierKeys.Ctrl => "Ctrl",
                ModifierKeys.Alt => "Alt",
                ModifierKeys.Shift => "Shift",
                ModifierKeys.CtrlAlt => "Ctrl+Alt",
                ModifierKeys.CtrlShift => "Ctrl+Shift",
                ModifierKeys.ShiftAlt => "Shift+Alt",
                ModifierKeys.CtrlShiftAlt => "Ctrl+Shift+Alt",
                _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            };

        public static void Initialize()
        {
            Keybindings.Add((Keys.D1, ModifierKeys.Shift, SetPencil11, "Set Pencil 1x1"));
            Keybindings.Add((Keys.D2, ModifierKeys.Shift, SetPencil33, "Set Pencil 3x3"));
            Keybindings.Add((Keys.D3, ModifierKeys.Shift, SetPencil55, "Set Pencil 5x5"));
            Keybindings.Add((Keys.D4, ModifierKeys.Shift, SetPencil99, "Set Pencil 9x9"));
            Keybindings.Add((Keys.A, ModifierKeys.Shift, SetToolMove, "Use tool Move"));
            Keybindings.Add((Keys.Q, ModifierKeys.Shift, SetToolPlace, "Use tool Place"));
            Keybindings.Add((Keys.W, ModifierKeys.Shift, SetToolQuickPlace, "Use tool Quick Place"));
            Keybindings.Add((Keys.E, ModifierKeys.Shift, SetToolSelect, "Use tool Select"));
            Keybindings.Add((Keys.R, ModifierKeys.Shift, SetToolQuickRemove, "Use tool Quick Remove"));
            Keybindings.Add((Keys.T, ModifierKeys.Shift, SetToolRemove, "Use tool Remove"));
            Keybindings.Add((Keys.Z, ModifierKeys.Shift, SetGroundFloor, "Go to ground floor"));
            Keybindings.Add((Keys.S, ModifierKeys.Ctrl, Save, "Save map"));
            Keybindings.Add((Keys.O, ModifierKeys.Ctrl, Load, "Load map"));
            Keybindings.Add((Keys.G, ModifierKeys.Ctrl, () => {
                var map = IoC.Get<Map>();
                map.DrawGameGrid = !map.DrawGameGrid;
            }, "Draw client grid"));

            // Keybindings.Add((Keys.P, ModifierKeys.Ctrl, () => Dialogs.Test.Show(new Point(400, 400))));
            // Keybindings.Add((Keys.O, ModifierKeys.Ctrl, () =>
            // MessageBox.Show(new Point(100, 100), "hello", "bye")));
            // Keybindings.Add((Keys.I, ModifierKeys.Ctrl, () =>
            // ConfirmationBox.Show(new Point(100, 100), "hello", null)));
        }

        public static Action SetToolMove;
        public static Action SetToolPlace;
        public static Action SetToolQuickPlace;
        public static Action SetToolRemove;
        public static Action SetToolQuickRemove;
        public static Action SetToolSelect;
        public static Action SetPencil11;
        public static Action SetPencil33;
        public static Action SetPencil55;
        public static Action SetPencil99;
        public static Action SetGroundFloor;
        public static Action Save;
        public static Action Load;
    }
}
