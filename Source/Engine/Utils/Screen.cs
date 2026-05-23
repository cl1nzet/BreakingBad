using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.Utils {
    public static class Screen {
        private static GraphicsDeviceManager _graphics;
        private static GameWindow _window;

        public static void Initialize(GraphicsDeviceManager graphics, GameWindow window) {
            _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            _window = window ?? throw new ArgumentNullException(nameof(window));

            _window.ClientSizeChanged += OnWindowSizeChanged;
        }

        public static int ScreenWidth => _graphics.GraphicsDevice.Viewport.Width;
        public static int ScreenHeight => _graphics.GraphicsDevice.Viewport.Height;

        public static int ScreenCenterX => ScreenWidth / 2;
        public static int ScreenCenterY => ScreenHeight / 2;
        public static Vector2 ScreenCenter => new Vector2(ScreenCenterX, ScreenCenterY);
        public static Vector2 ScreenTop => new Vector2(ScreenCenterX, 0);
        public static Vector2 ScreenDown => new Vector2(ScreenCenterX, ScreenHeight);
        public static Vector2 ScreenLeft => new Vector2(0, ScreenCenterY);
        public static Vector2 ScreenRight => new Vector2(ScreenWidth, ScreenCenterY);

        public static void Resize(int width, int height, bool isFullScreen = false) {
            if (_graphics == null) return;

            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.IsFullScreen = isFullScreen;
            _graphics.ApplyChanges();

            _graphics.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);
        }

        private static void OnWindowSizeChanged(object sender, EventArgs e) {
            if (_graphics.PreferredBackBufferWidth != ScreenWidth ||
                _graphics.PreferredBackBufferHeight != ScreenHeight) {
                _graphics.PreferredBackBufferWidth = ScreenWidth;
                _graphics.PreferredBackBufferHeight = ScreenHeight;
                _graphics.ApplyChanges();
            }
        }

        public static void SetWindowTitle(string title) => _window.Title = title;
    }
}