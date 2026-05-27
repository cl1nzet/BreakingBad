using System;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Models
{
    // Наследуемся от Text и реализуем IUpdateable (через интерфейс IVisualComponent)
    public sealed class InteractiveText : Text, Specs.IVisualComponent
    {
        private const float _blinkCooldown = 0.5f;

        private int _cursorIndex = 0;
        private float _cursorBlinkTimer = 0f;
        private bool _isCursorVisible = true;
        private float _cursorXOffset = 0f;
        private bool _isFocused = true;
        private MouseState _previousMouse;

        // Переопределяем свойство контента, чтобы при вводе текста сразу обновлять каретку
        public new string Content
        {
            get => base.Content;
            set
            {
                base.Content = value;
                ClampCursor();
                UpdateCursorOffset();
            }
        }

        // Переопределяем свойство шрифта для пересчета каретки
        public new SpriteFont Font
        {
            get => base.Font;
            set
            {
                base.Font = value;
                UpdateCursorOffset();
            }
        }

        public Color CursorColor { get; set; } = Color.White;
        public bool IsFocused { get => _isFocused; set => _isFocused = value; }

        public InteractiveText(Vector2 position, Vector2 scale, Scene scene, SpriteFont font, Color? color = null)
            : base(new Transform(position, scale), scene, string.Empty, font, color)
        {
            UpdateCursorOffset();
        }

        public void Update(GameTime gt)
        {
            if (!IsActive) return;

            MouseState currentMouse = Mouse.GetState();

            if (_isFocused)
            {
                _cursorBlinkTimer += (float)gt.ElapsedGameTime.TotalSeconds;
                if (_cursorBlinkTimer >= _blinkCooldown)
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

        public override void Draw(SpriteBatch sb)
        {
            if (Font == null || !IsActive) return;

            base.Draw(sb);

            if (_isFocused && _isCursorVisible) {
                float startX = Transform.Position.X - (Width * 0.5f);
                float startY = Transform.Position.Y - (Height * 0.5f);

                Vector2 cursorPosition = new Vector2(startX + (_cursorXOffset * Transform.Scale.X), Transform.Position.Y);

                Vector2 cursorSize = Font.MeasureString("|");
                Vector2 cursorOrigin = cursorSize * 0.5f;

                sb.DrawString(Font, "|", cursorPosition, CursorColor, 0f, cursorOrigin, Transform.Scale, SpriteEffects.None, 0f);
            }
        }

        public void InsertText(string input)
        {
            if (string.IsNullOrEmpty(input)) return;

            Content = Content.Insert(_cursorIndex, input);
            _cursorIndex += input.Length;
            ResetBlink();
            UpdateCursorOffset();
        }

        public void Backspace()
        {
            if (_cursorIndex > 0 && Content.Length > 0)
            {
                _cursorIndex--;
                Content = Content.Remove(_cursorIndex, 1);
                ResetBlink();
                UpdateCursorOffset();
            }
        }

        private void HandleClick(Vector2 mousePos)
        {
            Vector2 pos = Transform.Position;

            float halfWidth = Width * 0.5f;
            float halfHeight = Height * 0.5f;

            RectangleF bounds = new RectangleF(pos.X - halfWidth - 10f, pos.Y - halfHeight, Width + 20f, Height + 5f);

            if (!bounds.Contains(mousePos))
            {
                _isFocused = false;
                return;
            }

            _isFocused = true;

            float relativeX = mousePos.X - (pos.X - halfWidth);
            relativeX /= Transform.Scale.X;

            if (relativeX <= 0f || Content.Length == 0)
            {
                _cursorIndex = 0;
                UpdateCursorOffset();
                ResetBlink();
                return;
            }

            int closestIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i <= Content.Length; i++)
            {
                string sub = Content.Substring(0, i);
                float width = Font.MeasureString(sub).X;
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
            if (Font == null || _cursorIndex == 0 || string.IsNullOrEmpty(Content))
            {
                _cursorXOffset = 0f;
                return;
            }

            _cursorXOffset = Font.MeasureString(Content.Substring(0, _cursorIndex)).X;
        }

        private void ClampCursor()
        {
            if (_cursorIndex > Content.Length) _cursorIndex = Content.Length;
            if (_cursorIndex < 0) _cursorIndex = 0;
        }

        private void ResetBlink()
        {
            _isCursorVisible = true;
            _cursorBlinkTimer = 0f;
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
    }
}