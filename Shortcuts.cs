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
        private static readonly List<(Keys key, ModifierKeys modifier, Action action)> Keybindings = new List<(Keys key, ModifierKeys modifier, Action action)>();

        public static void Update()
        {
            foreach (var binding in Keybindings)
            {
                if (KeyboardManager.Pressed(binding.key) && KeyboardManager.IsModifierDown(binding.modifier))
                    binding.action();
            }
        }

        public static void Initialize()
        {
            Keybindings.Add((Keys.D1, ModifierKeys.Shift, SetPencil11));
            Keybindings.Add((Keys.D2, ModifierKeys.Shift, SetPencil33));
            Keybindings.Add((Keys.D3, ModifierKeys.Shift, SetPencil55));
            Keybindings.Add((Keys.D4, ModifierKeys.Shift, SetPencil99));
            Keybindings.Add((Keys.Q, ModifierKeys.Shift, SetToolPlace));
            Keybindings.Add((Keys.W, ModifierKeys.Shift, SetToolQuickPlace));
            Keybindings.Add((Keys.E, ModifierKeys.Shift, SetToolSelect));
            Keybindings.Add((Keys.R, ModifierKeys.Shift, SetToolQuickRemove));
            Keybindings.Add((Keys.T, ModifierKeys.Shift, SetToolRemove));
            Keybindings.Add((Keys.S, ModifierKeys.Ctrl, Save));
            Keybindings.Add((Keys.O, ModifierKeys.Ctrl, Load));
            Keybindings.Add((Keys.G, ModifierKeys.Ctrl, () => { 
                var map = IoC.Get<Map>();
                map.DrawGameGrid = !map.DrawGameGrid;
            }));
            
            // Keybindings.Add((Keys.P, ModifierKeys.Ctrl, () => Dialogs.Test.Show(new Point(400, 400))));
            // Keybindings.Add((Keys.O, ModifierKeys.Ctrl, () => 
            // MessageBox.Show(new Point(100, 100), "hello", "bye")));
            // Keybindings.Add((Keys.I, ModifierKeys.Ctrl, () => 
            // ConfirmationBox.Show(new Point(100, 100), "hello", null)));
        }

        public static Action SetToolPlace;
        public static Action SetToolQuickPlace;
        public static Action SetToolRemove;
        public static Action SetToolQuickRemove;
        public static Action SetToolSelect;
        public static Action SetPencil11;
        public static Action SetPencil33;
        public static Action SetPencil55;
        public static Action SetPencil99;
        public static Action Save;
        public static Action Load;
    }
}