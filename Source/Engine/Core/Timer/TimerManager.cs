using System;
using Microsoft.Xna.Framework;

namespace Engine.Core.Timer {
    public static class TimerManager {
        public const int TimersCapacity = 8;

        private static TimerData[] _timers = new TimerData[TimersCapacity];
        private static Action<int>[] _callbacks = new Action<int>[TimersCapacity];

        private static int[] _idToStructureIndex = new int[TimersCapacity];

        private static int _activeCount = 0;
        private static int _idCounter = 0;

        public static int Add(float duration, Action<int> onEnded) {
            if (_activeCount >= _timers.Length) {
                Grow();
            }

            int id = ++_idCounter;

            if (id >= _idToStructureIndex.Length) {
                Array.Resize(ref _idToStructureIndex, _idToStructureIndex.Length * 2);
            }

            _idToStructureIndex[id] = _activeCount;

            _timers[_activeCount] = new TimerData { Remaining = duration, Id = id };
            _callbacks[_activeCount] = onEnded;

            _activeCount++;
            return id;
        }

        public static void Update(GameTime gt) {
            if (_timers.Length == 0) return;
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < _activeCount;) {
                _timers[i].Remaining -= dt;

                if (_timers[i].Remaining <= 0f) {
                    int expiredId = _timers[i].Id;
                    var callback = _callbacks[i];

                    int lastIndex = _activeCount - 1;
                    if (i != lastIndex) {
                        _timers[i] = _timers[lastIndex];
                        _callbacks[i] = _callbacks[lastIndex];

                        _idToStructureIndex[_timers[i].Id] = i;
                    }

                    _activeCount--;

                    callback?.Invoke(expiredId);
                }
                else {
                    i++;
                }
            }
        }

        public static float GetRemainingTime(int id) {
            if (id <= 0 || id > _idCounter)
                return -1f;

            int realIndex = _idToStructureIndex[id];

            if (realIndex >= _activeCount || _timers[realIndex].Id != id)
            {
                return -1f;
            }

            return _timers[realIndex].Remaining;
        }

        private static void Grow() {
            int newSize = _timers.Length * 2;
            Array.Resize(ref _timers, newSize);
            Array.Resize(ref _callbacks, newSize);
        }

        public static void Clear() {
            _activeCount = 0;
            _idCounter = 0;
            Array.Clear(_callbacks, 0, _callbacks.Length);
            Array.Clear(_idToStructureIndex, 0, _idToStructureIndex.Length);
        }
    }
}