using System;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Engine.Models {
    public sealed class Slider : GameObject, Specs.IUpdateable, Specs.IDrawable {
        private readonly Image _track;
        private readonly Image _handle;

        private float _minValue;
        private float _maxValue;
        private float _currentValue;

        private bool _isDragging;
        private Rectangle _trackBounds;
        private Vector2 _lastPosition;

        public event Action<float> OnValueChanged;

        public float MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                UpdateHandlePosition();
            }
        }

        public float MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                UpdateHandlePosition();
            }
        }

        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                float clamped = Math.Clamp(value, _minValue, _maxValue);
                if (Math.Abs(_currentValue - clamped) > 0.0001f)
                {
                    _currentValue = clamped;
                    UpdateHandlePosition();
                    OnValueChanged?.Invoke(_currentValue);
                }
            }
        }

        public Slider(Vector2 position, Vector2 trackScale, Vector2 handleScale, Scene scene, Texture2D trackTexture, Texture2D handleTexture, float minValue = 0f, float maxValue = 1f, float startValue = 0f)
            : base(new Transform(position), scene)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _currentValue = Math.Clamp(startValue, minValue, maxValue);

            _track = new Image(Transform.Position, trackScale, scene, trackTexture);
            _handle = new Image(Transform.Position, handleScale, scene, handleTexture);

            UpdateBounds();

            Transform.onChanged += () =>
            {
                _track.Transform.Position = Transform.Position;
                UpdateBounds();
            };
        }

        public void Update(GameTime gt)
        {
            if (Transform.Position != _lastPosition)
            {
                UpdateBounds();
            }

            var mouseState = Mouse.GetState();
            var touchState = TouchPanel.GetState();

            bool hasInput = false;
            Vector2 inputPosition = Vector2.Zero;

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                hasInput = true;
                inputPosition = new Vector2(mouseState.X, mouseState.Y);
            }

            if (!hasInput && touchState.Count > 0)
            {
                var touch = touchState[0];
                if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved)
                {
                    hasInput = true;
                    inputPosition = touch.Position;
                }
            }

            if (hasInput)
            {
                Point inputPoint = new((int)inputPosition.X, (int)inputPosition.Y);

                if (_isDragging || _trackBounds.Contains(inputPoint) || _handle.Rect.Contains(inputPoint))
                {
                    _isDragging = true;
                    CalculateValueFromInput(inputPosition.X);
                }
            }
            else
            {
                _isDragging = false;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            _track?.Draw(sb);
            _handle?.Draw(sb);
        }

        private void UpdateBounds()
        {
            if (_track == null) return;

            _lastPosition = Transform.Position;
            _trackBounds = _track.Rect;
            UpdateHandlePosition();
        }

        private void UpdateHandlePosition()
        {
            if (_track == null || _handle == null) return;

            float range = _maxValue - _minValue;
            float percent = range == 0f ? 0f : (_currentValue - _minValue) / range;

            float leftLimit = _lastPosition.X - _track.Width * 0.5f;
            float newX = leftLimit + (percent * _track.Width);

            _handle.Transform.Position = new Vector2(newX, _lastPosition.Y);
        }

        private void CalculateValueFromInput(float inputX)
        {
            float leftLimit = _lastPosition.X - _track.Width * 0.5f;
            float percent = (inputX - leftLimit) / _track.Width;
            percent = Math.Clamp(percent, 0f, 1f);

            CurrentValue = _minValue + percent * (_maxValue - _minValue);
        }

        public override void OnToggled(bool val)
        {
            if (val)
            {
                CurrentScene?.Add((Specs.IUpdateable)this);
                CurrentScene?.Add((Specs.IDrawable)this);
            }
            else
            {
                CurrentScene?.Remove((Specs.IUpdateable)this);
                CurrentScene?.Remove((Specs.IDrawable)this);
            }
        }

        public void Dispose() => IsActive = false;
    }
}