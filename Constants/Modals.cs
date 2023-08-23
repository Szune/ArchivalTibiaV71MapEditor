using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Controls;
using Microsoft.Xna.Framework;

namespace ArchivalTibiaV71MapEditor.Constants
{
    public static class Modals
    {
        private static readonly List<IModal> Items = new List<IModal>();

        public static int Count => Items.Count;

        public static void Add(IModal modal)
        {
            Items.Add(modal);
        }

        public static void RemoveLast()
        {
            Items.RemoveAt(Items.Count - 1);
        }

        public static void Update()
        {
            Items.Last().Update();
        }

        public static void Draw(int i, SpriteBatch sb, GameTime gameTime, DrawComponents drawComponents)
        {
            Items[i].Draw(sb, gameTime, drawComponents);
        }

        public static void RemoveThis(Guid id)
        {
            Items.RemoveAll(x => x.Id.Equals(id));
        }
    }
}
