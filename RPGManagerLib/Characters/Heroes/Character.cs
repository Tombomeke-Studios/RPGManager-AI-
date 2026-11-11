using RPGManagerLib.Combat;
using RPGManagerLib.Exceptions;
using RPGManagerLib.Items;
using RPGManagerLib.Items.Weapons;
using RPGManagerLib.Worlds;

namespace RPGManagerLib.Characters.Heroes
{
    /// <summary>
    /// The things a stat point can be spent on.
    /// </summary>
    public enum HeroStat { Might, Guard, Arcana, Perception, Vitality }

    /// <summary>
    /// A hero bonded to the Keeper: name, vitals, stats, level, equipment and supplies.
    /// </summary>
    /// <remarks>
    /// The base class owns everything that is truly shared (levelling, inventory rules, vitals).
    /// Each subclass decides how it grows (<see cref="OnLevelUp"/>) and what it can wield.
    /// </remarks>
    public abstract class Character : ICombatant
    {
        /// <summary>Highest level reachable in the Ashlands.</summary>
        public const int MaxLevel = 15;

        /// <summary>Stat points earned on every level-up, for the player to spend.</summary>
        public const int PointsPerLevel = 2;

        /// <summary>Health gained per point put into Vitality.</summary>
        public const int HealthPerVitality = 8;

        /// <summary>Total inventory slots (small items take 1, large items take 2).</summary>
        public const int InventoryCapacity = 10;

        /// <summary>XP needed to go from level [index] to the next. Tune each step independently.</summary>
        public static readonly int[] XpToNextLevel = { 0, 60, 130, 220, 330, 460, 620, 800, 1000, 1250, 1500, 1800, 2150, 2550, 3000 };

        /// <summary>Current health.</summary>
        public double Health { get; internal set; }

        /// <summary>Gets or sets the character's name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the date and time when the character was created.</summary>
        public DateTime CreationDate { get; set; }

        /// <summary>Legacy overall rating; kept in step with <see cref="Level"/>.</summary>
        public int PowerLevel { get; set; }

        /// <summary>Gets or sets the character's amount of gold.</summary>
        public int Gold { get; set; }

        /// <summary>Gets or sets the character's mana amount.</summary>
        public double Mana { get; set; }

        /// <summary>Current level (1..<see cref="MaxLevel"/>).</summary>
        public int Level { get; set; } = 1;

        /// <summary>XP gathered toward the next level.</summary>
        public int Experience { get; set; }

        /// <summary>Physical strength: adds to melee damage.</summary>
        public int Might { get; set; }

        /// <summary>Toughness: reduces incoming damage.</summary>
        public int Guard { get; set; }

        /// <summary>Magical force: adds to spell damage and healing.</summary>
        public int Arcana { get; set; }

        /// <summary>Sharpness: bow damage, critical hits, escaping, noticing things.</summary>
        public int Perception { get; set; }

        /// <summary>Stat points waiting to be spent.</summary>
        public int UnspentPoints { get; set; }

        /// <summary>Permanent health gained from Vitality, boons and relics.</summary>
        public double BonusMaxHealth { get; set; }

        /// <summary>Everything the hero carries that can be equipped.</summary>
        public List<IEquipable> Weapons { get; set; } = new();

        /// <summary>The weapon the hero chose to fight with; null means "use my best".</summary>
        public Weapon? EquippedWeapon { get; set; }

        /// <summary>Potions, bombs and materials by supply id.</summary>
        public Dictionary<string, int> Supplies { get; set; } = new();

        /// <summary>Skills taught by the world rather than unlocked by level.</summary>
        public List<string> LearnedSkills { get; set; } = new();

        /// <summary>Where this hero currently stands.</summary>
        public string LocationId { get; set; } = "cinders_rest";

        /// <summary>Active combat conditions (not saved).</summary>
        public List<StatusEffect> Effects { get; } = new();

        /// <summary>Health at level 1.</summary>
        protected virtual double BaseHealth => 100.0;

        /// <summary>Health gained per level.</summary>
        protected virtual double HealthPerLevel => 12.0;

        /// <summary>Gets the maximum health.</summary>
        public virtual double MaxHealth => BaseHealth + (Level - 1) * HealthPerLevel + BonusMaxHealth;

        /// <summary>Gets the maximum mana.</summary>
        public virtual double MaxMana => 100.0;

        /// <summary>Name of the class resource shown in combat (Fury, Mana, Arrows).</summary>
        public abstract string ResourceName { get; }

        public abstract string CharacterType { get; }
        public World? CurrentWorld { get; internal set; }

        /// <summary>True while the hero has health left.</summary>
        public bool IsAlive => Health > 0;

        /// <summary>XP needed for the next level, or 0 at the cap.</summary>
        public int XpForNext => Level >= MaxLevel ? 0 : XpToNextLevel[Level];

        /// <summary>Inventory slots in use — always computed from the items, never stored.</summary>
        public int UsedSlots => Weapons.Sum(SlotsFor);

        /// <summary>
        /// Initializes a new instance of the <see cref="Character"/> class with default values.
        /// </summary>
        public Character() : this("John") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Character"/> class with a name.
        /// </summary>
        /// <param name="name">The character's name.</param>
        public Character(string name)
        {
            Name = name;
            CreationDate = DateTime.Now;
            PowerLevel = 1;
            Gold = 50;
            Mana = 100;
            SetBaseStats();
            Health = MaxHealth;
        }

        /// <summary>Sets the level-1 stats for the class.</summary>
        protected abstract void SetBaseStats();

        /// <summary>Applies the class's growth for one level.</summary>
        protected abstract void OnLevelUp();

        /// <summary>Whether this class can fight with the given weapon.</summary>
        public abstract bool CanWield(Weapon weapon);

        /// <summary>Inventory slots an item occupies.</summary>
        public static int SlotsFor(IEquipable item) => item.InventorySpaceAmount == InventorySpaceAmount.LARGE ? 2 : 1;

        /// <summary>The weapon used in combat: the chosen one if valid, otherwise the strongest usable.</summary>
        public Weapon? ActiveWeapon
        {
            get
            {
                if (EquippedWeapon != null && Weapons.Contains(EquippedWeapon) && CanWield(EquippedWeapon))
                    return EquippedWeapon;
                return Weapons.OfType<Weapon>().Where(CanWield).OrderByDescending(w => w.GetEffectiveDamage()).FirstOrDefault();
            }
        }

        /// <summary>Adds an item if there is room.</summary>
        /// <returns>False when the inventory is full.</returns>
        public bool TryAddItem(IEquipable item)
        {
            if (UsedSlots + SlotsFor(item) > InventoryCapacity) return false;
            Weapons.Add(item);
            return true;
        }

        /// <summary>How many of a supply the hero carries.</summary>
        public int SupplyCount(string id) => Supplies.TryGetValue(id, out int n) ? n : 0;

        /// <summary>Adds supplies.</summary>
        public void AddSupply(string id, int amount = 1) => Supplies[id] = SupplyCount(id) + amount;

        /// <summary>Consumes supplies if enough are carried.</summary>
        public bool UseSupply(string id, int amount = 1)
        {
            if (SupplyCount(id) < amount) return false;
            Supplies[id] -= amount;
            if (Supplies[id] <= 0) Supplies.Remove(id);
            return true;
        }

        /// <summary>
        /// Adds experience and levels up as often as the thresholds allow.
        /// </summary>
        /// <returns>The number of levels gained.</returns>
        public int GainExperience(int amount)
        {
            if (Level >= MaxLevel || amount <= 0) return 0;
            Experience += amount;
            int gained = 0;
            while (Level < MaxLevel && Experience >= XpToNextLevel[Level])
            {
                Experience -= XpToNextLevel[Level];
                RaiseLevel();
                gained++;
            }
            if (Level >= MaxLevel) Experience = 0;
            if (gained > 0) RestoreFully();
            return gained;
        }

        /// <summary>Raises the hero straight to a level (used for the Keeper's catch-up bond).</summary>
        public void SetLevel(int level)
        {
            while (Level < Math.Min(level, MaxLevel)) RaiseLevel();
            RestoreFully();
        }

        private void RaiseLevel()
        {
            Level++;
            PowerLevel = Level;
            OnLevelUp();
            UnspentPoints += PointsPerLevel;
        }

        /// <summary>The order this class spends points in when the game chooses for it.</summary>
        protected abstract HeroStat[] DefaultBuild { get; }

        /// <summary>Spends one stat point on the chosen stat.</summary>
        /// <returns>False when there are no points left.</returns>
        public bool SpendPoint(HeroStat stat)
        {
            if (UnspentPoints <= 0) return false;
            UnspentPoints--;
            switch (stat)
            {
                case HeroStat.Might: Might++; break;
                case HeroStat.Guard: Guard++; break;
                case HeroStat.Arcana: Arcana++; break;
                case HeroStat.Perception: Perception++; break;
                case HeroStat.Vitality:
                    BonusMaxHealth += HealthPerVitality;
                    Health += HealthPerVitality;
                    break;
            }
            return true;
        }

        /// <summary>Spends every waiting point following the class's default build.</summary>
        public void AutoSpendPoints()
        {
            int i = Level;
            while (UnspentPoints > 0) SpendPoint(DefaultBuild[i++ % DefaultBuild.Length]);
        }

        /// <summary>Full health and mana.</summary>
        public void RestoreFully()
        {
            Health = MaxHealth;
            Mana = MaxMana;
            Effects.Clear();
        }

        /// <inheritdoc />
        public void TakeDamage(double amount) => Health = Math.Max(0, Health - Math.Max(0, amount));

        /// <inheritdoc />
        public double Restore(double amount)
        {
            double before = Health;
            Health = Math.Min(MaxHealth, Health + Math.Max(0, amount));
            return Health - before;
        }

        /// <summary>Restores mana up to the maximum and returns how much was restored.</summary>
        public double RestoreMana(double amount)
        {
            double before = Mana;
            Mana = Math.Min(MaxMana, Mana + Math.Max(0, amount));
            return Mana - before;
        }

        /// <summary>
        /// Increases the character's health by the specified number of points.
        /// </summary>
        /// <remarks>
        /// Strict variant kept for the original API: over-healing or negative values are rejected with a message.
        /// Combat uses <see cref="Restore"/>, which clamps instead.
        /// </remarks>
        /// <param name="points">The number of health points to restore. Must be non-negative.</param>
        public void Heal(double points)
        {
            try
            {
                if (points < 0)
                {
                    throw new NegativeHealException();
                }
                if (points + Health > MaxHealth)
                {
                    throw new OverhealException();
                }

                Health += points;
                Console.WriteLine($"Healed {points} health points");
            }
            catch (CharacterException ex)
            {
                Console.WriteLine($"Heal failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Reduces the character's health by the specified number of points.
        /// </summary>
        /// <remarks>
        /// Strict variant kept for the original API. Combat uses <see cref="TakeDamage"/>.
        /// </remarks>
        /// <param name="points">The amount of health to subtract. Must be non-negative.</param>
        public void Damage(double points)
        {
            try
            {
                if (points < 0)
                {
                    throw new NegativeDamageException();
                }

                if (Health - points < -100)
                {
                    throw new OverkillException();
                }

                Health -= points;

                if (Health <= 0)
                {
                    Console.WriteLine("Character has died.");
                }
                else
                {
                    Console.WriteLine($"Damaged {points} health points.");
                }
            }
            catch (CharacterException ex)
            {
                Console.WriteLine($"Damage failed: {ex.Message}");
            }
        }

        public void TravelTo(World world)
        {
            if (world.IsUnlocked)
            {
                CurrentWorld = world;
            }
            else
            {
                Console.WriteLine($"world {world} not unlocked!");
            }
        }

        /// <summary>
        /// Returns the carried equipment as one line, e.g. "Sword (COMMON), Dagger (COMMON)".
        /// </summary>
        protected virtual string GetEquipmentLine()
        {
            return Weapons.Any()
                ? string.Join(", ", Weapons.Select(w => $"{w.Name} ({w.Rarity})"))
                : "none";
        }

        /// <summary>
        /// Returns a formatted stat block for this character.
        /// </summary>
        public override string ToString()
        {
            string header = $"--- {Name} the {CharacterType} ---";
            string row1 = $"  {"Level",-9}: {Level,-10}{"Health",-7}: {Health:0} / {MaxHealth:0}";
            string row2 = $"  {"XP",-9}: {(Level >= MaxLevel ? "max" : $"{Experience}/{XpForNext}"),-10}{"Mana",-7}: {Mana:0} / {MaxMana:0}";
            string row3 = $"  {"Might",-9}: {Might,-10}{"Guard",-7}: {Guard}";
            string row4 = $"  {"Arcana",-9}: {Arcana,-10}{"Percep.",-7}: {Perception}";
            string row5 = $"  {"Gold",-9}: {Gold}";
            string row6 = $"  Equipment: {GetEquipmentLine()}";
            return $"\n{header}\n{row1}\n{row2}\n{row3}\n{row4}\n{row5}\n{row6}";
        }
    }
}
