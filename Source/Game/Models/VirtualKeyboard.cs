using System;
using Engine.Core;
using Engine.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Game.Models {
    public sealed class VirtualKeyboard : GameObject, Engine.Specs.IVisualComponent
    {
        private readonly string[][] _layout = new string[][]
        {
            new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" },
            new string[] { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
            new string[] { "A", "S", "D", "F", "G", "H", "J", "K", "L", null },
            new string[] { "Z", "X", "C", "V", "B", "N", "M", "DEL", null, null }
        };

        private KeyboardButton[] _buttons;
        private int _buttonCount;
        private SpriteFont _font;
        private MouseState _previousMouse;
        private Texture2D _pixel;

        public Vector2 ButtonSize { get; set; } = new Vector2(50f, 50f);
        public Vector2 Spacing { get; set; } = new Vector2(5f, 5f);
        public Color ButtonColor { get; set; } = new Color(40, 40, 40);
        public Color TextColor { get; set; } = Color.White;

        public Action<string> OnKeyPressed;

        public VirtualKeyboard(Vector2 position, Scene scene, SpriteFont font, GraphicsDevice graphicsDevice)
            : base(new Transform(position), scene)
        {
            _font = font;

            _pixel = new Texture2D(graphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            BuildLayout();
        }

        public void BuildLayout()
        {
            int maxPossibleButtons = 40;
            _buttons = new KeyboardButton[maxPossibleButtons];
            _buttonCount = 0;

            Vector2 basePos = Transform.Position;

            for (int row = 0; row < _layout.Length; row++)
            {
                for (int col = 0; col < _layout[row].Length; col++)
                {
                    string key = _layout[row][col];
                    if (string.IsNullOrEmpty(key)) continue;

                    float posX = basePos.X + col * (ButtonSize.X + Spacing.X);
                    float posY = basePos.Y + row * (ButtonSize.Y + Spacing.Y);

                    Vector2 size = ButtonSize;
                    if (key == "DEL")
                    {
                        size.X = (ButtonSize.X * 2f) + Spacing.X;
                    }

                    _buttons[_buttonCount++] = new KeyboardButton(key, new Rectangle((int)posX, (int)posY, (int)size.X, (int)size.Y));
                }
            }
        }

        public void Update(GameTime gt)
        {
            if (!IsActive) return;

            MouseState currentMouse = Mouse.GetState();

            if (currentMouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
            {
                Point mousePos = currentMouse.Position;

                for (int i = 0; i < _buttonCount; i++)
                {
                    if (_buttons[i].Bounds.Contains(mousePos))
                    {
                        OnKeyPressed?.Invoke(_buttons[i].Key);
                        break;
                    }
                }
            }

            _previousMouse = currentMouse;
        }

        public void Draw(SpriteBatch sb)
        {
            if (!IsActive || _pixel == null || _font == null) return;

            for (int i = 0; i < _buttonCount; i++)
            {
                ref var btn = ref _buttons[i];

                sb.Draw(_pixel, btn.Bounds, ButtonColor);

                Vector2 textSize = _font.MeasureString(btn.Key);
                Vector2 textPos = new Vector2(
                    btn.Bounds.X + (btn.Bounds.Width - textSize.X) / 2f,
                    btn.Bounds.Y + (btn.Bounds.Height - textSize.Y) / 2f
                );

                sb.DrawString(_font, btn.Key, textPos, TextColor);
            }
        }

        public override void OnToggled(bool val) {
            if (val) {
                CurrentScene.Add((Engine.Specs.IUpdateable)this);
                CurrentScene.Add((Engine.Specs.IDrawable)this);
            }
            else {
                CurrentScene.Remove((Engine.Specs.IUpdateable)this);
                CurrentScene.Remove((Engine.Specs.IDrawable)this);
            }
        }

        public void Dispose() {
            IsActive = false;
            _pixel?.Dispose();
        }
    }
}