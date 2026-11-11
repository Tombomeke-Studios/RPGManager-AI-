using RPGManagerLib.Items.Weapons;

namespace RPGManagerLib.Combat;

/// <summary>
/// Broad turn-by-turn temperament. Keeps fights feeling different without real AI.
/// </summary>
public enum EnemyBehavior { Aggressive, Defensive, Random }

/// <summary>
/// A once-per-fight trick or boss script the battle loop knows how to run.
/// </summary>
public enum EnemyTrait { None, HealOnce, PhaseOnce, CallReinforcement, Thresh, CinderHand }

/// <summary>
/// One possible drop: an item id and the chance it drops.
/// Ids are supply ids, or "w:" followed by an <see cref="Items.ItemForge"/> id for equipment.
/// </summary>
public record LootEntry(string ItemId, double Chance);

/// <summary>
/// A hostile combatant. Mostly data; the battle loop reads it and decides what happens.
/// </summary>
public class Enemy : ICombatant
{
    public string Id { get; init; } = "";
    public string Name { get; set; } = "";
    public string Description { get; init; } = "";
    public int Level { get; init; } = 1;
    public double MaxHealth { get; init; }
    public double Health { get; private set; }
    public double Attack { get; init; }
    public int Guard { get; init; }
    public Element AttackElement { get; init; } = Element.NONE;
    public EnemyBehavior Behavior { get; init; } = EnemyBehavior.Aggressive;
    public EnemyTrait Trait { get; init; } = EnemyTrait.None;
    public int Xp { get; init; }
    public int GoldMin { get; init; }
    public int GoldMax { get; init; }

    /// <summary>Counter key used by kill quests ("bandit" covers every bandit variant).</summary>
    public string KillTag { get; init; } = "";

    /// <summary>Loose categories such as "spirit" or "court" that some skills care about.</summary>
    public string[] Tags { get; init; } = Array.Empty<string>();

    /// <summary>Chance to set the hero burning on a hit.</summary>
    public double BurnChance { get; init; }

    /// <summary>Damage multiplier against plain, non-elemental weapon hits.</summary>
    public double PhysicalMultiplier { get; init; } = 1.0;

    /// <summary>Damage multiplier against anything magical.</summary>
    public double SpellMultiplier { get; init; } = 1.0;

    /// <summary>Per-element multipliers: above 1 is a weakness, below 1 a resistance.</summary>
    public Dictionary<Element, double> ElementMultipliers { get; init; } = new();

    public List<LootEntry> Loot { get; init; } = new();
    public bool IsBoss { get; init; }

    public List<StatusEffect> Effects { get; } = new();

    // --- Fight-time state ---

    /// <summary>The once-per-fight trait has fired.</summary>
    public bool TraitUsed { get; set; }

    /// <summary>Phased out: attacks pass through.</summary>
    public bool Untargetable { get; set; }

    /// <summary>Winding up a heavy attack that lands next turn.</summary>
    public bool Telegraphing { get; set; }

    /// <summary>Turns taken, used for boss attack cycles.</summary>
    public int TurnCounter { get; set; }

    /// <summary>Multiplier on outgoing damage (boss rallies).</summary>
    public double AttackBuff { get; set; } = 1.0;

    public bool IsAlive => Health > 0;

    /// <summary>Sets health to full; called by the bestiary after construction.</summary>
    public Enemy Ready()
    {
        Health = MaxHealth;
        return this;
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

    /// <summary>
    /// Combined weakness/resistance multiplier for an incoming hit.
    /// </summary>
    /// <param name="element">Element of the hit (NONE for plain damage).</param>
    /// <param name="magic">True for spells and arcane bolts.</param>
    public double DamageMultiplier(Element element, bool magic)
    {
        double multiplier = 1.0;
        if (!magic && element == Element.NONE) multiplier *= PhysicalMultiplier;
        if (magic) multiplier *= SpellMultiplier;
        if (element != Element.NONE && ElementMultipliers.TryGetValue(element, out double m)) multiplier *= m;
        return multiplier;
    }
}
