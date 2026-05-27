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
        private sealed class FloatingErrorText : GameObject, Engine.Specs.IVisualComponent
        {
            private readonly Text _textMesh;
            private float _alpha = 1f;
            private const float FadeSpeed = 1.5f;
            private const float MoveSpeed = 60f;

            public FloatingErrorText(Vector2 position, Vector2 scale, Scene scene, SpriteFont font, string text = "Ошибка!") : base(new Transform(position), scene) {
                _textMesh = new Text(position, scale, scene, text, font, Color.Red);
            }

            public void Update(GameTime gt) {

                float dt = (float)gt.ElapsedGameTime.TotalSeconds;

                _alpha -= FadeSpeed * dt;

                Transform.Position += new Vector2(0, MoveSpeed * dt);
                _textMesh.Transform.Position = Transform.Position;

                _textMesh.Color = Color.Red * _alpha;

                if (_alpha <= 0f)
                {
                    Dispose();
                }
            }

            public void Draw(SpriteBatch sb) {
                _textMesh.Draw(sb);
            }

            public override void OnToggled(bool val) {
                if (val) {
                    CurrentScene.Add((Engine.Specs.IUpdateable)this);
                    CurrentScene.Add((Engine.Specs.IDrawable)this);
                }
                else
                {
                    CurrentScene.Remove((Engine.Specs.IUpdateable)this);
                    CurrentScene.Remove((Engine.Specs.IDrawable)this);
                }
            }

            public void Dispose() {
                IsActive = false;
                OnToggled(false);
            }
        }

        public override string SceneName { get; set; } = "Game";

        private InteractiveText _equationText;
        private Text _timerText;
        private SpriteFont _gameFont;
        private int _timerID = -1;
        private ReactionData _currentReaction;

        private static float _maxTime;

        private GraphicsDevice _graphicsDevice;
        public GameScene(GraphicsDevice graphicsDevice) {
            _graphicsDevice = graphicsDevice;
        }

        public override void Start() {
            _gameFont = AssetManager.GetFont("Arial");

            _equationText = new InteractiveText(
                position: new Vector2(Screen.ScreenCenterX - 700, Screen.ScreenCenterY - 400),
                scale: new Vector2(1.2f, 1.2f),
                scene: this,
                font: _gameFont,
                color: Color.White
            );

            _timerText = new Text(
                position: new Vector2(Screen.ScreenCenterX - 700, Screen.ScreenCenterY - 350),
                scale: Vector2.One,
                scene: this,
                text: "Осталось времени: 25с/25с",
                font: _gameFont,
                color: Color.Green
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
                position: new Vector2(Screen.ScreenCenterX - 700, Screen.ScreenCenterY - 200),
                scene: this,
                font: _gameFont,
                text: _equationText,
                graphicsDevice: _graphicsDevice,
                onVerify: VerifyEquation
            );

            exitButton.OnClick += SceneManager.LoadScene;

            StartNewRound();
        }

        public override void Update(GameTime gt) {
            base.Update(gt);

            if (_timerID != -1) {
                float remaining = TimerManager.GetRemainingTime(_timerID);
                if (remaining >= 0f) {
                    _timerText.Content = $"Осталось времени: {Math.Ceiling(remaining)}с/{_maxTime}с ";
                    _timerText.Color = GetTimerColor(remaining);
                }
            }
        }

        private Color GetTimerColor(float remainingTime) {
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
            bool isCorrect = ChemicalEngine.Verify(_currentReaction, equation);
            if (isCorrect) {
                StartNewRound();
            }
            else {
                Vector2 centerPosition = new Vector2(Screen.ScreenCenterX, Screen.ScreenCenterY);
                var errorPopup = new FloatingErrorText(centerPosition, new Vector2(3f), this, _gameFont);
                errorPopup.OnToggled(true); 
            }
        }

        private void StartNewRound() {
            _maxTime = ChemicalEngine.GetDifficultyMaxTime();
            _currentReaction = ChemicalEngine.Generate();

            _equationText.Content = FormatReaction(_currentReaction);

            _timerID = TimerManager.Add(_maxTime, OnTimerExpired);
        }

        private void OnTimerExpired(int id) {
            if (id == _timerID)
            {
                StartNewRound();
            }
        }

        private string FormatReaction(in ReactionData reaction) {
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