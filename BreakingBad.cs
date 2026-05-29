using Engine.Core;
using Engine.Core.Timer;
using Engine.Models;
using Engine.Utils;
using Game.Models.Scenes;
using Game.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace Breaking_Bad
{
    public sealed class BreakingBad : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private static Storage storage;
        public static Action AppQuit;
        private Image _background;
        private AudioService _audio;
        public BreakingBad()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            storage = new Storage();
            storage.onFileCreated += SetDefaults;
            storage.Initialize("config.txt");

            TouchPanel.EnabledGestures = GestureType.Tap;

            Screen.Initialize(_graphics, Window);

            Screen.SetWindowTitle(storage.Get("WinTitle", "ʙʀᴇᴀᴋɪɴɢ ʙᴀᴅ"));
            Window.AllowUserResizing = storage.Get("WindowResizing", false);

            AppQuit = Exit;

            base.Initialize();

            Screen.Resize(
                storage.Get("ScreenWidth", 1920),
                storage.Get("ScreenHeight", 1080),
                storage.Get("FullScreen", false)
            );

            Texture2D backgroundTexture = AssetManager.GetTexture("Background");

            _background = new Image(
                position: Screen.ScreenCenter,
                texture: backgroundTexture,
                scale: new Vector2((float)Screen.ScreenWidth / backgroundTexture.Width, (float)Screen.ScreenHeight / backgroundTexture.Height)
            );
            LoadScenes();
        }

        private void SetDefaults() {
            storage.Set("WinTitle", "ʙʀᴇᴀᴋɪɴɢ ʙᴀᴅ");
            storage.Set("ScreenWidth", "1920");
            storage.Set("ScreenHeight", "1080");
            storage.Set("FullScreen", "false");
            storage.Set("WindowResizing", "false");
            storage.Save();
        }

        protected override void LoadContent() {
            AssetManager.Init(Content);
            _audio = new AudioService(2);
            _audio.LoadMusic("MenuTheme");
            _audio.MasterVolume = 0.1f;
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        private void LoadScenes() {
            SceneManager.OnSceneChanged += PlaybackTheme;

            var menuScene = new MenuScene();
            var settingsScene = new SettingsScene();
            var difficultyScene = new DifficultyScene();
            var gameScene = new GameScene(GraphicsDevice);
            SceneManager.Add(menuScene);
            SceneManager.Add(settingsScene);
            SceneManager.Add(difficultyScene);
            SceneManager.Add(gameScene);

            SceneManager.LoadScene(0, 0);
        }

        protected override void Update(GameTime gameTime)
        {
            SceneManager.Update(gameTime);
            TimerManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            if (!IsActive) return;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            if (!(SceneManager.CurrentScene is GameScene)) {
                _background.Draw(_spriteBatch);
            }

            SceneManager.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void PlaybackTheme(Scene scene) {
            if (scene is GameScene) {
                _audio.StopMusic();
                return;
            }
            _audio.PlayMusic("MenuTheme");
        }
    }
}