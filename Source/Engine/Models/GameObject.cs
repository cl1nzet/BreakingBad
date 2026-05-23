using Engine.Core;

namespace Engine.Models {
    public class GameObject {
        private bool _isActive;
        public bool IsActive {
            get => _isActive;
            set {
                if (_isActive == value) return;
                _isActive = value;
                OnToggled(_isActive);
            }
        }
        public Transform Transform;
        public Scene CurrentScene { get; init; }
        public GameObject(Transform transform, Scene scene) {
            Transform = transform;
            CurrentScene = scene;

            IsActive = true;
        }
        public virtual void OnToggled(bool val) { }
    }
}