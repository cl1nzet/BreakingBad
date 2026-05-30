using Engine.Core;
using Engine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Models
{
    public sealed class AtlasImage : Image
    {
        private readonly SpriteAtlas _atlas;
        private Rectangle _sourceRect;
        private int _width;
        private int _height;

        public override int Width => _width;
        public override int Height => _height;

        public AtlasImage(Vector2 position, Vector2 scale, Scene scene, SpriteAtlas atlas, int startFrame = 0, Color? color = null)
            : base(position, scale, null, color)
        {
            _atlas = atlas;
            _texture = atlas.Texture;

            _sourceRect = _atlas.GetRegion(startFrame);

            Vector2 tScale = Transform.Scale;
            _width = (int)(_sourceRect.Width * tScale.X);
            _height = (int)(_sourceRect.Height * tScale.Y);
            _origin.X = _sourceRect.Width * 0.5f;
            _origin.Y = _sourceRect.Height * 0.5f;

            CurrentScene = scene;
            if (IsActive)
            {
                scene?.Add(this);
            }
        }

        public void SetFrame(int frameIndex)
        {
            _sourceRect = _atlas.GetRegion(frameIndex);
            UpdateBounds();
        }

        public void SetFrame(int column, int row)
        {
            _sourceRect = _atlas.GetRegion(column, row);
            UpdateBounds();
        }

        public void SetFrame(string regionName)
        {
            _sourceRect = _atlas.GetRegion(regionName);
            UpdateBounds();
        }

        private void UpdateBounds()
        {
            Vector2 tScale = Transform.Scale;
            _width = (int)(_sourceRect.Width * tScale.X);
            _height = (int)(_sourceRect.Height * tScale.Y);
            _origin.X = _sourceRect.Width * 0.5f;
            _origin.Y = _sourceRect.Height * 0.5f;
            _isDirty = true;
        }

        public override void Draw(SpriteBatch sb)
        {
            if (_texture == null || !IsActive) return;
            sb.Draw(_texture, Transform.Position, _sourceRect, Color, 0f, _origin, Transform.Scale, SpriteEffects.None, 0f);
        }
    }
}