namespace Mimi.Prototypes.LevelManagement
{
    public readonly struct LevelOrderEntry
    {
        public string Id { get; }
        public int Chapter { get; }

        public LevelOrderEntry(string id, int chapter)
        {
            Id = id;
            Chapter = chapter;
        }
    }
}
