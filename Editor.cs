using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArchivalTibiaV71MapEditor.Constants;
using ArchivalTibiaV71MapEditor.Controls;
using ArchivalTibiaV71MapEditor.Controls.Addons;
using ArchivalTibiaV71MapEditor.Extensions;
using ArchivalTibiaV71MapEditor.Fonts;
using ArchivalTibiaV71MapEditor.Readers;
using ArchivalTibiaV71MapEditor.Sprites;

namespace ArchivalTibiaV71MapEditor
{
    public class Editor : Game
    {
        private int _gameWidth = 1280;
        private int _gameHeight = 960;

        private UiRenderer _uiRenderer;
        private readonly GraphicsDeviceManager _graphics;

        private Window _window;
        private SpriteBatch _spriteBatch;
        private DrawComponents _drawComponents;
        private SpriteRenderer _spriteRenderer;
        private Map _map;
        private Panel[] _panels;
        private int _panelCount;
        private bool _waitForMouseUp;

        public Editor()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = _gameWidth;
            _graphics.PreferredBackBufferHeight = _gameHeight;
            _graphics.SynchronizeWithVerticalRetrace = true; // VSync
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsFixedTimeStep = true; // This removes the 60 fps limit
            IsMouseVisible = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
            Window.AllowUserResizing = true;
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            _window.SetSize(Window.ClientBounds.Width, Window.ClientBounds.Height);
        }

        protected override void LoadContent()
        {
            Window.Title = "ArchivalTibiaV71MapEditor";
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            var picReader = new PicReader(File.OpenRead("Tibia.pic"));
            var picSpriteSheets = picReader.ReadSpriteSheets(GraphicsDevice);
            Ui.SpriteSheet = picSpriteSheets[(int)SpriteSheets.Ui];
            
            Pixel.Setup(GraphicsDevice);
            Categories.Load();
            GameCollections.Load(GraphicsDevice);
            
            
            _window = new Window(_gameWidth, _gameHeight);
            IoC.Register<IFont, FontWithStroke>(new FontWithStroke(picSpriteSheets[(int)SpriteSheets.FontMonospacedWithStroke]));
            IoC.Register<IWindow, Window>(_window);
            IoC.Register<Game, Game>(this);
            MapEditorUi.Setup();
            Dialogs.Setup();
            Shortcuts.Initialize();
            _map = _window.GetControl<Map>();
            _panels = _window.GetControls<Panel>();
            _panelCount = _panels.Length;
            _uiRenderer = new UiRenderer();
            _spriteRenderer = new SpriteRenderer();
            _drawComponents = new DrawComponents(new FontRenderer(), _spriteRenderer);
        }

        protected override void Update(GameTime gameTime)
        {
            UiState.Update();
            Shortcuts.Update();

            if (!GraphicsDevice.Viewport.Bounds.Contains(UiState.Mouse.Position))
            {
                _window.LastHitTest = null;
            }

            if (Modals.Count > 0)
            {
                Modals.Update();
                _waitForMouseUp = true;
                return;
            }

            if (!_waitForMouseUp)
                _window.Update();
            else
                _waitForMouseUp = !MouseManager.AreAllUp();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _map.PreRender(_spriteBatch, _drawComponents);
            _uiRenderer.PreRender(_spriteBatch, Window.ClientBounds);
            for (var i = 0; i < _panelCount; i++)
            {
                _panels[i].PreRender(_spriteBatch, _drawComponents);
            }
            GraphicsDevice.SetRenderTarget(null);

            _spriteBatch.UsualBegin();

            _uiRenderer.DrawBackground(_spriteBatch);
            _window.Draw(_spriteBatch, _drawComponents);

            for (var i = 0; i < Modals.Count; i++)
                Modals.Draw(i, _spriteBatch, _drawComponents);
            
            _spriteBatch.UsualEnd();
        }
    }
}