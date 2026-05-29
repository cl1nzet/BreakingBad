using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Models;
using Game.Utils;

namespace Game.Core {
    public static class ChemicalEngine {
        private static readonly ReactionPoolManager PoolManager;

        private static readonly Dictionary<Difficulty, float> DifficultyTimers = new()
        {
            { Difficulty.Easy, 20f },
            { Difficulty.Normal, 25f },
            { Difficulty.Hard, 30f },
            { Difficulty.Impossible, 45f }
        };

        public static Difficulty CurrentDifficulty { get; set; }

        static ChemicalEngine() {
            var defaultPools = ReactionDatabase.GetPredefinedPools();
            PoolManager = new ReactionPoolManager(defaultPools);

            try
            {
                Storage storage = new Storage();
                storage.Initialize("reactions.json");

                bool isDirty = false;
                string[] difficulties = { "Easy", "Normal", "Hard", "Impossible" };

                for (int i = 0; i < difficulties.Length; i++)
                {
                    var saved = storage.Get<ReactionData[]>(difficulties[i]);
                    if (saved != null && saved.Length > 0)
                    {
                        PoolManager.OverwritePool(i, saved);
                    }
                    else
                    {
                        storage.Set(difficulties[i], defaultPools[i]);
                        isDirty = true;
                    }
                }

                if (isDirty) storage.Save();
            }
            catch
            {
                
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReactionData Generate() => PoolManager.Generate(CurrentDifficulty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReactionData Generate(Difficulty difficulty) => PoolManager.Generate(difficulty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDifficultyMaxTime() => DifficultyTimers[CurrentDifficulty];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDifficultyMaxTime(Difficulty difficulty) => DifficultyTimers[difficulty];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CalculateMolarMass(ReadOnlySpan<char> formula) => FormulaParser.CalculateMolarMass(formula);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryBalance(ReadOnlySpan<ChemicalComponent> reactants, ReadOnlySpan<ChemicalComponent> products, Span<int> outputCoefficients)
            => ReactionBalancer.TryBalance(reactants, products, outputCoefficients);

        public static bool Verify(in ReactionData reaction, string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            int rLen = reaction.Reactants.Length;
            int pLen = reaction.Products.Length;
            int totalLen = rLen + pLen;

            Span<int> values = stackalloc int[totalLen];
            int count = 0;

            ReadOnlySpan<char> inputSpan = input.AsSpan();

            for (int i = 0; i < inputSpan.Length; i++)
            {
                if (inputSpan[i] == '[')
                {
                    int start = i + 1;
                    while (i < inputSpan.Length && inputSpan[i] != ']') i++;

                    if (i < inputSpan.Length && inputSpan[i] == ']')
                    {
                        var token = inputSpan.Slice(start, i - start);
                        if (count >= totalLen) return false;

                        if (token.IsWhiteSpace())
                        {
                            values[count++] = 1;
                        }
                        else if (!int.TryParse(token, out values[count++]))
                        {
                            return false;
                        }
                    }
                }
            }

            if (count != totalLen) return false;
            return Verify(reaction, values.Slice(0, rLen), values.Slice(rLen, pLen));
        }

        public static bool Verify(in ReactionData reaction, ReadOnlySpan<int> inputReactants, ReadOnlySpan<int> inputProducts)
        {
            if (inputReactants.Length != reaction.Reactants.Length || inputProducts.Length != reaction.Products.Length)
                return false;

            for (int i = 0; i < inputReactants.Length; i++)
                if (inputReactants[i] != reaction.Reactants[i].MinimalCoefficient) return false;

            for (int i = 0; i < inputProducts.Length; i++)
                if (inputProducts[i] != reaction.Products[i].MinimalCoefficient) return false;

            return true;
        }
    }
}