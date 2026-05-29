using System;
using Game.Models;

namespace Game.Core {
    public static class ReactionBalancer {
        public static bool TryBalance(ReadOnlySpan<ChemicalComponent> reactants, ReadOnlySpan<ChemicalComponent> products, Span<int> outputCoefficients)
        {
            int totalComponents = reactants.Length + products.Length;
            if (outputCoefficients.Length < totalComponents) return false;

            Span<int> uniqueElements = stackalloc int[32];
            int uniqueCount = 0;

            Span<ChemicalToken> tokenBuffer = stackalloc ChemicalToken[128];
            Span<int> componentTokenOffsets = stackalloc int[totalComponents + 1];
            int totalTokens = 0;

            for (int i = 0; i < totalComponents; i++)
            {
                componentTokenOffsets[i] = totalTokens;
                ReadOnlySpan<char> formula = i < reactants.Length ? reactants[i].Formula.AsSpan() : products[i - reactants.Length].Formula.AsSpan();

                int parsed = FormulaParser.ParseFormulaTokens(formula, tokenBuffer.Slice(totalTokens));
                for (int t = 0; t < parsed; t++)
                {
                    int id = tokenBuffer[totalTokens + t].ElementId;
                    bool exists = false;
                    for (int u = 0; u < uniqueCount; u++)
                    {
                        if (uniqueElements[u] == id) { exists = true; break; }
                    }
                    if (!exists && uniqueCount < uniqueElements.Length)
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

            for (int multiplier = 1; multiplier <= 200; multiplier++)
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
    }
}