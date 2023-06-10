using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls.Addons;
using ArchivalTibiaV71MapEditor.Extensions;
using ArchivalTibiaV71MapEditor.Readers;
using ArchivalTibiaV71MapEditor.World;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ArchivalTibiaV71MapEditor.Controls
{
    public class Map : ControlBase, ICanHaveVerticalScrollBar, ICanHaveHorizontalScrollBar
    {
        enum ActionState
        {
            None,
            Hovering,
            AddingTiles
        }

        private bool _isWaitingForRelease;
        private MouseButton? _waitingForButtonRelease;
        private readonly Rectangle _offset;
        private ActionState _state;
        private int _xTiles;
        private int _yTiles;
        private ushort _xOffset;
        private ushort _yOffset;
        private readonly Dictionary<Position, Tile> _tiles = new Dictionary<Position, Tile>();
        private readonly Clickable _clickable = new Clickable();
        private readonly Draggable _draggable = new Draggable(MouseButton.Middle, 5);
        private MapTool _tool = MapTool.Place;
        private bool _lastOnTop;
        private IButton _activeToolButton;
        private IButton _activeSizeButton;
        private Point _pencil = new Point(1, 1);
        private readonly Point _pencilOne = new Point(1, 1);
        private byte _currentZ = 7;

        public const int MaxX = 4096;
        public const int MaxY = 4096;
        public const int MaxZ = 15;
        public const int MinX = 0;
        public const int MinY = 0;
        public const int MinZ = 0;
        private bool _performingMapAction = false;
        private readonly VerticalScrollBar _verticalScrollBar;
        private readonly HorizontalScrollBar _horizontalScrollBar;
        private readonly MouseScrollable _mouseScrollable;

        private Point _renderSize = new Point(32, 32);
        private readonly Point _renderSizeMin = new Point(4, 4);
        private readonly Point _renderSizeMax = new Point(128, 128);
        private readonly Point _renderSizeStep = new Point(8, 8);
        private RenderTarget2D _cachedMap;
        private MapTool _placeTool;
        private IButton _placeButton;
        private ContextMenu _floorContextMenu;
        private ContextMenu _contextMenu;
        private bool _drawGameGrid;
        private Position _worldPosOfLastRightClick;

        public bool DrawGameGrid
        {
            get => _drawGameGrid;
            set
            {
                if (_drawGameGrid == value)
                    return;
                _drawGameGrid = value;
                IsDirty = true;
            }
        }

        public Map(IWindow window, IControl parent, Rectangle offset, bool visible = true)
            : base(window, parent, visible)
        {
            _offset = offset;
            IsDirty = true;
            BorderSize = 0;
            BuildContextMenus();

            _clickable.OnHover = () => _state = ActionState.Hovering;
            _clickable.OnLeftDown = () =>
            {
                _state = ActionState.AddingTiles;
                if (_lastOnTop)
                    return;
                PerformLeftMouseButtonAction(true);
            };
            _clickable.OnLeftClick = () =>
            {
                _lastOnTop = false;
                _state = ActionState.AddingTiles;
                PerformLeftMouseButtonAction(false);
            };
            _clickable.OnLeftUp = () =>
            {
                if (_isWaitingForRelease && _waitingForButtonRelease == MouseButton.Left)
                {
                    _isWaitingForRelease = false;
                    _waitingForButtonRelease = null;
                }
            };
            _clickable.OnRightClick = () =>
            {
                _worldPosOfLastRightClick = ScreenToWorld(GetMouseMapLocation());
                _contextMenu.Show(UiState.Mouse.Position);
            };
            _clickable.OnMouseLeave = () => _state = ActionState.None;
            _verticalScrollBar = new VerticalScrollBar(window, this, Border.Right);
            _verticalScrollBar.Offset(Scroll.Width);
            _horizontalScrollBar = new HorizontalScrollBar(window, this, Border.Bottom);
            _horizontalScrollBar.Offset(Scroll.Height);
            _mouseScrollable = new MouseScrollable(this, ModifierKeys.Ctrl);
            _mouseScrollable.OnScrollBackward = ZoomOut;
            _mouseScrollable.OnScrollForward = ZoomIn;
        }

        private void BuildContextMenus()
        {
            var ctxFloorMenuItems = new MenuItem[16];
            byte z = 15;
            for (var i = 0; i < 16; i++)
            {
                ctxFloorMenuItems[i] = new MenuItem($"Z {z}",
                    new Func<byte, Action>(idx => () =>
                    {
                        _currentZ = idx;
                        IsDirty = true;
                    })(z));
                // ^ awful way to capture the current iterator value and not the final value, but meh
                z--;
            }

            _floorContextMenu = new ContextMenu(ctxFloorMenuItems);
            var ctxMenuItems = new[]
            {
                new MenuItem("Floor", () =>
                {
                    _floorContextMenu.Show(UiState.Mouse.Position);
                }),
                new MenuItem("Properties", () =>
                {
                    if (_tiles.TryGetValue(_worldPosOfLastRightClick, out var item))
                    {
                        MessageBox.Show(Window.Center(),
                            $"Properties of ({_worldPosOfLastRightClick.X}, {_worldPosOfLastRightClick.Y}, {_worldPosOfLastRightClick.Z})\nTop item id: {item.GetTopItem().Item.ItemId}");
                    }
                    else
                    {
                        MessageBox.Show(Window.Center(),
                            $"Properties of ({_worldPosOfLastRightClick.X}, {_worldPosOfLastRightClick.Y}, {_worldPosOfLastRightClick.Z})");
                    }
                }),
            };
            _contextMenu = new ContextMenu(ctxMenuItems);
        }


        private void ZoomIn(int steps)
        {
            // use _renderSize
            var (x, y) = _renderSize;
            var newSize = new Point(
                (x - steps * _renderSizeStep.X).Clamp(_renderSizeMin.X, _renderSizeMax.X),
                (y - steps * _renderSizeStep.Y).Clamp(_renderSizeMin.Y, _renderSizeMax.Y));
            if (newSize.Equals(_renderSize))
                return;
            _renderSize = newSize;
            IsDirty = true;
        }

        private void ZoomOut(int steps)
        {
            var (x, y) = _renderSize;
            var newSize = new Point(
                (x - steps * _renderSizeStep.X).Clamp(_renderSizeMin.X, _renderSizeMax.X),
                (y - steps * _renderSizeStep.Y).Clamp(_renderSizeMin.Y, _renderSizeMax.Y));
            if (newSize.Equals(_renderSize))
                return;
            _renderSize = newSize;
            IsDirty = true;
        }

        private void PerformLeftMouseButtonAction(bool spam)
        {
            if (_isWaitingForRelease && _waitingForButtonRelease == MouseButton.Left)
            {
                return;
            }

            switch (_tool)
            {
                case MapTool.Place:
                    if (spam)
                        AddTile();
                    else
                        AddOnTop();
                    break;
                case MapTool.QuickPlace:
                    QuickPlace();
                    break;
                case MapTool.Remove:
                    if (spam)
                        RemoveTile();
                    else
                        RemoveOnTop();
                    break;
                case MapTool.Select:
                    SelectOnTop();
                    break;
                case MapTool.QuickRemove:
                    QuickRemove();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown tool {_tool}");
            }

            IsDirty = true;
        }

        private void QuickPlace()
        {
            bool AddSingleTile(Position position)
            {
                var tile = new Tile(Ui.SelectedTile, position);
                if (!_tiles.TryGetValue(position, out var oldTile))
                {
                    // no tile here yet, add tile
                    _tiles[position] = tile;
                    return true;
                }
                else if (oldTile.Ground.Item.ItemId != tile.Ground.Item.ItemId)
                {
                    // new tile id, change existing ground tile but keep the items on top
                    _tiles[position].Ground = tile.Ground;
                    return true;
                }

                return false;
            }

            void AddSingleTileOnTop(Position position)
            {
                if (!_tiles.TryGetValue(position, out var tile))
                    return;
                if (tile.OnTop.Count > 0) // check if it's a new id
                {
                    // TODO: add dialog box to allow adding same on same? or just make it happen and actually draw them correctly so that it's visible?
                    var last = tile.OnTop[^1];
                    // for now, you can't add the multiple of the same item stacked on top of each other, will fix later
                    if (last.Item.ItemId != Ui.SelectedTile.Item.ItemId)
                        tile.AddItem(Ui.SelectedTile);
                }
                else // just add
                {
                    tile.AddItem(Ui.SelectedTile);
                }
            }

            void AddSomething(Position position)
            {
                if (!ValidatePosition(position))
                    return;
                if (Ui.SelectedTile.Item.Type == DatCategories.Tiles && AddSingleTile(position))
                    return;
                AddSingleTileOnTop(position);
            }

            if (Ui.SelectedTile == null)
                return;

            var pos = ScreenToWorld(GetMouseMapLocation());
            if (_pencilOne.Equals(_pencil))
            {
                AddSomething(pos);
            }
            else
            {
                for (var y = 0; y < _pencil.Y; y++)
                {
                    for (var x = 0; x < _pencil.X; x++)
                    {
                        var cPos = new Position((ushort) (pos.X + x), (ushort) (pos.Y + y), pos.Z);
                        AddSomething(cPos);
                    }
                }
            }
        }


        public override void Draw(SpriteBatch sb, DrawComponents drawComponents)
        {
            if (!Visible)
                return;
            if (IsDirty)
                Recalculate();
            sb.Draw(_cachedMap, CleanRect, Color.White);

            _verticalScrollBar.Draw(sb, drawComponents);
            _horizontalScrollBar.Draw(sb, drawComponents);
            if (_state == ActionState.None) return;
            // draw location should lock on to the closest 32x32 tiles
            var hoverLocation = GetHoverLocation();
            if (!CleanRect.Contains(hoverLocation))
                return;
            if ((_tool == MapTool.Place || _tool == MapTool.QuickPlace) && Ui.SelectedTile != null)
            {
                DrawHoverSprite(sb, drawComponents, hoverLocation);
            }

            DrawHoverGrid(sb, hoverLocation.ToPoint());
            if (DrawGameGrid)
                DrawGameScreenGrid(sb, hoverLocation.ToPoint());
            var worldPos = ScreenToWorld(GetMouseMapLocation());
            Ui.StatusTextLabel.Text = $"({worldPos.X}, {worldPos.Y}, {worldPos.Z})";
        }

        private void DrawHoverSprite(SpriteBatch sb, DrawComponents drawComponents, Vector2 location)
        {
            if (_pencilOne.Equals(_pencil))
            {
                var parts = Ui.SelectedTile.GetParts(location, _renderSize);
                drawComponents.SpriteRenderer.Draw(sb, parts, Color.White);
            }
            else
            {
                var topLeftX = location.X;
                var topLeftY = location.Y;
                for (var y = 0; y < _pencil.Y; y++)
                {
                    for (var x = 0; x < _pencil.X; x++)
                    {
                        var drawLocation = new Vector2(topLeftX + (x * _renderSize.X), topLeftY + (y * _renderSize.Y));
                        if (!CleanRect.Contains(drawLocation))
                            continue;
                        var parts = Ui.SelectedTile.GetParts(drawLocation, _renderSize);
                        drawComponents.SpriteRenderer.Draw(sb, parts, Color.White);
                    }
                }
            }
        }

        private void DrawHoverGrid(SpriteBatch sb, Point location)
        {
            if (_pencilOne.Equals(_pencil))
            {
                DrawSingleGrid(sb, location.X, location.Y);
            }
            else
            {
                var topLeftX = location.X;
                var topLeftY = location.Y;
                for (var y = 0; y < _pencil.Y; y++)
                {
                    for (var x = 0; x < _pencil.X; x++)
                    {
                        var drawLocation = new Point(topLeftX + (x * _renderSize.X), topLeftY + (y * _renderSize.Y));
                        if (!CleanRect.Contains(drawLocation))
                            continue;
                        DrawSingleGrid(sb, drawLocation.X, drawLocation.Y);
                    }
                }
            }
        }

        private void DrawGameScreenGrid(SpriteBatch sb, Point location)
        {
            var topLeftX = location.X;
            var topLeftY = location.Y;
            var width = _renderSize.X * 15;
            var height = _renderSize.Y * 11;
            DrawHollowRect(sb, topLeftX, topLeftY, width, height, Color.Chartreuse);
        }

        private void DrawHollowRect(SpriteBatch sb, int x, int y, int width, int height, Color color)
        {
            sb.Draw(Pixel.White, new Rectangle(x, y, width, 1), color);
            sb.Draw(Pixel.White, new Rectangle(x, y, 1, height), color);
            sb.Draw(Pixel.White, new Rectangle(x + width, y, 1, height),
                color);
            sb.Draw(Pixel.White, new Rectangle(x, y + height, width, 1),
                color);
        }

        private void DrawSingleGrid(SpriteBatch sb, int x, int y)
        {
            sb.Draw(Pixel.White, new Rectangle(x, y, _renderSize.X, 1), Color.White);
            sb.Draw(Pixel.White, new Rectangle(x, y, 1, _renderSize.Y), Color.White);
            sb.Draw(Pixel.White, new Rectangle(x + _renderSize.X, y, 1, _renderSize.Y),
                Color.White);
            sb.Draw(Pixel.White, new Rectangle(x, y + _renderSize.Y, _renderSize.X, 1),
                Color.White);
        }

        private Vector2 WorldToRenderTarget(Position position)
        {
            return new Vector2((position.X - _xOffset) * _renderSize.X,
                (position.Y - _yOffset) * _renderSize.Y);
        }

        private Vector2 WorldToScreen(Position position)
        {
            return new Vector2(CleanRect.Left + (position.X - _xOffset) * _renderSize.X,
                CleanRect.Top + (position.Y - _yOffset) * _renderSize.Y);
        }

        private Position ScreenToWorld(Vector2 position)
        {
            return new Position((ushort) (_xOffset + (ushort) (position.X / _renderSize.X)),
                (ushort) (_yOffset + (ushort) (position.Y / _renderSize.Y)), _currentZ);
        }

        private Vector2 GetHoverLocation()
        {
            var mapMouse = GetMouseMapLocation();
            return new Vector2(CleanRect.Left + (int) (mapMouse.X / _renderSize.X) * _renderSize.X,
                CleanRect.Top + (int) (mapMouse.Y / _renderSize.Y) * _renderSize.Y);
        }

        private Vector2 GetMouseMapLocation()
        {
            var (windowMouseX, windowMouseY) = UiState.Mouse.Position.ToVector2();
            return new Vector2(windowMouseX - CleanRect.Left, windowMouseY - CleanRect.Top);
        }

        public override HitBox HitTest()
        {
            if (!IsVisible())
                return HitBox.Miss;

            if (_performingMapAction)
            {
                _clickable.HitTest(MouseButton.Left);
                if (_clickable.IsDown(MouseButton.Left))
                    return HitBox.Hit(this);
                else
                    _performingMapAction = false;
                return HitBox.Miss;
            }

            HitBox hit;
            if ((hit = _verticalScrollBar.HitTest()).IsHit)
            {
                return hit;
            }
            else if ((hit = _horizontalScrollBar.HitTest()).IsHit)
            {
                return hit;
            }
            else if ((hit = _mouseScrollable.HitTest()).IsHit)
            {
                return hit;
            }
            else if (_draggable.HitTest().IsHit)
            {
                var delta = _draggable.GetMoveDelta();
                _xOffset = (ushort) ((int) (_xOffset + delta.X)).Clamp(MinX, MaxX);
                _yOffset = (ushort) ((int) (_yOffset + delta.Y)).Clamp(MinY, MaxY);
                IsDirty = true;
                _draggable.InvalidateDelta();
                return HitBox.Hit(this);
            }
            else if (_clickable.HitTest(MouseButton.Left))
            {
                _performingMapAction = true;
                return HitBox.Hit(this);
            }
            else if (_clickable.HitTest(MouseButton.Right))
            {
                return HitBox.Hit(this);
            }

            return HitBox.Miss;
        }

        private void AddOnTop()
        {
            void AddSingleTileOnTop(Position position)
            {
                if (!ValidatePosition(position))
                    return;
                if (!_tiles.TryGetValue(position, out var tile))
                    return;
                if (tile.OnTop.Count > 0) // check if it's a new id
                {
                    // TODO: add dialog box to allow adding same on same? or just make it happen and actually draw them correctly so that it's visible?
                    var last = tile.OnTop[^1];
                    // for now, you can't add the multiple of the same item stacked on top of each other, will fix later
                    if (last.Item.ItemId != Ui.SelectedTile.Item.ItemId)
                        tile.AddItem(Ui.SelectedTile);
                }
                else // just add
                {
                    tile.AddItem(Ui.SelectedTile);
                }

                _lastOnTop = true;
            }

            if (Ui.SelectedTile == null)
                return;
            if (Ui.SelectedTile.Item.Type == DatCategories.Tiles)
                return;

            var pos = ScreenToWorld(GetMouseMapLocation());
            if (_pencilOne.Equals(_pencil))
            {
                AddSingleTileOnTop(pos);
            }
            else
            {
                for (var y = 0; y < _pencil.Y; y++)
                {
                    for (var x = 0; x < _pencil.X; x++)
                    {
                        var cPos = new Position((ushort) (pos.X + x), (ushort) (pos.Y + y), pos.Z);
                        AddSingleTileOnTop(cPos);
                    }
                }
            }
        }

        private static bool ValidatePosition(Position pos) => pos.X <= MaxX && pos.Y <= MaxY && pos.Z <= MaxZ;

        private void AddTile()
        {
            void AddSingleTile(Position position)
            {
                if (!ValidatePosition(position))
                    return;
                var tile = new Tile(Ui.SelectedTile, position);
                if (!_tiles.TryGetValue(position, out var oldTile))
                {
                    // no tile here yet, add tile
                    _tiles[position] = tile;
                }
                else if (oldTile.Ground.Item.ItemId != tile.Ground.Item.ItemId)
                {
                    // new tile id, change existing ground tile but keep the items on top
                    _tiles[position].Ground = tile.Ground;
                }
            }

            if (Ui.SelectedTile == null)
                return;
            if (Ui.SelectedTile.Item.Type != DatCategories.Tiles)
                return;
            var pos = ScreenToWorld(GetMouseMapLocation());
            if (_pencilOne.Equals(_pencil))
            {
                AddSingleTile(pos);
            }
            else
            {
                for (var y = 0; y < _pencil.Y; y++)
                {
                    for (var x = 0; x < _pencil.X; x++)
                    {
                        var cPos = new Position((ushort) (pos.X + x), (ushort) (pos.Y + y), pos.Z);
                        AddSingleTile(cPos);
                    }
                }
            }
        }

        private void QuickRemove()
        {
            var pos = ScreenToWorld(GetMouseMapLocation());
            if (_pencilOne.Equals(_pencil))
            {
                if (!_tiles.ContainsKey(pos))
                    return;
                _tiles.Remove(pos);
            }
            else
            {
                for (var y = 0; y < _pencil.Y; y++)
                {
                    for (var x = 0; x < _pencil.X; x++)
                    {
                        var cPos = new Position((ushort) (pos.X + x), (ushort) (pos.Y + y), pos.Z);
                        if (!_tiles.ContainsKey(cPos))
                            continue;
                        _tiles.Remove(cPos);
                    }
                }
            }
        }

        private void RemoveTile()
        {
            void RemoveSingleTile(Position position)
            {
                if (!_tiles.TryGetValue(position, out var oldTile))
                    return;
                if (oldTile.OnTop.Count > 0)
                    return;
                _tiles.Remove(position);
            }

            var pos = ScreenToWorld(GetMouseMapLocation());
            if (_pencilOne.Equals(_pencil))
            {
                RemoveSingleTile(pos);
            }
            else
            {
                for (var y = 0; y < _pencil.Y; y++)
                {
                    for (var x = 0; x < _pencil.X; x++)
                    {
                        var cPos = new Position((ushort) (pos.X + x), (ushort) (pos.Y + y), pos.Z);
                        RemoveSingleTile(cPos);
                    }
                }
            }
        }

        private void RemoveOnTop()
        {
            void RemoveSingleOnTop(Position position)
            {
                if (!_tiles.TryGetValue(position, out var tile))
                    return;
                if (tile.OnTop.Count < 1)
                    return;
                _tiles[position].OnTop.RemoveAt(_tiles[position].OnTop.Count - 1);

                _lastOnTop = true;
            }

            var pos = ScreenToWorld(GetMouseMapLocation());
            if (_pencilOne.Equals(_pencil))
            {
                RemoveSingleOnTop(pos);
            }
            else
            {
                for (var y = 0; y < _pencil.Y; y++)
                {
                    for (var x = 0; x < _pencil.X; x++)
                    {
                        var cPos = new Position((ushort) (pos.X + x), (ushort) (pos.Y + y), pos.Z);
                        RemoveSingleOnTop(cPos);
                    }
                }
            }
        }

        private void SelectOnTop()
        {
            var pos = ScreenToWorld(GetMouseMapLocation());
            if (!_tiles.TryGetValue(pos, out var tile))
                return;
            if (tile.OnTop.Count < 1)
                Ui.SelectedTile = tile.Ground;
            else
                Ui.SelectedTile = tile.OnTop.Last();

            var category =
                Categories.Items.FirstOrDefault(it => it.Name != "All" && it.Contains(Ui.SelectedTile.Item.ItemId));
            if (category == null)
            {
                var listBox = Categories.Translations[Categories.NoCategory];
                Categories.List.Select(Categories.NoCategory);
                listBox.Select(Ui.SelectedTile.Item.ItemId);
            }
            else
            {
                var listBox = Categories.Translations[category.Name];
                Categories.List.Select(category.Name);
                listBox.Select(Ui.SelectedTile.Item.ItemId);
            }

            if (_placeButton != null)
            {
                _isWaitingForRelease = true;
                _waitingForButtonRelease = MouseButton.Left;
                SetTool(_placeTool, _placeButton);
            }
        }

        public override void Recalculate()
        {
            var width = Window.Width - _offset.Left - _offset.Width;
            var height = Window.Height - _offset.Top - _offset.Height;
            var x = _offset.Left;
            var y = _offset.Top;
            _xTiles = width / _renderSize.X;
            _yTiles = height / _renderSize.Y;
            width = _xTiles * _renderSize.X;
            height = _yTiles * _renderSize.Y;
            var mapRect = new Rectangle(x, y, width, height);
            SetRect(mapRect);
            _clickable.SetRect(mapRect);
            _draggable.SetRect(mapRect);
            _verticalScrollBar.Recalculate();
            _horizontalScrollBar.Recalculate();
            IsDirty = false;
        }

        public void Write(MapWriter writer)
        {
            foreach (var tile in _tiles)
            {
                writer.Write(tile.Value);
            }
        }

        public void WriteText(string fileName)
        {
            using var fs = File.CreateText(fileName);
            fs.BaseStream.SetLength(0);
            fs.Flush();
            var first = true;
            foreach (var tile in _tiles)
            {
                var pos = tile.Value.Position;
                if (first)
                {
                    fs.Write(
                        $"Tile {tile.Value.Ground.Item.ItemId} ({pos.X},{pos.Y},{pos.Z}) [{string.Join(", ", tile.Value.OnTop.Select(it => it.Item.ItemId))}]");
                    first = false;
                }
                else
                {
                    fs.Write(
                        $" | Tile {tile.Value.Ground.Item.ItemId} ({pos.X},{pos.Y},{pos.Z}) [{string.Join(", ", tile.Value.OnTop.Select(it => it.Item.ItemId))}]");
                }
            }
        }

        public void Read(MapReader reader)
        {
            using var fs = File.CreateText("map-fixes.log");
            fs.BaseStream.SetLength(0);
            fs.Flush();
            _tiles.Clear();
            (Tile tile, bool done, string err) t;
            while (!((t = reader.Read()).done))
            {
                if (t.err != null)
                    fs.WriteLine(t.err);

                if (t.tile == null)
                    continue;
                _tiles[t.tile.Position] = t.tile;
            }

            IsDirty = true;
        }

        public void SetTool(MapTool tool, IButton activeToolButton)
        {
            _tool = tool;
            if (_activeToolButton != null)
                _activeToolButton.Color = Color.White;
            _activeToolButton = activeToolButton;
            _activeToolButton.Color = Color.Red;
        }


        public void SetPencilSize(int width, int height, IButton button)
        {
            _pencil = new Point(width, height);
            if (_activeSizeButton != null)
                _activeSizeButton.Color = Color.White;
            _activeSizeButton = button;
            _activeSizeButton.Color = Color.Red;
        }

        public void WriteMinimap(string fileName)
        {
            if (_tiles.Count < 1)
                return;
            var minX = _tiles.Values.Min(t => t.Position.X);
            var minY = _tiles.Values.Min(t => t.Position.Y);
            var maxX = _tiles.Values.Max(t => t.Position.X);
            var maxY = _tiles.Values.Max(t => t.Position.Y);
            var width = maxX - minX;
            var height = maxY - minY;
            // TODO: cross-platform bitmap code
            using (var sprite = new Bitmap(width, height))
            {
                var sprData = sprite.LockBits(new System.Drawing.Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                unsafe // need for speed
                {
                    for (ushort y = minY; y < maxY - minY; y++)
                    {
                        for (ushort x = minX; x < maxX - minX; x++)
                        {
                            var pixel = (((y - minY) * maxX) + (x - minX) % maxX) * 4;
                            var bmpPtr = (byte*) sprData.Scan0 + pixel;
                            if (_tiles.TryGetValue(new Position(x, y, _currentZ), out var tile))
                            {
                                var minimapColor = Math.Max(tile.Ground.Item.MinimapColor,
                                    tile.OnTop.Count > 0 ? tile.OnTop.Max(s => s.Item.MinimapColor) : 0);
                                bmpPtr[0] = (byte) ((minimapColor % 6) * 0x33); // blue
                                bmpPtr[1] = (byte) (((minimapColor / 6) % 6) * 0x33); // green
                                bmpPtr[2] = (byte) ((minimapColor / 0x24) * 0x33); // red
                                bmpPtr[3] = 255; // alpha
                            }
                            else
                            {
                                bmpPtr[0] = 0;
                                bmpPtr[1] = 0;
                                bmpPtr[2] = 0;
                                bmpPtr[3] = 255;
                            }
                        }
                    }
                }

                sprite.UnlockBits(sprData);
                sprite.Save(fileName);
            }
        }

        public void Clear()
        {
            _tiles.Clear();
            IsDirty = true;
        }

        #region Scroll

        public int VerticalItemCount => MaxY;

        public int VerticalScrollIndex
        {
            get => _yOffset;
            private set
            {
                if (_yOffset == (ushort) value)
                    return;
                _yOffset = (ushort) value;
                IsDirty = true;
            }
        }

        public int VerticalMaxVisibleItems => _yTiles;
        public int VerticalMaxScrollIndex => Math.Max(VerticalItemCount - VerticalMaxVisibleItems, 0);

        public void VerticalScroll(int delta)
        {
            VerticalScrollIndex = (VerticalScrollIndex + delta).Clamp(0, VerticalMaxScrollIndex);
        }

        public void VerticalScrollTo(int index)
        {
            VerticalScrollIndex = index;
        }

        public int HorizontalItemCount => MaxX;

        public int HorizontalScrollIndex
        {
            get => _xOffset;
            private set
            {
                if (_xOffset == (ushort) value)
                    return;
                _xOffset = (ushort) value;
                IsDirty = true;
            }
        }

        public int HorizontalMaxVisibleItems => _xTiles;
        public int HorizontalMaxScrollIndex => Math.Max(HorizontalItemCount - HorizontalMaxVisibleItems, 0);

        public void HorizontalScroll(int delta)
        {
            HorizontalScrollIndex = (HorizontalScrollIndex + delta).Clamp(0, HorizontalMaxScrollIndex);
        }

        public void HorizontalScrollTo(int index)
        {
            HorizontalScrollIndex = index;
        }

        #endregion

        public void PreRender(SpriteBatch sb, DrawComponents drawComponents)
        {
            // two phase pre render
            if (!IsDirty) return;
            Recalculate();
            // draw to rendertarget2d and cache until dirty again
            _cachedMap?.Dispose();
            _cachedMap = new RenderTarget2D(sb.GraphicsDevice, CleanRect.Width, CleanRect.Height, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            sb.GraphicsDevice.SetRenderTarget(_cachedMap);
            sb.UsualBegin();
            sb.Draw(Pixel.Black, new Rectangle(0, 0, CleanRect.Width, CleanRect.Height), Color.White);

            byte startZ, endZ;
            if (_currentZ >= 7)
            {
                startZ = 7;
                endZ = (byte) (_currentZ + 1);
            }
            else
            {
                startZ = 0;
                endZ = 7;
            }

            for (short z = startZ; z != endZ; z++)
            {
                // draw tiles on current floor
                for (ushort y = _yOffset; y < _yOffset + _yTiles; y++)
                {
                    for (ushort x = _xOffset; x < _xOffset + _xTiles; x++)
                    {
                        if (!_tiles.TryGetValue(new Position(x, y, (byte) z), out var tile)) continue;
                        var location = WorldToRenderTarget(tile.Position);
                        //if (!CleanRect.Contains(location + (Vector2.One * (_renderSize.X - 1)))) continue;
                        var tileParts = tile.Ground.GetParts(location, _renderSize);
                        drawComponents.SpriteRenderer.Draw(
                            sb,
                            tileParts,
                            z == _currentZ
                                ? Color.White
                                : Color.DarkSlateGray
                        );
                    }
                }

                // draw items on top of tiles on current floor
                for (ushort y = _yOffset; y < _yOffset + _yTiles; y++)
                {
                    for (ushort x = _xOffset; x < _xOffset + _xTiles; x++)
                    {
                        if (!_tiles.TryGetValue(new Position(x, y, (byte) z), out var tile)) continue;
                        var location = WorldToRenderTarget(tile.Position);
                        //if (!CleanRect.Contains(location + (Vector2.One * (_renderSize.X - 1)))) continue;
                        for (int a = 0; a < tile.OnTop.Count; a++)
                        {
                            // var pixels = tile.OnTop[a].Item.BlendFrames;
                            // var offset = pixels - _renderSize.X;
                            // var onTopParts = tile.OnTop[a]
                            //     .GetParts(location - (Vector2.One * offset), new Point(pixels, pixels));
                            var onTopParts = tile.OnTop[a].GetParts(location, _renderSize);
                            drawComponents.SpriteRenderer.Draw(
                                sb,
                                onTopParts,
                                z == _currentZ
                                    ? Color.White
                                    : Color.DarkSlateGray
                            );
                        }
                    }
                }
            }

            sb.End();
            IsDirty = false;
        }

        public void SinglePhasePreRender(SpriteBatch sb, DrawComponents drawComponents)
        {
            if (!IsDirty) return;
            Recalculate();
            // draw to rendertarget2d and cache until dirty again
            _cachedMap?.Dispose();
            _cachedMap = new RenderTarget2D(sb.GraphicsDevice, CleanRect.Width, CleanRect.Height, false,
                SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            sb.GraphicsDevice.SetRenderTarget(_cachedMap);
            sb.UsualBegin();
            sb.Draw(Pixel.Black, new Rectangle(0, 0, CleanRect.Width, CleanRect.Height), Color.White);

            byte startZ, endZ;
            if (_currentZ >= 7)
            {
                startZ = 7;
                endZ = (byte) (_currentZ + 1);
            }
            else
            {
                startZ = 0;
                endZ = 7;
            }

            for (short z = startZ; z != endZ; z++)
            {
                for (ushort y = _yOffset; y < _yOffset + _yTiles; y++)
                {
                    for (ushort x = _xOffset; x < _xOffset + _xTiles; x++)
                    {
                        if (!_tiles.TryGetValue(new Position(x, y, (byte) z), out var tile)) continue;
                        var location = WorldToRenderTarget(tile.Position);
                        //if (!CleanRect.Contains(location + (Vector2.One * (_renderSize.X - 1)))) continue;
                        var tileParts = tile.Ground.GetParts(location, _renderSize);
                        drawComponents.SpriteRenderer.Draw(
                            sb,
                            tileParts,
                            z == _currentZ
                                ? Color.White
                                : Color.DarkSlateGray
                        );
                        for (int a = 0; a < tile.OnTop.Count; a++)
                        {
                            // var pixels = tile.OnTop[a].Item.BlendFrames;
                            // var offset = pixels - _renderSize.X;
                            // var onTopParts = tile.OnTop[a]
                            //     .GetParts(location - (Vector2.One * offset), new Point(pixels, pixels));
                            var onTopParts = tile.OnTop[a].GetParts(location, _renderSize);
                            drawComponents.SpriteRenderer.Draw(
                                sb,
                                onTopParts,
                                z == _currentZ
                                    ? Color.White
                                    : Color.DarkSlateGray
                            );
                        }
                    }
                }
            }

            sb.End();
            IsDirty = false;
        }

        public void SetPlaceTool(MapTool place, IButton placeButton)
        {
            _placeTool = place;
            _placeButton = placeButton;
        }
    }
}
