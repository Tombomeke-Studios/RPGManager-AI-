using RPGManagerLib.Adventure;
using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Combat;
using RPGManagerLib.Items;
using RPGManagerLib.Items.Weapons;
using RPGManagerLib.Quests;
using RPGManagerLib.Worlds;

namespace RPGManagerLib.UI;

/// <summary>
/// The Keeper's menu: hero sheet, pack, journal, chronicle, standing and roster.
/// </summary>
public static class HeroScreens
{
    /// <summary>Shows the Keeper's menu until the player goes back. Returns false when the player quits the game.</summary>
    public static bool KeeperMenu()
    {
        while (true)
        {
            var hero = Game.Hero;
            Ui.Header("The Keeper's Menu");
            Ui.Line($"  Guiding: {hero.Name} the {hero.CharacterType}, level {hero.Level}", ConsoleColor.Cyan);

            bool back = false, quit = false;
            Ui.Menu(new List<MenuOption>
            {
                new("1", "Hero", hero.UnspentPoints > 0 ? $"stats and skills — {hero.UnspentPoints} stat points to spend!" : "stats and skills", Sheet),
                new("2", "Pack", "equip weapons, use supplies", Pack),
                new("3", "Journal", "active and finished quests", Journal),
                new("4", "Standing", "reputation and the state of the world", Standing),
                new("5", "Chronicle", "what your heroes have done", Chronicle),
                new("6", "Roster", "switch hero or bond a new one", Roster),
                new("7", "Save", "write the game to disk", () => { Game.Save(); Ui.Good("Saved."); Ui.Pause(); }),
                new("8", "Save and quit", "leave Tavaryn for now", () => quit = true),
                new("0", "Back", "", () => back = true),
            });
            if (quit) return false;
            if (back) return true;
        }
    }

    /// <summary>Full character sheet for the active hero.</summary>
    public static void Sheet()
    {
        var hero = Game.Hero;
        Ui.Header($"{hero.Name} the {hero.CharacterType}");
        WriteVitals(hero);
        Ui.Line();
        Ui.Line($"  Might {hero.Might,-4} melee damage           Guard {hero.Guard,-4} damage reduction");
        Ui.Line($"  Arcana {hero.Arcana,-3} spell power            Perception {hero.Perception,-3} bows, crits, escapes");
        Ui.Line($"  Gold {hero.Gold}", ConsoleColor.Yellow);

        Ui.Sub("Fighting with");
        var weapon = hero.ActiveWeapon;
        Ui.Line("  " + (weapon == null ? "bare hands" : ItemForge.Describe(weapon)));
        if (hero is Archer archer) Ui.Line($"  {archer.ArrowCapacity} arrows per fight");

        Ui.Sub($"Skills ({hero.ResourceName})");
        var skills = SkillBook.For(hero);
        foreach (var skill in skills)
        {
            Ui.Write($"  {skill.Name,-16}", ConsoleColor.White);
            Ui.Write($"{(skill.Cost > 0 ? skill.Cost.ToString() : "free"),-6}", ConsoleColor.DarkCyan);
            Ui.Line(skill.Description, ConsoleColor.DarkGray);
        }
        var upcoming = SkillBook.All
            .Where(s => s.HeroClass == hero.CharacterType && s.UnlockLevel > hero.Level)
            .OrderBy(s => s.UnlockLevel).FirstOrDefault();
        if (upcoming != null) Ui.Note($"  Next: {upcoming.Name} at level {upcoming.UnlockLevel}.");
        if (hero.UnspentPoints > 0) SpendPoints();
        else Ui.Pause();
    }

    /// <summary>
    /// Lets the player spend waiting stat points one at a time. This is where a build is decided:
    /// the same class can be a wall, a cannon or something in between.
    /// </summary>
    public static void SpendPoints()
    {
        var hero = Game.Hero;
        while (hero.UnspentPoints > 0)
        {
            Ui.Sub($"Stat points to spend: {hero.UnspentPoints}");
            int choice = Ui.Choose(new List<string>
            {
                $"Might {hero.Might} → {hero.Might + 1}        melee and dagger damage",
                $"Guard {hero.Guard} → {hero.Guard + 1}        take less damage from every hit",
                $"Arcana {hero.Arcana} → {hero.Arcana + 1}       spell damage, healing and wards",
                $"Perception {hero.Perception} → {hero.Perception + 1}   bow damage, critical hits, escaping, noticing things",
                $"Vitality                +{Character.HealthPerVitality} maximum health ({hero.MaxHealth:0} → {hero.MaxHealth + Character.HealthPerVitality:0})",
            }, "Decide later (Keeper's menu → Hero)");
            if (choice < 0) return;
            hero.SpendPoint((HeroStat)choice);
        }
        Game.Save();
    }

    /// <summary>Writes the health, resource and XP lines for a hero.</summary>
    public static void WriteVitals(Character hero)
    {
        Ui.Write($"  Level {hero.Level}   ", ConsoleColor.White);
        Ui.Write("HP ");
        Ui.Write(Ui.Bar(hero.Health, hero.MaxHealth, 14), Ui.HealthColor(hero.Health, hero.MaxHealth));
        Ui.Write($" {hero.Health:0}/{hero.MaxHealth:0}");
        if (hero is Mage)
        {
            Ui.Write("   Mana ", ConsoleColor.DarkCyan);
            Ui.Write(Ui.Bar(hero.Mana, hero.MaxMana, 10), ConsoleColor.Blue);
            Ui.Write($" {hero.Mana:0}/{hero.MaxMana:0}");
        }
        Ui.Line();
        if (hero.Level >= Character.MaxLevel) Ui.Line("  XP: at the peak of the Ashlands", ConsoleColor.DarkGray);
        else Ui.Line($"  XP {Ui.Bar(hero.Experience, hero.XpForNext, 14)} {hero.Experience}/{hero.XpForNext}", ConsoleColor.DarkGray);
    }

    /// <summary>Inventory: equip, use, drop.</summary>
    public static void Pack()
    {
        while (true)
        {
            var hero = Game.Hero;
            Ui.Header($"{hero.Name}'s Pack");
            Ui.Line($"  Slots {hero.UsedSlots}/{Character.InventoryCapacity}     Gold {hero.Gold}", ConsoleColor.DarkGray);

            Ui.Sub("Equipment");
            var active = hero.ActiveWeapon;
            var options = new List<string>();
            var actions = new List<Action>();

            foreach (var item in hero.Weapons)
            {
                string mark = item == active ? "  ◄ in hand" : item is Weapon w && !hero.CanWield(w) ? "  (can't wield)" : "";
                options.Add(ItemForge.Describe(item) + mark);
                actions.Add(() => ItemMenu(item));
            }
            var supplies = hero.Supplies.Keys.Select(SupplyCatalog.Get).OrderBy(s => s.Kind).ToList();
            foreach (var supply in supplies)
            {
                options.Add($"{supply.Name} x{hero.SupplyCount(supply.Id)} — {supply.Description}");
                actions.Add(() => UseSupplyOutOfCombat(supply));
            }

            if (options.Count == 0)
            {
                Ui.Note("Your pack is empty.");
                Ui.Pause();
                return;
            }

            int choice = Ui.Choose(options, "Back");
            if (choice < 0) return;
            actions[choice]();
        }
    }

    private static void ItemMenu(IEquipable item)
    {
        var hero = Game.Hero;
        Ui.Line();
        Ui.Line("  " + ItemForge.Describe(item), ConsoleColor.White);
        var options = new List<string>();
        var actions = new List<Action>();

        if (item is Weapon weapon && hero.CanWield(weapon) && hero.ActiveWeapon != weapon)
        {
            options.Add("Fight with this");
            actions.Add(() =>
            {
                hero.EquippedWeapon = weapon;
                Ui.Good($"{hero.Name} now fights with the {weapon.Name}.");
                Ui.Pause();
            });
        }
        options.Add("Drop it");
        actions.Add(() =>
        {
            if (!Ui.Confirm($"Drop the {item.Name} for good?")) return;
            hero.Weapons.Remove(item);
            if (hero.EquippedWeapon == item) hero.EquippedWeapon = null;
        });

        int choice = Ui.Choose(options, "Back");
        if (choice >= 0) actions[choice]();
    }

    private static void UseSupplyOutOfCombat(Supply supply)
    {
        var hero = Game.Hero;
        if (!supply.OutOfCombat)
        {
            Ui.Note(supply.Kind == SupplyKind.Material ? "A crafting material — someone in the Ashlands will want it." : "That is only useful in a fight.");
            Ui.Pause();
            return;
        }
        if (supply.Id == SupplyCatalog.ManaDraught && hero is not Mage)
        {
            Ui.Note("It tastes of tin and does nothing for you. Only a Mage can use this.");
            Ui.Pause();
            return;
        }

        hero.UseSupply(supply.Id);
        switch (supply.Id)
        {
            case SupplyCatalog.HealingPotion: Ui.Good($"Restored {hero.Restore(50):0} health."); break;
            case SupplyCatalog.GreaterHealingPotion: Ui.Good($"Restored {hero.Restore(120):0} health."); break;
            case SupplyCatalog.ManaDraught: Ui.Good($"Restored {hero.RestoreMana(60):0} mana."); break;
        }
        Ui.Pause();
    }

    /// <summary>Quest journal.</summary>
    public static void Journal()
    {
        Ui.Header("Journal");
        var world = Game.World;
        var active = QuestLog.Active();
        Ui.Sub("Active");
        if (active.Count == 0) Ui.Note("Nothing on your plate. Sera Voss at the Dustfall Inn usually knows what needs doing.");
        foreach (var quest in active)
        {
            Ui.Line($"  {quest.Title}", ConsoleColor.Cyan);
            Ui.Note($"    {quest.Summary}");
            foreach (var objective in quest.Objectives)
            {
                bool done = objective.IsComplete(world, quest.Id);
                Ui.Line($"    {(done ? "✔" : "·")} {objective.Describe(world, quest.Id)}", done ? ConsoleColor.Green : ConsoleColor.Gray);
            }
            if (QuestLog.ObjectivesMet(quest.Id) && quest.TurnIn.Length > 0)
                Ui.Line($"    → Hand in: {quest.TurnIn}", ConsoleColor.Yellow);
        }

        var done2 = QuestLog.Completed();
        if (done2.Count > 0)
        {
            Ui.Sub("Completed");
            foreach (var quest in done2) Ui.Line($"  ✔ {quest.Title}", ConsoleColor.DarkGray);
        }
        Ui.Pause();
    }

    /// <summary>Reputation and a summary of how the world currently stands.</summary>
    public static void Standing()
    {
        var world = Game.World;
        Ui.Header("The Keeper's Standing");
        int score = world.GetReputation();
        Ui.Write("  The Ashlands: ");
        Ui.Write($"{world.Tier()}", ConsoleColor.Yellow);
        int? next = WorldState.NextTierAt(score);
        Ui.Line(next.HasValue ? $"  ({score} — next tier at {next})" : $"  ({score})", ConsoleColor.DarkGray);
        Ui.Note("  Reputation belongs to the Keeper, not to one hero. Prices, secrets and trust follow all of them.");

        Ui.Sub("The world as your heroes have made it");
        foreach (string line in WorldSummary()) Ui.Narrate("• " + line);
        Ui.Pause();
    }

    /// <summary>Plain-language list of the consequences currently in effect.</summary>
    public static List<string> WorldSummary()
    {
        var w = Game.World;
        var lines = new List<string>();
        string By(string flag) => w.DoneBy(flag) is string name ? $" ({name})" : "";

        if (w.Has(WorldFlags.AshlandsScavengerBounty)) lines.Add($"The scavenger packs near Cinder's Rest were thinned{By(WorldFlags.AshlandsScavengerBounty)}.");
        if (w.Has(WorldFlags.ScorchedPathPatrolCleared)) lines.Add($"The bandit patrol on the Scorched Path was broken{By(WorldFlags.ScorchedPathPatrolCleared)}.");
        if (w.Has(WorldFlags.DrenMorrowAllied)) lines.Add($"Dren Morrow was saved and now drinks at the Dustfall Inn{By(WorldFlags.DrenMorrowAllied)}.");
        if (w.Has(WorldFlags.DrenMorrowBetrayed)) lines.Add($"Dren Morrow was handed over, and quietly collected by strangers{By(WorldFlags.DrenMorrowBetrayed)}.");
        if (w.Has(WorldFlags.HammerReturned)) lines.Add($"Korin has his apprentice's hammer back{By(WorldFlags.HammerReturned)}.");
        if (w.Has(WorldFlags.ThreshDefeated)) lines.Add($"Commander Thresh is dead and his bandits scattered; the road is open{By(WorldFlags.ThreshDefeated)}.");
        if (w.Has(WorldFlags.ThreshAlly)) lines.Add($"Commander Thresh turned on his paymasters and watches the Court for the Keeper{By(WorldFlags.ThreshAlly)}.");
        else if (w.Has(WorldFlags.ThreshNegotiated)) lines.Add($"Commander Thresh pulled his men off the road without a blade drawn{By(WorldFlags.ThreshNegotiated)}.");
        if (w.Has(WorldFlags.ThreshLost)) lines.Add("The Court's Sentinels reached Thresh before help did.");
        if (w.Has(WorldFlags.ThreshSaved)) lines.Add($"Thresh's camp survived the Court's reprisal{By(WorldFlags.ThreshSaved)}.");
        if (w.Has(WorldFlags.EmberSealBroken)) lines.Add($"The sealed chamber under the Emberveil academy stands open{By(WorldFlags.EmberSealBroken)}.");
        if (w.Has(WorldFlags.EmberArtifactSecured)) lines.Add($"The Emberveil Focus is in the Keeper's hands{By(WorldFlags.EmberArtifactSecured)}.");
        if (w.Has(WorldFlags.EmberArtifactLeft)) lines.Add("The Emberveil Focus was left on its plinth, behind a broken seal.");
        if (w.Has(WorldFlags.AshlandsCourtExposed)) lines.Add("The Keeper knows the name: the Hollow Court.");
        else if (w.Has(WorldFlags.AshlandsCourtFound)) lines.Add("Someone organised is at work in the Ashlands. They wear a crown with no top.");
        if (w.Has(WorldFlags.CinderHandDefeated)) lines.Add($"The Cinder Hand fell at the Ashen Throne{By(WorldFlags.CinderHandDefeated)}.");
        if (w.Has(WorldFlags.AshenThroneSat)) lines.Add($"A hero of the Keeper sat on the Ashen Throne{By(WorldFlags.AshenThroneSat)}.");
        if (w.Has(WorldFlags.AshenThroneRefused)) lines.Add($"A hero of the Keeper walked away from the Ashen Throne{By(WorldFlags.AshenThroneRefused)}.");
        if (Prices.RoadIsOpen) lines.Add("Caravans are using the Scorched Path again. Supplies are cheaper for everyone.");
        foreach (var contract in Contracts.All.Where(c => Contracts.Done(c.Id)))
            lines.Add($"{contract.WorldEffect}{By(contract.Flag)}");
        if (lines.Count == 0) lines.Add("Nothing yet. The Ashlands have not noticed you.");
        return lines;
    }

    /// <summary>The Keeper's Chronicle: recent events across all heroes.</summary>
    public static void Chronicle()
    {
        Ui.Header("The Keeper's Chronicle");
        var log = Game.World.EventLog;
        if (log.Count == 0) Ui.Note("Nothing has been written yet.");
        foreach (string entry in log.TakeLast(22)) Ui.Narrate("· " + entry);
        if (log.Count > 22) Ui.Note($"  ({log.Count - 22} older entries)");
        Ui.Pause();
    }

    /// <summary>Roster screen: switch hero or bond a new one.</summary>
    public static void Roster()
    {
        var roster = Game.Roster;
        Ui.Header("The Keeper's Roster");
        Ui.Narrate("You can hold one hero at a time. The others wait where you left them — and live in the world the rest have made.");
        Ui.Line($"\n  Bonds: {roster.Heroes.Count}/{roster.UnlockedSlots} (of {roster.MaxSlots})", ConsoleColor.DarkGray);
        if (roster.NextUnlockHint != null) Ui.Note("  " + roster.NextUnlockHint);

        var options = new List<string>();
        foreach (var hero in roster.Heroes)
        {
            string where = Travel.NameOf(hero.LocationId);
            string mark = hero == roster.Active ? "  ◄ guiding now" : "";
            options.Add($"{hero.Name} the {hero.CharacterType}, level {hero.Level} — {where}{mark}");
        }
        if (roster.CanAdd) options.Add($"Bond a new hero (starts at level {roster.StartingLevelForNewHero})");

        int choice = Ui.Choose(options, "Back");
        if (choice < 0) return;

        if (choice < roster.Heroes.Count)
        {
            var chosen = roster.Heroes[choice];
            if (chosen == roster.Active) return;
            roster.SetActive(chosen);
            Ui.Line();
            Ui.Vision($"You let go of one heartbeat and take hold of another. {chosen.Name} straightens, as if remembering something.");
            Game.World.Log($"The Keeper turned to {chosen.Name}.");
            Game.Save();
            Ui.Pause();
            return;
        }

        BondNewHero();
    }

    /// <summary>Creates a new hero at the Keeper's catch-up level and makes them active.</summary>
    public static void BondNewHero()
    {
        var roster = Game.Roster;
        int level = roster.StartingLevelForNewHero;
        var hero = CharacterFactory.CreateCharacter();
        hero.SetLevel(level);
        CharacterFactory.GiveStartingSupplies(hero);
        roster.TryAdd(hero);
        if (hero.UnspentPoints > 0)
        {
            Ui.Line();
            Ui.Narrate($"{hero.Name} arrives with some of the Keeper's experience already in their bones. Shape it.");
            SpendPoints();
            hero.RestoreFully();
        }
        Game.World.Log($"The Keeper bonded with {hero.Name} the {hero.CharacterType}.");

        Ui.Line();
        Ui.Vision($"The bond takes. {hero.Name} arrives in Cinder's Rest already carrying some of what you have learned (level {hero.Level}).");
        Ui.World("This hero walks into the world your others have already changed. Watch who remembers what.");
        Game.Save();
        Ui.Pause();
    }

    /// <summary>The Keeper's Vault: equipment shared between all heroes.</summary>
    public static void Vault()
    {
        while (true)
        {
            var hero = Game.Hero;
            var vault = Game.World.Vault;
            Ui.Header("The Keeper's Vault");
            Ui.Narrate("A locked chest under the stairs of the Dustfall Inn. Sera keeps the key and asks no questions. Anything left here can be collected by any of your heroes.");
            Ui.Line($"\n  Pack slots {hero.UsedSlots}/{Character.InventoryCapacity}", ConsoleColor.DarkGray);

            var options = new List<string>();
            foreach (var item in vault) options.Add("Take:  " + ItemForge.Describe(item));
            foreach (var item in hero.Weapons) options.Add("Store: " + ItemForge.Describe(item));
            if (options.Count == 0)
            {
                Ui.Note("The chest is empty and so is your pack.");
                Ui.Pause();
                return;
            }

            int choice = Ui.Choose(options, "Back");
            if (choice < 0) return;

            if (choice < vault.Count)
            {
                var item = vault[choice];
                if (hero.TryAddItem(item)) vault.Remove(item);
                else
                {
                    Ui.Bad("Your pack is full.");
                    Ui.Pause();
                }
            }
            else
            {
                var item = hero.Weapons[choice - vault.Count];
                hero.Weapons.Remove(item);
                if (hero.EquippedWeapon == item) hero.EquippedWeapon = null;
                vault.Add(item);
            }
            Game.Save();
        }
    }
}
