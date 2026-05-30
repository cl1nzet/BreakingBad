using Engine.Core;
using Engine.Models;
using Engine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Models.Scenes {
    public sealed class GamemodeScene : Scene {
        public override string SceneName { get; set; } = "Gamemode";
        public override void Start() {
            Vector2 buttonScale = new Vector2(0.5f);
            Texture2D buttonTexture = AssetManager.GetTexture("Button");
            SpriteFont font = AssetManager.GetFont("Arial");
            Outline outline = new Outline(Color.Black, 0.4f);

            var exitButton = new Button(Screen.ScreenRightDown - new Vector2(150f, 100f), buttonScale, this, buttonTexture, "Обратно", font, Color.Black);
            var classicModeButton = new Button(Screen.ScreenCenter - new Vector2(200f, 0f), buttonScale, this, buttonTexture, "Классический", font, Color.Black);
            var userModeButton = new Button(Screen.ScreenCenter + new Vector2(200f, 0f), buttonScale, this, buttonTexture, "Пользовательский", font, Color.Black);
            var HeaderText = new Text(Screen.ScreenTop + new Vector2(0f, 40f), Vector2.One * 2.5f, this, "Выберите игровой режим:", font, Color.White);
            HeaderText.AddOutline(outline);

            exitButton.OnClick += () => SceneManager.LoadScene("Menu");
            classicModeButton.OnClick += () => SceneManager.LoadScene("Difficulty");
        }
    }
}