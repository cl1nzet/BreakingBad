namespace Engine.Core {
    public static class AudioHash {
        public static int Get(string name)
        {
            unchecked
            {
                int hash = (int)2166136261;
                foreach (char c in name)
                    hash = (hash ^ c) * 16777619;
                return hash;
            }
        }
    }
}