using Microsoft.Xna.Framework;
using ArchivalTibiaV71MapEditor.Controls;
using ArchivalTibiaV71MapEditor.Controls.Addons;

namespace ArchivalTibiaV71MapEditor.Constants
{
    public static class Dialogs
    {
        public static DialogWindow Test => IoC.Get<DialogWindow>(nameof(Test));

        public static void Setup()
        {
            var testDialog = new DialogWindow(new Point(400, 500), "Test");
            var menu = new Menu(IoC.Get<IWindow>(), testDialog, new Point(2, 2));
            var ctxMenuFile = new ContextMenu(new[]
            {
                new MenuItem("Save", () => {MessageBox.Show(new Point(400, 400), "didn't save, sorry");}), 
                new MenuItem("Load", () => {MessageBox.Show(new Point(400, 400), "didn't load, sorry");}), 
            });
            
            var ctxMenuFile2 = new ContextMenu(new[]
            {
                new MenuItem("oof", () => {MessageBox.Show(new Point(400, 400), "oh no");}), 
                new MenuItem("aff", () => {MessageBox.Show(new Point(400, 400), "ouch");}), 
            });
            menu.AddItem("File", ctxMenuFile);
            menu.AddItem("Gile", ctxMenuFile2);
            menu.AddItem("Brile", ctxMenuFile2);
            testDialog.AddControl(menu);
            var lst = new ListBox<int>(IoC.Get<IWindow>(), testDialog, new Rectangle(40, menu.Y + menu.Height + 5, 100, 200));
            lst.AddItem("testtesth", 1);
            lst.AddItem("okserifk", 2);
            testDialog.AddControl(lst);
            var imgLst = new ImageListBox<int>(IoC.Get<IWindow>(), testDialog, new Rectangle(150, menu.Y + menu.Height + 5, 200, 200));
            imgLst.AddItem(new ImageBox(GameCollections.Items.GetItem(1411).GetSprite(), new Point(32,32)), "Backpack", 1);
            imgLst.AddItem(new ImageBox(GameCollections.Items.GetItem(1410).GetSprite(), new Point(32,32)), "Bag", 2);
            testDialog.AddControl(imgLst);
            IoC.Register(nameof(Test), testDialog);
        }
    }
}