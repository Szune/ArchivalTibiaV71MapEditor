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
            var categoryReader = new CategoryReader(File.OpenRead("categories.cats"));
            var categories = categoryReader.ReadToEnd();
            Items = categories;
        }
    }
}