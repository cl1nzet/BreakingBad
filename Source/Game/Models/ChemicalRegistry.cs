using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.Models {
    public static class ChemicalRegistry {
        private static readonly Dictionary<int, float> AtomicWeights = new() {
            { GetId("H"), 1.008f },   { GetId("He"), 4.0026f }, { GetId("Li"), 6.94f },   { GetId("Be"), 9.0122f },
            { GetId("B"), 10.81f },   { GetId("C"), 12.011f },  { GetId("N"), 14.007f },  { GetId("O"), 15.999f },
            { GetId("F"), 18.998f },  { GetId("Ne"), 20.180f },
            { GetId("Na"), 22.990f }, { GetId("Mg"), 24.305f }, { GetId("Al"), 26.982f }, { GetId("Si"), 28.085f },
            { GetId("P"), 30.974f },  { GetId("S"), 32.06f },   { GetId("Cl"), 35.45f },  { GetId("Ar"), 39.948f },
            { GetId("K"), 39.098f },  { GetId("Ca"), 40.078f }, { GetId("Sc"), 44.956f }, { GetId("Ti"), 47.867f },
            { GetId("V"), 50.942f },  { GetId("Cr"), 51.996f }, { GetId("Mn"), 54.938f }, { GetId("Fe"), 55.845f },
            { GetId("Co"), 58.933f }, { GetId("Ni"), 58.693f }, { GetId("Cu"), 63.546f }, { GetId("Zn"), 65.38f },
            { GetId("Ga"), 69.723f }, { GetId("Ge"), 72.630f }, { GetId("As"), 74.922f }, { GetId("Se"), 78.971f },
            { GetId("Br"), 79.904f }, { GetId("Kr"), 83.798f },
            { GetId("Rb"), 85.468f }, { GetId("Sr"), 87.62f },  { GetId("Y"), 88.906f },  { GetId("Zr"), 91.224f },
            { GetId("Mo"), 95.95f },  { GetId("Ag"), 107.87f }, { GetId("Cd"), 112.41f }, { GetId("Sn"), 118.71f },
            { GetId("I"), 126.90f },  { GetId("Xe"), 131.29f }, { GetId("Cs"), 132.91f }, { GetId("Ba"), 137.33f },
            { GetId("W"), 183.84f },  { GetId("Pt"), 195.08f }, { GetId("Au"), 196.97f }, { GetId("Hg"), 200.59f },
            { GetId("Pb"), 207.2f },  { GetId("Bi"), 208.98f }, { GetId("U"), 238.03f }
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetId(ReadOnlySpan<char> symbol)
        {
            if (symbol.Length == 0) return 0;
            return (symbol[0] << 16) | (symbol.Length > 1 ? symbol[1] : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetAtomicWeight(int elementId)
        {
            return AtomicWeights.TryGetValue(elementId, out float weight) ? weight : 0f;
        }

        public static string GetSymbol(int elementId)
        {
            char c1 = (char)(elementId >> 16);
            char c2 = (char)(elementId & 0xFFFF);
            return c2 == 0 ? c1.ToString() : $"{c1}{c2}";
        }
    }
}