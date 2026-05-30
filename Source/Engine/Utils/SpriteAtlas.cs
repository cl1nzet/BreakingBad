using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Utils {
    public class SpriteAtlas {
        private readonly int _cellWidth;
        private readonly int _cellHeight;
        private readonly int _columns;

        private int[] _customKeys;
        private Rectangle[] _customRects;
        private int _customCount;

        public Texture2D Texture { get; }
        public int CellWidth => _cellWidth;
        public int CellHeight => _cellHeight;

        public SpriteAtlas(Texture2D texture, int columns, int rows)
        {
            Texture = texture;
            _columns = columns;
            _cellWidth = texture.Width / columns;
            _cellHeight = texture.Height / rows;

            _customKeys = System.Array.Empty<int>();
            _customRects = System.Array.Empty<Rectangle>();
        }

        public SpriteAtlas(Texture2D texture)
        {
            Texture = texture;
            _columns = 0;
            _cellWidth = texture.Width;
            _cellHeight = texture.Height;

            _customKeys = System.Array.Empty<int>();
            _customRects = System.Array.Empty<Rectangle>();
        }

        public Rectangle GetRegion(int frameIndex)
        {
            int col = frameIndex % _columns;
            int row = frameIndex / _columns;
            return new Rectangle(col * _cellWidth, row * _cellHeight, _cellWidth, _cellHeight);
        }

        public Rectangle GetRegion(int column, int row)
        {
            return new Rectangle(column * _cellWidth, row * _cellHeight, _cellWidth, _cellHeight);
        }

        public void AddRegion(string name, Rectangle region)
        {
            int hashCode = name.GetHashCode(System.StringComparison.Ordinal);

            if (_customCount >= _customKeys.Length)
            {
                int newSize = _customKeys.Length == 0 ? 4 : _customKeys.Length * 2;
                System.Array.Resize(ref _customKeys, newSize);
                System.Array.Resize(ref _customRects, newSize);
            }

            _customKeys[_customCount] = hashCode;
            _customRects[_customCount] = region;
            _customCount++;
        }

        public Rectangle GetRegion(string name)
        {
            int hashCode = name.GetHashCode(System.StringComparison.Ordinal);

            for (int i = 0; i < _customCount; i++)
            {
                if (_customKeys[i] == hashCode)
                {
                    return _customRects[i];
                }
            }

            return Texture.Bounds;
        }
    }
}