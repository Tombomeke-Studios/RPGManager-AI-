using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Combat;
using RPGManagerLib.Quests;
using RPGManagerLib.UI;
using RPGManagerLib.Worlds;

namespace RPGManagerLib.Adventure;

/// <summary>
/// The Ashen Throne: the pilgrim stair, the Court's camp, the Cinder Hand, and the end of the Ashlands arc.
/// Almost everything here changes depending on what the Keeper's heroes did earlier.
/// </summary>
public static class AshenThroneLocation
{
    private static bool ThreshUnresolved =>
        !Game.Has(WorldFlags.ThreshDefeated) && !Game.Has(WorldFlags.ThreshNegotiated);

    private static bool ThreshFightsBesideYou =>
        Game.Has(WorldFlags.ThreshAlly) && !Game.Has(WorldFlags.ThreshLost);

    /// <summary>Shows the location once. Returns false when the player quits.</summary>
    public static bool Visit()
    {
        string description = "A plateau of black glass above the ash-line. At its peak, a dragon's skeleton the size of a cathedral stands with wings half-spread, as if it died in the act of landing and nobody has told it. Between its foreclaws is a seat of fused stone.";
        if (Game.Has(WorldFlags.CinderHandDefeated)) description += " The Court's tents are slumped and empty. The wind has already started taking them apart.";
        else description += " Lantern-light moves among tents near the summit. Black and gold.";

        LocationMenu.Banner("The Ashen Throne", description);
        Gossip.Tell(Gossip.Throne, 1);

        var options = new List<MenuOption>();
        if (!Game.Has(WorldFlags.CinderHandDefeated))
            options.Add(new MenuOption("1", "Climb the pilgrim stair", "quest: The Hollow Crown (recommended level 8+)", Climb));
        else
        {
            if (!Game.Has(WorldFlags.AshenThroneSat) && !Game.Has(WorldFlags.AshenThroneRefused))
                options.Add(new MenuOption("1", "Approach the throne", "the seat between the dragon's claws", ThroneChoice));
            options.Add(new MenuOption("2", "Hunt the plateau", "wraiths, golems and Court stragglers (levels 8–15)", () =>
                Encounters.Fight("The Plateau", "Not everything up here left when the Hand fell.",
                    Encounters.PlateauGroup(Game.Hero.Level))));
        }
        return LocationMenu.Show(options);
    }

    private static bool PressOn(string prompt)
    {
        while (true)
        {
            Ui.Line();
            HeroScreens.WriteVitals(Game.Hero);
            int choice = Ui.Choose(new List<string> { prompt, "Open your pack first", "Go back down" });
            if (choice == 0) return true;
            if (choice == 2) return false;
            HeroScreens.Pack();
        }
    }

    private static void Climb()
    {
        var hero = Game.Hero;

        // The Court moved on Thresh and nobody answered the horn.
        if (QuestLog.IsActive(QuestLog.HollowReprisal) && !Game.Has(WorldFlags.ThreshSaved) && !Game.Has(WorldFlags.ThreshLost))
        {
            Game.Set(WorldFlags.ThreshLost);
            QuestLog.Complete(QuestLog.HollowReprisal, hero);
            Ui.Header("The Pilgrim Stair");
            Ui.Narrate("From the first landing you can see the Scorched Path, and the north ridge, and the smoke going up from it. Nobody answered Thresh's horn.");
            Ui.World("Commander Thresh is dead. The Court does not leave loose ends.");
            Ui.Pause();
        }

        // --- Stage 1: the stair ---
        Ui.Header("The Pilgrim Stair");
        if (!Game.Has(WorldFlags.ThroneStairCleared))
        {
            Ui.Narrate("Nine hundred steps, cut by people who believed their god was waiting at the top. Every thirtieth step has a small dragon carved into the riser. Someone has methodically chiselled the head off each one.");
            BattleOutcome fight;
            if (ThreshUnresolved)
            {
                Ui.Narrate("At the first landing a broad, grey, tidy man is waiting with his shield already on his arm. Behind him, one of his Enforcers.");
                Ui.Say("Thresh", "The Guild pays double to hold this stair. I don't ask why. Turn around. I'd rather not, but I will.");
                fight = Encounters.Fight("The Pilgrim Stair — Commander Thresh",
                    "He never learned who was paying him. There is no time to explain it now.",
                    Bestiary.Group(Bestiary.Thresh, Bestiary.BanditEnforcer), new BattleOptions { CanFlee = false });
                if (fight == BattleOutcome.Victory)
                {
                    Game.Set(WorldFlags.ThreshDefeated);
                    Game.World.Log($"{hero.Name} killed Commander Thresh on the pilgrim stair.");
                    Ui.Header("The Pilgrim Stair");
                    Ui.Say("Thresh", "Guild's gold... in my coat. Smooth coins. Never did like... the look of them.");
                    Ui.World("Thresh died never knowing who he worked for. The Scorched Path is open.");
                }
            }
            else if (Game.Has(WorldFlags.ThreshDefeated))
            {
                fight = Encounters.Fight("The Pilgrim Stair",
                    "Thresh's men needed a new paymaster, and the Court was hiring. One of his old Enforcers holds the landing beside a Sentinel, and looks like he regrets it already.",
                    Bestiary.Group(Bestiary.CourtSentinel, Bestiary.BanditEnforcer), new BattleOptions { CanFlee = true });
            }
            else
            {
                Ui.World("There are no bandits on the stair. Thresh took his men off the mountain — the Court is short-handed tonight.");
                var sentinel = Bestiary.Group(Bestiary.CourtSentinel);
                fight = Encounters.Fight("The Pilgrim Stair",
                    "A single Sentinel holds the landing where there should have been a dozen hired blades.",
                    sentinel, new BattleOptions { CanFlee = true });
            }
            if (fight != BattleOutcome.Victory) return;
            Game.Set(WorldFlags.ThroneStairCleared);
            Ui.Header("The Pilgrim Stair");
            Ui.Narrate("The landing is yours. Above, the stair bends toward the lantern-light.");
        }
        else
        {
            Ui.Narrate("The first landing is clear. The headless dragons watch you climb.");
        }
        if (!PressOn("Climb toward the camp")) return;

        // --- Stage 2: the camp ---
        Ui.Header("The Court's Camp");
        if (!Game.Has(WorldFlags.ThroneCampCleared))
        {
            Ui.Narrate("Six tents in two exact rows. No fire. No talk. Two Sentinels on a slow, overlapping circuit, like the hands of a clock.");
            var pair = Bestiary.Group(Bestiary.CourtSentinel, Bestiary.CourtSentinel);
            pair[1].TraitUsed = true;
            var fight = Encounters.Fight("The Court's Camp",
                "There is no clever way past a patrol that never looks away. You step into the lantern-light.",
                pair, new BattleOptions { CanFlee = true });
            if (fight != BattleOutcome.Victory) return;
            Game.Set(WorldFlags.ThroneCampCleared);
        }
        Tent();
        if (!PressOn("Walk up to the dragon's bones")) return;

        CinderHandFight();
    }

    private static void Tent()
    {
        var hero = Game.Hero;
        Ui.Header("The Operations Tent");
        Ui.Narrate("The largest tent holds a map table, a locked document case — the lock is now a matter of opinion — and a cot.");

        if (!Game.Has(WorldFlags.AshlandsCourtExposed) || !Game.Has(WorldFlags.ThroneTentRested))
        {
            Ui.Narrate("The papers are in a clean, small hand. You read by the Court's own lantern:");
            Ui.Vision("\"To the Archivist, by the usual road. The Emberveil Focus remains behind its seal. The hermit continues to obstruct. The Hand requests another season and assures the Thrones that the Ashlands operation remains deniable: the road is held by a proxy who believes he serves a guild.\"");
            if (Game.Has(WorldFlags.DrenMorrowBetrayed))
                Vision("\"Asset Morrow recovered through local cooperation. Debriefed. Reassigned to the Mistwood concern, under supervision. He will not run again.\"");
            else if (Game.Has(WorldFlags.DrenMorrowAllied))
                Vision("\"Asset Morrow is lost, presumed turned. Recommend the Hand's codename be retired after this season.\"");
            if (Game.Has(WorldFlags.ThreshAlly))
                Vision("\"The proxy on the road has stopped answering. A corrective team has been dispatched.\"");
            Ui.Vision("\"Seven seals; seven Thrones to hold them once they are turned. The Eighth Throne remains empty, as the First insists, until its proper occupant is brought to sit in it — willingly or otherwise. The First is certain it has woken. The Focus will know it when it comes.\"");
            Ui.Line();
            Ui.Narrate("You read the last paragraph twice. 'Its proper occupant.' 'It has woken.'");
            Ui.Vision("They are not only after an artifact. They have a chair set aside. For you.");
            if (!Game.Has(WorldFlags.AshlandsCourtExposed))
            {
                Game.Set(WorldFlags.AshlandsCourtExposed);
                Game.Set(WorldFlags.AshlandsCourtFound);
                Ui.World("The Keeper knows the name now: the Hollow Court — and knows the Court is looking for the Keeper.");
            }
            Game.World.Log($"{hero.Name} read the Hollow Court's dispatches at the Ashen Throne.");
        }

        if (!Game.Has(WorldFlags.ThroneTentRested))
        {
            Ui.Line();
            if (Ui.Confirm("The cot is right there, and nobody is coming. Rest before the summit? (once only)"))
            {
                Game.Set(WorldFlags.ThroneTentRested);
                hero.RestoreFully();
                Ui.Good($"{hero.Name} sleeps an hour in an enemy's bed and wakes fully restored.");
            }
        }
        Game.Save();
    }

    private static void Vision(string text) => Ui.Vision(text);

    private static void CinderHandFight()
    {
        var hero = Game.Hero;
        bool focus = Game.Has(WorldFlags.EmberArtifactSecured);
        bool knowsWound = Game.Has(WorldFlags.DrenMorrowAllied);

        Ui.Header("The Dragon's Bones");
        Ui.Narrate("Under the arch of the ribs, beside the fused-stone seat, a figure in a travelling cloak is warming one hand at a brazier. The other hand does not need warming: it is sheathed to the elbow in a gauntlet of black iron and amber that glows like a banked forge.");
        Ui.Narrate("She pushes back her hood. No mask. A tired, intelligent, entirely ordinary face — the face of someone's favourite aunt.");
        Ui.Say("The Cinder Hand", "So you're what's been breaking my arrangements. I expected something taller.");
        Ui.Say("The Cinder Hand", "Not you. You're a perfectly good " + hero.CharacterType.ToLowerInvariant() + ". I mean the one standing behind you. Hello, Keeper. We've kept a chair for you for a very long time.");
        if (focus)
        {
            Ui.Narrate("Her eyes go to your pack, and for the first time she looks less than comfortable.");
            Ui.Say("The Cinder Hand", "And you've brought the Focus. How thoughtful. I'll have it back when you're done dying.");
        }
        else
        {
            Ui.Narrate("The gauntlet pulses in time with something far below — something on a plinth, in an open room.");
            Ui.Say("The Cinder Hand", "You left it in the chamber. Sentimental. My people cut through an hour after you walked away.");
        }
        if (knowsWound) Ui.World("Dren told you: she favours her left side. An old wound, under the ribs. You know where to strike first.");
        if (ThreshFightsBesideYou)
        {
            Ui.Narrate("Boots on the stair behind you. Commander Thresh comes up onto the plateau, shield on his arm, breathing hard, and plants himself at your shoulder.");
            Ui.Say("Thresh", "You're the 'Guild', then. I've wanted a word about my wages.");
            Ui.World("Thresh fights beside you — because of what your heroes chose on the Scorched Path.");
        }

        var options = new BattleOptions
        {
            CanFlee = false,
            FocusDenied = focus,
            OpeningExposedTurns = knowsWound ? 3 : 0,
            AllyName = ThreshFightsBesideYou ? "Commander Thresh" : null,
            AllyDamage = 18
        };
        var outcome = Encounters.Fight("The Cinder Hand",
            "The brazier goes out. The gauntlet does not.",
            Bestiary.Group(Bestiary.CinderHand), options);
        if (outcome != BattleOutcome.Victory) return;

        Game.Set(WorldFlags.CinderHandDefeated);
        Game.World.Log($"{hero.Name} defeated the Cinder Hand at the Ashen Throne.");

        Ui.Header("The Dragon's Bones");
        Ui.Narrate("The gauntlet cracks. Light leaks out of it and does not come back. The Cinder Hand sits down against a rib the size of a ship's keel, and looks, mostly, annoyed.");
        Ui.Say("The Cinder Hand", "Two years. Two years of perfectly good work. You've no idea what you've — no. You do, don't you. You read my desk.");
        Ui.Say("The Cinder Hand", "Then understand this, Keeper: I am the smallest of them. I'm a clerk with a glove. There are seven more seals and eight chairs, and the First has been setting the table since before this dragon died. You were always going to come to dinner.");
        Ui.Narrate("She closes her eyes. The plateau is very quiet. Far below, for the first time in two years, there are no lanterns on the mountain.");
        Ui.Line();
        Rewards.GiveItem(Story.ByClass("cinder_brand", "hollow_scepter", "thronewing_bow"));
        Ui.Pause();

        ThroneChoice();
        Ui.Line();
        Ui.Note("  (Sera will want to hear how this ended. Ask her for work at the Dustfall Inn.)");
        Game.Save();
        Ui.Pause();
    }

    private static void ThroneChoice()
    {
        var hero = Game.Hero;
        Ui.Header("The Ashen Throne");
        Ui.Narrate("Between the dragon's foreclaws: a seat of black stone, melted and re-set, with a hollow worn into it as if by long use. The locals say whoever sits there inherits the dragon's curse.");
        Ui.Narrate("It does not look cursed. It looks comfortable. It looks, in fact, exactly your size — though you do not have a size.");

        int choice = Ui.Choose(new List<string>
        {
            "Sit.",
            "Walk away. You have had enough of chairs that other people set out for you."
        });

        if (choice == 0)
        {
            Game.Set(WorldFlags.AshenThroneSat);
            Ui.Line();
            Ui.Vision($"{hero.Name} sits. The stone is warm.");
            Ui.Vision("And you remember. Not everything. A plateau — this plateau — and a dragon coming down out of a red sky. A woman with a staff at your side: the Chair of Applied Flame, your hero, two hundred years ago. You leaned on her choice. She burned the sky to kill a god, and the fire came down the mountain, and you could not hold on to her.");
            Ui.Vision("You were here. The Emberveil Kingdom did not destroy itself. It had help. It had you.");
            Ui.Vision("The dragon's skull turns, very slightly, on its dead neck. 'KEEPER,' it says, in no voice at all. 'AGAIN? WELL. SIT, THEN. BUT THIS TIME, LEARN TO LET GO.'");
            Ui.Line();
            switch (hero)
            {
                case Warrior: hero.Might += 3; Ui.Gain("The Dragon's Brand: Might +3 (permanent)."); break;
                case Mage: hero.Arcana += 3; Ui.Gain("The Dragon's Brand: Arcana +3 (permanent)."); break;
                default: hero.Perception += 3; Ui.Gain("The Dragon's Brand: Perception +3 (permanent)."); break;
            }
            hero.BonusMaxHealth -= 15;
            hero.Health = Math.Min(hero.Health, hero.MaxHealth);
            Ui.Bad("The Dragon's Curse: maximum Health −15 (permanent). Something in the bones has taken its tithe.");
            Ui.World("A hero of the Keeper has sat on the Ashen Throne. The throne is cold now; it will not answer another.");
            Game.World.Log($"{hero.Name} sat on the Ashen Throne, and the Keeper remembered the fall of Emberveil.");
        }
        else
        {
            Game.Set(WorldFlags.AshenThroneRefused);
            Ui.Line();
            Ui.Narrate($"{hero.Name} turns their back on it and starts down the nine hundred steps. Behind you, very faintly, something enormous and long dead lets out a breath that might be disappointment and might be respect.");
            Ui.Narrate("By the time you reach the Scorched Path the story has outrun you, the way stories do: the Keeper's hero was offered a throne, and laughed, and went home for supper.");
            Rewards.GiveReputation(15);
            Game.World.Log($"{hero.Name} walked away from the Ashen Throne.");
        }
        Game.Save();
    }

    /// <summary>Closing scene with Sera: pays out the final quest and tells the player what their Ashlands became.</summary>
    public static void Epilogue()
    {
        var hero = Game.Hero;
        Ui.Header("The Dustfall Inn");
        Ui.Narrate("Sera listens to all of it without interrupting, which you have never seen her do. When you finish she pours two cups of something that is not on the menu.");
        if (Game.Has(WorldFlags.AshenThroneSat))
            Ui.Say("Sera", "You sat in it. Of course you did. I'd have told you not to and you'd have done it anyway. You look different. Not worse. Just... like you've read the ending of something.");
        else if (Game.Has(WorldFlags.AshenThroneRefused))
            Ui.Say("Sera", "And you walked away from the chair. Half the town already knows. Marrit is telling people you spat on it. I'm not correcting her.");
        Ui.Say("Sera", "The Hollow Court. Eight chairs. And one of them with your name on it. I came to the Ashlands to be somewhere nothing happened.");
        Ui.Say("Sera", "...Well. It's been a while since anything happened that I was on the right side of. To the Keeper, then. And to whichever of you is drinking this.");

        Rewards.CompleteQuest(QuestLog.HollowCrown);
        Game.Set(WorldFlags.AshlandsComplete);
        Game.Set(WorldFlags.EpilogueSeen);
        Game.World.Log("The Ashlands arc is complete. The road to the Mistwood lies open.");
        Ui.Pause();

        Ui.Header("The Ashlands — As You Leave Them");
        foreach (string line in HeroScreens.WorldSummary()) Ui.Narrate("• " + line);
        Ui.Line();
        Ui.Narrate("Heroes of the Keeper:");
        foreach (var h in Game.Roster.Heroes)
            Ui.Line($"    {h.Name} the {h.CharacterType}, level {h.Level}", ConsoleColor.Cyan);
        Ui.Line();
        Ui.Vision("North of the Ashlands the ground stops being grey. There is a forest there that was old when Emberveil was young, and something in it has started cutting down trees that had contracts.");
        Ui.Vision("The Mistwood is waiting. So are seven seals, and eight chairs.");
        Ui.Line();
        Ui.Line("  ══════ END OF THE ASHLANDS ══════", ConsoleColor.DarkYellow);
        Ui.Note("  You can keep playing: finish side quests, level other heroes, or send a different class down into the ruins to find what it kept for them.");
        Game.Save();
        Ui.Pause();
    }
}
