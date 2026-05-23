namespace Game.Models {
    public readonly struct ReactionData {
        public ChemicalComponent[] Reactants { get; }
        public ChemicalComponent[] Products { get; }
        public Difficulty Difficulty { get; }

        public ReactionData(ChemicalComponent[] reactants, ChemicalComponent[] products, Difficulty difficulty) {
            Reactants = reactants;
            Products = products;
            Difficulty = difficulty;
        }
    }
}