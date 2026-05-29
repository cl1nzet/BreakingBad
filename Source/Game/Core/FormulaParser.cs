using Game.Models;
using System;

namespace Game.Core {
    public static class FormulaParser {
        public static float CalculateMolarMass(ReadOnlySpan<char> formula) {
            Span<ChemicalToken> tokens = stackalloc ChemicalToken[64];
            int count = ParseFormulaTokens(formula, tokens);
            float totalMass = 0f;

            for (int i = 0; i < count; i++)
            {
                totalMass += ChemicalRegistry.GetAtomicWeight(tokens[i].ElementId) * tokens[i].Count;
            }

            return totalMass;
        }

        public static int ParseFormulaTokens(ReadOnlySpan<char> formula, Span<ChemicalToken> tokens) {
            int tokenCount = 0;
            int currentLevel = 0;
            Span<int> groupStartIndices = stackalloc int[16];
            Span<int> groupLevels = stackalloc int[16];
            int groupPtr = 0;

            int len = formula.Length;
            int i = 0;

            while (i < len)
            {
                char c = formula[i];

                if (c == '(' || c == '[')
                {
                    currentLevel++;
                    if (groupPtr < groupStartIndices.Length)
                    {
                        groupStartIndices[groupPtr] = tokenCount;
                        groupLevels[groupPtr] = currentLevel;
                        groupPtr++;
                    }
                    i++;
                }
                else if (c == ')' || c == ']')
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
                            ElementId = ChemicalRegistry.GetId(elementSymbol),
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
    }
}