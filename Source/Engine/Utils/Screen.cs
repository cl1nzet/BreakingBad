using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Engine.Utils
{
    public static class Screen
    {
        private static GraphicsDeviceManager _graphics;
        private static GameWindow _window;

        public static void Initialize(GraphicsDeviceManager graphics, GameWindow window)
        {
            _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            _window = window ?? throw new ArgumentNullException(nameof(window));

            _window.ClientSizeChanged += OnWindowSizeChanged;
        }

        public static int ScreenWidth => _graphics.GraphicsDevice.Viewport.Width;
        public static int ScreenHeight => _graphics.GraphicsDevice.Viewport.Height;

        public static int ScreenCenterX => _graphics.GraphicsDevice.Viewport.Width / 2;
        public static int ScreenCenterY => _graphics.GraphicsDevice.Viewport.Height / 2;

        public static Vector2 ScreenCenter => new Vector2(_graphics.GraphicsDevice.Viewport.Width * 0.5f, _graphics.GraphicsDevice.Viewport.Height * 0.5f);
        public static Vector2 ScreenTop => new Vector2(_graphics.GraphicsDevice.Viewport.Width * 0.5f, 0f);
        public static Vector2 ScreenDown => new Vector2(_graphics.GraphicsDevice.Viewport.Width * 0.5f, _graphics.GraphicsDevice.Viewport.Height);
        public static Vector2 ScreenLeft => new Vector2(0f, _graphics.GraphicsDevice.Viewport.Height * 0.5f);
        public static Vector2 ScreenRight => new Vector2(_graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height * 0.5f);

        public static Vector2 ScreenTopLeft => Vector2.Zero;
        public static Vector2 ScreenTopRight => new Vector2(_graphics.GraphicsDevice.Viewport.Width, 0f);
        public static Vector2 ScreenLeftDown => new Vector2(0f, _graphics.GraphicsDevice.Viewport.Height);
        public static Vector2 ScreenRightDown => new Vector2(_graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height);

        public static void Resize(int width, int height, bool isFullScreen = false)
        {
            if (_graphics == null) return;

            _graphics.PreferredBackBufferHeight = height;
            _graphics.PreferredBackBufferWidth = width;
            _graphics.IsFullScreen = isFullScreen;
            _graphics.ApplyChanges();

            if (_graphics.GraphicsDevice != null)
            {
                _graphics.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);
            }
        }

        private static void OnWindowSizeChanged(object sender, EventArgs e)
        {
            int width = _window.ClientBounds.Width;
            int height = _window.ClientBounds.Height;

            if (_graphics.PreferredBackBufferWidth != width || _graphics.PreferredBackBufferHeight != height)
            {
                _graphics.PreferredBackBufferWidth = width;
                _graphics.PreferredBackBufferHeight = height;
                _graphics.ApplyChanges();

                if (_graphics.GraphicsDevice != null)
                {
                    _graphics.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);
                }
            }
        }

        public static void SetWindowTitle(string title) => _window.Title = title;
    }
}