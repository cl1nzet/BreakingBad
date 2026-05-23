using Engine.Core;
using Engine.Core.Timer;
using Engine.Models;
using Engine.Utils;
using Game.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game.Models.Scenes {
    public sealed class GameScene : Scene
    {
        public override string SceneName { get; set; } = "Game";

        private InteractiveText _equationText;
        private Text _timerText;
        private int _timerID = -1;
        private ReactionData _currentReaction;

        public override void Start() {
            SpriteFont font = AssetManager.GetFont("Arial");

            _equationText = new InteractiveText(
                position: Screen.ScreenCenter,
                scene: this,
                font: font,
                color: Color.White
            );

            _timerText = new Text(
                position: new Vector2(Screen.ScreenCenterX, Screen.ScreenCenterY - 200),
                scene: this,
                text: "Осталось времени: 30с",
                font: font,
                color: Color.Red
            );

            var exitButton = new Button(
                position: new Vector2(Screen.ScreenWidth - 200, Screen.ScreenHeight - 200),
                scale: new Vector2(0.5f, 0.5f),
                scene: this,
                texture: AssetManager.GetTexture("Button"),
                text: "Обратно",
                font: AssetManager.GetFont("Arial"),
                textColor: Color.Black
            );

            exitButton.OnClick += SceneManager.LoadScene;

            StartNewRound();
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);

            if (_timerID != -1)
            {
                float remaining = TimerManager.GetRemainingTime(_timerID);
                if (remaining >= 0f)
                {
                    _timerText.Content = $"Осталось времени: {Math.Ceiling(remaining)}с";
                }
            }
        }

        private void StartNewRound()
        {
            _currentReaction = ChemicalEngine.Generate(Difficulty.Normal);

            _equationText.Content = FormatReaction(_currentReaction);

            _timerID = TimerManager.Add(30f, OnTimerExpired);
        }

        private void OnTimerExpired(int id)
        {
            if (id == _timerID)
            {
                StartNewRound();
            }
        }

        private string FormatReaction(in ReactionData reaction)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < reaction.Reactants.Length; i++)
            {
                sb.Append(reaction.Reactants[i].Formula);
                if (i < reaction.Reactants.Length - 1) sb.Append(" + ");
            }

            sb.Append(" = ");

            for (int i = 0; i < reaction.Products.Length; i++)
            {
                sb.Append(reaction.Products[i].Formula);
                if (i < reaction.Products.Length - 1) sb.Append(" + ");
            }

            return sb.ToString();
        }
    }
}