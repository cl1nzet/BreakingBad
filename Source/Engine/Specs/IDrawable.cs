using Microsoft.Xna.Framework.Graphics;

namespace Engine.Specs {
    public interface IDrawable {
        void Draw(SpriteBatch sb);
        void Dispose();
    }
}