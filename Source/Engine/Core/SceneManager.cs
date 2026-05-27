using System.Threading.Tasks;
using Engine.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Core {
    public static class SceneManager {
        public const int ScenesCount = 8;

        public static Scene CurrentScene { get; private set; }

        public static int NextScene => (CurrentScene != null && CurrentScene.Index + 1 < _scenes.Length) ? CurrentScene.Index + 1 : -1;
        public static int PreviousScene => (CurrentScene != null && CurrentScene.Index - 1 >= 0) ? CurrentScene.Index - 1 : -1;

        private readonly static Arcane<Scene> _scenes = new(ScenesCount);

        public static void Add(Scene scene)
        {
            scene.Index = _scenes.Length;
            _scenes.Add(scene);
        }

        public static async Task LoadSceneAsync(Scene scene, int delay = 100)
        {
            if (scene == null || !_scenes.Contains(scene)) return;

            CurrentScene?.Unload();

            await Task.Delay(delay);

            CurrentScene = scene;
            CurrentScene.Start();
        }

        public static void LoadScene() => _ = LoadSceneAsync(0);

        public static async Task LoadSceneAsync(int index)
        {
            if (index < 0 || index >= _scenes.Length) return;

            await LoadSceneAsync(_scenes[index]);
        }

        public static async Task LoadSceneAsync(string name)
        {
            for (int i = 0; i < _scenes.Length; i++)
            {
                if (_scenes[i].SceneName == name)
                {
                    await LoadSceneAsync(_scenes[i]);
                    return;
                }
            }
        }

        public static void LoadScene(int index) => _ = LoadSceneAsync(index);
        public static void LoadScene(string name) => _ = LoadSceneAsync(name);

        public static void Start() => CurrentScene?.Start();
        public static void Update(GameTime gt) => CurrentScene?.Update(gt);
        public static void Draw(SpriteBatch sb) => CurrentScene?.Draw(sb);
    }
}