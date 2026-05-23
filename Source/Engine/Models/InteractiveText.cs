using System;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Models {
    public sealed class InteractiveText : GameObject, Specs.IDrawable {
        private string _text = string.Empty;
        private SpriteFont _font;
        private Vector2 _size;
        private bool _isSizeDirty = true;

        private int _cursorIndex = 0;
        private float _cursorBlinkTimer = 0f;
        private bool _isCursorVisible = true;
        private float _cursorXOffset = 0f;
        private bool _isFocused = true;
        private MouseState _previousMouse;

        public string Content
        {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value ?? string.Empty;
                _isSizeDirty = true;
                ClampCursor();
                UpdateCursorOffset();
            }
        }

        public SpriteFont Font
        {
            get => _font;
            set
            {
                if (_font == value) return;
                _font = value;
                _isSizeDirty = true;
                UpdateCursorOffset();
            }
        }

        public Color Color { get; set; } = Color.White;
        public Color CursorColor { get; set; } = Color.White;
        public float Width => MeasureString().X;
        public float Height => MeasureString().Y;
        public bool IsFocused { get => _isFocused; set => _isFocused = value; }

        public InteractiveText(Vector2 position, Vector2 scale, Scene scene, SpriteFont font, Color? color = null)
            : base(new Transform(position, scale), scene)
        {
            _font = font;
            Color = color ?? Color.White;
            UpdateCursorOffset();
        }

        public void Update(GameTime gt)
        {
            if (!IsActive) return;

            MouseState currentMouse = Mouse.GetState();

            if (_isFocused)
            {
                _cursorBlinkTimer += (float)gt.ElapsedGameTime.TotalSeconds;
                if (_cursorBlinkTimer >= 0.5f)
                {
                    _isCursorVisible = !_isCursorVisible;
                    _cursorBlinkTimer = 0f;
                }
            }

            if (currentMouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
            {
                HandleClick(currentMouse.Position.ToVector2());
            }

            _previousMouse = currentMouse;
        }

        public void Draw(SpriteBatch sb)
        {
            if (_font == null || !IsActive) return;

            if (!string.IsNullOrEmpty(_text))
            {
                sb.DrawString(_font, _text, Transform.Position, Color);
            }

            if (_isFocused && _isCursorVisible)
            {
                Vector2 cursorPosition = new Vector2(Transform.Position.X + _cursorXOffset, Transform.Position.Y);
                sb.DrawString(_font, "|", cursorPosition, CursorColor);
            }
        }

        public void InsertText(string input)
        {
            if (string.IsNullOrEmpty(input)) return;

            _text = _text.Insert(_cursorIndex, input);
            _cursorIndex += input.Length;
            _isSizeDirty = true;
            ResetBlink();
            UpdateCursorOffset();
        }

        public void Backspace()
        {
            if (_cursorIndex > 0 && _text.Length > 0)
            {
                _cursorIndex--;
                _text = _text.Remove(_cursorIndex, 1);
                _isSizeDirty = true;
                ResetBlink();
                UpdateCursorOffset();
            }
        }

        private void HandleClick(Vector2 mousePos)
        {
            Vector2 pos = Transform.Position;
            Vector2 size = MeasureString();

            RectangleF bounds = new RectangleF(pos.X - 10f, pos.Y, size.X + 20f, size.Y + 5f);

            if (!bounds.Contains(mousePos))
            {
                _isFocused = false;
                return;
            }

            _isFocused = true;
            float relativeX = mousePos.X - pos.X;

            if (relativeX <= 0f || _text.Length == 0)
            {
                _cursorIndex = 0;
                UpdateCursorOffset();
                ResetBlink();
                return;
            }

            int closestIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i <= _text.Length; i++)
            {
                string sub = _text.Substring(0, i);
                float width = _font.MeasureString(sub).X;
                float distance = Math.Abs(width - relativeX);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            _cursorIndex = closestIndex;
            UpdateCursorOffset();
            ResetBlink();
        }

        private void UpdateCursorOffset()
        {
            if (_font == null || _cursorIndex == 0 || string.IsNullOrEmpty(_text))
            {
                _cursorXOffset = 0f;
                return;
            }

            _cursorXOffset = _font.MeasureString(_text.Substring(0, _cursorIndex)).X;
        }

        private void ClampCursor()
        {
            if (_cursorIndex > _text.Length) _cursorIndex = _text.Length;
            if (_cursorIndex < 0) _cursorIndex = 0;
        }

        private void ResetBlink()
        {
            _isCursorVisible = true;
            _cursorBlinkTimer = 0f;
        }

        private Vector2 MeasureString()
        {
            if (_isSizeDirty)
            {
                _size = (_font != null && !string.IsNullOrEmpty(_text)) ? _font.MeasureString(_text) : Vector2.Zero;
                _isSizeDirty = false;
            }
            return _size;
        }

        public override void OnToggled(bool val)
        {
            if (val) CurrentScene.Add(this);
            else CurrentScene.Remove(this);
        }

        public void Dispose() => IsActive = false;
    }
}