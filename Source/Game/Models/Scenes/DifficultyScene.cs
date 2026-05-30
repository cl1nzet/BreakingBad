using Engine.Core;
using Engine.Models;
using Engine.Utils;
using Game.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Models.Scenes
{
    public sealed class DifficultyScene : Scene
    {
        public override string SceneName { get; set; } = "Difficulty";

        public override void Start()
        {
            Texture2D buttonTexture = AssetManager.GetTexture("Button");
            SpriteFont font = AssetManager.GetFont("Arial");
            SpriteAtlas atlas = new SpriteAtlas(AssetManager.GetTexture("icons_atlas"), 2, 4);

            float scaleY = 0.5f;
            float buttonHeight = buttonTexture.Height * scaleY;
            float spacing = 10f;
            float totalHeight = (buttonHeight * 4f) + (spacing * 3f);
            float startY = Screen.ScreenCenterY - (totalHeight * 0.5f) + (buttonHeight * 0.5f);

            Vector2 buttonScale = new Vector2(0.5f, scaleY);
            Vector2 iconScale = new Vector2(0.5f, 0.5f);
            float offset = 160f;
            Outline outline = new Outline(Color.Black, 0.4f);

            Vector2 easyPos = new Vector2(Screen.ScreenCenterX, startY);
            var EasyDifficultyButton = new Button(easyPos, buttonScale, this, buttonTexture, "Лёгкая", font, Color.Black);
            var EasyIcon = new AtlasImage(new Vector2(easyPos.X - offset, easyPos.Y), iconScale, this, atlas, 4);

            Vector2 normalPos = new Vector2(Screen.ScreenCenterX, startY + (buttonHeight + spacing));
            var NormalDifficultyButton = new Button(normalPos, buttonScale, this, buttonTexture, "Нормальная", font, Color.Black);
            var NormalIcon = new AtlasImage(new Vector2(normalPos.X - offset, normalPos.Y), iconScale, this, atlas, 5);

            Vector2 hardPos = new Vector2(Screen.ScreenCenterX, startY + 2f * (buttonHeight + spacing));
            var HardDifficultyButton = new Button(hardPos, buttonScale, this, buttonTexture, "Сложная", font, Color.Black);
            var HardIcon = new AtlasImage(new Vector2(hardPos.X - offset, hardPos.Y), iconScale, this, atlas, 6);

            Vector2 impossiblePos = new Vector2(Screen.ScreenCenterX, startY + 3f * (buttonHeight + spacing));
            var ImpossibleDifficultyButton = new Button(impossiblePos, buttonScale, this, buttonTexture, "Невозможная", font, Color.Black);
            var ImpossibleIcon = new AtlasImage(new Vector2(impossiblePos.X - offset, impossiblePos.Y), iconScale, this, atlas, 7);

            var HeaderText = new Text(Screen.ScreenTop + new Vector2(0f, 40f), Vector2.One * 2.5f, this, "Выберите сложность:", font, Color.White);
            HeaderText.AddOutline(outline);

            var exitButton = new Button(Screen.ScreenRightDown - new Vector2(150f, 100f), buttonScale, this, buttonTexture, "Обратно", font, Color.Black);

            exitButton.OnClick += () => SceneManager.LoadScene("Gamemode");
            EasyDifficultyButton.OnClick += () => SetDiff(Difficulty.Easy);
            NormalDifficultyButton.OnClick += () => SetDiff(Difficulty.Normal);
            HardDifficultyButton.OnClick += () => SetDiff(Difficulty.Hard);
            ImpossibleDifficultyButton.OnClick += () => SetDiff(Difficulty.Impossible);
        }

        public void SetDiff(Difficulty difficulty)
        {
            ChemicalEngine.CurrentDifficulty = difficulty;
            SceneManager.LoadScene("Game");
        }
    }
}