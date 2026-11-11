using RPGManagerLib.Items.Weapons;

namespace RPGManagerLib.Characters.Heroes
{
    /// <summary>
    /// Frontline fighter. Tough, hits hard with melee weapons, and builds Fury as the fight goes on.
    /// </summary>
    public class Warrior : Character
    {
        public override string CharacterType => "Warrior";
        public override string ResourceName => "Fury";
        protected override double BaseHealth => 100.0;
        protected override double HealthPerLevel => 14.0;

        /// <summary>Initializes a warrior with the default name.</summary>
        public Warrior() : base() { }

        /// <summary>Initializes a warrior with a name.</summary>
        /// <param name="name">The warrior's name.</param>
        public Warrior(string name) : base(name) { }

        protected override void SetBaseStats()
        {
            Might = 7;
            Guard = 5;
            Arcana = 0;
            Perception = 3;
        }

        protected override void OnLevelUp()
        {
            Might += 1;
            if (Level % 2 == 0) Guard += 1;
        }

        protected override HeroStat[] DefaultBuild => new[] { HeroStat.Might, HeroStat.Guard, HeroStat.Might, HeroStat.Vitality };

        public override bool CanWield(Weapon weapon) =>
            weapon.Type is WeaponType.SWORD or WeaponType.AXE or WeaponType.SPEAR or WeaponType.DAGGER;
    }
}
