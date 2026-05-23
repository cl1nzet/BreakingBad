using Microsoft.Xna.Framework;

namespace Engine.Specs {
    public interface IUpdateable {
        void Update(GameTime gt);
        void Dispose();
    }
}