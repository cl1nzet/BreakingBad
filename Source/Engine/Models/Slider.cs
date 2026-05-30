using System;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Engine.Models
{
    public sealed class Slider : GameObject, Specs.IUpdateable, Specs.IDrawable
    {
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

        public Slider(Vector2 position, Vector2 trackScale, Vector2 handleScale, Scene scene, Texture2D trackTexture = null, Texture2D handleTexture = null, GraphicsDevice graphicsDevice = null, float minValue = 0f, float maxValue = 1f, float startValue = 0f)
            : base(new Transform(position), scene)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _currentValue = Math.Clamp(startValue, minValue, maxValue);

            if (trackTexture == null && graphicsDevice != null)
            {
                trackTexture = GenerateDefaultTrack(graphicsDevice);
            }

            if (handleTexture == null && graphicsDevice != null)
            {
                handleTexture = GenerateDefaultHandle(graphicsDevice);
            }

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

        private Texture2D GenerateDefaultTrack(GraphicsDevice device)
        {
            int width = 160;
            int height = 20;
            Texture2D texture = new Texture2D(device, width, height);
            Color[] data = new Color[width * height];
            Color trackColor = new Color(180, 180, 180, 255);
            Color borderColor = new Color(130, 130, 130, 255);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        data[y * width + x] = borderColor;
                    }
                    else
                    {
                        data[y * width + x] = trackColor;
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        private Texture2D GenerateDefaultHandle(GraphicsDevice device)
        {
            int size = 24;
            Texture2D texture = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];
            float radius = size * 0.5f;
            float innerRadius = radius - 1f;

            Color handleColor = new Color(245, 245, 245, 255);
            Color borderColor = new Color(150, 150, 150, 255);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - radius + 0.5f;
                    float dy = y - radius + 0.5f;
                    float distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared <= radius * radius)
                    {
                        if (distanceSquared >= innerRadius * innerRadius)
                        {
                            data[y * size + x] = borderColor;
                        }
                        else
                        {
                            data[y * size + x] = handleColor;
                        }
                    }
                    else
                    {
                        data[y * size + x] = Color.Transparent;
                    }
                }
            }

            texture.SetData(data);
            return texture;
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