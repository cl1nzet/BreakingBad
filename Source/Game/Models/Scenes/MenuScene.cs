using Breaking_Bad;
using Engine.Core;
using Engine.Models;
using Engine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Models.Scenes
{
    public sealed class MenuScene : Scene
    {
        private const string AuthorName = "cl1nzet";
        public override string SceneName { get; set; } = "Menu";
        public override void Start()
        {
            Texture2D buttonTexture = AssetManager.GetTexture("Button");
            SpriteFont textFont = AssetManager.GetFont("Arial");
            SpriteAtlas atlas = new SpriteAtlas(AssetManager.GetTexture("icons_atlas"), 2, 4);

            Vector2 buttonScale = new Vector2(0.5f, 0.5f);
            Vector2 iconScale = new Vector2(0.5f, 0.5f);
            float offset = 160f;

            Vector2 playPos = new Vector2(Screen.ScreenCenterX, Screen.ScreenCenterY - 200);
            var PlayButton = new Button(playPos, buttonScale, this, buttonTexture, "Начать", textFont, Color.Black);
            var PlayIcon = new AtlasImage(new Vector2(playPos.X - offset, playPos.Y), iconScale, this, atlas, 0);

            Vector2 settingsPos = Screen.ScreenCenter;
            var SettingsButton = new Button(settingsPos, buttonScale, this, buttonTexture, "Настройки", textFont, Color.Black);
            var SettingsIcon = new AtlasImage(new Vector2(settingsPos.X - offset, settingsPos.Y), iconScale, this, atlas, 2);

            Vector2 exitPos = new Vector2(Screen.ScreenCenterX, Screen.ScreenCenterY + 200);
            var ExitButton = new Button(exitPos, buttonScale, this, buttonTexture, "Выйти", textFont, Color.Black);
            var ExitIcon = new AtlasImage(new Vector2(exitPos.X - offset, exitPos.Y), iconScale, this, atlas, 1);

            var AuthorText = new Text(Screen.ScreenRightDown - new Vector2(100, 65), Vector2.One * 1.4f, this, $"by {AuthorName}", textFont, Color.Red);
            AuthorText.AddOutline(new Outline(Color.Black, 0.4f));

            PlayButton.OnClick += () => SceneManager.LoadScene("Gamemode");
            SettingsButton.OnClick += () => SceneManager.LoadScene("Settings", 0);
            ExitButton.OnClick += BreakingBad.AppQuit.Invoke;
        }
    }
}