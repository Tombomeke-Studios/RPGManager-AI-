using RPGManagerLib.Items;
using RPGManagerLib.Items.Weapons;

namespace RPGManagerLib.Combat;

/// <summary>
/// Every enemy of the Ashlands. Each call builds a fresh instance at full health.
/// </summary>
public static class Bestiary
{
    public const string AshScavenger = "ash_scavenger";
    public const string ScavengerAlpha = "scavenger_alpha";
    public const string Bandit = "bandit";
    public const string BanditBowman = "bandit_bowman";
    public const string BanditEnforcer = "bandit_enforcer";
    public const string EmberSprite = "ember_sprite";
    public const string AshWraith = "ash_wraith";
    public const string CourtSentinel = "court_sentinel";
    public const string Thresh = "commander_thresh";
    public const string CinderHand = "cinder_hand";

    /// <summary>Creates an enemy by id.</summary>
    /// <exception cref="ArgumentException">Thrown for an unknown id.</exception>
    public static Enemy Create(string id) => (id switch
    {
        AshScavenger => new Enemy
        {
            Id = id, Name = "Ash Scavenger", Level = 1, MaxHealth = 28, Attack = 9, Guard = 0,
            Xp = 15, GoldMin = 2, GoldMax = 5, KillTag = "scavenger",
            Description = "A feral ash-grey dog, all ribs and teeth.",
            Loot = { new(SupplyCatalog.Scraps, 0.6) }
        },
        ScavengerAlpha => new Enemy
        {
            Id = id, Name = "Scavenger Alpha", Level = 2, MaxHealth = 56, Attack = 13, Guard = 1,
            Xp = 30, GoldMin = 5, GoldMax = 10, KillTag = "scavenger",
            Description = "The pack's scarred leader. It learned long ago that travellers carry food.",
            Loot = { new(SupplyCatalog.Scraps, 1.0), new(SupplyCatalog.Scraps, 0.5) }
        },
        Bandit => new Enemy
        {
            Id = id, Name = "Scorched Path Bandit", Level = 3, MaxHealth = 54, Attack = 15, Guard = 2,
            Xp = 30, GoldMin = 10, GoldMax = 25, KillTag = "bandit",
            Description = "Mismatched armour, a rusty blade, and the eyes of someone who has run out of options.",
            Loot = { new("w:rusty_sword", 0.10), new("w:notched_dagger", 0.10), new(SupplyCatalog.HealingPotion, 0.12), new(SupplyCatalog.Scraps, 0.3) }
        },
        BanditBowman => new Enemy
        {
            Id = id, Name = "Bandit Bowman", Level = 3, MaxHealth = 40, Attack = 17, Guard = 1,
            Xp = 30, GoldMin = 10, GoldMax = 25, KillTag = "bandit",
            Description = "Keeps to the rocks and shoots at whatever is busy with someone else.",
            Loot = { new("w:bandit_shortbow", 0.10), new(SupplyCatalog.SmokeBomb, 0.12), new(SupplyCatalog.Scraps, 0.3) }
        },
        BanditEnforcer => new Enemy
        {
            Id = id, Name = "Bandit Enforcer", Level = 5, MaxHealth = 115, Attack = 20, Guard = 5,
            Behavior = EnemyBehavior.Defensive, Trait = EnemyTrait.HealOnce,
            Xp = 60, GoldMin = 30, GoldMax = 60, KillTag = "bandit",
            Description = "One of Thresh's lieutenants. Real armour, a real blade, and a potion on the belt.",
            Loot = { new("w:enforcer_blade", 0.15), new("w:enforcer_axe", 0.12), new(SupplyCatalog.HealingPotion, 0.35) }
        },
        EmberSprite => new Enemy
        {
            Id = id, Name = "Ember Sprite", Level = 4, MaxHealth = 50, Attack = 18, Guard = 1,
            AttackElement = Element.FIRE, Behavior = EnemyBehavior.Random, BurnChance = 0.30,
            ElementMultipliers = { [Element.ICE] = 1.5, [Element.FIRE] = 0.25 },
            Xp = 40, GoldMin = 8, GoldMax = 15, KillTag = "sprite", Tags = new[] { "elemental" },
            Description = "A darting knot of living flame. It does not think. It flickers.",
            Loot = { new(SupplyCatalog.FireEssence, 0.6) }
        },
        AshWraith => new Enemy
        {
            Id = id, Name = "Ash Wraith", Level = 6, MaxHealth = 115, Attack = 23, Guard = 3,
            AttackElement = Element.FIRE, Behavior = EnemyBehavior.Defensive, Trait = EnemyTrait.PhaseOnce,
            BurnChance = 0.20, PhysicalMultiplier = 0.5, SpellMultiplier = 1.4,
            ElementMultipliers = { [Element.ICE] = 1.4, [Element.FIRE] = 0.5 },
            Xp = 80, GoldMin = 15, GoldMax = 30, KillTag = "wraith", Tags = new[] { "spirit" },
            Description = "A dead scholar of Emberveil, still casting the last spell they ever cast.",
            Loot = { new(SupplyCatalog.AcademyPage, 0.6), new(SupplyCatalog.ManaDraught, 0.25), new(SupplyCatalog.FireEssence, 0.3) }
        },
        CourtSentinel => new Enemy
        {
            Id = id, Name = "Hollow Court Sentinel", Level = 8, MaxHealth = 165, Attack = 27, Guard = 8,
            Behavior = EnemyBehavior.Defensive, Trait = EnemyTrait.CallReinforcement,
            Xp = 120, GoldMin = 50, GoldMax = 100, KillTag = "sentinel", Tags = new[] { "court" },
            Description = "Black and gold plate, a featureless mask. It does not speak. It does not negotiate.",
            Loot = { new("w:sentinel_blade", 0.15), new("w:sentinel_pike", 0.12), new(SupplyCatalog.GreaterHealingPotion, 0.3) }
        },
        Thresh => new Enemy
        {
            Id = id, Name = "Commander Thresh", Level = 7, MaxHealth = 360, Attack = 25, Guard = 6,
            Behavior = EnemyBehavior.Defensive, Trait = EnemyTrait.Thresh, IsBoss = true,
            Xp = 300, GoldMin = 150, GoldMax = 200, KillTag = "thresh",
            Description = "Twelve years a border soldier, and it shows in how little he wastes.",
            Loot = { new("w:garrison_longsword", 1.0), new(SupplyCatalog.GreaterHealingPotion, 1.0) }
        },
        CinderHand => new Enemy
        {
            Id = id, Name = "The Cinder Hand", Level = 10, MaxHealth = 540, Attack = 32, Guard = 8,
            AttackElement = Element.FIRE, Behavior = EnemyBehavior.Defensive, Trait = EnemyTrait.CinderHand,
            BurnChance = 0.30, IsBoss = true,
            ElementMultipliers = { [Element.FIRE] = 0.6, [Element.ICE] = 1.25 },
            Xp = 500, GoldMin = 280, GoldMax = 340, KillTag = "cinder_hand",
            Description = "The Court's handler in the Ashlands. An unmasked face, and a gauntlet that glows like a banked forge."
        },
        _ => throw new ArgumentException($"Unknown enemy id: {id}")
    }).Ready();

    /// <summary>Creates a whole encounter group from ids, numbering duplicates so they can be told apart.</summary>
    public static List<Enemy> Group(params string[] ids)
    {
        var enemies = ids.Select(Create).ToList();
        foreach (var same in enemies.GroupBy(e => e.Name).Where(g => g.Count() > 1))
        {
            int n = 0;
            foreach (var enemy in same) enemy.Name = $"{enemy.Name} {(char)('A' + n++)}";
        }
        return enemies;
    }
}
