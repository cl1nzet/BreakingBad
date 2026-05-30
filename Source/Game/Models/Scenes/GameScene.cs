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
    public sealed class GameScene : Scene
    {
        public override string SceneName { get; set; } = "Game";

        private InteractiveText _equationText;
        private Text _timerText;
        private Text _solvedEquationsText;
        private SpriteFont _gameFont;

        private int _timerID = -1;
        private int _skipAttemps = 0;
        private int _skips = 0;
        private ReactionData _currentReaction;
        private static float _maxTime;
        private GraphicsDevice _graphicsDevice;
        private const int _maxEquations = 10;
        private const int _maxSkipAttemps = 3;
        private int _solvedEquations = -1;
        private int _correctSolvedEquations = 0;

        private float _totalTimeSpent = 0f;
        private int _lastRemainingSec = -1;
        private bool _isSessionEnded = false;
        private float _panelAnimProgress = 0f;

        private Image _endPanelBg;
        private Text _endTitle;
        private Text _endDiffText;
        private Text _endScoreText;
        private Text _endSkipsText;
        private Text _endAvgTimeText;

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
                text: "",
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
                font: _gameFont,
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

            exitButton.OnClick += () => SceneManager.LoadScene("Difficulty");

            StartNewRound();
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);

            if (_isSessionEnded)
            {
                if (_panelAnimProgress < 1f)
                {
                    _panelAnimProgress += (float)gt.ElapsedGameTime.TotalSeconds * 2f;
                    if (_panelAnimProgress > 1f) _panelAnimProgress = 1f;

                    float t = _panelAnimProgress;
                    float ease = 1f - (1f - t) * (1f - t) * (1f - t);

                    Vector2 startPos = new Vector2(Screen.ScreenCenterX, Screen.ScreenHeight + 600f);
                    Vector2 currentCenter = Vector2.Lerp(startPos, Screen.ScreenCenter, ease);

                    _endPanelBg.Transform.Position = currentCenter;
                    _endTitle.Transform.Position = currentCenter - new Vector2(0f, 160f);
                    _endDiffText.Transform.Position = currentCenter - new Vector2(0f, 80f);
                    _endScoreText.Transform.Position = currentCenter - new Vector2(0f, 20f);
                    _endSkipsText.Transform.Position = currentCenter + new Vector2(0f, 40f);
                    _endAvgTimeText.Transform.Position = currentCenter + new Vector2(0f, 100f);
                }
                return;
            }

            if (_timerID != -1)
            {
                _totalTimeSpent += (float)gt.ElapsedGameTime.TotalSeconds;
                float remaining = TimerManager.GetRemainingTime(_timerID);

                if (remaining >= 0f)
                {
                    int currentSec = (int)Math.Ceiling(remaining);
                    if (currentSec != _lastRemainingSec)
                    {
                        _lastRemainingSec = currentSec;
                        _timerText.Content = $"Осталось времени: {currentSec}с/{_maxTime}с";
                        _timerText.Color = GetTimerColor(remaining);
                    }
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

            float tLow = percentage * 2f;
            return Color.Lerp(Color.Red, Color.Yellow, tLow);
        }

        private void VerifyEquation(string equation)
        {
            if (_isSessionEnded) return;

            Vector2 centerPosition = Screen.ScreenCenter;
            bool isCorrect = ChemicalEngine.Verify(_currentReaction, equation);

            if (isCorrect)
            {
                _correctSolvedEquations++;

                if (_solvedEquations >= _maxEquations - 1)
                {
                    SessionEnd();
                    return;
                }

                _ = new FloatingText(centerPosition, new Vector2(4f), this, _gameFont, Color.Green, "Правильно! (+1)");
                StartNewRound();
            }
            else
            {
                _ = new FloatingText(centerPosition, new Vector2(4f), this, _gameFont, Color.Red, "Ошибка! (X)");
            }
        }

        private void SkipRound()
        {
            if (_isSessionEnded) return;

            if (_skipAttemps >= _maxSkipAttemps)
            {
                _ = new FloatingText(Screen.ScreenCenter, new Vector2(4f), this, _gameFont, Color.Red, "Все попытки утрачены! (X)");
                return;
            }

            _skipAttemps++;
            _skips++;
            _ = new FloatingText(Screen.ScreenCenter, new Vector2(4f), this, _gameFont, Color.Gray, "Пропущено (->)");
            StartNewRound();
        }

        private void StartNewRound()
        {
            _solvedEquations++;
            _solvedEquationsText.Content = $"Решено: {_solvedEquations}/{_maxEquations} примеров ({_correctSolvedEquations})";
            ResetTimer();

            if (_solvedEquations >= _maxEquations)
            {
                SessionEnd();
                return;
            }

            _maxTime = ChemicalEngine.GetDifficultyMaxTime();
            _currentReaction = ChemicalEngine.Generate();

            _equationText.Content = FormatReaction(_currentReaction);
            _timerID = TimerManager.Add(_maxTime, OnTimerExpired);
            _lastRemainingSec = -1;
        }

        private void SessionEnd()
        {
            ResetTimer();
            _isSessionEnded = true;
            _panelAnimProgress = 0f;

            float avgTime = _maxEquations > 0 ? _totalTimeSpent / _maxEquations : 0f;
            Vector2 startPos = new Vector2(Screen.ScreenCenterX, Screen.ScreenHeight + 600f);
            Outline outline = new Outline(Color.Black, 0.4f);

            _endPanelBg = new Image(startPos, new Vector2(0.5f, 0.8f), this, AssetManager.GetTexture("sign"), Color.DarkGray);

            _endTitle = new Text(startPos, Vector2.One * 1.5f, this, "Итоги", _gameFont, Color.White);
            _endTitle.AddOutline(outline);

            _endDiffText = new Text(startPos, Vector2.One * 1.2f, this, $"Сложность: {GetDifficultyName()}", _gameFont, Color.Red);
            _endDiffText.AddOutline(outline);

            _endScoreText = new Text(startPos, Vector2.One * 1.2f, this, $"Решено: {_correctSolvedEquations}/{_maxEquations}", _gameFont, Color.Green);
            _endScoreText.AddOutline(outline);

            _endSkipsText = new Text(startPos, Vector2.One * 1.2f, this, $"Пропущено: {_skips}/{_maxEquations}", _gameFont, Color.Gray);
            _endSkipsText.AddOutline(outline);

            _endAvgTimeText = new Text(startPos, Vector2.One * 1.2f, this, $"Среднее время: {avgTime:F1}с", _gameFont, Color.Yellow);
            _endAvgTimeText.AddOutline(outline);
        }

        private string GetDifficultyName()
        {
            return ChemicalEngine.CurrentDifficulty switch
            {
                Difficulty.Easy => "Лёгкая",
                Difficulty.Normal => "Нормальная",
                Difficulty.Hard => "Сложная",
                Difficulty.Impossible => "Невозможная",
                _ => "Неизвестно"
            };
        }

        private void OnTimerExpired(int id)
        {
            if (id == _timerID && !_isSessionEnded)
            {
                _ = new FloatingText(Screen.ScreenCenter, new Vector2(4f), this, _gameFont, Color.Gray, "Пропущено (->)");
                _skips++;
                StartNewRound();
            }
        }

        private void ResetTimer()
        {
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

        public override void Unload()
        {
            base.Unload();
            ResetTimer();
            _solvedEquations = -1;
            _correctSolvedEquations = 0;
            _skipAttemps = 0;
            _skips = 0;
            _totalTimeSpent = 0f;
            _isSessionEnded = false;
        }
    }
}