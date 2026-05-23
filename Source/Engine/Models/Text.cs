using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Models {
    public sealed class Text : GameObject, Specs.IDrawable {
        private string _text;
        private SpriteFont _font;
        private Vector2 _size;
        private bool _isSizeDirty = true;

        public string Content {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value;
                _isSizeDirty = true;
            }
        }

        public SpriteFont Font {
            get => _font;
            set
            {
                if (_font == value) return;
                _font = value;
                _isSizeDirty = true;
            }
        }

        public Color Color { get; set; } = Color.White;
        public float Width => MeasureString().X;
        public float Height => MeasureString().Y;

        public Text(Transform transform = null, Scene scene = null, string text = null, SpriteFont font = null, Color? color = null)
            : base(transform, scene) {
            _text = text;
            _font = font;
            Color = color ?? Color.White;
        }

        public Text(Vector2 position, Scene scene, string text, SpriteFont font, Color? color = null)
            : base(new Transform(position), scene) {
            _text = text;
            _font = font;
            Color = color ?? Color.White;
        }

        public void Draw(SpriteBatch sb) {
            if (_font == null || string.IsNullOrEmpty(_text) || !IsActive) return;
            sb.DrawString(_font, _text, Transform.Position, Color);
        }

        private Vector2 MeasureString() {
            if (_isSizeDirty)
            {
                _size = (_font != null && !string.IsNullOrEmpty(_text)) ? _font.MeasureString(_text) : Vector2.Zero;
                _isSizeDirty = false;
            }
            return _size;
        }

        public override void OnToggled(bool val) {
            if (val) CurrentScene.Add(this);
            else CurrentScene.Remove(this);
        }

        public void Dispose() => IsActive = false;
    }
}