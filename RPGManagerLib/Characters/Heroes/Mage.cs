using RPGManagerLib.Items.Weapons;
using RPGManagerLib.Spells;

namespace RPGManagerLib.Characters.Heroes
{
    /// <summary>
    /// Spellcaster. Fragile, but spells ignore armour tricks and hit elemental weaknesses. Mana carries over between fights.
    /// </summary>
    public class Mage : Character
    {
        /// <summary>
        /// Legacy spell objects (the original direct-cast API). Combat uses the skill book instead.
        /// </summary>
        public List<Spell> Spells { get; set; } = new();

        public override string CharacterType => "Mage";
        public override string ResourceName => "Mana";
        public override double MaxMana => 150.0 + (Level - 1) * 10;
        protected override double BaseHealth => 70.0;
        protected override double HealthPerLevel => 8.0;

        /// <summary>Initializes a mage with the default name.</summary>
        public Mage() : base()
        {
            Mana = MaxMana;
        }

        /// <summary>Initializes a mage with a name.</summary>
        /// <param name="name">The name of the mage.</param>
        public Mage(string name) : base(name)
        {
            Mana = MaxMana;
        }

        protected override void SetBaseStats()
        {
            Might = 2;
            Guard = 2;
            Arcana = 8;
            Perception = 4;
        }

        protected override void OnLevelUp()
        {
            Arcana += 2;
            if (Level % 2 == 0) Guard += 1;
            else Perception += 1;
        }

        public override bool CanWield(Weapon weapon) => weapon.Type is WeaponType.STAFF or WeaponType.DAGGER;

        /// <summary>Casts a legacy spell directly at a target (original API).</summary>
        public void CastSpell(int i, Character target)
        {
            if (i < 0 || i >= Spells.Count)
            {
                Console.WriteLine("Invalid spell.");
                return;
            }

            Spells[i].Cast(this, target);
        }
    }
}
