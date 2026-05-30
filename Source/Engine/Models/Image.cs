using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Models
{
    public class Image : GameObject, Specs.IDrawable
    {
        protected readonly Texture2D _texture;
        private Color _color;
        protected Vector2 _origin;

        private Rectangle _cachedRect;
        protected bool _isDirty = true;

        public virtual int Width => _texture != null ? (int)(_texture.Width * Transform.Scale.X) : 0;
        public virtual int Height => _texture != null ? (int)(_texture.Height * Transform.Scale.Y) : 0;

        public Rectangle Rect
        {
            get
            {
                if (_isDirty)
                {
                    int x = (int)(Transform.Position.X - Width * 0.5f);
                    int y = (int)(Transform.Position.Y - Height * 0.5f);
                    _cachedRect = new Rectangle(x, y, Width, Height);
                    _isDirty = false;
                }
                return _cachedRect;
            }
        }
        public Color Color
        {
            get => _color;
            set => _color = value;
        }
        public Texture2D Texture => _texture;

        public Image(Transform transform = null, Scene scene = null, Texture2D texture = null, Color? color = null) : base(transform, scene)
        {
            _texture = texture;
            _color = color ?? Color.White;

            Initialize();
        }

        public Image(Vector2 position, Vector2 scale, Scene scene = null, Texture2D texture = null, Color? color = null) : base(new Transform(position, scale), scene)
        {
            _texture = texture;
            _color = color ?? Color.White;
            Initialize();
        }

        private void Initialize()
        {
            if (_texture != null)
            {
                _origin = new Vector2(_texture.Width * 0.5f, _texture.Height * 0.5f);
            }
            Transform.onChanged += () => _isDirty = true;
        }

        public virtual void Draw(SpriteBatch sb)
        {
            if (_texture == null || !IsActive) return;

            sb.Draw(_texture, Transform.Position, null, _color, 0f, _origin, Transform.Scale, SpriteEffects.None, 0f);
        }

        public void Dispose() => IsActive = false;

        public override void OnToggled(bool val)
        {
            if (val) { CurrentScene?.Add(this); }
            else { CurrentScene?.Remove(this); }
        }
    }
}