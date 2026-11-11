using RPGManagerLib.Items;

namespace RPGManagerLib.Worlds;

/// <summary>
/// How the people of a region see the Keeper. Always computed from the raw score, never stored.
/// </summary>
public enum ReputationTier { Stranger, Known, Trusted, Honored }

/// <summary>
/// Lifecycle of a quest in the shared world.
/// </summary>
public enum QuestStatus { Unknown, Active, Completed }

/// <summary>
/// Names of every world flag, in one place so no system has to type a string literal.
/// </summary>
public static class WorldFlags
{
    public const string IntroSeen = "IntroSeen";
    public const string AshlandsScavengerBounty = "AshlandsScavengerBounty";
    public const string KorinSharpenOwed = "KorinSharpenOwed";
    public const string ScorchedPathOpen = "ScorchedPathOpen";
    public const string ScorchedPathPatrolCleared = "ScorchedPathPatrolCleared";
    public const string WagonLooted = "WagonLooted";
    public const string DrenMorrowFound = "DrenMorrowFound";
    public const string DrenMorrowAllied = "DrenMorrowAllied";
    public const string DrenMorrowBetrayed = "DrenMorrowBetrayed";
    public const string HasApprenticeHammer = "HasApprenticeHammer";
    public const string HammerReturned = "HammerReturned";
    public const string LiraMet = "LiraMet";
    public const string RuinsSurfaceCleared = "RuinsSurfaceCleared";
    public const string RuinsHallsCleared = "RuinsHallsCleared";
    public const string RuinsDigTeamDefeated = "RuinsDigTeamDefeated";
    public const string WraithLectureHeard = "WraithLectureHeard";
    public const string EmberSealBroken = "EmberSealBroken";
    public const string HasCourtSigil = "HasCourtSigil";
    public const string AshlandsCourtFound = "AshlandsCourtFound";
    public const string EmberArtifactSecured = "EmberArtifactSecured";
    public const string EmberArtifactLeft = "EmberArtifactLeft";
    public const string RuinsCacheWarrior = "RuinsCacheWarrior";
    public const string RuinsCacheMage = "RuinsCacheMage";
    public const string RuinsCacheArcher = "RuinsCacheArcher";
    public const string ThreshCampFound = "ThreshCampFound";
    public const string ThreshDefeated = "ThreshDefeated";
    public const string ThreshNegotiated = "ThreshNegotiated";
    public const string ThreshAlly = "ThreshAlly";
    public const string ThreshSaved = "ThreshSaved";
    public const string ThreshLost = "ThreshLost";
    public const string ThroneStairCleared = "ThroneStairCleared";
    public const string ThroneCampCleared = "ThroneCampCleared";
    public const string ThroneTentRested = "ThroneTentRested";
    public const string AshlandsCourtExposed = "AshlandsCourtExposed";
    public const string CinderHandDefeated = "CinderHandDefeated";
    public const string AshenThroneSat = "AshenThroneSat";
    public const string AshenThroneRefused = "AshenThroneRefused";
    public const string AshlandsComplete = "AshlandsComplete";
    public const string EpilogueSeen = "EpilogueSeen";
}

/// <summary>
/// The shared state of Tavaryn: everything that is true for the Keeper rather than for one hero.
/// Flags, counters, reputation, quests, the chronicle and the Keeper's Vault all live here,
/// so whatever one hero does is visible to every other hero.
/// </summary>
public class WorldState
{
    /// <summary>Region key for the Ashlands reputation score.</summary>
    public const string Ashlands = "Ashlands";

    /// <summary>The chronicle keeps only the most recent entries.</summary>
    public const int MaxLogEntries = 200;

    /// <summary>The world state of the loaded save.</summary>
    public static WorldState Current { get; set; } = new();

    public Dictionary<string, bool> Flags { get; set; } = new();
    public Dictionary<string, int> Counters { get; set; } = new();
    public Dictionary<string, int> Reputation { get; set; } = new();

    /// <summary>Which hero set which flag — lets NPCs talk about "your other one".</summary>
    public Dictionary<string, string> Deeds { get; set; } = new();
    public Dictionary<string, QuestStatus> Quests { get; set; } = new();
    public List<string> EventLog { get; set; } = new();

    /// <summary>Items stored at the Dustfall Inn, reachable by every hero.</summary>
    public List<IEquipable> Vault { get; set; } = new();

    /// <summary>Returns true when the flag has been set.</summary>
    public bool Has(string flag) => Flags.TryGetValue(flag, out bool value) && value;

    /// <summary>Sets a flag and remembers which hero did it.</summary>
    public void Set(string flag, string? byHero = null)
    {
        if (Has(flag)) return;
        Flags[flag] = true;
        if (byHero != null) Deeds[flag] = byHero;
    }

    /// <summary>Name of the hero who set the flag, or null.</summary>
    public string? DoneBy(string flag) => Deeds.TryGetValue(flag, out string? name) ? name : null;

    /// <summary>Reads a counter (0 when never written).</summary>
    public int Count(string key) => Counters.TryGetValue(key, out int value) ? value : 0;

    /// <summary>Adds to a counter.</summary>
    public void Add(string key, int amount = 1) => Counters[key] = Count(key) + amount;

    /// <summary>Raw reputation score for a region.</summary>
    public int GetReputation(string region = Ashlands) => Reputation.TryGetValue(region, out int value) ? value : 0;

    /// <summary>Changes reputation for a region.</summary>
    public void AddReputation(int amount, string region = Ashlands) => Reputation[region] = GetReputation(region) + amount;

    /// <summary>Reputation tier for a region, computed from the score.</summary>
    public ReputationTier Tier(string region = Ashlands) => TierFor(GetReputation(region));

    /// <summary>Maps a raw score to its tier.</summary>
    public static ReputationTier TierFor(int score) =>
        score >= 100 ? ReputationTier.Honored :
        score >= 60 ? ReputationTier.Trusted :
        score >= 30 ? ReputationTier.Known :
        ReputationTier.Stranger;

    /// <summary>Score needed for the next tier, or null at the top.</summary>
    public static int? NextTierAt(int score) => score < 30 ? 30 : score < 60 ? 60 : score < 100 ? 100 : null;

    /// <summary>Adds a line to the Keeper's Chronicle, dropping the oldest when full.</summary>
    public void Log(string entry)
    {
        EventLog.Add(entry);
        if (EventLog.Count > MaxLogEntries) EventLog.RemoveRange(0, EventLog.Count - MaxLogEntries);
    }
}
