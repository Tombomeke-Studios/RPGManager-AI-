using RPGManagerLib.Items.Weapons;
using RPGManagerLib.Weapons.Quivers;

namespace RPGManagerLib.Characters.Heroes
{
    /// <summary>
    /// Ranged skirmisher. Critical hits, poison and evasion; every fight is an arrow budget set by the quiver.
    /// </summary>
    public class Archer : Character
    {
        /// <summary>Arrows available per fight without any quiver.</summary>
        public const int LooseArrows = 6;

        public override string CharacterType => "Archer";
        public override string ResourceName => "Arrows";
        protected override double BaseHealth => 85.0;
        protected override double HealthPerLevel => 10.0;

        /// <summary>Initializes an archer with the default name.</summary>
        public Archer() : base() { }

        /// <summary>Initializes an archer with a name.</summary>
        public Archer(string name) : base(name) { }

        /// <summary>Arrows carried into each fight: the largest quiver's capacity.</summary>
        public int ArrowCapacity
        {
            get
            {
                var quivers = Weapons.OfType<Quiver>().ToList();
                return quivers.Count == 0 ? LooseArrows : quivers.Max(q => q.Capacity);
            }
        }

        protected override void SetBaseStats()
        {
            Might = 4;
            Guard = 3;
            Arcana = 0;
            Perception = 8;
        }

        protected override void OnLevelUp()
        {
            Perception += 1;
            Might += 1;
            if (Level % 2 == 0) Guard += 1;
        }

        public override bool CanWield(Weapon weapon) => weapon.Type is WeaponType.BOW or WeaponType.DAGGER;
    }
}
