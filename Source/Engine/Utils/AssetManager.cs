using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Engine.Utils
{
    public static class AssetManager
    {
        private static ContentManager _content;

        private static readonly Dictionary<string, object> _cache = new(128);

        public static void Init(ContentManager content) => _content = content;

        public static T Get<T>(string path)
        {
            if (_cache.TryGetValue(path, out var asset))
                return (T)asset;

            T newAsset = _content.Load<T>(path);
            _cache.Add(path, newAsset);
            return newAsset;
        }
        
        public static void Unload()
        {
            _cache.Clear();
            _content.Unload();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Texture2D GetTexture(string name) => Get<Texture2D>($"{name}");
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SpriteFont GetFont(string name) => Get<SpriteFont>($"{name}");
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Song GetMusic(string name) => Get<Song>($"{name}");
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SoundEffect GetSound(string name) => Get<SoundEffect>($"{name}");
    }
}