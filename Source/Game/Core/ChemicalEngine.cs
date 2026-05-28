using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Models;
using Game.Utils;

namespace Game.Core
{
    public static class ChemicalEngine
    {
        private struct ChemicalToken
        {
            public int ElementId;
            public int Count;
            public int GroupLevel;
        }

        private static readonly ReactionData[][] Pools;
        private static readonly Random Rnd = new();

        private readonly static Dictionary<Difficulty, float> _diffTimes = new()
        {
            { Difficulty.Easy, 25f },
            { Difficulty.Normal, 20f },
            { Difficulty.Hard, 15f },
            { Difficulty.Impossible, 10f }
        };

        private static Difficulty _currentDifficulty;
        public static Difficulty CurrentDifficulty
        {
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
            int idx = (int)difficulty;
            if (idx < 0 || idx >= Pools.Length) return Pools[0][0];
            var pool = Pools[idx];
            lock (Rnd)
            {
                return pool[Rnd.Next(pool.Length)];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetDifficultyMaxTime() => GetDifficultyMaxTime(CurrentDifficulty);

        public static float GetDifficultyMaxTime(Difficulty difficulty) => _diffTimes[difficulty];

        public static bool Verify(in ReactionData reaction, string input) {
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

                    while (i < inputSpan.Length && inputSpan[i] != ']')
                    {
                        i++;
                    }

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

        public static bool TryBalance(ReadOnlySpan<ChemicalComponent> reactants, ReadOnlySpan<ChemicalComponent> products, Span<int> outputCoefficients)
        {
            int totalComponents = reactants.Length + products.Length;
            if (outputCoefficients.Length < totalComponents) return false;

            Span<int> uniqueElements = stackalloc int[16];
            int uniqueCount = 0;

            Span<ChemicalToken> tokenBuffer = stackalloc ChemicalToken[64];
            Span<int> componentTokenOffsets = stackalloc int[totalComponents + 1];
            int totalTokens = 0;

            for (int i = 0; i < totalComponents; i++)
            {
                componentTokenOffsets[i] = totalTokens;
                ReadOnlySpan<char> formula = i < reactants.Length ? reactants[i].Formula.AsSpan() : products[i - reactants.Length].Formula.AsSpan();

                int parsed = ParseFormulaTokens(formula, tokenBuffer.Slice(totalTokens));
                for (int t = 0; t < parsed; t++)
                {
                    int id = tokenBuffer[totalTokens + t].ElementId;
                    bool exists = false;
                    for (int u = 0; u < uniqueCount; u++)
                    {
                        if (uniqueElements[u] == id) { exists = true; break; }
                    }
                    if (!exists && uniqueCount < 16)
                    {
                        uniqueElements[uniqueCount++] = id;
                    }
                }
                totalTokens += parsed;
            }
            componentTokenOffsets[totalComponents] = totalTokens;

            Span<float> matrix = stackalloc float[uniqueCount * totalComponents];
            matrix.Clear();

            for (int c = 0; c < totalComponents; c++)
            {
                int sign = c < reactants.Length ? 1 : -1;
                int startIdx = componentTokenOffsets[c];
                int endIdx = componentTokenOffsets[c + 1];

                for (int t = startIdx; t < endIdx; t++)
                {
                    int id = tokenBuffer[t].ElementId;
                    int row = -1;
                    for (int u = 0; u < uniqueCount; u++)
                    {
                        if (uniqueElements[u] == id) { row = u; break; }
                    }
                    if (row != -1)
                    {
                        matrix[row * totalComponents + c] += tokenBuffer[t].Count * sign;
                    }
                }
            }

            int vars = totalComponents - 1;
            Span<float> sysMatrix = stackalloc float[uniqueCount * (vars + 1)];

            for (int r = 0; r < uniqueCount; r++)
            {
                for (int c = 0; c < vars; c++)
                {
                    sysMatrix[r * (vars + 1) + c] = matrix[r * totalComponents + c];
                }
                sysMatrix[r * (vars + 1) + vars] = -matrix[r * totalComponents + totalComponents - 1];
            }

            int currentRow = 0;
            for (int col = 0; col < vars && currentRow < uniqueCount; col++)
            {
                int pivot = currentRow;
                for (int r = currentRow + 1; r < uniqueCount; r++)
                {
                    if (MathF.Abs(sysMatrix[r * (vars + 1) + col]) > MathF.Abs(sysMatrix[pivot * (vars + 1) + col]))
                    {
                        pivot = r;
                    }
                }

                if (MathF.Abs(sysMatrix[pivot * (vars + 1) + col]) < 1e-5f) continue;

                for (int c = 0; c <= vars; c++)
                {
                    float temp = sysMatrix[currentRow * (vars + 1) + c];
                    sysMatrix[currentRow * (vars + 1) + c] = sysMatrix[pivot * (vars + 1) + c];
                    sysMatrix[pivot * (vars + 1) + c] = temp;
                }

                for (int r = 0; r < uniqueCount; r++)
                {
                    if (r != currentRow)
                    {
                        float factor = sysMatrix[r * (vars + 1) + col] / sysMatrix[currentRow * (vars + 1) + col];
                        for (int c = col; c <= vars; c++)
                        {
                            sysMatrix[r * (vars + 1) + c] -= factor * sysMatrix[currentRow * (vars + 1) + c];
                        }
                    }
                }
                currentRow++;
            }

            Span<float> rawSol = stackalloc float[totalComponents];
            rawSol[totalComponents - 1] = 1.0f;

            for (int c = 0; c < vars; c++)
            {
                float val = 0f;
                for (int r = 0; r < uniqueCount; r++)
                {
                    if (MathF.Abs(sysMatrix[r * (vars + 1) + c]) > 1e-4f)
                    {
                        val = sysMatrix[r * (vars + 1) + vars] / sysMatrix[r * (vars + 1) + c];
                        break;
                    }
                }
                rawSol[c] = val;
            }

            float minVal = float.MaxValue;
            for (int i = 0; i < totalComponents; i++)
            {
                float absVal = MathF.Abs(rawSol[i]);
                if (absVal > 1e-4f && absVal < minVal) minVal = absVal;
            }

            if (minVal == float.MaxValue) return false;
            for (int i = 0; i < totalComponents; i++) rawSol[i] = MathF.Abs(rawSol[i] / minVal);

            for (int multiplier = 1; multiplier <= 120; multiplier++)
            {
                bool allInt = true;
                for (int i = 0; i < totalComponents; i++)
                {
                    float target = rawSol[i] * multiplier;
                    float rounded = MathF.Round(target);
                    if (MathF.Abs(target - rounded) > 0.05f || rounded < 1f)
                    {
                        allInt = false;
                        break;
                    }
                }

                if (allInt)
                {
                    for (int i = 0; i < totalComponents; i++)
                    {
                        outputCoefficients[i] = (int)MathF.Round(rawSol[i] * multiplier);
                    }
                    return true;
                }
            }

            return false;
        }

        public static float CalculateMolarMass(ReadOnlySpan<char> formula)
        {
            Span<ChemicalToken> tokens = stackalloc ChemicalToken[32];
            int count = ParseFormulaTokens(formula, tokens);
            float totalMass = 0f;

            for (int i = 0; i < count; i++)
            {
                totalMass += GetAtomicWeight(tokens[i].ElementId) * tokens[i].Count;
            }

            return totalMass;
        }

        private static int ParseFormulaTokens(ReadOnlySpan<char> formula, Span<ChemicalToken> tokens)
        {
            int tokenCount = 0;
            int currentLevel = 0;
            Span<int> groupStartIndices = stackalloc int[8];
            Span<int> groupLevels = stackalloc int[8];
            int groupPtr = 0;

            int len = formula.Length;
            int i = 0;

            while (i < len)
            {
                char c = formula[i];

                if (c == '(')
                {
                    currentLevel++;
                    if (groupPtr < 8)
                    {
                        groupStartIndices[groupPtr] = tokenCount;
                        groupLevels[groupPtr] = currentLevel;
                        groupPtr++;
                    }
                    i++;
                }
                else if (c == ')')
                {
                    i++;
                    int mult = 1;
                    int startNum = i;
                    while (i < len && char.IsDigit(formula[i])) i++;
                    if (i > startNum)
                    {
                        int.TryParse(formula.Slice(startNum, i - startNum), out mult);
                    }

                    for (int g = groupPtr - 1; g >= 0; g--)
                    {
                        if (groupLevels[g] == currentLevel)
                        {
                            int startToken = groupStartIndices[g];
                            for (int t = startToken; t < tokenCount; t++)
                            {
                                if (tokens[t].GroupLevel == currentLevel)
                                {
                                    tokens[t].Count *= mult;
                                }
                            }
                            groupPtr = g;
                            break;
                        }
                    }
                    currentLevel--;
                }
                else if (char.IsUpper(c))
                {
                    int startEl = i;
                    i++;
                    if (i < len && char.IsLower(formula[i])) i++;
                    ReadOnlySpan<char> elementSymbol = formula.Slice(startEl, i - startEl);

                    int count = 1;
                    if (i < len && char.IsDigit(formula[i]))
                    {
                        int startNum = i;
                        while (i < len && char.IsDigit(formula[i])) i++;
                        int.TryParse(formula.Slice(startNum, i - startNum), out count);
                    }

                    if (tokenCount < tokens.Length)
                    {
                        tokens[tokenCount++] = new ChemicalToken
                        {
                            ElementId = (elementSymbol[0] << 16) | (elementSymbol.Length > 1 ? elementSymbol[1] : 0),
                            Count = count,
                            GroupLevel = currentLevel
                        };
                    }
                }
                else
                {
                    i++;
                }
            }

            return tokenCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetAtomicWeight(int elementId)
        {
            char c1 = (char)(elementId >> 16);
            char c2 = (char)(elementId & 0xFFFF);

            if (c2 == 0)
            {
                switch (c1)
                {
                    case 'H': return 1.008f;
                    case 'C': return 12.011f;
                    case 'O': return 15.999f;
                    case 'S': return 32.06f;
                    case 'P': return 30.974f;
                    case 'N': return 14.007f;
                    case 'K': return 39.098f;
                    case 'I': return 126.90f;
                    case 'B': return 10.81f;
                    case 'F': return 18.998f;
                    case 'V': return 50.942f;
                    case 'W': return 183.84f;
                    case 'U': return 238.03f;
                }
            }
            else
            {
                if (c1 == 'M' && c2 == 'g') return 24.305f;
                if (c1 == 'Z' && c2 == 'n') return 65.38f;
                if (c1 == 'C' && c2 == 'l') return 35.45f;
                if (c1 == 'N' && c2 == 'a') return 22.99f;
                if (c1 == 'F' && c2 == 'e') return 55.845f;
                if (c1 == 'C' && c2 == 'u') return 63.546f;
                if (c1 == 'A' && c2 == 'l') return 26.982f;
                if (c1 == 'B' && c2 == 'a') return 137.33f;
                if (c1 == 'M' && c2 == 'n') return 54.938f;
                if (c1 == 'C' && c2 == 'r') return 51.996f;
                if (c1 == 'A' && c2 == 'g') return 107.87f;
                if (c1 == 'A' && c2 == 'u') return 196.97f;
                if (c1 == 'P' && c2 == 'b') return 207.2f;
                if (c1 == 'P' && c2 == 't') return 195.08f;
                if (c1 == 'B' && c2 == 'r') return 79.904f;
                if (c1 == 'C' && c2 == 'a') return 40.078f;
                if (c1 == 'L' && c2 == 'i') return 6.94f;
                if (c1 == 'S' && c2 == 'r') return 87.62f;
                if (c1 == 'N' && c2 == 'i') return 58.693f;
                if (c1 == 'C' && c2 == 'o') return 58.933f;
            }
            return 0f;
        }
    }
}