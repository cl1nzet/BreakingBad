using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Engine.Core
{
    public abstract class Scene
    {
        private const int InitialCapacity = 16;

        private readonly List<Specs.IDrawable> _drawables = new(InitialCapacity);
        private readonly List<Specs.IUpdateable> _updateables = new(InitialCapacity);

        private readonly List<Specs.IDrawable> _drawablesToRemove = new();
        private readonly List<Specs.IDrawable> _drawablesToAdd = new();
        private readonly List<Specs.IUpdateable> _updateablesToRemove = new();
        private readonly List<Specs.IUpdateable> _updateablesToAdd = new();

        public abstract string SceneName { get; set; }
        public int Index { get; set; }

        public void Add(Specs.IDrawable drawable) => _drawablesToAdd.Add(drawable);
        public void Remove(Specs.IDrawable drawable) => _drawablesToRemove.Add(drawable);
        public void Add(Specs.IUpdateable updateable) => _updateablesToAdd.Add(updateable);
        public void Remove(Specs.IUpdateable updateable) => _updateablesToRemove.Add(updateable);

        public virtual void Draw(SpriteBatch sb)
        {
            PostProcessCollections();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            for (int i = 0; i < _drawables.Count; i++) {
                _drawables[i].Draw(sb);
            }

            sb.End();
        }

        public virtual void Update(GameTime gt)
        {
            PostProcessCollections();

            for (int i = 0; i < _updateables.Count; i++)
            {
                _updateables[i].Update(gt);
            }
        }

        private void PostProcessCollections() {
            if (_updateablesToRemove.Count > 0)
            {
                for (int i = 0; i < _updateablesToRemove.Count; i++) _updateables.Remove(_updateablesToRemove[i]);
                _updateablesToRemove.Clear();
            }

            if (_drawablesToRemove.Count > 0)
            {
                for (int i = 0; i < _drawablesToRemove.Count; i++) _drawables.Remove(_drawablesToRemove[i]);
                _drawablesToRemove.Clear();
            }

            if (_updateablesToAdd.Count > 0)
            {
                for (int i = 0; i < _updateablesToAdd.Count; i++) _updateables.Add(_updateablesToAdd[i]);
                _updateablesToAdd.Clear();
            }

            if (_drawablesToAdd.Count > 0)
            {
                for (int i = 0; i < _drawablesToAdd.Count; i++) _drawables.Add(_drawablesToAdd[i]);
                _drawablesToAdd.Clear();
            }
        }

        public abstract void Start();

        public virtual void Unload() {
            for (int i = 0; i < _drawables.Count; i++) _drawables[i].Dispose();
            for (int i = 0; i < _updateables.Count; i++) _updateables[i].Dispose();

            _drawables.Clear();
            _updateables.Clear();
        }
    }
}