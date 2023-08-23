using System.IO;
using Microsoft.Xna.Framework;
using ArchivalTibiaV71MapEditor.Controls;
using ArchivalTibiaV71MapEditor.Controls.Addons;
using ArchivalTibiaV71MapEditor.World;

namespace ArchivalTibiaV71MapEditor.Constants
{
    public static class Dialogs
    {
        public static DialogWindow Test => IoC.Get<DialogWindow>(nameof(Test));
        public static DialogWindow LoadMap => IoC.Get<DialogWindow>(nameof(LoadMap));
        public static DialogWindow SaveMap => IoC.Get<DialogWindow>(nameof(SaveMap));

        public static void Setup()
        {
            LoadMapDialog();
            SaveMapDialog();
            TestDialog();
        }

        private static void TestDialog()
        {
            var testDialog = new DialogWindow(new Point(400, 500), "Test");
            var menu = new Menu(IoC.Get<IWindow>(), testDialog, new Point(2, 2));
            var ctxMenuFile = new ContextMenu(new[]
            {
                new MenuItem("Save", () => { MessageBox.Show(new Point(400, 400), "didn't save, sorry"); }),
                new MenuItem("Load", () => { MessageBox.Show(new Point(400, 400), "didn't load, sorry"); }),
            });

            var ctxMenuFile2 = new ContextMenu(new[]
            {
                new MenuItem("oof", () => { MessageBox.Show(new Point(400, 400), "oh no"); }),
                new MenuItem("aff", () => { MessageBox.Show(new Point(400, 400), "ouch"); }),
            });
            menu.AddItem("File", ctxMenuFile);
            menu.AddItem("Gile", ctxMenuFile2);
            menu.AddItem("Brile", ctxMenuFile2);
            testDialog.AddControl(menu);
            var lst = new ListBox<int>(IoC.Get<IWindow>(), testDialog, new Rectangle(40, menu.Y + menu.Height + 5, 100, 200));
            lst.AddItem("testtesth", 1);
            lst.AddItem("okserifk", 2);
            testDialog.AddControl(lst);
            var imgLst = new ImageListBox<int>(IoC.Get<IWindow>(), testDialog,
                new Rectangle(150, menu.Y + menu.Height + 5, 200, 200));
            imgLst.AddItem(new ImageBox(GameCollections.Items.GetItem(1411).GetSprite(), new Point(32, 32)), "Backpack", 1);
            imgLst.AddItem(new ImageBox(GameCollections.Items.GetItem(1410).GetSprite(), new Point(32, 32)), "Bag", 2);
            testDialog.AddControl(imgLst);
            IoC.Register(nameof(Test), testDialog);
        }

        private static void LoadMapDialog()
        {
            var window = IoC.Get<IWindow>();
            var dialog = new DialogWindow(new Point(300, 400), "Load Map...");
            var files =
                new DirectoryInfo(Settings.MapSaveLocation)
                    .EnumerateFiles("*.map", new EnumerationOptions
                    {
                        RecurseSubdirectories = true,
                        MatchType = MatchType.Simple,
                        IgnoreInaccessible = true,
                        MatchCasing = MatchCasing.CaseInsensitive
                    });
            var fileListBox = new ImageListBox<FileInfo>(window, dialog, new Rectangle(20, 30, 250, 300));

            foreach (var file in files)
            {
                var imageBox = new ImageBox(GameCollections.Items.GetItem(1380).GetSprite(), new Point(32, 32));
                fileListBox.AddItem(imageBox, file.Name, file);
            }
            dialog.AddControl(fileListBox);

            var loadButton = new SmallButton(window, dialog, new Rectangle(20, 400 - 60, 50, 24), "Load");
            loadButton.OnClick = () =>
            {
                var list = fileListBox;
                if (list.SelectedValue == null) return;

                var map = window.GetControl<Map>();
                using var fs = File.OpenRead(list.SelectedValue.FullName);
                var reader = new MapReader(fs);
                map.Read(reader);
                MessageBox.Show(window.Center(), "Loaded map " + list.SelectedValue?.Name);
                dialog.Close();
            };
            dialog.AddControl(loadButton);

            var cancelButton = new SmallButton(window, dialog, new Rectangle(90, 400 - 60, 50, 24), "Cancel");
            cancelButton.OnClick = () =>
            {
                dialog.Close();
            };
            dialog.AddControl(cancelButton);

            IoC.Register(nameof(LoadMap), dialog);

        }

        private static void SaveMapDialog()
        {
            var window = IoC.Get<IWindow>();
            var dialog = new DialogWindow(new Point(300, 500), "Save Map...");
            var files =
                new DirectoryInfo(Settings.MapSaveLocation)
                    .EnumerateFiles("*.map", new EnumerationOptions
                    {
                        RecurseSubdirectories = true,
                        MatchType = MatchType.Simple,
                        IgnoreInaccessible = true,
                        MatchCasing = MatchCasing.CaseInsensitive
                    });
            var fileListBox = new ImageListBox<FileInfo>(window, dialog, new Rectangle(20, 30, 250, 300));

            foreach (var file in files)
            {
                var imageBox = new ImageBox(GameCollections.Items.GetItem(1380).GetSprite(), new Point(32, 32));
                fileListBox.AddItem(imageBox, file.Name, file);
            }
            dialog.AddControl(fileListBox);

            var textSave = new TextBox(window, dialog, new Rectangle(20, 500 - 100, 200, 24));
            dialog.AddControl(textSave);

            fileListBox.OnSelect = (x) =>
            {
                var textBox = textSave;
                textBox.Text = x.Name;
            };

            var saveButton = new SmallButton(window, dialog, new Rectangle(20, 500 - 60, 50, 24), "Save");
            saveButton.OnClick = () =>
            {
                var text = textSave;

                if (string.IsNullOrWhiteSpace(text.Text))
                    return;

                var map = window.GetControl<Map>();
                using var fs = File.OpenWrite(Settings.MapSaveLocation + text.Text);
                fs.SetLength(0);
                fs.Flush();
                var writer = new MapWriter(fs);
                map.Write(writer);
                MessageBox.Show(window.Center(), "Saved map " + text.Text);
                dialog.Close();
            };
            dialog.AddControl(saveButton);

            var cancelButton = new SmallButton(window, dialog, new Rectangle(90, 500 - 60, 50, 24), "Cancel");
            cancelButton.OnClick = () =>
            {
                dialog.Close();
            };
            dialog.AddControl(cancelButton);

            IoC.Register(nameof(SaveMap), dialog);

        }
    }
}
