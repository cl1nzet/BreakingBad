using Engine.Core;
using Engine.Core.Timer;
using Engine.Models;
using Engine.Utils;
using Game.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game.Models.Scenes
{
    public sealed class GameScene : Scene {
        public override string SceneName { get; set; } = "Game";

        private InteractiveText _equationText;
        private Text _timerText;
        private Text _solvedEquationsText;
        private SpriteFont _gameFont;

        private int _timerID = -1;
        private ReactionData _currentReaction;
        private static float _maxTime;
        private GraphicsDevice _graphicsDevice;
        private const int _maxEquations = 10;
        private int _solvedEquations = -1;

        public GameScene(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public override void Start()
        {
            _gameFont = AssetManager.GetFont("Arial");

            _equationText = new InteractiveText(
                position: new Vector2(Screen.ScreenCenterX, Screen.ScreenCenterY - 400),
                scale: Vector2.One * 1.2f,
                scene: this,
                font: _gameFont,
                color: Color.White
            );

            _timerText = new Text(
                position: new Vector2(Screen.ScreenCenterX, Screen.ScreenCenterY - 350),
                scale: Vector2.One * 1.2f,
                scene: this,
                text: "Осталось времени: 25с/25с",
                font: _gameFont,
                color: Color.Green
            );

            _solvedEquationsText = new Text(
                position: Screen.ScreenTop + new Vector2(0f, 25f),
                scale: Vector2.One * 1.2f,
                scene: this,
                text: $"Решено: 0/{_maxEquations} примеров",
                font: _gameFont,
                color: Color.LightBlue
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

            var keyboard = new VirtualKeyboard(
                position: new Vector2(Screen.ScreenCenterX - 225, Screen.ScreenCenterY - 250),
                scene: this,
                font: _gameFont,
                text: _equationText,
                graphicsDevice: _graphicsDevice,
                onVerify: VerifyEquation
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
                    _timerText.Content = $"Осталось времени: {Math.Ceiling(remaining)}с/{_maxTime}с ";
                    _timerText.Color = GetTimerColor(remaining);
                }
            }
        }

        private Color GetTimerColor(float remainingTime)
        {
            float percentage = MathHelper.Clamp(remainingTime / _maxTime, 0f, 1f);

            if (percentage > 0.5f)
            {
                float t = (percentage - 0.5f) * 2f;
                return Color.Lerp(Color.Yellow, Color.Green, t);
            }
            else
            {
                float t = percentage * 2f;
                return Color.Lerp(Color.Red, Color.Yellow, t);
            }
        }

        private void VerifyEquation(string equation) {
            Vector2 centerPosition = new Vector2(Screen.ScreenCenterX, Screen.ScreenCenterY);
            bool isCorrect = ChemicalEngine.Verify(_currentReaction, equation);

            if (isCorrect) {
                var textPopup = new FloatingText(centerPosition, new Vector2(4f), this, _gameFont, Color.Green, "Правильно! (+1)");
                StartNewRound();
            }
            else {
                var errorPopup = new FloatingText(centerPosition, new Vector2(4f), this, _gameFont, Color.Red);
            }
        }

        private void StartNewRound()
        {
            if(_solvedEquations >= _maxEquations - 1) {
                SessionEnd();
                return;
            }
            if (_timerID != -1)
            {
                TimerManager.Remove(_timerID);
                _timerID = -1;
            }

            _maxTime = ChemicalEngine.GetDifficultyMaxTime();
            _currentReaction = ChemicalEngine.Generate();

            _equationText.Content = FormatReaction(_currentReaction);

            _timerID = TimerManager.Add(_maxTime, OnTimerExpired);
            _solvedEquations++;
            _solvedEquationsText.Content = $"Решено: {_solvedEquations}/{_maxEquations} примеров";
        }

        private void SessionEnd() {
            SceneManager.LoadScene();
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
                sb.Append("[]");
                sb.Append(reaction.Reactants[i].Formula);
                if (i < reaction.Reactants.Length - 1) sb.Append(" + ");
            }

            sb.Append(" = ");

            for (int i = 0; i < reaction.Products.Length; i++)
            {
                sb.Append("[]");
                sb.Append(reaction.Products[i].Formula);
                if (i < reaction.Products.Length - 1) sb.Append(" + ");
            }

            return sb.ToString();
        }
    }
}