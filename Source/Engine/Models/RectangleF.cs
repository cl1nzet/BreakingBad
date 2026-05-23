using Microsoft.Xna.Framework;

namespace Engine.Models {
    public readonly struct RectangleF {
        public readonly float X;
        public readonly float Y;
        public readonly float Width;
        public readonly float Height;

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Contains(Vector2 value)
        {
            return value.X >= X && value.X <= X + Width && value.Y >= Y && value.Y <= Y + Height;
        }
    }
}