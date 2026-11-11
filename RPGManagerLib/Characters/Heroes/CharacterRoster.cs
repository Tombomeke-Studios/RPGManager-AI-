namespace RPGManagerLib.Characters.Heroes
{
    /// <summary>
    /// The Keeper's bonded heroes. Owns the rules a raw list cannot enforce:
    /// how many slots are unlocked, who is active, and how strong a newly bonded hero starts.
    /// </summary>
    public class CharacterRoster
    {
        /// <summary>Slot number → hero level (of any hero) required to unlock it.</summary>
        public static readonly Dictionary<int, int> SlotUnlockLevels = new()
        {
            { 2, 3 },
            { 3, 6 }
        };

        private readonly List<Character> heroes = new();

        /// <summary>All bonded heroes.</summary>
        public IReadOnlyList<Character> Heroes => heroes;

        /// <summary>The hero the Keeper is currently guiding.</summary>
        public Character? Active { get; private set; }

        /// <summary>Highest level among all heroes (0 with an empty roster).</summary>
        public int HighestLevel => heroes.Count == 0 ? 0 : heroes.Max(h => h.Level);

        /// <summary>How many heroes the Keeper can currently hold a bond with.</summary>
        public int UnlockedSlots => 1 + SlotUnlockLevels.Count(slot => HighestLevel >= slot.Value);

        /// <summary>Total slots that exist.</summary>
        public int MaxSlots => 1 + SlotUnlockLevels.Count;

        /// <summary>True when another hero can be bonded.</summary>
        public bool CanAdd => heroes.Count < UnlockedSlots;

        /// <summary>
        /// A new hero inherits some of the Keeper's experience: two levels below the strongest hero.
        /// </summary>
        public int StartingLevelForNewHero => Math.Max(1, HighestLevel - 2);

        /// <summary>Text describing the next slot unlock, or null when every slot is open.</summary>
        public string? NextUnlockHint
        {
            get
            {
                var next = SlotUnlockLevels.OrderBy(s => s.Key).FirstOrDefault(s => HighestLevel < s.Value);
                return next.Key == 0 ? null : $"Slot {next.Key} opens when any hero reaches level {next.Value}.";
            }
        }

        /// <summary>Adds a hero if a slot is free and makes them active.</summary>
        /// <returns>False when the roster is full.</returns>
        public bool TryAdd(Character hero)
        {
            if (!CanAdd) return false;
            heroes.Add(hero);
            Active = hero;
            return true;
        }

        /// <summary>Restores a roster from a save, ignoring slot rules.</summary>
        public void Load(IEnumerable<Character> saved, int activeIndex)
        {
            heroes.Clear();
            heroes.AddRange(saved);
            Active = heroes.Count == 0 ? null : heroes[Math.Clamp(activeIndex, 0, heroes.Count - 1)];
        }

        /// <summary>Switches the active hero.</summary>
        public void SetActive(Character hero)
        {
            if (heroes.Contains(hero)) Active = hero;
        }
    }
}
