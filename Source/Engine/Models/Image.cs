using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Models {
    public sealed class Image : GameObject, Specs.IDrawable {
        private readonly Texture2D _texture;
        private Color _color;

        private Rectangle _cachedRect;
        private bool _isDirty = true;

        public int Width => (int)(_texture.Width * Transform.Scale.X);
        public int Height => (int)(_texture.Height * Transform.Scale.Y);

        public Rectangle Rect {
            get {
                if (_isDirty) {
                    _cachedRect = new Rectangle((int)Transform.Position.X, (int)Transform.Position.Y, Width, Height);
                    _isDirty = false;
                }
                return _cachedRect;
            }
        }
        public Color Color {
            get => _color;
            set => _color = value;
        }
        public Texture2D Texture => _texture;
        public Image(Transform transform = null, Scene scene = null, Texture2D texture = null, Color? color = null) : base(transform, scene) {
            _texture = texture;
            _color = color ?? Color.White;

            Transform.onChanged += () => _isDirty = true;
        }
        public Image(Vector2 position, Vector2 scale, Scene scene = null, Texture2D texture = null, Color? color = null) : base(new Transform(position, scale), scene) {
            _texture = texture;
            _color = color ?? Color.White;

            Transform.onChanged += () => _isDirty = true;
        }

        public void Draw(SpriteBatch sb) {
            sb.Draw(_texture, Rect, _color);
        }

        public void Dispose() => IsActive = false;

        public override void OnToggled(bool val) {
            if(val) { CurrentScene.Add(this); }
            else { CurrentScene.Remove(this); }
        }
    }
}