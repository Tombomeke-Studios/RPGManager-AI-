using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Combat;
using RPGManagerLib.Core;
using RPGManagerLib.Items;
using RPGManagerLib.Quests;
using RPGManagerLib.Saves;
using RPGManagerLib.UI;
using RPGManagerLib.Worlds;

namespace RPGManagerLib.Adventure;

/// <summary>
/// The running game session: the roster, the shared world, and saving.
/// </summary>
public static class Game
{
    /// <summary>The Keeper's heroes.</summary>
    public static CharacterRoster Roster { get; private set; } = new();

    /// <summary>The shared world.</summary>
    public static WorldState World => WorldState.Current;

    /// <summary>The hero currently being guided.</summary>
    public static Character Hero => Roster.Active ?? throw new InvalidOperationException("No active hero.");

    /// <summary>Shortcut: is a world flag set?</summary>
    public static bool Has(string flag) => World.Has(flag);

    /// <summary>Sets a world flag, credited to the active hero.</summary>
    public static void Set(string flag) => World.Set(flag, Roster.Active?.Name);

    /// <summary>Current Ashlands reputation tier.</summary>
    public static ReputationTier Tier => World.Tier();

    /// <summary>Loads the save from disk, or starts with an empty roster and a fresh world.</summary>
    public static void Load()
    {
        Roster = new CharacterRoster();
        WorldState.Current = new WorldState();

        var save = SaveManager.LoadGame();
        if (save == null) return;
        try
        {
            Roster.Load(save.Characters.Select(c => c.ToCharacter()), save.ActiveIndex);
            WorldState.Current = save.World.ToWorldState();
        }
        catch (Exception)
        {
            Console.WriteLine("Save file could not be read. Starting a new game.");
            Roster = new CharacterRoster();
            WorldState.Current = new WorldState();
        }
    }

    /// <summary>Writes the whole game to disk.</summary>
    public static void Save()
    {
        if (Roster.Heroes.Count == 0) return;
        SaveManager.SaveGame(Roster.Heroes, Roster.Active, World);
    }

    /// <summary>Wipes the session and the save file.</summary>
    public static void Reset()
    {
        SaveManager.DeleteGame();
        Roster = new CharacterRoster();
        WorldState.Current = new WorldState();
    }
}

/// <summary>
/// Hands out gold, experience, reputation and items, and announces level-ups and tier changes.
/// </summary>
public static class Rewards
{
    /// <summary>Gives gold, XP and reputation to the active hero and the world.</summary>
    public static void Give(int gold = 0, int xp = 0, int reputation = 0)
    {
        var hero = Game.Hero;
        if (gold > 0)
        {
            hero.Gold += gold;
            Ui.Gain($"+{gold} gold");
        }
        if (xp > 0) GiveXp(xp);
        if (reputation != 0) GiveReputation(reputation);
    }

    /// <summary>Gives XP and reports any level-ups, new skills and newly opened roster slots.</summary>
    public static void GiveXp(int xp)
    {
        var hero = Game.Hero;
        if (hero.Level >= Character.MaxLevel)
        {
            Ui.Note($"({hero.Name} has reached the peak of what the Ashlands can teach.)");
            return;
        }

        int slotsBefore = Game.Roster.UnlockedSlots;
        int levelBefore = hero.Level;
        var before = (hero.Might, hero.Guard, hero.Arcana, hero.Perception, hero.MaxHealth, hero.MaxMana);
        Ui.Gain($"+{xp} XP");
        int gained = hero.GainExperience(xp);
        if (gained == 0) return;

        Ui.Line();
        Ui.Line($"  ★ {hero.Name} reaches level {hero.Level}! ★", ConsoleColor.Yellow);
        var parts = new List<string> { $"Health +{hero.MaxHealth - before.MaxHealth:0}", $"{hero.UnspentPoints} stat points" };
        if (hero.Might > before.Might) parts.Add($"Might +{hero.Might - before.Might}");
        if (hero.Guard > before.Guard) parts.Add($"Guard +{hero.Guard - before.Guard}");
        if (hero.Arcana > before.Arcana) parts.Add($"Arcana +{hero.Arcana - before.Arcana}");
        if (hero.Perception > before.Perception) parts.Add($"Perception +{hero.Perception - before.Perception}");
        if (hero.MaxMana > before.MaxMana) parts.Add($"Mana +{hero.MaxMana - before.MaxMana:0}");
        Ui.Good("  " + string.Join(", ", parts) + ". Fully restored.");

        for (int level = levelBefore + 1; level <= hero.Level; level++)
            foreach (var skill in SkillBook.UnlockedAt(hero, level))
                Ui.Line($"  New skill: {skill.Name} — {skill.Description}", ConsoleColor.Cyan);

        Game.World.Log($"{hero.Name} reached level {hero.Level}.");
        HeroScreens.SpendPoints();

        if (Game.Roster.UnlockedSlots > slotsBefore)
        {
            Ui.Line();
            Ui.World("The bond deepens. The Keeper can hold another hero now — a new roster slot has opened.");
            Ui.Note("  (Keeper's menu → Roster → Bond a new hero.)");
            Game.World.Log("The Keeper's bond deepened: a new hero slot opened.");
        }
    }

    /// <summary>Changes Ashlands reputation and announces tier changes.</summary>
    public static void GiveReputation(int amount)
    {
        var tierBefore = Game.Tier;
        Game.World.AddReputation(amount);
        Ui.Gain($"{(amount > 0 ? "+" : "")}{amount} Ashlands reputation ({Game.World.GetReputation()})");
        if (Game.Tier > tierBefore)
        {
            Ui.World($"The Ashlands now consider the Keeper {Game.Tier}. Every hero you guide will feel it.");
            Game.World.Log($"The Keeper became {Game.Tier} in the Ashlands.");
        }
    }

    /// <summary>Pays out a finished quest.</summary>
    public static void CompleteQuest(string questId)
    {
        var quest = QuestLog.Complete(questId, Game.Hero);
        Ui.Line();
        Ui.Line($"  ✔ Quest complete: {quest.Title}", ConsoleColor.Green);
        int reputation = quest.Reputation;
        if (quest.Repeatable && Game.World.Count(QuestLog.BountiesDone) > 10) reputation = 0;
        int gold = quest.Gold;
        if (quest.Repeatable && Contracts.Done(Contracts.ThePaymaster))
        {
            gold = (int)(gold * 1.5);
            Ui.World("Paid from the Court's own strongbox.");
        }
        Give(gold, quest.Xp, reputation);
    }

    /// <summary>Accepts a quest and announces it.</summary>
    public static void AcceptQuest(string questId)
    {
        QuestLog.Accept(questId, Game.Hero);
        var quest = QuestLog.Get(questId);
        Ui.Line();
        Ui.Line($"  ✎ New quest: {quest.Title}", ConsoleColor.Cyan);
        Ui.Note("  " + quest.Summary);
    }

    /// <summary>Gives an equipable item; if the pack is full it goes to the Keeper's Vault.</summary>
    public static void GiveItem(string forgeId) => GiveItem(ItemForge.Create(forgeId));

    /// <summary>Gives an equipable item; if the pack is full it goes to the Keeper's Vault.</summary>
    public static void GiveItem(IEquipable item)
    {
        if (Game.Hero.TryAddItem(item))
        {
            Ui.Gain($"Received: {ItemForge.Describe(item)}");
        }
        else
        {
            Game.World.Vault.Add(item);
            Ui.Gain($"Received: {ItemForge.Describe(item)}");
            Ui.Note("  Your pack is full — it was sent to the Keeper's Vault at the Dustfall Inn.");
        }
    }

    /// <summary>Gives supplies.</summary>
    public static void GiveSupply(string supplyId, int amount = 1)
    {
        Game.Hero.AddSupply(supplyId, amount);
        Ui.Gain($"Received: {SupplyCatalog.Get(supplyId).Name}{(amount > 1 ? $" x{amount}" : "")}");
    }
}

/// <summary>
/// Starts fights and settles everything that follows: kill counters, loot, XP, or the walk of shame home.
/// </summary>
public static class Encounters
{
    /// <summary>
    /// Runs one fight for the active hero.
    /// </summary>
    /// <param name="title">Screen title.</param>
    /// <param name="intro">Narration shown before the fight.</param>
    /// <param name="enemies">The enemy group.</param>
    /// <param name="options">Fight rules; null for an ordinary, fleeable fight.</param>
    public static BattleOutcome Fight(string title, string intro, List<Enemy> enemies, BattleOptions? options = null)
    {
        var hero = Game.Hero;
        Ui.Header(title);
        Ui.Narrate(intro);
        Ui.Line();
        foreach (var kind in enemies.GroupBy(e => e.Id))
        {
            var first = kind.First();
            string count = kind.Count() > 1 ? $" x{kind.Count()}" : "";
            Ui.Line($"  ▸ {Bestiary.Exact(first.Id).Name}{count} (level {first.Level})", ConsoleColor.Red);
            Ui.Note("    " + first.Description);
        }
        foreach (var strong in enemies.Where(e => e.Rank != EnemyRank.Normal))
        {
            if (strong.Rank == EnemyRank.Elite)
                Ui.Line($"  ☠ {strong.Name} is an ELITE — far stronger than its kind ({strong.MaxHealth:0} health). Big rewards if you win.{((options?.CanFlee ?? true) ? " There is no shame in fleeing." : "")}", ConsoleColor.Magenta);
            else
                Ui.Line($"  ▲ {strong.Name} is a veteran — tougher than the rest.", ConsoleColor.Yellow);
        }
        if (enemies.Any(e => e.Rank == EnemyRank.Elite)) Game.World.Add("elites_met");
        options ??= new BattleOptions();
        if (Contracts.Done(Contracts.WingsOverTheThrone)) options.BurnResistance = 0.5;
        if (Contracts.Done(Contracts.InquisitorsLedger))
        {
            var warded = enemies.Where(e => e.Tags.Contains("court")).ToList();
            foreach (var enemy in warded) enemy.TakeDamage(Math.Round(enemy.MaxHealth * 0.15));
            if (warded.Count > 0) Ui.World("You know the Court's ward-sigils from the Inquisitor's ledger. Their protections are already unravelling.");
        }
        Ui.Pause("Press ENTER to fight...");

        // RPGMANAGER_AUTOBATTLE=1 lets the bot fight, so scripted playtests have a predictable input count.
        IBattleController controller = Environment.GetEnvironmentVariable("RPGMANAGER_AUTOBATTLE") == "1"
            ? new BotBattleController()
            : new ConsoleBattleController();
        var result = new Battle(hero, enemies, controller, options).Run();

        switch (result.Outcome)
        {
            case BattleOutcome.Victory:
                Ui.Sub("Victory");
                Settle(result);
                break;
            case BattleOutcome.Fled:
                Ui.Sub("Escaped");
                Ui.Narrate("You put distance between yourself and the fight. Nothing gained, nothing lost but pride.");
                break;
            case BattleOutcome.Defeat:
                Defeat();
                break;
        }

        Game.Save();
        Ui.Pause();
        return result.Outcome;
    }

    private static void Settle(BattleResult result)
    {
        var world = Game.World;
        bool sentinelSlain = false;
        foreach (var enemy in result.Slain)
        {
            if (enemy.KillTag.Length > 0) world.Add(KillObjective.CounterKey(enemy.KillTag));
            if (enemy.KillTag == "sentinel") sentinelSlain = true;
        }

        Rewards.Give(result.Gold, result.Xp);

        foreach (var loot in result.Loot.GroupBy(id => id))
        {
            if (loot.Key.StartsWith("w:"))
                foreach (string id in loot) Rewards.GiveItem(id[2..]);
            else Rewards.GiveSupply(loot.Key, loot.Count());
        }

        if (sentinelSlain && !Game.Has(WorldFlags.HasCourtSigil))
        {
            Game.Set(WorldFlags.HasCourtSigil);
            Game.Set(WorldFlags.AshlandsCourtFound);
            Ui.Line();
            Ui.Gain("Key item: Court Sigil");
            Ui.Narrate("Under the Sentinel's gorget hangs a disc of black iron inlaid with gold: a crown with no top, open to the sky. It is cold, and it stays cold in your hand.");
            Ui.World("Someone who knows the old houses might recognise this. All of your heroes now carry the knowledge of it.");
            world.Log($"{Game.Hero.Name} took a Court Sigil from a fallen Sentinel.");
        }

        foreach (var quest in QuestLog.Active().Where(q => QuestLog.ObjectivesMet(q.Id) && q.TurnIn.Length > 0))
            Ui.Line($"  ✔ \"{quest.Title}\" is ready to hand in: {quest.TurnIn}.", ConsoleColor.Green);
    }

    private static void Defeat()
    {
        var hero = Game.Hero;
        int lost = (int)(hero.Gold * 0.15);
        hero.Gold -= lost;
        hero.Health = Math.Round(hero.MaxHealth * 0.4);
        if (hero is Mage) hero.Mana = Math.Max(hero.Mana, hero.MaxMana * 0.4);
        hero.LocationId = "cinders_rest";

        Ui.Sub("Defeat");
        Ui.Bad($"{hero.Name} goes down. The ash closes over everything.");
        Ui.Vision("But the bond does not break. You hold on — to a heartbeat, to a name — and you do not let go.");
        Ui.Narrate($"{hero.Name} wakes on a cot in the back room of the Dustfall Inn. Sera's people found them on the road and carried them home. {(lost > 0 ? $"Someone along the way helped themselves to {lost} gold." : "At least there was nothing in the purse worth stealing.")}");
        Game.World.Log($"{hero.Name} was beaten and carried back to Cinder's Rest.");
    }

    /// <summary>Extra elite chance on ground the hero has outgrown, so high levels stay dangerous.</summary>
    private static double EliteBonus(int level) => level >= 11 ? 0.14 : level >= 8 ? 0.05 : 0;

    private static List<Enemy> PickWild(int level, List<string[]> groups) =>
        Bestiary.Wild(EliteBonus(level), Rng.Pick(groups));

    /// <summary>A level-appropriate group outside Cinder's Rest: scavengers, goblins, boars.</summary>
    public static List<Enemy> ScavengerPack(int level)
    {
        var groups = new List<string[]> { new[] { Bestiary.AshScavenger, Bestiary.AshScavenger } };
        if (level >= 2)
        {
            groups.Add(new[] { Bestiary.AshScavenger, Bestiary.AshScavenger, Bestiary.AshScavenger });
            groups.Add(new[] { Bestiary.CinderGoblin, Bestiary.CinderGoblin });
            groups.Add(new[] { Bestiary.CinderGoblin, Bestiary.GoblinSlinger });
        }
        if (level >= 3)
        {
            groups.RemoveAt(0);
            groups.Add(new[] { Bestiary.ScavengerAlpha, Bestiary.AshScavenger, Bestiary.AshScavenger });
            groups.Add(new[] { Bestiary.CinderGoblin, Bestiary.GoblinSlinger, Bestiary.CinderGoblin });
            groups.Add(new[] { Bestiary.AshbackBoar });
        }
        if (level >= 4) groups.Add(new[] { Bestiary.GoblinShaman, Bestiary.CinderGoblin });
        if (level >= 6)
        {
            groups.Add(new[] { Bestiary.AshbackBoar, Bestiary.AshbackBoar });
            groups.Add(new[] { Bestiary.GoblinShaman, Bestiary.CinderGoblin, Bestiary.GoblinSlinger, Bestiary.CinderGoblin });
        }
        if (level >= 8) groups.Add(new[] { Bestiary.EmberHound, Bestiary.EmberHound });
        return PickWild(level, groups);
    }

    /// <summary>A level-appropriate group on the Scorched Path.</summary>
    public static List<Enemy> BanditGroup(int level)
    {
        var groups = new List<string[]>
        {
            new[] { Bestiary.Bandit, Bestiary.Bandit },
            new[] { Bestiary.Bandit, Bestiary.BanditBowman },
        };
        if (level <= 2) groups.Add(new[] { Bestiary.Bandit });
        if (level >= 3) groups.Add(new[] { Bestiary.AshbackBoar });
        if (level >= 4) groups.Add(new[] { Bestiary.Bandit, Bestiary.BanditBowman, Bestiary.Bandit });
        if (level >= 5) groups.Add(new[] { Bestiary.BanditEnforcer, Bestiary.Bandit });
        if (level >= 6) groups.Add(new[] { Bestiary.BanditCutthroat, Bestiary.BanditBowman });
        if (level >= 7) groups.Add(new[] { Bestiary.BanditEnforcer, Bestiary.BanditBowman, Bestiary.Bandit });
        if (level >= 9) groups.Add(new[] { Bestiary.BanditCutthroat, Bestiary.BanditEnforcer, Bestiary.BanditCutthroat });
        return PickWild(level, groups);
    }

    /// <summary>A level-appropriate group around the ruins.</summary>
    public static List<Enemy> RuinsGroup(int level)
    {
        var groups = new List<string[]>
        {
            new[] { Bestiary.EmberSprite },
            new[] { Bestiary.EmberSprite, Bestiary.EmberSprite },
        };
        if (level >= 5)
        {
            groups.Add(new[] { Bestiary.EmberSprite, Bestiary.EmberSprite, Bestiary.EmberSprite });
            groups.Add(new[] { Bestiary.SlagBeetle });
            groups.Add(new[] { Bestiary.SlagBeetle, Bestiary.EmberSprite });
        }
        if (level >= 6)
        {
            groups.Add(new[] { Bestiary.AshWraith });
            groups.Add(new[] { Bestiary.EmberHound });
        }
        if (level >= 7)
        {
            groups.Add(new[] { Bestiary.AshWraith, Bestiary.EmberSprite });
            groups.Add(new[] { Bestiary.EmberHound, Bestiary.EmberHound });
        }
        if (level >= 8) groups.Add(new[] { Bestiary.RuinGolem });
        if (level >= 10) groups.Add(new[] { Bestiary.RuinGolem, Bestiary.SlagBeetle });
        return PickWild(level, groups);
    }

    /// <summary>Whatever still walks the plateau of the Ashen Throne.</summary>
    public static List<Enemy> PlateauGroup(int level)
    {
        var groups = new List<string[]>
        {
            new[] { Bestiary.CourtSentinel },
            new[] { Bestiary.AshWraith, Bestiary.AshWraith },
            new[] { Bestiary.RuinGolem },
            new[] { Bestiary.EmberHound, Bestiary.EmberHound, Bestiary.EmberHound },
        };
        if (level >= 10) groups.Add(new[] { Bestiary.CourtInquisitor, Bestiary.CourtSentinel });
        if (level >= 12) groups.Add(new[] { Bestiary.CourtInquisitor, Bestiary.RuinGolem });
        return PickWild(level, groups);
    }
}

/// <summary>
/// Price rules. Prices are always computed for the hero who is buying, so reputation and
/// world events change them without any stored "current price" drifting out of date.
/// </summary>
public static class Prices
{
    /// <summary>True once the bandits no longer hold the road.</summary>
    public static bool RoadIsOpen => Game.Has(WorldFlags.ThreshDefeated) || Game.Has(WorldFlags.ThreshNegotiated);

    private static double TierDiscount => Game.Tier switch
    {
        ReputationTier.Known => 0.95,
        ReputationTier.Trusted => 0.90,
        ReputationTier.Honored => 0.80,
        _ => 1.0
    };

    /// <summary>What Korin charges this hero for an item worth <paramref name="value"/>.</summary>
    public static int Smithy(int value)
    {
        double price = value * TierDiscount;
        if (Game.Hero is Archer && Game.Tier == ReputationTier.Stranger) price *= 1.15;
        if (Contracts.Done(Contracts.BlackGullyBoar)) price *= 0.9;
        return Math.Max(1, (int)Math.Round(price));
    }

    /// <summary>What the Dustfall Inn charges for supplies.</summary>
    /// <param name="value">Base price.</param>
    /// <param name="supplyId">The supply being priced, when known.</param>
    public static int Supplies(int value, string? supplyId = null)
    {
        double price = value * TierDiscount * (RoadIsOpen ? 0.85 : 1.0);
        if (supplyId is Items.SupplyCatalog.EmberSalve or Items.SupplyCatalog.ManaDraught && Contracts.Done(Contracts.RestlessSpring)) price *= 0.5;
        return Math.Max(1, (int)Math.Round(price));
    }

    /// <summary>What a shop pays when buying from the hero — always well under value.</summary>
    public static int Sell(int value) => Math.Max(1, (int)(value * (Game.Tier == ReputationTier.Honored ? 0.5 : 0.4)));
}
