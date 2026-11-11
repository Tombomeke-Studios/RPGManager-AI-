namespace RPGManagerLib.Combat;

/// <summary>
/// Every temporary condition a combatant can be under.
/// </summary>
public enum StatusType { Burn, Poison, Freeze, Stun, Weaken, Exposed, Ward, Evasion, Guarding }

/// <summary>
/// One active condition on a combatant. Effects live on a shared list and are ticked in one pass per round.
/// </summary>
public class StatusEffect
{
    public StatusType Type { get; set; }

    /// <summary>Rounds remaining.</summary>
    public int Turns { get; set; }

    /// <summary>Damage per tick (Burn/Poison) or absorb pool (Ward).</summary>
    public double Potency { get; set; }

    public StatusEffect(StatusType type, int turns, double potency = 0)
    {
        Type = type;
        Turns = turns;
        Potency = potency;
    }

    /// <summary>Short text for the status line.</summary>
    public string Label => Type == StatusType.Ward ? $"Ward {Potency:0}" : $"{Type} {Turns}";
}

/// <summary>
/// The contract the battle loop needs from both heroes and enemies.
/// </summary>
public interface ICombatant
{
    string Name { get; }
    double Health { get; }
    double MaxHealth { get; }
    bool IsAlive { get; }
    int Guard { get; }
    List<StatusEffect> Effects { get; }

    /// <summary>Removes health, never below zero.</summary>
    void TakeDamage(double amount);

    /// <summary>Restores health up to the maximum and returns how much was restored.</summary>
    double Restore(double amount);
}

/// <summary>
/// Status helpers shared by every combatant.
/// </summary>
public static class CombatantExtensions
{
    public static bool Has(this ICombatant c, StatusType type) => c.Effects.Any(e => e.Type == type);

    public static StatusEffect? Get(this ICombatant c, StatusType type) => c.Effects.FirstOrDefault(e => e.Type == type);

    /// <summary>Applies an effect, refreshing it when already present.</summary>
    public static void Apply(this ICombatant c, StatusType type, int turns, double potency = 0)
    {
        var existing = c.Get(type);
        if (existing == null)
        {
            c.Effects.Add(new StatusEffect(type, turns, potency));
            return;
        }
        existing.Turns = Math.Max(existing.Turns, turns);
        existing.Potency = Math.Max(existing.Potency, potency);
    }

    public static void Remove(this ICombatant c, StatusType type) => c.Effects.RemoveAll(e => e.Type == type);

    /// <summary>Status line such as "[Burn 2] [Ward 30]".</summary>
    public static string StatusLine(this ICombatant c) => string.Join(" ", c.Effects.Select(e => $"[{e.Label}]"));
}
