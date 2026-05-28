using Engine.Core;
using Engine.Models;
using Engine.Utils;
using Game.Core;
using Microsoft.Xna.Framework;

namespace Game.Models.Scenes {
    public sealed class DifficultyScene : Scene {
        public override string SceneName { get; set; } = "Difficulty";

        public override void Start() {
            float buttonRawHeight = AssetManager.GetTexture("Button").Height;
            float scaleY = 0.5f;
            float buttonHeight = buttonRawHeight * scaleY;
            float spacing = 10f;

            int totalButtons = 4;

            float totalHeight = (buttonHeight * totalButtons) + (spacing * (totalButtons - 1));

            float startY = Screen.ScreenCenterY - (totalHeight * 0.5f) + (buttonHeight * 0.5f);

            var EasyDifficultyButton = new Button(
                position: new Vector2(Screen.ScreenCenterX, startY),
                scale: new Vector2(0.5f, scaleY),
                scene: this,
                texture: AssetManager.GetTexture("Button"),
                text: "Лёгкая",
                font: AssetManager.GetFont("Arial"),
                textColor: Color.Black
            );

            var NormalDifficultyButton = new Button(
                position: new Vector2(Screen.ScreenCenterX, startY + 1 * (buttonHeight + spacing)),
                scale: new Vector2(0.5f, scaleY),
                scene: this,
                texture: AssetManager.GetTexture("Button"),
                text: "Нормальная",
                font: AssetManager.GetFont("Arial"),
                textColor: Color.Black
            );

            var HardDifficultyButton = new Button(
                position: new Vector2(Screen.ScreenCenterX, startY + 2 * (buttonHeight + spacing)),
                scale: new Vector2(0.5f, scaleY),
                scene: this,
                texture: AssetManager.GetTexture("Button"),
                text: "Сложная",
                font: AssetManager.GetFont("Arial"),
                textColor: Color.Black
            );

            var ImpossibleDifficultyButton = new Button(
                position: new Vector2(Screen.ScreenCenterX, startY + 3 * (buttonHeight + spacing)),
                scale: new Vector2(0.5f, scaleY),
                scene: this,
                texture: AssetManager.GetTexture("Button"),
                text: "Невозможная",
                font: AssetManager.GetFont("Arial"),
                textColor: Color.Black
            );

            var HeaderText = new Text(
                position: Screen.ScreenTop + new Vector2(0, 25f),
                scale: new Vector2(2f),
                scene: this,
                text: "Выберите сложность:",
                font: AssetManager.GetFont("Arial"),
                color: Color.LightYellow
            );

            var exitButton = new Button(
                position: Screen.ScreenRightDown - new Vector2(150f, 100f),
                scale: new Vector2(0.5f, 0.5f),
                scene: this,
                texture: AssetManager.GetTexture("Button"),
                text: "Обратно",
                font: AssetManager.GetFont("Arial"),
                textColor: Color.Black
            );

            exitButton.OnClick += SceneManager.LoadScene;
            EasyDifficultyButton.OnClick += () => SetDiff(Difficulty.Easy);
            NormalDifficultyButton.OnClick += () => SetDiff(Difficulty.Normal);
            HardDifficultyButton.OnClick += () => SetDiff(Difficulty.Hard);
            ImpossibleDifficultyButton.OnClick += () => SetDiff(Difficulty.Impossible);
        }

        public void SetDiff(Difficulty difficulty) {
            ChemicalEngine.CurrentDifficulty = difficulty;
            SceneManager.LoadScene("Game");
        }
    }
}