using Breaking_Bad;
using Engine.Core;
using Engine.Models;
using Engine.Utils;
using Microsoft.Xna.Framework;

namespace Game.Models.Scenes {
    public sealed class MenuScene : Scene {
        public override string SceneName { get; set; } = "Menu";
        public override void Start() {
            var PlayButton = new Button(
                position: new Vector2(Screen.ScreenCenterX, Screen.ScreenCenterY - 200),
                scale: new Vector2(0.5f, 0.5f),
                scene: this,
                texture: AssetManager.GetTexture("Button"),
                text: "Начать",
                font: AssetManager.GetFont("Arial"),
                textColor: Color.Black
            );
            var SettingsButton = new Button(
                position: Screen.ScreenCenter,
                scale: new Vector2(0.5f, 0.5f),
                scene: this,
                texture: AssetManager.GetTexture("Button"),
                text: "Настройки",
                font: AssetManager.GetFont("Arial"),
                textColor: Color.Black
            );
            var ExitButton = new Button(
                position: new Vector2(Screen.ScreenCenterX, Screen.ScreenCenterY + 200),
                scale: new Vector2(0.5f, 0.5f),
                scene: this,
                texture: AssetManager.GetTexture("Button"),
                text: "Выйти",
                font: AssetManager.GetFont("Arial"),
                textColor: Color.Black
            );

            PlayButton.OnClick += () => SceneManager.LoadScene("Game");
            ExitButton.OnClick += BreakingBad.AppQuit.Invoke;
        }
    }
}