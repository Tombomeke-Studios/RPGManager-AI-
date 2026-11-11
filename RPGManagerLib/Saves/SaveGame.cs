using RPGManagerLib.Worlds;

namespace RPGManagerLib.Saves
{
    /// <summary>
    /// Serializable snapshot of the shared world: flags, counters, reputation, quests, chronicle and vault.
    /// </summary>
    public class WorldStateSaveData
    {
        public Dictionary<string, bool> Flags { get; set; } = new();
        public Dictionary<string, int> Counters { get; set; } = new();
        public Dictionary<string, int> Reputation { get; set; } = new();
        public Dictionary<string, string> Deeds { get; set; } = new();
        public Dictionary<string, QuestStatus> Quests { get; set; } = new();
        public List<string> EventLog { get; set; } = new();
        public List<EquipableSaveData> Vault { get; set; } = new();

        public WorldStateSaveData() { }

        /// <summary>Captures a world state for saving.</summary>
        public WorldStateSaveData(WorldState world)
        {
            Flags = new Dictionary<string, bool>(world.Flags);
            Counters = new Dictionary<string, int>(world.Counters);
            Reputation = new Dictionary<string, int>(world.Reputation);
            Deeds = new Dictionary<string, string>(world.Deeds);
            Quests = new Dictionary<string, QuestStatus>(world.Quests);
            EventLog = new List<string>(world.EventLog);
            Vault = world.Vault.Select(CharacterSaveData.FromEquipable).ToList();
        }

        /// <summary>Rebuilds the world state.</summary>
        public WorldState ToWorldState() => new()
        {
            Flags = new Dictionary<string, bool>(Flags),
            Counters = new Dictionary<string, int>(Counters),
            Reputation = new Dictionary<string, int>(Reputation),
            Deeds = new Dictionary<string, string>(Deeds),
            Quests = new Dictionary<string, QuestStatus>(Quests),
            EventLog = new List<string>(EventLog),
            Vault = Vault.Select(e => e.ToEquipable()).ToList()
        };
    }

    /// <summary>
    /// Root of the save file: the whole roster plus the world they share.
    /// </summary>
    public class SaveGame
    {
        /// <summary>Save format version, for future migrations.</summary>
        public int Version { get; set; } = 2;
        public List<CharacterSaveData> Characters { get; set; } = new();

        /// <summary>Index of the hero that was active when the game was saved.</summary>
        public int ActiveIndex { get; set; }
        public WorldStateSaveData World { get; set; } = new();
    }
}
