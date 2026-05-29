using System;
using System.Collections.Generic;
using System.Reflection;
using Engine.Core;
using Engine.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game.Models
{
    public sealed class VirtualKeyboard : GameObject, Engine.Specs.IVisualComponent
    {
        private static readonly string[][] Layout =
        {
            new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" },
            new[] { "*", "->", null, null, "<", ">", null, "DEL", null, null}
        };

        private static readonly Dictionary<string, float> KeyWidthMultipliers = new()
        {
            { "DEL", 2f },
            { "->", 1.5f }
        };

        private static readonly FieldInfo CursorField = typeof(InteractiveText).GetField("_cursorIndex", BindingFlags.NonPublic | BindingFlags.Instance);

        private KeyboardButton[] _buttons;
        private int _buttonCount;
        private SpriteFont _font;
        private MouseState _previousMouse;
        private Texture2D _pixel;
        private readonly InteractiveText _targetText;
        private readonly Action<string> _onVerify;
        private readonly Action _onSkip;

        private Vector2 _buttonSize = new(50f, 50f);
        private Vector2 _spacing = new(5f, 5f);
        private float _bigButtonSpacing = 15f;

        public Vector2 ButtonSize
        {
            get => _buttonSize;
            set
            {
                if (_buttonSize == value) return;
                _buttonSize = value;
                BuildLayout();
            }
        }

        public Vector2 Spacing
        {
            get => _spacing;
            set
            {
                if (_spacing == value) return;
                _spacing = value;
                BuildLayout();
            }
        }

        public float BigButtonSpacing
        {
            get => _bigButtonSpacing;
            set
            {
                if (_bigButtonSpacing == value) return;
                _bigButtonSpacing = value;
                BuildLayout();
            }
        }

        public Color ButtonColor { get; set; } = new(40, 40, 40);
        public Color TextColor { get; set; } = Color.White;

        public Action<string> OnKeyPressed;

        public VirtualKeyboard(Vector2 position, Scene scene, SpriteFont font, InteractiveText text, GraphicsDevice graphicsDevice, Action<string> onVerify, Action onSkip)
            : base(new Transform(position), scene)
        {
            _font = font;
            _targetText = text;
            _onVerify = onVerify;
            _onSkip = onSkip;

            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            BuildLayout();
        }

        public void BuildLayout()
        {
            if (_font == null) return;

            int count = 0;
            for (int r = 0; r < Layout.Length; r++)
            {
                for (int c = 0; c < Layout[r].Length; c++)
                {
                    if (!string.IsNullOrEmpty(Layout[r][c])) count++;
                }
            }

            _buttons = new KeyboardButton[count];
            _buttonCount = 0;

            float bWidth = _buttonSize.X;
            float bHeight = _buttonSize.Y;
            float sX = _spacing.X;
            float sY = _spacing.Y;

            for (int row = 0; row < Layout.Length; row++) {
                float currentX = 0f;

                for (int col = 0; col < Layout[row].Length; col++)
                {
                    string key = Layout[row][col];

                    if (string.IsNullOrEmpty(key)) {
                        currentX += bWidth + sX;
                        continue;
                    }

                    bool isBigKey = KeyWidthMultipliers.TryGetValue(key, out float multiplier);
                    if (!isBigKey) multiplier = 1f;

                    float currentWidth = (bWidth * multiplier) + (sX * (multiplier - 1f));

                    float posX = currentX;
                    float posY = row * (bHeight + sY);

                    Rectangle relativeBounds = new((int)posX, (int)posY, (int)currentWidth, (int)bHeight);
                    Vector2 textSize = _font.MeasureString(key);
                    Vector2 textOffset = new(
                        posX + (currentWidth - textSize.X) * 0.5f,
                        posY + (bHeight - textSize.Y) * 0.5f
                    );

                    _buttons[_buttonCount++] = new KeyboardButton(key, relativeBounds, textOffset);

                    float currentSpacing = isBigKey ? _bigButtonSpacing : sX;
                    currentX += currentWidth + currentSpacing;
                }
            }
        }

        public void Update(GameTime gt) {

            MouseState currentMouse = Mouse.GetState();

            if (currentMouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
            {
                Point mousePos = currentMouse.Position;
                Vector2 basePos = Transform.Position;

                int relativeMouseX = mousePos.X - (int)basePos.X;
                int relativeMouseY = mousePos.Y - (int)basePos.Y;

                for (int i = 0; i < _buttonCount; i++)
                {
                    ref readonly var btn = ref _buttons[i];
                    if (btn.RelativeBounds.Contains(relativeMouseX, relativeMouseY))
                    {
                        ProcessKeyPress(btn.Key);
                        break;
                    }
                }
            }

            _previousMouse = currentMouse;
        }

        private void ProcessKeyPress(string key)
        {
            if (key == "*")
            {
                _onVerify?.Invoke(_targetText?.Content ?? string.Empty);
            }
            else if (key == "->")
            {
                _onSkip?.Invoke();
            }
            else if (key == "<")
            {
                _targetText?.MoveToPreviousBracket();
            }
            else if (key == ">")
            {
                _targetText?.MoveToNextBracket();
            }
            else if (key == "DEL")
            {
                if (_targetText != null)
                {
                    string content = _targetText.Content;
                    if (!string.IsNullOrEmpty(content))
                    {
                        int cursorIdx = content.Length;
                        if (CursorField != null)
                        {
                            cursorIdx = (int)CursorField.GetValue(_targetText);
                        }

                        if (cursorIdx > 0 && cursorIdx <= content.Length)
                        {
                            if (char.IsDigit(content[cursorIdx - 1]))
                            {
                                _targetText.Backspace();
                            }
                        }
                    }
                }
            }
            else
            {
                _targetText?.InsertText(key);
            }

            OnKeyPressed?.Invoke(key);
        }

        public void Draw(SpriteBatch sb) {
            if (_pixel == null || _font == null) return;

            Vector2 basePos = Transform.Position;
            int baseX = (int)basePos.X;
            int baseY = (int)basePos.Y;

            for (int i = 0; i < _buttonCount; i++)
            {
                ref readonly var btn = ref _buttons[i];

                Rectangle absBounds = btn.RelativeBounds;
                absBounds.X += baseX;
                absBounds.Y += baseY;

                sb.Draw(_pixel, absBounds, ButtonColor);
                sb.DrawString(_font, btn.Key, basePos + btn.TextOffset, TextColor);
            }
        }

        public override void OnToggled(bool val)
        {
            if (val)
            {
                CurrentScene.Add((Engine.Specs.IUpdateable)this);
                CurrentScene.Add((Engine.Specs.IDrawable)this);
            }
            else
            {
                CurrentScene.Remove((Engine.Specs.IUpdateable)this);
                CurrentScene.Remove((Engine.Specs.IDrawable)this);
            }
        }

        public void Dispose()
        {
            IsActive = false;
            _pixel?.Dispose();
            _pixel = null;
        }
    }
}