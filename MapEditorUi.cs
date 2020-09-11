using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls;
using ArchivalTibiaV71MapEditor.Controls.Addons;
using ArchivalTibiaV71MapEditor.World;

namespace ArchivalTibiaV71MapEditor
{
    public static class MapEditorUi
    {
        public static void Setup()
        {
            MessageBox.Setup();
            ConfirmationBox.Setup();
            ContextMenu.Setup();
            var window = IoC.Get<IWindow>();
            var items = GameCollections.Items;

            var menu = CreateMenu(window);
            window.AddControl(menu);

            var leftPanel = new Panel(window, null, new Rectangle(0, 20, 300, window.Width));
            var statusLabel = new Label(window, leftPanel, Color.LimeGreen, new Rectangle(15, 10, 100, 16),
                "Position:");
            leftPanel.AddControl(statusLabel);

            var statusTextLabel = new Label(window, leftPanel, Color.White, new Rectangle(15, 26, 100, 16));
            leftPanel.AddControl(statusTextLabel);
            Ui.StatusTextLabel = statusTextLabel;
            
            // var paletteBox = new PaletteBox<int>(window, leftPanel, new Rectangle(150, 50, 120, 100), 5);
            // for (int i = 0; i < 20; i++)
            // {
            //     paletteBox.AddItem(new ImageBox(items.GetItem((uint)(1408 + i)).GetSprite(), new Point(32,32)), 1408 + i);
            // }
            //
            // paletteBox.OnSelect = item =>
            // {
            //     MessageBox.Show(window.Center(), $"You selected item with id {item}");
            // };
            // leftPanel.AddControl(paletteBox);

            /* Category list box */
            Categories.List = new ListBox<string>(
                window, leftPanel, new Rectangle(10, 50, 120, 116));
            Categories.List.AddItem(Categories.NoCategory, Categories.NoCategory);
            Categories.List.Select(Categories.NoCategory);

            leftPanel.AddControl(Categories.List);

            /* Tile List Boxes */
            static Rectangle TileListBoxRect() => new Rectangle(10, 170, 270, 760);

            /* no category */
            var noCategoryTileListBox =
                new MultiViewBox<int>(window, leftPanel, TileListBoxRect());
            noCategoryTileListBox.OnSelect = item => { Ui.SelectedTile = items.GetItem((uint) item).GetSprite(); };
            var uncategorized = items
                .All
                .Where(di => di.Categories.All(c => c == Categories.Items.First().Name))
                .Select(i => new ImageBox(i.GetSprite(), new Point(32, 32)));
            noCategoryTileListBox.AddRange(uncategorized, s => $"Id {s.Item.ItemId}", s => s.Item.ItemId);
            leftPanel.AddControl(noCategoryTileListBox);


            /* categories from categories.cats */
            MultiViewBox<int> CreateListBox(string name)
            {
                Categories.List.AddItem(name, name);
                var lst = new MultiViewBox<int>(window, leftPanel, TileListBoxRect(),
                    visible: false);
                lst.OnSelect = item => { Ui.SelectedTile = items.GetItem((uint) item).GetSprite(); };
                var toAdd = items
                    .All
                    .Where(i => i.Categories.Contains(name))
                    .Select(i => new ImageBox(i.GetSprite(), new Point(32, 32)));
                lst.AddRange(toAdd, s => $"Id {s.Item.ItemId}", s => s.Item.ItemId);
                leftPanel.AddControl(lst);
                return lst;
            }

            Categories.Translations = Categories.Items
                .Where(s => s.Name != "")
                .Select(s => s.Name)
                .ToDictionary(key => key, CreateListBox);
            Categories.Translations[Categories.NoCategory] = noCategoryTileListBox;

            Categories.List.OnSelect = Categories.Select;
            Categories.Select(Categories.NoCategory);

            window.AddControl(leftPanel);


            /* Tool Buttons */
            const int leftToolsX = Window.BorderSize;
            const int leftToolsY = Window.BorderSize;
            const int toolHeight = 24;
            const int toolMargin = 5;
            const int toolRightMargin = 60;
            var rightTopPanel = new Panel(window, null, new Rectangle(300, Window.BorderSize, 700, 64));

            var leftToolXOffset = leftToolsX;
            var leftToolYOffset = leftToolsY;

            IButton ToolButton(int width, int height, string text)
            {
                return new SmallButton(window, rightTopPanel,
                    new Rectangle(400, 0, width, height), text);
            }

            void AddTool(IButton button)
            {
                button.X = leftToolXOffset;
                button.Y = leftToolYOffset;
                leftToolXOffset += button.Width + toolMargin;
                rightTopPanel.AddControl(button);
            }

            void AddMargin(int width)
            {
                leftToolXOffset += width;
            }

            void NextRow()
            {
                leftToolYOffset += toolHeight + toolMargin;
                leftToolXOffset = leftToolsX;
            }

            IButton placeButton;
            IButton removeButton;
            IButton oneXButton;
            IButton fiveXButton;
            IButton selectButton;

            void AddFirstRow()
            {
                placeButton = ToolButton(60, 24, "Place");
                removeButton = ToolButton(80, 24, "Remove");
                selectButton = ToolButton(toolRightMargin - 5, 53, "Select");
                AddTool(placeButton);
                AddTool(removeButton);
                AddTool(selectButton);
                oneXButton = ToolButton(40, 24, "1x1");
                AddTool(oneXButton);
                fiveXButton = ToolButton(40, 24, "5x5");
                AddTool(fiveXButton);
            }


            IButton fastPlaceButton;
            IButton fastRemoveButton;
            IButton threeXButton;
            IButton nineXButton;

            void AddSecondRow()
            {
                fastPlaceButton = ToolButton(60, 24, "Q place");
                AddTool(fastPlaceButton);
                fastRemoveButton = ToolButton(80, 24, "Q remove");
                AddTool(fastRemoveButton);
                AddMargin(toolRightMargin);
                threeXButton = ToolButton(40, 24, "3x3");
                AddTool(threeXButton);
                nineXButton = ToolButton(40, 24, "9x9");
                AddTool(nineXButton);
            }

            AddFirstRow();
            NextRow();
            AddSecondRow();

            window.AddControl(rightTopPanel);

            /* Map */
            var map = new Map(window, null, new Rectangle(300, 64 + Window.BorderSize, Window.BorderSize + Scroll.Width,
                Window.BorderSize + Scroll.Height));
            window.AddControl(map);
            map.SetPlaceTool(MapTool.Place, placeButton);
            placeButton.OnClick = Shortcuts.SetToolPlace = () => { map.SetTool(MapTool.Place, placeButton); };
            fastPlaceButton.OnClick = Shortcuts.SetToolQuickPlace = () =>
            {
                map.SetTool(MapTool.QuickPlace, fastPlaceButton);
            };
            removeButton.OnClick = Shortcuts.SetToolRemove = () => { map.SetTool(MapTool.Remove, removeButton); };
            fastRemoveButton.OnClick = Shortcuts.SetToolQuickRemove = () =>
            {
                map.SetTool(MapTool.QuickRemove, fastRemoveButton);
            };
            selectButton.OnClick = Shortcuts.SetToolSelect = () => { map.SetTool(MapTool.Select, selectButton); };

            oneXButton.OnClick = Shortcuts.SetPencil11 = () => { map.SetPencilSize(1, 1, oneXButton); };
            threeXButton.OnClick = Shortcuts.SetPencil33 = () => { map.SetPencilSize(3, 3, threeXButton); };
            fiveXButton.OnClick = Shortcuts.SetPencil55 = () => { map.SetPencilSize(5, 5, fiveXButton); };
            nineXButton.OnClick = Shortcuts.SetPencil99 = () => { map.SetPencilSize(9, 9, nineXButton); };

            map.SetTool(MapTool.Place, placeButton);
            map.SetPencilSize(1, 1, oneXButton);
            IoC.Register<Map, Map>(map);
        }

        private static Menu CreateMenu(IWindow window)
        {
            var menu = new Menu(window, null, new Point(7, 3));
            /* file */
            var fileMenu = new ContextMenu(new[]
            {
                new MenuItem("New", () =>
                {
                    ConfirmationBox.Show(window.Center() - new Point(180, 100),
                        "Do you want to create a new map?",
                        () =>
                        {
                            var map = window.GetControl<Map>();
                            map.Clear();
                        });
                }),
                new MenuItem("Load\tCtrl+O",
                    Shortcuts.Load = () =>
                    {
                        ConfirmationBox.Show(window.Center() - new Point(180, 100),
                            "Do you want to load a map?",
                            () =>
                            {
                                var map = window.GetControl<Map>();
                                using var fs = File.OpenRead("world.map");
                                var reader = new MapReader(fs);
                                map.Read(reader);
                            });
                    }),
                new MenuItem("Save\tCtrl+S",
                    Shortcuts.Save = () =>
                    {
                        ConfirmationBox.Show(window.Center() - new Point(180, 100),
                            "Do you want to save the current map?",
                            () =>
                            {
                                var map = window.GetControl<Map>();
                                using var fs = File.OpenWrite("world.map");
                                fs.SetLength(0);
                                fs.Flush();
                                var writer = new MapWriter(fs);
                                map.Write(writer);
                                //map.WriteText("map.txt");
                            });
                    }),
                new MenuItem("----"),
                new MenuItem("Exit", IoC.Get<Game>().Exit),
            });
            menu.AddItem("File", fileMenu);

            /* view */
            var viewMenu = new ContextMenu(new[]
            {
                new MenuItem("Palette mode", Categories.DisplayPalette),
                new MenuItem("List mode", Categories.DisplayList),
            });
            menu.AddItem("View", viewMenu);
            
            /* generate */
            var generateMenu = new ContextMenu(new[]
            {
                new MenuItem("Minimap",
                    () =>
                    {
                        var map = window.GetControl<Map>();
                        map.WriteMinimap("minimap.png");
                    }),
            });
            menu.AddItem("Generate", generateMenu);

            /* help */
            var helpMenu = new ContextMenu(new[]
            {
                new MenuItem("About", () => { MessageBox.Show(window.Center(), "Author: Erik Iwarson"); }),
            });
            menu.AddItem("Help", helpMenu);
            return menu;
        }
    }
}