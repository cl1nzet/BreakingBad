using Engine.Core;
using Engine.Models;
using Engine.Utils;
using Game.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Models.Scenes
{
    public sealed class SettingsScene : Scene
    {
        public override string SceneName { get; set; } = "Settings";
        private readonly Storage _config;
        private readonly GraphicsDevice _graphics;
        private readonly AudioService _audio;

        public SettingsScene(Storage config, GraphicsDevice graphics, AudioService audio)
        {
            _config = config;
            _graphics = graphics;
            _audio = audio;
        }

        public override void Start()
        {
            var gameFont = AssetManager.GetFont("Arial");
            Outline outline = new Outline(Color.Black, 0.4f);

            var HeaderText = new Text(
                position: Screen.ScreenTop + new Vector2(0f, 25f),
                scale: Vector2.One * 1.7f,
                scene: this,
                text: "Настройки",
                font: gameFont,
                color: Color.Gray
            );
            HeaderText.AddOutline(outline);

            var ExitButton = new Button(
                position: Screen.ScreenRightDown - new Vector2(150f, 100f),
                scale: new Vector2(0.5f, 0.5f),
                scene: this,
                texture: AssetManager.GetTexture("Button"),
                text: "Обратно",
                font: AssetManager.GetFont("Arial"),
                textColor: Color.Black
            );

            float savedVolume = _config.Get("MusicVolume", 1.0f);

            var VolumeHeader = new Text(
                position: Screen.ScreenTopLeft + new Vector2(215f, 175f),
                scale: Vector2.One * 2f,
                scene: this,
                text: $"Громкость: {savedVolume:F1}",
                font: gameFont,
                color: Color.LightBlue
            );
            VolumeHeader.AddOutline(outline);

            var VolumeSlider = new Slider(
                position: Screen.ScreenTopLeft + new Vector2(200f, 225f),
                trackScale: Vector2.One * 2f,
                handleScale: Vector2.One * 2f,
                scene: this,
                trackTexture: null,
                handleTexture: null,
                graphicsDevice: _graphics,
                minValue: 0f,
                maxValue: 1f,
                startValue: savedVolume
            );

            VolumeSlider.OnValueChanged += (newValue) =>
            {
                _audio.MusicVolume = newValue * 0.2f;
                VolumeHeader.Content = $"Громкость: {newValue:F1}";
                _config.Set("MusicVolume", newValue.ToString());
                _config.Save();
            };

            ExitButton.OnClick += () => SceneManager.LoadScene(0);
        }
    }
}