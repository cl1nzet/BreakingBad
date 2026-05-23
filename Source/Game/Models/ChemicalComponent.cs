namespace Game.Models {
    public readonly struct ChemicalComponent {
        public string Formula { get; }
        public int MinimalCoefficient { get; }

        public ChemicalComponent(string formula, int minimalCoefficient) {
            Formula = formula;
            MinimalCoefficient = minimalCoefficient;
        }
    }
}