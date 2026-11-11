using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Items.Weapons;

namespace RPGManagerLib.Combat;

/// <summary>Who a skill is aimed at.</summary>
public enum SkillTarget { Enemy, AllEnemies, Self }

/// <summary>Behaviour that goes beyond "deal damage, maybe apply a status".</summary>
public enum SkillSpecial { None, Execute, Heal, Ward, Evade, WarCry, Unbinding, Pierce }

/// <summary>
/// A class ability. Skills only describe intent — cost, numbers, element, status —
/// and the battle loop resolves what that means against resistances and active effects.
/// </summary>
public class Skill
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";

    /// <summary>Class name this skill belongs to.</summary>
    public string HeroClass { get; init; } = "";

    /// <summary>Level at which it unlocks; 0 means it must be taught.</summary>
    public int UnlockLevel { get; init; }

    /// <summary>Cost in the class resource (Fury, Mana or Arrows).</summary>
    public int Cost { get; init; }

    /// <summary>Rounds before it can be used again.</summary>
    public int Cooldown { get; init; }
    public SkillTarget Target { get; init; } = SkillTarget.Enemy;

    /// <summary>Spells use Arcana and the staff; everything else uses the weapon.</summary>
    public bool IsSpell { get; init; }
    public Element Element { get; init; } = Element.NONE;

    /// <summary>Multiplier on weapon damage (weapon skills).</summary>
    public double WeaponMultiplier { get; init; } = 1.0;

    /// <summary>Flat base damage (spells).</summary>
    public double BaseDamage { get; init; }
    public double CritBonus { get; init; }
    public StatusType? Status { get; init; }
    public double StatusChance { get; init; }
    public int StatusTurns { get; init; }
    public SkillSpecial Special { get; init; } = SkillSpecial.None;
}

/// <summary>
/// All class skills and the rules for which a hero currently knows.
/// </summary>
public static class SkillBook
{
    public const string Unbinding = "unbinding";
    public const string Emberlance = "emberlance";

    public static readonly IReadOnlyList<Skill> All = new List<Skill>
    {
        // --- Warrior: spends Fury, which builds by attacking, defending and being hit ---
        new() { Id = "power_strike", Name = "Power Strike", HeroClass = "Warrior", UnlockLevel = 1, Cost = 25, WeaponMultiplier = 1.7,
                Description = "A heavy overhead blow. 170% weapon damage." },
        new() { Id = "cleave", Name = "Cleave", HeroClass = "Warrior", UnlockLevel = 3, Cost = 35, WeaponMultiplier = 0.9, Target = SkillTarget.AllEnemies,
                Description = "One wide swing through every enemy. 90% weapon damage each." },
        new() { Id = "second_wind", Name = "Second Wind", HeroClass = "Warrior", UnlockLevel = 4, Cost = 30, Target = SkillTarget.Self, Special = SkillSpecial.Heal, Cooldown = 3,
                Description = "Grit your teeth and recover 25% of your health." },
        new() { Id = "war_cry", Name = "War Cry", HeroClass = "Warrior", UnlockLevel = 5, Cost = 30, Target = SkillTarget.AllEnemies, Special = SkillSpecial.WarCry,
                Description = "Shake their nerve: all enemies deal 35% less damage for 3 rounds." },
        new() { Id = "execute", Name = "Execute", HeroClass = "Warrior", UnlockLevel = 7, Cost = 45, WeaponMultiplier = 2.0, Special = SkillSpecial.Execute,
                Description = "200% weapon damage — 320% against a target below 35% health." },

        // --- Mage: spends Mana, which carries over between fights ---
        new() { Id = "fireball", Name = "Fireball", HeroClass = "Mage", UnlockLevel = 1, Cost = 20, IsSpell = true, Element = Element.FIRE, BaseDamage = 30,
                Status = StatusType.Burn, StatusChance = 0.30, StatusTurns = 3,
                Description = "Fire damage with a 30% chance to Burn." },
        new() { Id = "ice_spike", Name = "Ice Spike", HeroClass = "Mage", UnlockLevel = 1, Cost = 15, IsSpell = true, Element = Element.ICE, BaseDamage = 25,
                Status = StatusType.Freeze, StatusChance = 0.25, StatusTurns = 1,
                Description = "Ice damage with a 25% chance to Freeze the target for a turn." },
        new() { Id = "mend", Name = "Mend", HeroClass = "Mage", UnlockLevel = 2, Cost = 25, IsSpell = true, Target = SkillTarget.Self, Special = SkillSpecial.Heal,
                Description = "Knit flesh with careful magic. Restores health based on Arcana." },
        new() { Id = "arcane_ward", Name = "Arcane Ward", HeroClass = "Mage", UnlockLevel = 3, Cost = 20, IsSpell = true, Target = SkillTarget.Self, Special = SkillSpecial.Ward, Cooldown = 3,
                Description = "A shell of force that absorbs damage before you do." },
        new() { Id = "chain_lightning", Name = "Chain Lightning", HeroClass = "Mage", UnlockLevel = 5, Cost = 35, IsSpell = true, Element = Element.LIGHTNING, BaseDamage = 20,
                Target = SkillTarget.AllEnemies, Status = StatusType.Stun, StatusChance = 0.15, StatusTurns = 1,
                Description = "Lightning leaps through every enemy; 15% chance to Stun each." },
        new() { Id = Unbinding, Name = "Unbinding", HeroClass = "Mage", UnlockLevel = 0, Cost = 30, IsSpell = true, BaseDamage = 32, Special = SkillSpecial.Unbinding,
                Description = "Lira's counter-spell. Pure force; double damage to spirits and the Court's warded soldiers." },
        new() { Id = Emberlance, Name = "Emberlance", HeroClass = "Mage", UnlockLevel = 0, Cost = 40, IsSpell = true, Element = Element.FIRE, BaseDamage = 55,
                Status = StatusType.Burn, StatusChance = 0.60, StatusTurns = 3,
                Description = "A lost Emberveil war-spell. Heavy fire damage, 60% chance to Burn." },

        // --- Archer: spends Arrows, a per-fight budget set by the quiver ---
        new() { Id = "aimed_shot", Name = "Aimed Shot", HeroClass = "Archer", UnlockLevel = 1, Cost = 3, WeaponMultiplier = 1.6, CritBonus = 0.30,
                Description = "Take the time to place it. 160% damage, +30% critical chance." },
        new() { Id = "poison_arrow", Name = "Poison Arrow", HeroClass = "Archer", UnlockLevel = 2, Cost = 3, WeaponMultiplier = 1.0,
                Status = StatusType.Poison, StatusChance = 1.0, StatusTurns = 4,
                Description = "Normal damage, then Poison for 4 rounds." },
        new() { Id = "evade", Name = "Evade", HeroClass = "Archer", UnlockLevel = 3, Cost = 0, Target = SkillTarget.Self, Special = SkillSpecial.Evade, Cooldown = 3,
                Description = "Dodge every attack this round. Your next shot is a guaranteed critical." },
        new() { Id = "volley", Name = "Volley", HeroClass = "Archer", UnlockLevel = 4, Cost = 5, WeaponMultiplier = 0.85, Target = SkillTarget.AllEnemies,
                Description = "Arrows for everyone. 85% damage to each enemy." },
        new() { Id = "pinning_shot", Name = "Pinning Shot", HeroClass = "Archer", UnlockLevel = 6, Cost = 4, WeaponMultiplier = 1.2,
                Status = StatusType.Stun, StatusChance = 1.0, StatusTurns = 1,
                Description = "120% damage and the target loses its next turn." },
        new() { Id = "heartseeker", Name = "Heartseeker", HeroClass = "Archer", UnlockLevel = 8, Cost = 6, WeaponMultiplier = 2.3, Special = SkillSpecial.Pierce,
                Description = "230% damage that ignores armour entirely." },
    };

    /// <summary>Looks up a skill by id.</summary>
    public static Skill Get(string id) => All.First(s => s.Id == id);

    /// <summary>Skills the hero can currently use: unlocked by level or taught.</summary>
    public static List<Skill> For(Character hero) =>
        All.Where(s => s.HeroClass == hero.CharacterType &&
                       ((s.UnlockLevel > 0 && hero.Level >= s.UnlockLevel) || hero.LearnedSkills.Contains(s.Id)))
           .ToList();

    /// <summary>Skills that unlock exactly at the given level for the hero's class.</summary>
    public static List<Skill> UnlockedAt(Character hero, int level) =>
        All.Where(s => s.HeroClass == hero.CharacterType && s.UnlockLevel == level).ToList();
}
