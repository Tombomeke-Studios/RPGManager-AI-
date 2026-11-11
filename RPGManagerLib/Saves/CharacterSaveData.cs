using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Items;
using RPGManagerLib.Items.Weapons;
using RPGManagerLib.Weapons.Quivers;

namespace RPGManagerLib.Saves
{
    /// <summary>
    /// Serializable snapshot of a character for saving and loading.
    /// </summary>
    public class CharacterSaveData
    {
        /// <summary>
        /// Discriminator describing the concrete character type (e.g., Warrior, Mage).
        /// </summary>
        public string CharacterType { get; set; } = "";

        /// <summary>
        /// Character name.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Current health value.
        /// </summary>
        public double Health { get; set; }

        /// <summary>
        /// Original creation date of the character.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Current power level.
        /// </summary>
        public int PowerLevel { get; set; }

        /// <summary>
        /// Serialized equipable items (Weapons, Quivers, etc).
        /// Using polymorphism to keep JSON clean.
        /// </summary>
        public List<EquipableSaveData> Equipables { get; set; } = new();

        /// <summary>
        /// Mana boost value for mages.
        /// </summary>
        public double Mana { get; set; } // alleen voor Mage

        public int Gold { get; set; }

        /// <summary>Current level (0 in saves made before levelling existed).</summary>
        public int Level { get; set; }
        public int Experience { get; set; }
        public int Might { get; set; }
        public int Guard { get; set; }
        public int Arcana { get; set; }
        public int Perception { get; set; }
        public double BonusMaxHealth { get; set; }

        /// <summary>Index of the chosen weapon inside <see cref="Equipables"/>, or -1.</summary>
        public int EquippedIndex { get; set; } = -1;
        public Dictionary<string, int> Supplies { get; set; } = new();
        public List<string> LearnedSkills { get; set; } = new();
        public string LocationId { get; set; } = "cinders_rest";

        public CharacterSaveData() { }

        /// <summary>
        /// Captures a character's full state for saving.
        /// </summary>
        /// <param name="c">The character to snapshot.</param>
        /// <exception cref="Exception">Thrown if an unknown item type is encountered.</exception>
        public CharacterSaveData(Character c)
        {
            Name = c.Name;
            Health = c.Health;
            CreationDate = c.CreationDate;
            PowerLevel = c.PowerLevel;
            CharacterType = c.CharacterType;
            Gold = c.Gold;
            Mana = c.Mana;
            Level = c.Level;
            Experience = c.Experience;
            Might = c.Might;
            Guard = c.Guard;
            Arcana = c.Arcana;
            Perception = c.Perception;
            BonusMaxHealth = c.BonusMaxHealth;
            Supplies = new Dictionary<string, int>(c.Supplies);
            LearnedSkills = new List<string>(c.LearnedSkills);
            LocationId = c.LocationId;
            Equipables = c.Weapons.Select(FromEquipable).ToList();
            EquippedIndex = c.EquippedWeapon == null ? -1 : c.Weapons.IndexOf(c.EquippedWeapon);
        }

        /// <summary>Converts one item to its matching save data type.</summary>
        /// <exception cref="Exception">Thrown for an item type with no save data.</exception>
        public static EquipableSaveData FromEquipable(IEquipable item) => item switch
        {
            Weapon weapon => new WeaponSaveData(weapon),
            Quiver quiver => new QuiverSaveData(quiver),
            _ => throw new Exception($"Unknown item type: {item.GetType().Name}")
        };

        /// <summary>
        /// Rebuilds a character from this save data.
        /// </summary>
        /// <returns>A Warrior, Archer or Mage with the saved state.</returns>
        /// <exception cref="Exception">Thrown if the character type is not recognized.</exception>
        public Character ToCharacter()
        {
            Character c = CharacterType switch
            {
                "Warrior" => new Warrior(Name),
                "Archer" => new Archer(Name),
                "Mage" => new Mage(Name),
                _ => throw new Exception($"Unknown character type: {CharacterType}")
            };

            c.CreationDate = CreationDate;
            c.Gold = Gold;

            // Saves from before levelling have Level 0: keep the class's level-1 stats.
            if (Level > 0)
            {
                c.Level = Level;
                c.PowerLevel = Level;
                c.Experience = Experience;
                c.Might = Might;
                c.Guard = Guard;
                c.Arcana = Arcana;
                c.Perception = Perception;
                c.BonusMaxHealth = BonusMaxHealth;
            }
            else
            {
                c.PowerLevel = PowerLevel;
            }

            c.Health = Math.Min(Health, c.MaxHealth);
            if (c is Mage || Level > 0) c.Mana = Mana;
            c.Supplies = new Dictionary<string, int>(Supplies);
            c.LearnedSkills = new List<string>(LearnedSkills);
            c.LocationId = string.IsNullOrEmpty(LocationId) ? "cinders_rest" : LocationId;

            c.Weapons.Clear();
            c.Weapons.AddRange(Equipables.Select(e => e.ToEquipable()));
            if (EquippedIndex >= 0 && EquippedIndex < c.Weapons.Count)
                c.EquippedWeapon = c.Weapons[EquippedIndex] as Weapon;

            return c;
        }
    }
}
