using Microsoft.Xna.Framework;
using System;

namespace Engine.Models {
    public sealed class Transform {
        public readonly static Transform NULL = new Transform(Vector2.Zero);

        private Vector2 _position;
        private Vector2 _scale;
        private float _rotation;

        public Vector2 Position {
            get => _position;
            set {
                if(value != _position) {
                    _position = value;
                    onChanged?.Invoke();
                }
            }
        }
        public Vector2 Scale {
            get => _scale;
            set
            {
                if (value != _scale)
                {
                    _scale = value;
                    onChanged?.Invoke();
                }
            }
        }
        public float Rotation {
            get => _rotation;
            set
            {
                if (value != _rotation)
                {
                    _rotation = value;
                    onChanged?.Invoke();
                }
            }
        }

        public Action onChanged;
        public Transform(Vector2 position, Vector2? scale = null, float rotation = 0f) {
            Position = position;
            Scale = scale ?? Vector2.One;
            Rotation = rotation;
        }
    }
}