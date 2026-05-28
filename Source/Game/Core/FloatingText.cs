using Engine.Core;
using Engine.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game.Core {
    public sealed class FloatingText : GameObject, Engine.Specs.IVisualComponent {
        private readonly Text _textMesh;
        private readonly Color _color;
        private float _alpha = 1f;
        private const float FadeSpeed = 1.5f;
        private const float MoveSpeed = 60f;

        public FloatingText(Vector2 position, Vector2 scale, Scene scene, SpriteFont font, Color color, string text = "Ошибка!") : base(new Transform(position), scene) {
            _textMesh = new Text(position, scale, scene, text, font, color);
            _color = color;
        }

        public void Update(GameTime gt) {
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;
            _alpha -= FadeSpeed * dt;

            Transform.Position += new Vector2(0, MoveSpeed * dt);
            _textMesh.Transform.Position = Transform.Position;
            _textMesh.Color = _color * _alpha;

            if (_alpha <= 0f) {
                Dispose();
            }
        }

        public void Draw(SpriteBatch sb) {
            _textMesh.Draw(sb);
        }

        public override void OnToggled(bool val) {
            if (val) {
                CurrentScene.Add((Engine.Specs.IUpdateable)this);
                CurrentScene.Add((Engine.Specs.IDrawable)this);
            }
            else {
                CurrentScene.Remove((Engine.Specs.IUpdateable)this);
                CurrentScene.Remove((Engine.Specs.IDrawable)this);
            }
        }

        public void Dispose() {
            IsActive = false;
            OnToggled(false);
        }
    }
}