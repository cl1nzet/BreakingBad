using Engine.Core;
using Engine.Models;
using Engine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Models.Scenes {
    public sealed class SettingsScene : Scene {
        public override string SceneName { get; set; } = "Settings";
        public override void Start() {
            var exitButton = new Button(
                position: Screen.ScreenRightDown - new Vector2(150f, 100f),
                scale: new Vector2(0.5f, 0.5f),
                scene: this,
                texture: AssetManager.GetTexture("Button"),
                text: "Обратно",
                font: AssetManager.GetFont("Arial"),
                textColor: Color.Black
            );

            exitButton.OnClick += () => SceneManager.LoadScene(0);
        }
    }
}