using Microsoft.Xna.Framework;

namespace Game.Models {
    public readonly struct KeyboardButton {
        public readonly string Key;
        public readonly Rectangle Bounds;

        public KeyboardButton(string key, Rectangle bounds)
        {
            Key = key;
            Bounds = bounds;
        }
    }
}