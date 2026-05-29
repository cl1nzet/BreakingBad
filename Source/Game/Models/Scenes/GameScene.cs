using Engine.Core;
using Engine.Core.Timer;
using Engine.Models;
using Engine.Utils;
using Game.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;

namespace Game.Models.Scenes
{
    public sealed class GameScene : Scene {
        public override string SceneName { get; set; } = "Game";

        private InteractiveText _equationText;
        private Text _timerText;
        private Text _solvedEquationsText;
        private SpriteFont _gameFont;

        private int _timerID = -1;
        private int _skipAttemps = 0;
        private ReactionData _currentReaction;
        private static float _maxTime;
        private GraphicsDevice _graphicsDevice;
        private const int _maxEquations = 10;
        private const int _maxSkipAttemps = 3;
        private int _solvedEquations = -1;
        private int _correctSolvedEquations = 0;

        public GameScene(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        public override void Start()
        {
            _gameFont = AssetManager.GetFont("Arial");
            Outline outline = new Outline(Color.Black, 0.4f);

            _equationText = new InteractiveText(
                position: new Vector2(Screen.ScreenCenterX, Screen.ScreenCenterY - 400),
                scale: Vector2.One * 1.2f,
                scene: this,
                font: _gameFont,
                color: Color.White
            );
            _equationText.AddOutline(outline);
            _timerText = new Text(
                position: new Vector2(Screen.ScreenCenterX, Screen.ScreenCenterY - 325),
                scale: Vector2.One * 1.2f,
                scene: this,
                text: "Осталось времени: 25с/25с",
                font: _gameFont,
                color: Color.Green
            );
            _timerText.AddOutline(outline);

            _solvedEquationsText = new Text(
                position: Screen.ScreenTop + new Vector2(0f, 25f),
                scale: Vector2.One * 1.2f,
                scene: this,
                text: $"Решено: 0/{_maxEquations} примеров",
                font: _gameFont,
                color: Color.LightBlue
            );
            _solvedEquationsText.AddOutline(outline);

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
                onVerify: VerifyEquation,
                onSkip: SkipRound
            );

            exitButton.OnClick += () => SceneManager.LoadScene(0);

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
            Vector2 centerPosition = Screen.ScreenCenter;
            bool isCorrect = ChemicalEngine.Verify(_currentReaction, equation);

            if (isCorrect) {
                _correctSolvedEquations++;

                if (_solvedEquations >= _maxEquations - 1) {
                    SessionEnd();
                    return;
                }

                var textPopup = new FloatingText(centerPosition, new Vector2(4f), this, _gameFont, Color.Green, "Правильно! (+1)");
                StartNewRound();
            }
            else {
                var errorPopup = new FloatingText(centerPosition, new Vector2(4f), this, _gameFont, Color.Red, "Ошибка! (X)");
            }
        }

        private void SkipRound() {
            FloatingText textPopup;
            if (_skipAttemps >= _maxSkipAttemps) {
                textPopup = new FloatingText(Screen.ScreenCenter, new Vector2(4f), this, _gameFont, Color.Red, "Все попытки утрачены! (X)");
                return;
            }
            _skipAttemps++;
            textPopup = new FloatingText(Screen.ScreenCenter, new Vector2(4f), this, _gameFont, Color.Gray, "Пропущено (->)");
            StartNewRound();
        }

        private void StartNewRound() {
            _solvedEquations++;
            _solvedEquationsText.Content = $"Решено: {_solvedEquations}/{_maxEquations} примеров ({_correctSolvedEquations})";
            ResetTimer();

            if (_solvedEquations >= _maxEquations) {
                SessionEnd();
                return;
            }
            _maxTime = ChemicalEngine.GetDifficultyMaxTime();
            _currentReaction = ChemicalEngine.Generate();

            _equationText.Content = FormatReaction(_currentReaction);
            
            _timerID = TimerManager.Add(_maxTime, OnTimerExpired);
        }

        private async Task AsyncSessionEnd() {
            var textPopup = new FloatingText(Screen.ScreenCenter, new Vector2(4f), this, _gameFont, Color.Green, "Сессия завершена! (ожидайте 2с)");
            _solvedEquations = -1;
            await Task.Delay(2000);
            SceneManager.LoadScene();
        }

        private void SessionEnd() => _ = AsyncSessionEnd();
        private void OnTimerExpired(int id)
        {
            if (id == _timerID) {
                var textPopup = new FloatingText(Screen.ScreenCenter, new Vector2(4f), this, _gameFont, Color.Gray, "Пропущено (->)");
                StartNewRound();
            }
        }
        private void ResetTimer() {
            if (_timerID != -1)
            {
                TimerManager.Remove(_timerID);
                _timerID = -1;
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

        public override void Unload() {
            base.Unload();
            ResetTimer();
            _solvedEquations = -1;
            _correctSolvedEquations = 0;
        }
    }
}