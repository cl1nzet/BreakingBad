using System;
using System.Collections.Generic;
using Game.Models;

namespace Game.Core {
    public sealed class ReactionPoolManager {
        private readonly ReactionData[][] _pools;
        private readonly List<int>[] _unusedIndices;
        private readonly Random _rnd = new();

        public ReactionPoolManager(ReactionData[][] populatedPools) {
            _pools = populatedPools;
            _unusedIndices = new List<int>[_pools.Length];

            for (int i = 0; i < _pools.Length; i++)
            {
                _unusedIndices[i] = new List<int>(_pools[i].Length);
                ResetAndShufflePool(i);
            }
        }

        public void OverwritePool(int difficultyIdx, ReactionData[] newPool) {
            lock (_rnd)
            {
                if (difficultyIdx >= 0 && difficultyIdx < _pools.Length)
                {
                    _pools[difficultyIdx] = newPool;
                    ResetAndShufflePool(difficultyIdx);
                }
            }
        }

        public ReactionData[] GetPool(int difficultyIdx) => _pools[difficultyIdx];

        public ReactionData Generate(Difficulty difficulty) {
            int idx = (int)difficulty;
            if (idx < 0 || idx >= _pools.Length) return _pools[0][0];

            lock (_rnd)
            {
                if (_unusedIndices[idx].Count == 0)
                {
                    ResetAndShufflePool(idx);
                }

                int nextReactionIdx = _unusedIndices[idx][0];
                _unusedIndices[idx].RemoveAt(0);

                return _pools[idx][nextReactionIdx];
            }
        }

        private void ResetAndShufflePool(int poolIdx) {
            var list = _unusedIndices[poolIdx];
            list.Clear();

            int count = _pools[poolIdx].Length;
            for (int i = 0; i < count; i++) list.Add(i);

            for (int i = count - 1; i > 0; i--)
            {
                int j = _rnd.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}