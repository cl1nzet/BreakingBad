using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Models;
using Game.Utils;

namespace Game.Core
{
    public static class ChemicalEngine
    {
        private static readonly ReactionData[][] Pools;
        private static readonly Random Rnd = new();

        private readonly static Dictionary<Difficulty, float> _diffTimes = new() {
        { Difficulty.Easy, 25f },
        { Difficulty.Normal, 20f },
        { Difficulty.Hard, 15f },
        { Difficulty.Impossible, 10f }
        };

        private static Difficulty _currentDifficulty;
        public static Difficulty CurrentDifficulty {
            get => _currentDifficulty;
            set => _currentDifficulty = value;
        }

        static ChemicalEngine()
        {
            var easyDefault = new[]
            {
                new ReactionData(new[] { new ChemicalComponent("H2", 2), new ChemicalComponent("O2", 1) }, new[] { new ChemicalComponent("H2O", 2) }, Difficulty.Easy),
                new ReactionData(new[] { new ChemicalComponent("C", 1), new ChemicalComponent("O2", 1) }, new[] { new ChemicalComponent("CO2", 1) }, Difficulty.Easy),
                new ReactionData(new[] { new ChemicalComponent("Mg", 2), new ChemicalComponent("O2", 1) }, new[] { new ChemicalComponent("MgO", 2) }, Difficulty.Easy),
                new ReactionData(new[] { new ChemicalComponent("S", 1), new ChemicalComponent("O2", 1) }, new[] { new ChemicalComponent("SO2", 1) }, Difficulty.Easy),
                new ReactionData(new[] { new ChemicalComponent("P", 4), new ChemicalComponent("O2", 5) }, new[] { new ChemicalComponent("P2O5", 2) }, Difficulty.Easy)
            };

            var normalDefault = new[]
            {
                new ReactionData(new[] { new ChemicalComponent("Zn", 1), new ChemicalComponent("HCl", 2) }, new[] { new ChemicalComponent("ZnCl2", 1), new ChemicalComponent("H2", 1) }, Difficulty.Normal),
                new ReactionData(new[] { new ChemicalComponent("Na", 2), new ChemicalComponent("H2O", 2) }, new[] { new ChemicalComponent("NaOH", 2), new ChemicalComponent("H2", 1) }, Difficulty.Normal),
                new ReactionData(new[] { new ChemicalComponent("CH4", 1), new ChemicalComponent("O2", 2) }, new[] { new ChemicalComponent("CO2", 1), new ChemicalComponent("H2O", 2) }, Difficulty.Normal),
                new ReactionData(new[] { new ChemicalComponent("Fe", 1), new ChemicalComponent("CuSO4", 1) }, new[] { new ChemicalComponent("FeSO4", 1), new ChemicalComponent("Cu", 1) }, Difficulty.Normal),
                new ReactionData(new[] { new ChemicalComponent("H2O2", 2) }, new[] { new ChemicalComponent("H2O", 2), new ChemicalComponent("O2", 1) }, Difficulty.Normal)
            };

            var hardDefault = new[]
            {
                new ReactionData(new[] { new ChemicalComponent("Al", 2), new ChemicalComponent("Cl2", 3) }, new[] { new ChemicalComponent("AlCl3", 2) }, Difficulty.Hard),
                new ReactionData(new[] { new ChemicalComponent("C3H8", 1), new ChemicalComponent("O2", 5) }, new[] { new ChemicalComponent("CO2", 3), new ChemicalComponent("H2O", 4) }, Difficulty.Hard),
                new ReactionData(new[] { new ChemicalComponent("Fe(OH)3", 2) }, new[] { new ChemicalComponent("Fe2O3", 1), new ChemicalComponent("H2O", 3) }, Difficulty.Hard),
                new ReactionData(new[] { new ChemicalComponent("BaCl2", 3), new ChemicalComponent("Al2(SO4)3", 1) }, new[] { new ChemicalComponent("BaSO4", 3), new ChemicalComponent("AlCl3", 2) }, Difficulty.Hard),
                new ReactionData(new[] { new ChemicalComponent("NH3", 4), new ChemicalComponent("O2", 5) }, new[] { new ChemicalComponent("NO", 4), new ChemicalComponent("H2O", 6) }, Difficulty.Hard)
            };

            var impossibleDefault = new[]
            {
                new ReactionData(new[] { new ChemicalComponent("KMnO4", 2), new ChemicalComponent("HCl", 16) }, new[] { new ChemicalComponent("KCl", 2), new ChemicalComponent("MnCl2", 2), new ChemicalComponent("Cl2", 5), new ChemicalComponent("H2O", 8) }, Difficulty.Impossible),
                new ReactionData(new[] { new ChemicalComponent("Cu", 1), new ChemicalComponent("HNO3", 4) }, new[] { new ChemicalComponent("Cu(NO3)2", 1), new ChemicalComponent("NO2", 2), new ChemicalComponent("H2O", 2) }, Difficulty.Impossible),
                new ReactionData(new[] { new ChemicalComponent("Cu", 3), new ChemicalComponent("HNO3", 8) }, new[] { new ChemicalComponent("Cu(NO3)2", 3), new ChemicalComponent("NO", 2), new ChemicalComponent("H2O", 4) }, Difficulty.Impossible),
                new ReactionData(new[] { new ChemicalComponent("K2Cr2O7", 1), new ChemicalComponent("HCl", 14) }, new[] { new ChemicalComponent("KCl", 2), new ChemicalComponent("CrCl3", 2), new ChemicalComponent("Cl2", 3), new ChemicalComponent("H2O", 7) }, Difficulty.Impossible),
                new ReactionData(new[] { new ChemicalComponent("FeSO4", 10), new ChemicalComponent("KMnO4", 2), new ChemicalComponent("H2SO4", 8) }, new[] { new ChemicalComponent("Fe2(SO4)3", 5), new ChemicalComponent("MnSO4", 2), new ChemicalComponent("K2SO4", 1), new ChemicalComponent("H2O", 8) }, Difficulty.Impossible)
            };

            Pools = new[] { easyDefault, normalDefault, hardDefault, impossibleDefault };

            try
            {
                Storage storage = new Storage();
                storage.Initialize("reactions.json");

                bool isDirty = false;

                var easy = storage.Get<ReactionData[]>("Easy");
                if (easy != null) Pools[(int)Difficulty.Easy] = easy; else { storage.Set("Easy", easyDefault); isDirty = true; }

                var normal = storage.Get<ReactionData[]>("Normal");
                if (normal != null) Pools[(int)Difficulty.Normal] = normal; else { storage.Set("Normal", normalDefault); isDirty = true; }

                var hard = storage.Get<ReactionData[]>("Hard");
                if (hard != null) Pools[(int)Difficulty.Hard] = hard; else { storage.Set("Hard", hardDefault); isDirty = true; }

                var imp = storage.Get<ReactionData[]>("Impossible");
                if (imp != null) Pools[(int)Difficulty.Impossible] = imp; else { storage.Set("Impossible", impossibleDefault); isDirty = true; }

                if (isDirty) storage.Save();
            }
            catch { }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReactionData Generate() => Generate(CurrentDifficulty);

        public static ReactionData Generate(Difficulty difficulty)
        {
            var pool = Pools[(int)difficulty];
            lock (Rnd)
            {
                return pool[Rnd.Next(pool.Length)];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDifficultyMaxTime() => GetDifficultyMaxTime(CurrentDifficulty);

        public static float GetDifficultyMaxTime(Difficulty difficulty) => _diffTimes[difficulty];

        public static bool Verify(in ReactionData reaction, string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            string processed = input.Trim().ToLower();

            int rLen = reaction.Reactants.Length;
            int pLen = reaction.Products.Length;
            int totalLen = rLen + pLen;

            Span<int> values = stackalloc int[totalLen];
            int count = 0;
            int start = 0;

            for (int i = 0; i <= processed.Length; i++)
            {
                if (i == processed.Length || char.IsWhiteSpace(processed[i]))
                {
                    if (i > start)
                    {
                        if (count >= totalLen || !int.TryParse(processed.AsSpan(start, i - start), out values[count++]))
                        {
                            return false;
                        }
                    }
                    start = i + 1;
                }
            }

            if (count != totalLen) return false;

            return Verify(reaction, values.Slice(0, rLen), values.Slice(rLen, pLen));
        }

        public static bool Verify(in ReactionData reaction, ReadOnlySpan<int> inputReactants, ReadOnlySpan<int> inputProducts)
        {
            if (inputReactants.Length != reaction.Reactants.Length || inputProducts.Length != reaction.Products.Length)
            {
                return false;
            }

            for (int i = 0; i < inputReactants.Length; i++)
            {
                if (inputReactants[i] != reaction.Reactants[i].MinimalCoefficient) return false;
            }

            for (int i = 0; i < inputProducts.Length; i++)
            {
                if (inputProducts[i] != reaction.Products[i].MinimalCoefficient) return false;
            }

            return true;
        }
    }
}