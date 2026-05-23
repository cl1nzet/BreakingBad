using Microsoft.Xna.Framework;
using System;

namespace Engine.Utils {
    public static class Screen {
        private static GraphicsDeviceManager _graphics;
        private static GameWindow _window;

        private const int offsetX = -300;
        private const int offsetY = -150;

        public static void Initialize(GraphicsDeviceManager graphics, GameWindow window) {
            _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            _window = window ?? throw new ArgumentNullException(nameof(window));

            _window.ClientSizeChanged += OnWindowSizeChanged;
        }

        public static int ScreenWidth => _window.ClientBounds.Width + offsetX;
        public static int ScreenHeight => _window.ClientBounds.Height + offsetY;

        public static int ScreenCenterX => ScreenWidth / 2;
        public static int ScreenCenterY => ScreenHeight / 2;
        public static Vector2 ScreenCenter => new Vector2(ScreenCenterX, ScreenCenterY);

        public static void Resize(int width, int height, bool isFullScreen = false) {
            if (_graphics == null) return;

            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;
            _graphics.IsFullScreen = isFullScreen;
            _graphics.ApplyChanges();
        }

        private static void OnWindowSizeChanged(object sender, EventArgs e) {
            if (_graphics.PreferredBackBufferWidth != _window.ClientBounds.Width ||
                _graphics.PreferredBackBufferHeight != _window.ClientBounds.Height) {
                _graphics.PreferredBackBufferWidth = _window.ClientBounds.Width;
                _graphics.PreferredBackBufferHeight = _window.ClientBounds.Height;
                _graphics.ApplyChanges();
            }
        }

        public static void SetWindowTitle(string title) => _window.Title = title;
    }
}