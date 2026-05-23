using System;
using Engine.Core;
using Engine.Specs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Engine.Models {
    public sealed class Button : GameObject, IVisualComponent {
        private readonly Image _image;
        private Text _text;

        private MouseState _currentMouse;
        private MouseState _previousMouse;
        private Rectangle _bounds;
        private Vector2 _lastPosition;

        public event Action OnClick;

        public Color NormalColor { get; set; } = Color.White;
        public Color HoverColor { get; set; } = Color.LightGray;

        public Button(Image image, Text text = null) : base(image.Transform, image.CurrentScene) {
            _image = image;
            _text = text;
            UpdateBounds();
        }

        public Button(Vector2 position, Vector2 scale, Scene scene, Texture2D texture, string text = null, SpriteFont font = null, Color? textColor = null)
            : base(new Transform(position), scene) {
            _image = new Image(position, scale, scene, texture, NormalColor);
            if (!string.IsNullOrEmpty(text) && font != null)
            {
                _text = new Text(position, scene, text, font, textColor ?? Color.White);
            }
            UpdateBounds();
        }

        public void AddText(Color color, SpriteFont font, string text)
        {
            if (_text != null) return;
            _text = new Text(Transform.Position, _image.CurrentScene, text, font, color);
            CenterText();
        }

        public void Update(GameTime gt) {
            if (!IsActive) return;

            if (Transform.Position != _lastPosition) {
                UpdateBounds();
            }

            _currentMouse = Mouse.GetState();
            var touchState = TouchPanel.GetState();

            bool isHovered = false;
            bool isClicked = false;

            Point mousePoint = new(_currentMouse.X, _currentMouse.Y);
            if (_bounds.Contains(mousePoint))
            {
                isHovered = true;
                if (_currentMouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
                {
                    isClicked = true;
                }
            }

            for (int i = 0; i < touchState.Count; i++) {
                var touch = touchState[i];
                Point touchPoint = new((int)touch.Position.X, (int)touch.Position.Y);

                if (_bounds.Contains(touchPoint)) {
                    isHovered = true;
                    if (touch.State == TouchLocationState.Pressed)
                    {
                        isClicked = true;
                    }
                }
            }

            if (_image != null) {
                _image.Color = isHovered ? HoverColor : NormalColor;
            }

            if (isClicked) {
                OnClick?.Invoke();
            }

            _previousMouse = _currentMouse;
        }

        public void Draw(SpriteBatch sb) {
            if (!IsActive) return;

            _image?.Draw(sb);
            _text?.Draw(sb);
        }

        private void UpdateBounds() {
            if (_image == null) return;

            _lastPosition = Transform.Position;
            _bounds = new Rectangle((int)_lastPosition.X, (int)_lastPosition.Y, _image.Width, _image.Height);

            CenterText();
        }

        private void CenterText() {
            if (_text == null || _image == null) return;

            _text.Transform.Position = new Vector2(
                _lastPosition.X + (_image.Width * 0.5f) - (_text.Width * 0.5f),
                _lastPosition.Y + (_image.Height * 0.5f) - (_text.Height * 0.5f)
            );
        }

        public override void OnToggled(bool val) {
            if (val) {
                CurrentScene.Add((Specs.IUpdateable)this);
                CurrentScene.Add((Specs.IDrawable)this);
            }
            else {
                CurrentScene.Remove((Specs.IUpdateable)this);
                CurrentScene.Remove((Specs.IDrawable)this);
            }
        }

        public void Dispose() => IsActive = false;
    }
}