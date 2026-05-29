using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Models {
    public sealed class Outline {
        public Color Color { get; set; }
        public float Thickness { get; set; }

        public Outline(Color color, float thickness) {
            Color = color;
            Thickness = thickness;
        }

        public void Draw(SpriteBatch sb, SpriteFont font, string text, Vector2 position, Vector2 origin, Vector2 scale) {
            if (Thickness <= 0f || font == null || string.IsNullOrEmpty(text)) return;

            sb.DrawString(font, text, new Vector2(position.X - Thickness, position.Y - Thickness), Color, 0f, origin, scale, SpriteEffects.None, 0f);
            sb.DrawString(font, text, new Vector2(position.X + Thickness, position.Y - Thickness), Color, 0f, origin, scale, SpriteEffects.None, 0f);
            sb.DrawString(font, text, new Vector2(position.X - Thickness, position.Y + Thickness), Color, 0f, origin, scale, SpriteEffects.None, 0f);
            sb.DrawString(font, text, new Vector2(position.X + Thickness, position.Y + Thickness), Color, 0f, origin, scale, SpriteEffects.None, 0f);
            sb.DrawString(font, text, new Vector2(position.X - Thickness, position.Y), Color, 0f, origin, scale, SpriteEffects.None, 0f);
            sb.DrawString(font, text, new Vector2(position.X + Thickness, position.Y), Color, 0f, origin, scale, SpriteEffects.None, 0f);
            sb.DrawString(font, text, new Vector2(position.X, position.Y - Thickness), Color, 0f, origin, scale, SpriteEffects.None, 0f);
            sb.DrawString(font, text, new Vector2(position.X, position.Y + Thickness), Color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }
}