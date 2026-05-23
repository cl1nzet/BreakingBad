using Engine.Core;
using Engine.Utils;
using Game.Models.Scenes;
using Game.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.IO;

namespace Breaking_Bad
{
    public sealed class BreakingBad : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private static Storage storage;
        public static Action AppQuit;
        public BreakingBad()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            storage = new Storage();
            storage.Initialize(path);
            TouchPanel.EnabledGestures = GestureType.Tap;

            Screen.Initialize(_graphics, Window);
            Screen.Resize(1920, 1080, false);
            Screen.SetWindowTitle(storage.Get("WinTitle", "Breaking Bad"));
            Window.AllowUserResizing = storage.Get("WindowResizing", false);

            AppQuit = Exit;

            base.Initialize();
        }

        protected override void LoadContent() {
            AssetManager.Init(Content);
            Content.Load<Texture2D>("Button");
            Content.Load<SpriteFont>("Arial");
            var menuScene = new MenuScene();
            var gameScene = new GameScene();
            SceneManager.Add(menuScene);
            SceneManager.Add(gameScene);

            SceneManager.LoadScene();

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }
        protected override void Update(GameTime gameTime)
        {
            SceneManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!IsActive) {
                return;
            }
            GraphicsDevice.Clear(Color.CornflowerBlue);
            SceneManager.Draw(_spriteBatch);

            base.Draw(gameTime);
        }
    }
}
