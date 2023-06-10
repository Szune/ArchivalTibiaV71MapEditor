using System.Collections.Generic;
using System.IO;
using ArchivalTibiaV71MapEditor.Controls;
using ArchivalTibiaV71MapEditor.Readers;

namespace ArchivalTibiaV71MapEditor.Constants
{
    public static class Categories
    {
        public const string NoCategory = "No Category";
        public static Dictionary<string, MultiViewBox<int>> Translations;
        public static MultiViewBox<int> ActiveCategoryViewBox;
        public static bool ViewAsImageList { get; private set; }= true;
        public static ListBox<string> List;
        public static List<Category> Items;

        public static void DisplayList()
        {
            ViewAsImageList = true;
            ActiveCategoryViewBox?.Dirty();
        }

        public static void DisplayPalette()
        {
            ViewAsImageList = false;
            ActiveCategoryViewBox?.Dirty();
        }

        public static void Select(string item)
        {
            if (!Translations.TryGetValue(item, out var lstBox))
                return;
            SetCurrentTileListBox(lstBox);
        }

        private static void SetCurrentTileListBox(MultiViewBox<int> lst)
        {
            if (ActiveCategoryViewBox != null)
            {
                ActiveCategoryViewBox.Visible = false;
            }
            ActiveCategoryViewBox = lst;
            ActiveCategoryViewBox.Dirty();
            ActiveCategoryViewBox.Visible = true;
        }

        public static void Load()
        {
            const string fileName = "categories.cats";
            if (!File.Exists(fileName))
            {
                const string fileContents = "[All]\nRange(0,9999)\n";
                File.WriteAllText(fileName , fileContents);
            }

            var categoryReader = new CategoryReader(File.OpenRead(fileName));
            var categories = categoryReader.ReadToEnd();
            Items = categories;
        }
    }
}
