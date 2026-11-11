using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Worlds;

namespace RPGManagerLib.Quests;

/// <summary>
/// Every quest in the Ashlands and the rules for accepting and completing them.
/// Quest state lives in <see cref="WorldState"/>: quests belong to the Keeper, so any hero can advance them.
/// </summary>
public static class QuestLog
{
    public const string TeethAtTheGate = "teeth_at_the_gate";
    public const string ScorchedRoad = "scorched_road";
    public const string StrangersBurden = "strangers_burden";
    public const string CommandersPrice = "commanders_price";
    public const string EchoesInTheRuins = "echoes_in_the_ruins";
    public const string HollowCrown = "hollow_crown";
    public const string ApprenticesHammer = "apprentices_hammer";
    public const string WhatTheAcademyKnew = "what_the_academy_knew";
    public const string HollowReprisal = "hollow_reprisal";
    public const string BountyScavengers = "bounty_scavengers";
    public const string BountyBandits = "bounty_bandits";
    public const string BountySprites = "bounty_sprites";

    /// <summary>Counter of Academy Pages handed to Lira.</summary>
    public const string PagesDelivered = "pages_delivered";

    /// <summary>Counter of bounties handed in (their reputation gain is capped).</summary>
    public const string BountiesDone = "bounties_done";

    public static readonly IReadOnlyList<Quest> All = new List<Quest>
    {
        new()
        {
            Id = TeethAtTheGate, Title = "Teeth at the Gate", Giver = "Sera Voss", TurnIn = "Sera Voss at the Dustfall Inn",
            Summary = "Ash Scavengers are harassing anyone who steps outside Cinder's Rest. Thin the pack.",
            Objectives = { new KillObjective("scavenger", 5, "Kill Ash Scavengers beyond the gates") },
            Gold = 50, Xp = 80, Reputation = 10
        },
        new()
        {
            Id = ScorchedRoad, Title = "The Scorched Road", Giver = "Sera Voss", TurnIn = "Sera Voss at the Dustfall Inn",
            Summary = "A bandit patrol is taxing the only road out of town. Travel to the Scorched Path and break it.",
            Objectives = { new FlagObjective("Break the bandit patrol on the Scorched Path", WorldFlags.ScorchedPathPatrolCleared) },
            Gold = 100, Xp = 150, Reputation = 15
        },
        new()
        {
            Id = StrangersBurden, Title = "The Stranger's Burden", Giver = "Sera Voss", TurnIn = "",
            Summary = "A man was seen lying injured beside the Scorched Path. Find him and decide what to do with him.",
            Objectives = { new FlagObjective("Find the injured man and make a choice", WorldFlags.DrenMorrowAllied, WorldFlags.DrenMorrowBetrayed) },
            Xp = 150
        },
        new()
        {
            Id = CommandersPrice, Title = "The Commander's Price", Giver = "Sera Voss", TurnIn = "Sera Voss at the Dustfall Inn",
            Summary = "Commander Thresh runs the bandits from a war-camp off the Scorched Path. End his hold on the road — by steel or by words.",
            Objectives = { new FlagObjective("Deal with Commander Thresh", WorldFlags.ThreshDefeated, WorldFlags.ThreshNegotiated) },
            Gold = 120, Xp = 250, Reputation = 15
        },
        new()
        {
            Id = EchoesInTheRuins, Title = "Echoes in the Ruins", Giver = "Lira the Ashen", TurnIn = "Lira the Ashen at the ruin's edge",
            Summary = "Something has been working at the sealed chamber under the Emberveil academy. Go down and reach it first.",
            Objectives = { new FlagObjective("Reach the sealed chamber beneath the academy", WorldFlags.EmberSealBroken) },
            Gold = 80, Xp = 300, Reputation = 20
        },
        new()
        {
            Id = HollowCrown, Title = "The Hollow Crown", Giver = "Lira the Ashen", TurnIn = "Sera Voss at the Dustfall Inn",
            Summary = "The Court's people gather on the plateau of the Ashen Throne. Climb the pilgrim stair and cut the hand off the arm.",
            Objectives = { new FlagObjective("Defeat the Court's handler at the Ashen Throne", WorldFlags.CinderHandDefeated) },
            Gold = 300, Xp = 500, Reputation = 25
        },
        new()
        {
            Id = ApprenticesHammer, Title = "The Apprentice's Hammer", Giver = "Korin Ashwell", TurnIn = "Korin Ashwell at the smithy",
            Summary = "Korin's apprentice died in a bandit ambush six months ago. His hammer is probably still in the burned camp off the Scorched Path.",
            Objectives = { new FlagObjective("Find the hammer in the burned camp", WorldFlags.HasApprenticeHammer) },
            Xp = 120, Reputation = 8
        },
        new()
        {
            Id = WhatTheAcademyKnew, Title = "What the Academy Knew", Giver = "Lira the Ashen", TurnIn = "Lira the Ashen at the ruin's edge",
            Summary = "The wraiths still carry pages of the academy's research. Bring Lira three of them.",
            Objectives = { new CounterObjective(PagesDelivered, 3, "Bring Academy Pages to Lira") },
            Gold = 60, Xp = 150, Reputation = 5
        },
        new()
        {
            Id = HollowReprisal, Title = "Hollow Reprisal", Giver = "A bandit runner", TurnIn = "",
            Summary = "The Court has sent Sentinels to silence Thresh for turning on them. His camp will not hold alone.",
            Objectives = { new FlagObjective("Reach Thresh's camp before it falls", WorldFlags.ThreshSaved, WorldFlags.ThreshLost) },
            Xp = 200, Reputation = 5
        },
        new()
        {
            Id = BountyScavengers, Title = "Bounty: Cull the Pack", Giver = "Bounty board", TurnIn = "the bounty board at the Dustfall Inn",
            Summary = "Standing bounty on Ash Scavengers.",
            Objectives = { new KillObjective("scavenger", 4, "Kill Ash Scavengers") },
            Gold = 30, Xp = 40, Reputation = 2, Repeatable = true
        },
        new()
        {
            Id = BountyBandits, Title = "Bounty: Road Tax", Giver = "Bounty board", TurnIn = "the bounty board at the Dustfall Inn",
            Summary = "Standing bounty on Scorched Path bandits.",
            Objectives = { new KillObjective("bandit", 3, "Kill bandits") },
            Gold = 70, Xp = 70, Reputation = 2, Repeatable = true
        },
        new()
        {
            Id = BountySprites, Title = "Bounty: Snuff the Sprites", Giver = "Bounty board", TurnIn = "the bounty board at the Dustfall Inn",
            Summary = "Standing bounty on Ember Sprites near the ruins.",
            Objectives = { new KillObjective("sprite", 3, "Kill Ember Sprites") },
            Gold = 80, Xp = 90, Reputation = 2, Repeatable = true
        },
    };

    private static WorldState World => WorldState.Current;

    /// <summary>Looks up a quest by id.</summary>
    public static Quest Get(string id) => All.First(q => q.Id == id);

    public static QuestStatus Status(string id) => World.Quests.TryGetValue(id, out var status) ? status : QuestStatus.Unknown;

    public static bool IsActive(string id) => Status(id) == QuestStatus.Active;

    public static bool IsDone(string id) => Status(id) == QuestStatus.Completed;

    /// <summary>True once the quest has been accepted at any point.</summary>
    public static bool Started(string id) => Status(id) != QuestStatus.Unknown;

    /// <summary>All currently active quests.</summary>
    public static List<Quest> Active() => All.Where(q => IsActive(q.Id)).ToList();

    /// <summary>All finished quests.</summary>
    public static List<Quest> Completed() => All.Where(q => IsDone(q.Id)).ToList();

    /// <summary>Accepts a quest and lets its objectives record their starting point.</summary>
    public static void Accept(string id, Character hero)
    {
        var quest = Get(id);
        World.Quests[id] = QuestStatus.Active;
        foreach (var objective in quest.Objectives) objective.OnAccept(World, id);
        World.Log($"{hero.Name} took on \"{quest.Title}\".");
    }

    /// <summary>True when the quest is active and every objective is met.</summary>
    public static bool ObjectivesMet(string id) =>
        IsActive(id) && Get(id).Objectives.All(o => o.IsComplete(World, id));

    /// <summary>
    /// Marks a quest finished (or resets it when repeatable) and returns it so the caller can pay out.
    /// </summary>
    public static Quest Complete(string id, Character hero)
    {
        var quest = Get(id);
        if (quest.Repeatable)
        {
            World.Quests.Remove(id);
            World.Add(BountiesDone);
        }
        else
        {
            World.Quests[id] = QuestStatus.Completed;
        }
        World.Log($"{hero.Name} completed \"{quest.Title}\".");
        return quest;
    }
}
