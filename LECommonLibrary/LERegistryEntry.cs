namespace LECommonLibrary
{
    public struct LERegistryEntry
    {
        public string Data;
        public string Key;
        public string Name;
        public string Root;
        public string Type;

        public LERegistryEntry(string root,
                               string key,
                               string name,
                               string type,
                               string data)
        {
            Root = root;
            Key = key;
            Name = name;
            Type = type;
            Data = data;
        }
    }
}