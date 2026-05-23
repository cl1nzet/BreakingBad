using System;
using Game.Models;
using Game.Utils;

namespace Game.Core {
    public static class ChemicalEngine {
        private static readonly ReactionData[][] Pools;
        private static readonly Random Rnd = new();

        static ChemicalEngine() {
            Pools = new ReactionData[4][];

            ReactionData[] easyDefault = new[]
            {
                new ReactionData(new[] { new ChemicalComponent("H2", 2), new ChemicalComponent("O2", 1) }, new[] { new ChemicalComponent("H2O", 2) }, Difficulty.Easy),
                new ReactionData(new[] { new ChemicalComponent("C", 1), new ChemicalComponent("O2", 1) }, new[] { new ChemicalComponent("CO2", 1) }, Difficulty.Easy),
                new ReactionData(new[] { new ChemicalComponent("Mg", 2), new ChemicalComponent("O2", 1) }, new[] { new ChemicalComponent("MgO", 2) }, Difficulty.Easy),
                new ReactionData(new[] { new ChemicalComponent("S", 1), new ChemicalComponent("O2", 1) }, new[] { new ChemicalComponent("SO2", 1) }, Difficulty.Easy),
                new ReactionData(new[] { new ChemicalComponent("P", 4), new ChemicalComponent("O2", 5) }, new[] { new ChemicalComponent("P2O5", 2) }, Difficulty.Easy)
            };

            ReactionData[] normalDefault = new[]
            {
                new ReactionData(new[] { new ChemicalComponent("Zn", 1), new ChemicalComponent("HCl", 2) }, new[] { new ChemicalComponent("ZnCl2", 1), new ChemicalComponent("H2", 1) }, Difficulty.Normal),
                new ReactionData(new[] { new ChemicalComponent("Na", 2), new ChemicalComponent("H2O", 2) }, new[] { new ChemicalComponent("NaOH", 2), new ChemicalComponent("H2", 1) }, Difficulty.Normal),
                new ReactionData(new[] { new ChemicalComponent("CH4", 1), new ChemicalComponent("O2", 2) }, new[] { new ChemicalComponent("CO2", 1), new ChemicalComponent("H2O", 2) }, Difficulty.Normal),
                new ReactionData(new[] { new ChemicalComponent("Fe", 1), new ChemicalComponent("CuSO4", 1) }, new[] { new ChemicalComponent("FeSO4", 1), new ChemicalComponent("Cu", 1) }, Difficulty.Normal),
                new ReactionData(new[] { new ChemicalComponent("H2O2", 2) }, new[] { new ChemicalComponent("H2O", 2), new ChemicalComponent("O2", 1) }, Difficulty.Normal)
            };

            ReactionData[] hardDefault = new[]
            {
                new ReactionData(new[] { new ChemicalComponent("Al", 2), new ChemicalComponent("Cl2", 3) }, new[] { new ChemicalComponent("AlCl3", 2) }, Difficulty.Hard),
                new ReactionData(new[] { new ChemicalComponent("C3H8", 1), new ChemicalComponent("O2", 5) }, new[] { new ChemicalComponent("CO2", 3), new ChemicalComponent("H2O", 4) }, Difficulty.Hard),
                new ReactionData(new[] { new ChemicalComponent("Fe(OH)3", 2) }, new[] { new ChemicalComponent("Fe2O3", 1), new ChemicalComponent("H2O", 3) }, Difficulty.Hard),
                new ReactionData(new[] { new ChemicalComponent("BaCl2", 3), new ChemicalComponent("Al2(SO4)3", 1) }, new[] { new ChemicalComponent("BaSO4", 3), new ChemicalComponent("AlCl3", 2) }, Difficulty.Hard),
                new ReactionData(new[] { new ChemicalComponent("NH3", 4), new ChemicalComponent("O2", 5) }, new[] { new ChemicalComponent("NO", 4), new ChemicalComponent("H2O", 6) }, Difficulty.Hard)
            };

            ReactionData[] impossibleDefault = new[]
            {
                new ReactionData(new[] { new ChemicalComponent("KMnO4", 2), new ChemicalComponent("HCl", 16) }, new[] { new ChemicalComponent("KCl", 2), new ChemicalComponent("MnCl2", 2), new ChemicalComponent("Cl2", 5), new ChemicalComponent("H2O", 8) }, Difficulty.Impossible),
                new ReactionData(new[] { new ChemicalComponent("Cu", 1), new ChemicalComponent("HNO3", 4) }, new[] { new ChemicalComponent("Cu(NO3)2", 1), new ChemicalComponent("NO2", 2), new ChemicalComponent("H2O", 2) }, Difficulty.Impossible),
                new ReactionData(new[] { new ChemicalComponent("Cu", 3), new ChemicalComponent("HNO3", 8) }, new[] { new ChemicalComponent("Cu(NO3)2", 3), new ChemicalComponent("NO", 2), new ChemicalComponent("H2O", 4) }, Difficulty.Impossible),
                new ReactionData(new[] { new ChemicalComponent("K2Cr2O7", 1), new ChemicalComponent("HCl", 14) }, new[] { new ChemicalComponent("KCl", 2), new ChemicalComponent("CrCl3", 2), new ChemicalComponent("Cl2", 3), new ChemicalComponent("H2O", 7) }, Difficulty.Impossible),
                new ReactionData(new[] { new ChemicalComponent("FeSO4", 10), new ChemicalComponent("KMnO4", 2), new ChemicalComponent("H2SO4", 8) }, new[] { new ChemicalComponent("Fe2(SO4)3", 5), new ChemicalComponent("MnSO4", 2), new ChemicalComponent("K2SO4", 1), new ChemicalComponent("H2O", 8) }, Difficulty.Impossible)
            };

            try
            {
                Storage storage = new Storage();
                storage.Initialize("reactions.json");

                Pools[(int)Difficulty.Easy] = storage.Get<ReactionData[]>("Easy") ?? easyDefault;
                Pools[(int)Difficulty.Normal] = storage.Get<ReactionData[]>("Normal") ?? normalDefault;
                Pools[(int)Difficulty.Hard] = storage.Get<ReactionData[]>("Hard") ?? hardDefault;
                Pools[(int)Difficulty.Impossible] = storage.Get<ReactionData[]>("Impossible") ?? impossibleDefault;

                if (storage.Get<ReactionData[]>("Easy") == null)
                {
                    storage.Set("Easy", Pools[(int)Difficulty.Easy]);
                    storage.Set("Normal", Pools[(int)Difficulty.Normal]);
                    storage.Set("Hard", Pools[(int)Difficulty.Hard]);
                    storage.Set("Impossible", Pools[(int)Difficulty.Impossible]);
                    storage.Save();
                }
            }
            catch
            {
                Pools[(int)Difficulty.Easy] = easyDefault;
                Pools[(int)Difficulty.Normal] = normalDefault;
                Pools[(int)Difficulty.Hard] = hardDefault;
                Pools[(int)Difficulty.Impossible] = impossibleDefault;
            }
        }

        public static ReactionData Generate(Difficulty difficulty)
        {
            var pool = Pools[(int)difficulty];
            return pool[Rnd.Next(pool.Length)];
        }

        public static bool Verify(in ReactionData reaction, ReadOnlySpan<int> inputReactants, ReadOnlySpan<int> inputProducts)
        {
            if (inputReactants.Length != reaction.Reactants.Length || inputProducts.Length != reaction.Products.Length)
            {
                return false;
            }

            int firstInput = inputReactants[0];
            int firstCorrect = reaction.Reactants[0].MinimalCoefficient;

            if (firstInput <= 0 || firstInput % firstCorrect != 0)
            {
                return false;
            }

            int factor = firstInput / firstCorrect;

            for (int i = 1; i < inputReactants.Length; i++)
            {
                if (inputReactants[i] <= 0 || inputReactants[i] != reaction.Reactants[i].MinimalCoefficient * factor)
                {
                    return false;
                }
            }

            for (int i = 0; i < inputProducts.Length; i++)
            {
                if (inputProducts[i] <= 0 || inputProducts[i] != reaction.Products[i].MinimalCoefficient * factor)
                {
                    return false;
                }
            }

            int currentGcd = inputReactants[0];

            for (int i = 1; i < inputReactants.Length; i++)
            {
                currentGcd = ComputeGcd(currentGcd, inputReactants[i]);
            }

            for (int i = 0; i < inputProducts.Length; i++)
            {
                currentGcd = ComputeGcd(currentGcd, inputProducts[i]);
            }

            return currentGcd == 1;
        }

        private static int ComputeGcd(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
    }
}