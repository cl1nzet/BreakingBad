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
        private static Storage _config;
        public static Action AppQuit;
        private Image _background;
        private AudioService _audio;

        public BreakingBad()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _config = new Storage();
            _config.onFileCreated += SetDefaults;
            _config.Initialize("config.txt");

            TouchPanel.EnabledGestures = GestureType.Tap;

            Screen.Initialize(_graphics, Window);

            Screen.SetWindowTitle(_config.Get<string>("WinTitle", "ʙʀᴇᴀᴋɪɴɢ ʙᴀᴅ"));
            Window.AllowUserResizing = _config.Get<bool>("WindowResizing", false);

            AppQuit = Exit;

            base.Initialize();

            Screen.Resize(
                _config.Get<int>("ScreenWidth", 1920),
                _config.Get<int>("ScreenHeight", 1080),
                _config.Get<bool>("FullScreen", false)
            );

            Texture2D backgroundTexture = AssetManager.GetTexture("Background");

            _background = new Image(
                position: Screen.ScreenCenter,
                texture: backgroundTexture,
                scale: new Vector2((float)Screen.ScreenWidth / backgroundTexture.Width, (float)Screen.ScreenHeight / backgroundTexture.Height)
            );
            LoadScenes();
        }

        private void SetDefaults()
        {
            _config.Set("WinTitle", "ʙʀᴇᴀᴋɪɴɢ ʙᴀᴅ");
            _config.Set("ScreenWidth", "1920");
            _config.Set("ScreenHeight", "1080");
            _config.Set("FullScreen", "false");
            _config.Set("WindowResizing", "false");
            _config.Set("MusicVolume", "0.5");
            _config.Save();
        }

        protected override void LoadContent()
        {
            AssetManager.Init(Content);
            _audio = new AudioService(2);
            _audio.LoadMusic("MenuTheme");

            float savedVolume = _config.Get<float>("MusicVolume", 0.5f);
            _audio.MusicVolume = savedVolume * 0.2f;

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        private void LoadScenes()
        {
            SceneManager.OnSceneChanged += PlaybackTheme;

            var menuScene = new MenuScene();
            var settingsScene = new SettingsScene(_config, GraphicsDevice, _audio);
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

        protected override void Draw(GameTime gameTime)
        {
            if (!IsActive) return;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            if (!(SceneManager.CurrentScene is GameScene))
            {
                _background.Draw(_spriteBatch);
            }

            SceneManager.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void PlaybackTheme(Scene scene)
        {
            if (scene is GameScene)
            {
                _audio.StopMusic();
                return;
            }
            _audio.PlayMusic("MenuTheme");
        }
    }
}