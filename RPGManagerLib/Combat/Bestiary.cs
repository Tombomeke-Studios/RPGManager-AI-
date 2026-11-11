using RPGManagerLib.Core;
using RPGManagerLib.Items;
using RPGManagerLib.Items.Weapons;

namespace RPGManagerLib.Combat;

/// <summary>
/// Every enemy of the Ashlands. Each call builds a fresh individual: no two are quite alike,
/// and a wild one may turn out to be a Veteran or an Elite.
/// </summary>
public static class Bestiary
{
    public const string AshScavenger = "ash_scavenger";
    public const string ScavengerAlpha = "scavenger_alpha";
    public const string CinderGoblin = "cinder_goblin";
    public const string GoblinSlinger = "goblin_slinger";
    public const string GoblinShaman = "goblin_shaman";
    public const string AshbackBoar = "ashback_boar";
    public const string Bandit = "bandit";
    public const string BanditBowman = "bandit_bowman";
    public const string BanditCutthroat = "bandit_cutthroat";
    public const string BanditEnforcer = "bandit_enforcer";
    public const string EmberSprite = "ember_sprite";
    public const string SlagBeetle = "slag_beetle";
    public const string EmberHound = "ember_hound";
    public const string AshWraith = "ash_wraith";
    public const string RuinGolem = "ruin_golem";
    public const string CourtSentinel = "court_sentinel";
    public const string CourtInquisitor = "court_inquisitor";
    public const string AshDrake = "ash_drake";
    public const string Thresh = "commander_thresh";
    public const string CinderHand = "cinder_hand";

    /// <summary>Chance a wild enemy is a Veteran.</summary>
    public const double VeteranChance = 0.14;

    /// <summary>Chance a wild enemy is an Elite.</summary>
    public const double EliteChance = 0.05;

    /// <summary>Health multiplier for an Elite.</summary>
    public const double EliteHealth = 2.4;

    /// <summary>Global difficulty dials: every enemy's health and attack are scaled by these.</summary>
    public const double HealthScale = 1.15;
    public const double AttackScale = 1.2;

    /// <summary>Builds the template for an id with the global difficulty applied and no random variation.</summary>
    private static Enemy Template(string id)
    {
        var enemy = Base(id);
        enemy.MaxHealth = Math.Round(enemy.MaxHealth * HealthScale);
        enemy.Attack = Math.Round(enemy.Attack * AttackScale, 1);
        return enemy;
    }

    /// <summary>The raw stat block for an id.</summary>
    /// <exception cref="ArgumentException">Thrown for an unknown id.</exception>
    private static Enemy Base(string id) => id switch
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
        CinderGoblin => new Enemy
        {
            Id = id, Name = "Cinder Goblin", Level = 2, MaxHealth = 34, Attack = 11, Guard = 1,
            Xp = 22, GoldMin = 5, GoldMax = 12, KillTag = "goblin", Tags = new[] { "goblin" },
            Description = "Soot-black, knee-high, and wearing a kiln-tile as a breastplate. Their ancestors stoked the academy's furnaces. Nobody told them the job ended.",
            Loot = { new(SupplyCatalog.Scraps, 0.5), new(SupplyCatalog.Firebomb, 0.06) }
        },
        GoblinSlinger => new Enemy
        {
            Id = id, Name = "Goblin Slinger", Level = 2, MaxHealth = 26, Attack = 13, Guard = 0,
            Xp = 22, GoldMin = 5, GoldMax = 12, KillTag = "goblin", Tags = new[] { "goblin" },
            Description = "Whirls a sling loaded with still-warm clinker and cackles every time it hits something.",
            Loot = { new(SupplyCatalog.Scraps, 0.4), new(SupplyCatalog.SmokeBomb, 0.08) }
        },
        GoblinShaman => new Enemy
        {
            Id = id, Name = "Goblin Shaman", Level = 4, MaxHealth = 46, Attack = 15, Guard = 1,
            AttackElement = Element.FIRE, BurnChance = 0.25, Behavior = EnemyBehavior.Defensive, Trait = EnemyTrait.HealOnce,
            ElementMultipliers = { [Element.ICE] = 1.3 },
            Xp = 45, GoldMin = 12, GoldMax = 25, KillTag = "goblin", Tags = new[] { "goblin" },
            Description = "Wears a professor's mortarboard, upside down, as a bowl for embers. It has worked out that the hot stuff hurts people.",
            Loot = { new(SupplyCatalog.FireEssence, 0.5), new(SupplyCatalog.ManaDraught, 0.2) }
        },
        AshbackBoar => new Enemy
        {
            Id = id, Name = "Ashback Boar", Level = 3, MaxHealth = 72, Attack = 14, Guard = 3,
            Xp = 40, GoldMin = 0, GoldMax = 4, KillTag = "boar",
            Description = "Three hundred pounds of bristle and bad temper, with a hide like a cooled lava crust.",
            Loot = { new(SupplyCatalog.Scraps, 1.0), new(SupplyCatalog.Scraps, 0.6) }
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
        BanditCutthroat => new Enemy
        {
            Id = id, Name = "Bandit Cutthroat", Level = 6, MaxHealth = 78, Attack = 23, Guard = 3,
            Xp = 70, GoldMin = 25, GoldMax = 50, KillTag = "bandit",
            Description = "Not one of Thresh's soldiers. This one was a killer before the garrison ever fell, and joined for the opportunities.",
            Loot = { new("w:notched_dagger", 0.2), new(SupplyCatalog.SmokeBomb, 0.25), new(SupplyCatalog.HealingPotion, 0.2) }
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
        SlagBeetle => new Enemy
        {
            Id = id, Name = "Slag Beetle", Level = 5, MaxHealth = 64, Attack = 14, Guard = 10,
            ElementMultipliers = { [Element.LIGHTNING] = 1.5, [Element.ICE] = 1.3 },
            Xp = 55, GoldMin = 0, GoldMax = 6, KillTag = "beetle",
            Description = "A beetle the size of a wheelbarrow that eats cooling slag and wears the result. Hitting it is like hitting an anvil. Find a way around the shell.",
            Loot = { new(SupplyCatalog.Scraps, 1.0), new(SupplyCatalog.Scraps, 1.0), new(SupplyCatalog.FireEssence, 0.3) }
        },
        EmberHound => new Enemy
        {
            Id = id, Name = "Ember Hound", Level = 6, MaxHealth = 84, Attack = 22, Guard = 2,
            AttackElement = Element.FIRE, BurnChance = 0.25,
            ElementMultipliers = { [Element.ICE] = 1.5, [Element.FIRE] = 0.4 },
            Xp = 75, GoldMin = 5, GoldMax = 15, KillTag = "hound", Tags = new[] { "elemental" },
            Description = "What an Ash Scavenger becomes after a few generations of denning in the vents. Its breath shimmers.",
            Loot = { new(SupplyCatalog.FireEssence, 0.7), new(SupplyCatalog.Scraps, 0.5) }
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
        RuinGolem => new Enemy
        {
            Id = id, Name = "Ruin Golem", Level = 8, MaxHealth = 200, Attack = 26, Guard = 9,
            Behavior = EnemyBehavior.Random,
            ElementMultipliers = { [Element.LIGHTNING] = 1.5 },
            Xp = 130, GoldMin = 20, GoldMax = 50, KillTag = "golem", Tags = new[] { "construct" },
            Description = "An academy door-warden of fused brick, still asking for a pass nobody has carried in two hundred years. Slow, and sometimes it simply forgets you are there. Do not rely on that.",
            Loot = { new(SupplyCatalog.Scraps, 1.0), new(SupplyCatalog.Scraps, 1.0), new(SupplyCatalog.Scraps, 1.0), new(SupplyCatalog.GreaterHealingPotion, 0.25) }
        },
        CourtSentinel => new Enemy
        {
            Id = id, Name = "Hollow Court Sentinel", Level = 8, MaxHealth = 165, Attack = 27, Guard = 8,
            Behavior = EnemyBehavior.Defensive, Trait = EnemyTrait.CallReinforcement,
            Xp = 120, GoldMin = 50, GoldMax = 100, KillTag = "sentinel", Tags = new[] { "court" },
            Description = "Black and gold plate, a featureless mask. It does not speak. It does not negotiate.",
            Loot = { new("w:sentinel_blade", 0.15), new("w:sentinel_pike", 0.12), new(SupplyCatalog.GreaterHealingPotion, 0.3) }
        },
        CourtInquisitor => new Enemy
        {
            Id = id, Name = "Court Inquisitor", Level = 10, MaxHealth = 150, Attack = 31, Guard = 4,
            AttackElement = Element.FIRE, BurnChance = 0.2, Behavior = EnemyBehavior.Defensive, Trait = EnemyTrait.HealOnce,
            SpellMultiplier = 0.8,
            Xp = 170, GoldMin = 80, GoldMax = 140, KillTag = "inquisitor", Tags = new[] { "court" },
            Description = "No mask — a silver half-veil and ink-stained fingers. The Court's field scholars: they catalogue what the Sentinels kill.",
            Loot = { new(SupplyCatalog.ManaDraught, 0.6), new(SupplyCatalog.GreaterHealingPotion, 0.4), new(SupplyCatalog.AcademyPage, 0.3) }
        },
        AshDrake => new Enemy
        {
            Id = id, Name = "Ash Drake", Level = 12, MaxHealth = 820, Attack = 36, Guard = 9,
            AttackElement = Element.FIRE, BurnChance = 0.25, Behavior = EnemyBehavior.Defensive, Trait = EnemyTrait.Breath, IsBoss = true,
            ElementMultipliers = { [Element.FIRE] = 0.3, [Element.ICE] = 1.3 },
            Xp = 650, GoldMin = 250, GoldMax = 350, KillTag = "drake",
            Description = "Not a dragon. A dragon's distant, stupid, hungry cousin, grown fat on the warm updraughts of the plateau. It is still the size of a barn.",
            Loot = { new(SupplyCatalog.GreaterHealingPotion, 1.0), new(SupplyCatalog.FireEssence, 1.0), new(SupplyCatalog.FireEssence, 1.0) }
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
            Id = id, Name = "The Cinder Hand", Level = 10, MaxHealth = 640, Attack = 32, Guard = 8,
            AttackElement = Element.FIRE, Behavior = EnemyBehavior.Defensive, Trait = EnemyTrait.CinderHand,
            BurnChance = 0.30, IsBoss = true,
            ElementMultipliers = { [Element.FIRE] = 0.6, [Element.ICE] = 1.25 },
            Xp = 500, GoldMin = 280, GoldMax = 340, KillTag = "cinder_hand",
            Description = "The Court's handler in the Ashlands. An unmasked face, and a gauntlet that glows like a banked forge."
        },
        _ => throw new ArgumentException($"Unknown enemy id: {id}")
    };

    /// <summary>Creates an enemy with exactly the listed stats (for tests and comparisons).</summary>
    public static Enemy Exact(string id) => Template(id).Ready();

    /// <summary>
    /// Creates one individual. Health and attack vary a little so no two are identical;
    /// with <paramref name="wild"/> it may also be a Veteran or an Elite.
    /// </summary>
    /// <param name="id">Enemy id.</param>
    /// <param name="wild">Allow Veteran and Elite ranks (random encounters). Story fights pass false.</param>
    /// <param name="eliteBonus">Extra elite chance, for dangerous ground.</param>
    public static Enemy Create(string id, bool wild = false, double eliteBonus = 0)
    {
        var enemy = Template(id);
        if (!enemy.IsBoss)
        {
            enemy.MaxHealth = Math.Round(enemy.MaxHealth * (0.85 + Rng.NextDouble() * 0.30));
            enemy.Attack = Math.Round(enemy.Attack * (0.90 + Rng.NextDouble() * 0.20), 1);

            if (wild)
            {
                double roll = Rng.NextDouble();
                if (roll < EliteChance + eliteBonus) Promote(enemy, EnemyRank.Elite);
                else if (roll < EliteChance + eliteBonus + VeteranChance) Promote(enemy, EnemyRank.Veteran);
            }
        }
        return enemy.Ready();
    }

    /// <summary>Turns an ordinary enemy into a Veteran or an Elite of its kind.</summary>
    public static void Promote(Enemy enemy, EnemyRank rank)
    {
        enemy.Rank = rank;
        if (rank == EnemyRank.Veteran)
        {
            enemy.Name = "Hardened " + enemy.Name;
            enemy.MaxHealth = Math.Round(enemy.MaxHealth * 1.35);
            enemy.Attack = Math.Round(enemy.Attack * 1.15, 1);
            enemy.Guard += 1;
            enemy.Xp = (int)(enemy.Xp * 1.6);
            enemy.GoldMin *= 2;
            enemy.GoldMax *= 2;
        }
        else if (rank == EnemyRank.Elite)
        {
            enemy.Name = "Dread " + enemy.Name;
            enemy.MaxHealth = Math.Round(enemy.MaxHealth * EliteHealth);
            enemy.Attack = Math.Round(enemy.Attack * 1.5, 1);
            enemy.Guard += 3;
            enemy.Xp *= 3;
            enemy.GoldMin = enemy.GoldMin * 4 + 15;
            enemy.GoldMax = enemy.GoldMax * 4 + 30;
            enemy.Loot.Add(new LootEntry(enemy.Level >= 6 ? SupplyCatalog.GreaterHealingPotion : SupplyCatalog.HealingPotion, 1.0));
            enemy.Loot.Add(new LootEntry(SupplyCatalog.Scraps, 1.0));
            enemy.Loot.Add(new LootEntry(SupplyCatalog.Scraps, 1.0));
        }
        enemy.Ready();
    }

    /// <summary>Story encounter: individuals vary slightly, but nobody is secretly an Elite.</summary>
    public static List<Enemy> Group(params string[] ids) => Number(ids.Select(id => Create(id)).ToList());

    /// <summary>Random encounter: any member may be a Veteran or an Elite.</summary>
    public static List<Enemy> Wild(double eliteBonus, params string[] ids) =>
        Number(ids.Select(id => Create(id, wild: true, eliteBonus)).ToList());

    /// <summary>Random encounter with the default elite chance.</summary>
    public static List<Enemy> Wild(params string[] ids) => Wild(0, ids);

    /// <summary>Adds A/B/C to duplicate names so they can be told apart.</summary>
    private static List<Enemy> Number(List<Enemy> enemies)
    {
        foreach (var same in enemies.GroupBy(e => e.Name).Where(g => g.Count() > 1))
        {
            int n = 0;
            foreach (var enemy in same) enemy.Name = $"{enemy.Name} {(char)('A' + n++)}";
        }
        return enemies;
    }
}
