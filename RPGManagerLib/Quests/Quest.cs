using RPGManagerLib.Worlds;

namespace RPGManagerLib.Quests;

/// <summary>
/// One thing a quest asks for. Each objective type knows how to read its own progress from the world.
/// </summary>
public interface IQuestObjective
{
    /// <summary>Called when the quest is accepted, so objectives can record a starting point.</summary>
    void OnAccept(WorldState world, string questId);

    bool IsComplete(WorldState world, string questId);

    /// <summary>Progress text for the journal, e.g. "Kill Ash Scavengers (3/5)".</summary>
    string Describe(WorldState world, string questId);
}

/// <summary>
/// Kill a number of enemies sharing a kill tag. Progress is a world counter written by combat,
/// so any hero's kills count.
/// </summary>
public class KillObjective : IQuestObjective
{
    private readonly string killTag;
    private readonly int count;
    private readonly string label;

    public KillObjective(string killTag, int count, string label)
    {
        this.killTag = killTag;
        this.count = count;
        this.label = label;
    }

    /// <summary>World counter key for kills of a tag.</summary>
    public static string CounterKey(string killTag) => $"kills:{killTag}";

    private string BaseKey(string questId) => $"quest:{questId}:base:{killTag}";

    private int Progress(WorldState world, string questId) =>
        Math.Min(count, world.Count(CounterKey(killTag)) - world.Count(BaseKey(questId)));

    public void OnAccept(WorldState world, string questId) => world.Counters[BaseKey(questId)] = world.Count(CounterKey(killTag));

    public bool IsComplete(WorldState world, string questId) => Progress(world, questId) >= count;

    public string Describe(WorldState world, string questId) => $"{label} ({Progress(world, questId)}/{count})";
}

/// <summary>
/// Complete when any one of the listed world flags is set.
/// </summary>
public class FlagObjective : IQuestObjective
{
    private readonly string label;
    private readonly string[] flags;

    public FlagObjective(string label, params string[] flags)
    {
        this.label = label;
        this.flags = flags;
    }

    public void OnAccept(WorldState world, string questId) { }

    public bool IsComplete(WorldState world, string questId) => flags.Any(world.Has);

    public string Describe(WorldState world, string questId) => $"{label} ({(IsComplete(world, questId) ? "done" : "not yet")})";
}

/// <summary>
/// Complete when a world counter reaches a value (deliveries, pages handed in, and so on).
/// </summary>
public class CounterObjective : IQuestObjective
{
    private readonly string counterKey;
    private readonly int count;
    private readonly string label;

    public CounterObjective(string counterKey, int count, string label)
    {
        this.counterKey = counterKey;
        this.count = count;
        this.label = label;
    }

    public void OnAccept(WorldState world, string questId) { }

    public bool IsComplete(WorldState world, string questId) => world.Count(counterKey) >= count;

    public string Describe(WorldState world, string questId) => $"{label} ({Math.Min(count, world.Count(counterKey))}/{count})";
}

/// <summary>
/// A quest is pure data: what to do and what it pays. <see cref="QuestLog"/> owns all state changes.
/// </summary>
public class Quest
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Giver { get; init; } = "";
    public string Summary { get; init; } = "";

    /// <summary>Where to hand it in, shown in the journal once objectives are met.</summary>
    public string TurnIn { get; init; } = "";
    public List<IQuestObjective> Objectives { get; init; } = new();
    public int Gold { get; init; }
    public int Xp { get; init; }
    public int Reputation { get; init; }

    /// <summary>Repeatable quests (bounties) reset after being handed in.</summary>
    public bool Repeatable { get; init; }
}
