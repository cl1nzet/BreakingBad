using Engine.Core;
using Engine.Core.Timer;
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
            storage = new Storage();
            storage.Initialize("config.json");
            TouchPanel.EnabledGestures = GestureType.Tap;

            Screen.Initialize(_graphics, Window);
            Screen.SetWindowTitle(storage.Get("WinTitle", "Breaking Bad"));
            Window.AllowUserResizing = storage.Get("WindowResizing", false);

            AppQuit = Exit;

            base.Initialize();
            Screen.Resize(1920, 1080, false);

            LoadScenes();
        }

        protected override void LoadContent() {
            AssetManager.Init(Content);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        private void LoadScenes() {
            var menuScene = new MenuScene();
            var difficultyScene = new DifficultyScene();
            var gameScene = new GameScene(GraphicsDevice);
            SceneManager.Add(menuScene);
            SceneManager.Add(difficultyScene);
            SceneManager.Add(gameScene);

            SceneManager.LoadScene();
        }

        protected override void Update(GameTime gameTime)
        {
            SceneManager.Update(gameTime);
            TimerManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!IsActive) {
                return;
            }
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            SceneManager.Draw(_spriteBatch);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
