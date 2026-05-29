using Breaking_Bad;
using Engine.Core;
using Engine.Models;
using Engine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Models.Scenes {
    public sealed class MenuScene : Scene {
        private const string AuthorName = "cl1nzet";
        public override string SceneName { get; set; } = "Menu";
        public override void Start() {
            Texture2D buttonTexture = AssetManager.GetTexture("Button");
            SpriteFont textFont = AssetManager.GetFont("Arial");

            var PlayButton = new Button(
                position: new Vector2(Screen.ScreenCenterX, Screen.ScreenCenterY - 200),
                scale: new Vector2(0.5f, 0.5f),
                scene: this,
                texture: buttonTexture,
                text: "Начать",
                font: textFont,
                textColor: Color.Black
            );
            var SettingsButton = new Button(
                position: Screen.ScreenCenter,
                scale: new Vector2(0.5f, 0.5f),
                scene: this,
                texture: buttonTexture,
                text: "Настройки",
                font: textFont,
                textColor: Color.Black
            );
            var ExitButton = new Button(
                position: new Vector2(Screen.ScreenCenterX, Screen.ScreenCenterY + 200),
                scale: new Vector2(0.5f, 0.5f),
                scene: this,
                texture: buttonTexture,
                text: "Выйти",
                font: textFont,
                textColor: Color.Black
            );
            var AuthorText = new Text(
                position: Screen.ScreenRightDown - new Vector2(100, 65),
                scale: Vector2.One * 1.4f,
                scene: this,
                text: $"by {AuthorName}",
                font: textFont,
                color: Color.Red
            );
            AuthorText.AddOutline(new Outline(Color.Black, 0.4f));

            PlayButton.OnClick += () => SceneManager.LoadScene("Difficulty");
            SettingsButton.OnClick += () => SceneManager.LoadScene("Settings", 0);
            ExitButton.OnClick += BreakingBad.AppQuit.Invoke;
        }
    }
}