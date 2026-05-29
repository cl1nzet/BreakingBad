using System;
using System.Collections.Generic;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Models
{
    public sealed class InteractiveText : Text, Specs.IVisualComponent
    {
        private sealed class AnimatedChar
        {
            public char Character;
            public float Alpha;
            public bool IsTargetForDeletion;

            public AnimatedChar(char character, float initialAlpha)
            {
                Character = character;
                Alpha = initialAlpha;
                IsTargetForDeletion = false;
            }
        }

        private const float _blinkCooldown = 0.5f;
        private const float _fadeSpeed = 8f;
        private const float _cursorSmoothSpeed = 22f; // Скорость доводки курсора (чем выше, тем быстрее)

        private readonly List<AnimatedChar> _animatedChars = new();

        private int _cursorIndex = 0;
        private float _cursorBlinkTimer = 0f;
        private bool _isCursorVisible = true;

        private float _cursorXOffset = 0f;       // Текущая отрендеренная позиция курсора
        private float _targetCursorXOffset = 0f; // Целевая позиция, куда курсор стремится

        private bool _isFocused = true;
        private MouseState _previousMouse;

        public new string Content
        {
            get
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < _animatedChars.Count; i++)
                {
                    if (!_animatedChars[i].IsTargetForDeletion)
                        sb.Append(_animatedChars[i].Character);
                }
                return sb.ToString();
            }
            set
            {
                _animatedChars.Clear();
                string val = value ?? string.Empty;
                for (int i = 0; i < val.Length; i++)
                {
                    _animatedChars.Add(new AnimatedChar(val[i], 1f));
                }
                base.Content = val;
                ClampCursor();
                UpdateCursorOffset(snap: true); // При полной смене текста мгновенно перемещаем курсор
            }
        }

        public new SpriteFont Font
        {
            get => base.Font;
            set
            {
                base.Font = value;
                UpdateCursorOffset(snap: true);
            }
        }

        public Color CursorColor { get; set; } = Color.White;
        public bool IsFocused { get => _isFocused; set => _isFocused = value; }

        public InteractiveText(Vector2 position, Vector2 scale, Scene scene, SpriteFont font, Color? color = null)
            : base(new Transform(position, scale), scene, string.Empty, font, color)
        {
            UpdateCursorOffset(snap: true);
        }

        public void Update(GameTime gt)
        {
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;

            if (_cursorXOffset != _targetCursorXOffset)
            {
                _cursorXOffset = MathHelper.Lerp(_cursorXOffset, _targetCursorXOffset, _cursorSmoothSpeed * dt);

                if (Math.Abs(_cursorXOffset - _targetCursorXOffset) < 0.2f)
                {
                    _cursorXOffset = _targetCursorXOffset;
                }
            }

            bool listChanged = false;
            for (int i = _animatedChars.Count - 1; i >= 0; i--)
            {
                var c = _animatedChars[i];
                if (c.IsTargetForDeletion)
                {
                    c.Alpha -= _fadeSpeed * dt;
                    if (c.Alpha <= 0f)
                    {
                        _animatedChars.RemoveAt(i);
                        listChanged = true;
                    }
                }
                else if (c.Alpha < 1f)
                {
                    c.Alpha += _fadeSpeed * dt;
                    if (c.Alpha > 1f) c.Alpha = 1f;
                }
            }

            if (listChanged)
            {
                base.Content = Content;
                UpdateCursorOffset();
            }

            MouseState currentMouse = Mouse.GetState();

            if (_isFocused)
            {
                _cursorBlinkTimer += dt;
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
            if (Font == null) return;

            float startX = Transform.Position.X - (Width * 0.5f);
            float startY = Transform.Position.Y - (Height * 0.5f);

            Vector2 currentPos = new Vector2(startX, Transform.Position.Y);

            for (int i = 0; i < _animatedChars.Count; i++)
            {
                var c = _animatedChars[i];
                string charStr = c.Character.ToString();
                Vector2 charSize = Font.MeasureString(charStr);
                Vector2 charOrigin = charSize * 0.5f;

                Vector2 drawPos = currentPos + new Vector2(charSize.X * 0.5f * Transform.Scale.X, 0f);

                Color charColor = Color * c.Alpha;

                sb.DrawString(Font, charStr, drawPos, charColor, 0f, charOrigin, Transform.Scale, SpriteEffects.None, 0f);

                currentPos.X += charSize.X * Transform.Scale.X;
            }

            if (_isFocused && _isCursorVisible)
            {
                Vector2 cursorPosition = new Vector2(startX + (_cursorXOffset * Transform.Scale.X), Transform.Position.Y);
                Vector2 cursorSize = Font.MeasureString("|");
                Vector2 cursorOrigin = cursorSize * 0.5f;

                sb.DrawString(Font, "|", cursorPosition, CursorColor, 0f, cursorOrigin, Transform.Scale, SpriteEffects.None, 0f);
            }
        }

        public void InsertText(string input)
        {
            if (string.IsNullOrEmpty(input)) return;

            int realTargetIdx = GetRealIndex(_cursorIndex);

            for (int i = 0; i < input.Length; i++)
            {
                _animatedChars.Insert(realTargetIdx + i, new AnimatedChar(input[i], 0f));
            }

            _cursorIndex += input.Length;
            base.Content = Content;
            ResetBlink();
            UpdateCursorOffset();
        }

        public void MoveToNextBracket()
        {
            string content = Content;
            int nextOpenIdx = -1;

            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '[')
                {
                    if (i >= _cursorIndex)
                    {
                        nextOpenIdx = i;
                        break;
                    }
                }
            }

            if (nextOpenIdx != -1)
            {
                int closeIdx = content.IndexOf(']', nextOpenIdx);
                if (closeIdx != -1)
                {
                    _cursorIndex = closeIdx;
                    ResetBlink();
                    UpdateCursorOffset();
                }
            }
        }

        public void MoveToPreviousBracket()
        {
            string content = Content;
            List<int> openIndices = new List<int>();

            for (int i = 0; i < content.Length; i++)
            {
                if (content[i] == '[') openIndices.Add(i);
            }

            if (openIndices.Count == 0) return;

            int currentBracketIdx = -1;
            for (int i = 0; i < openIndices.Count; i++)
            {
                int closeIdx = content.IndexOf(']', openIndices[i]);
                if (closeIdx != -1 && closeIdx >= _cursorIndex)
                {
                    currentBracketIdx = i;
                    break;
                }
            }

            int targetBracketIdx = currentBracketIdx == -1 ? openIndices.Count - 1 : currentBracketIdx - 1;

            if (targetBracketIdx >= 0)
            {
                int closeIdx = content.IndexOf(']', openIndices[targetBracketIdx]);
                if (closeIdx != -1)
                {
                    _cursorIndex = closeIdx;
                    ResetBlink();
                    UpdateCursorOffset();
                }
            }
        }

        public void Backspace()
        {
            if (_cursorIndex > 0 && Content.Length > 0)
            {
                _cursorIndex--;
                int realTargetIdx = GetRealIndex(_cursorIndex);

                _animatedChars[realTargetIdx].IsTargetForDeletion = true;

                ResetBlink();
                UpdateCursorOffset();
            }
        }

        private int GetRealIndex(int visibleIdx)
        {
            int currentVisible = 0;
            for (int i = 0; i < _animatedChars.Count; i++)
            {
                if (currentVisible == visibleIdx && !_animatedChars[i].IsTargetForDeletion)
                    return i;

                if (!_animatedChars[i].IsTargetForDeletion)
                    currentVisible++;
            }
            return _animatedChars.Count;
        }

        private void HandleClick(Vector2 mousePos)
        {
            Vector2 pos = Transform.Position;

            float halfWidth = Width * 0.5f;
            float halfHeight = Height * 0.5f;

            RectangleF bounds = new RectangleF(pos.X - halfWidth - 10f, pos.Y - halfHeight, Width + 20f, Height + 5f);

            if (!bounds.Contains(mousePos))
            {
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

        private void UpdateCursorOffset(bool snap = false)
        {
            if (Font == null || _cursorIndex == 0 || string.IsNullOrEmpty(Content))
            {
                _targetCursorXOffset = 0f;
            }
            else
            {
                _targetCursorXOffset = Font.MeasureString(Content.Substring(0, _cursorIndex)).X;
            }

            if (snap)
            {
                _cursorXOffset = _targetCursorXOffset;
            }
            else
            {
                ResetBlink();
            }
        }

        private void ClampCursor()
        {
            int length = Content.Length;
            if (_cursorIndex > length) _cursorIndex = length;
            if (_cursorIndex < 0) _cursorIndex = 0;
        }

        private void ResetBlink()
        {
            _isCursorVisible = true;
            _cursorBlinkTimer = 0f;
        }

        public override void OnToggled(bool val)
        {
            if (val)
            {
                CurrentScene.Add((Specs.IUpdateable)this);
                CurrentScene.Add((Specs.IDrawable)this);
            }
            else
            {
                CurrentScene.Remove((Specs.IUpdateable)this);
                CurrentScene.Remove((Specs.IDrawable)this);
            }
        }
    }
}