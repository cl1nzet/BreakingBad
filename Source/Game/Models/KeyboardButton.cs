using Microsoft.Xna.Framework;

namespace Game.Models {
    public readonly struct KeyboardButton {
        public readonly string Key;
        public readonly Rectangle RelativeBounds;
        public readonly Vector2 TextOffset;

        public KeyboardButton(string key, Rectangle relativeBounds, Vector2 textOffset)
        {
            Key = key;
            RelativeBounds = relativeBounds;
            TextOffset = textOffset;
        }
    }
}