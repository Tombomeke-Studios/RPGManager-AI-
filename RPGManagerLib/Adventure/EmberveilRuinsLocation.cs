using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Combat;
using RPGManagerLib.Core;
using RPGManagerLib.Dialogue;
using RPGManagerLib.Items;
using RPGManagerLib.Items.Weapons;
using RPGManagerLib.Quests;
using RPGManagerLib.UI;
using RPGManagerLib.Worlds;

namespace RPGManagerLib.Adventure;

/// <summary>
/// The Emberveil Ruins: Lira's camp, the vents, and the descent to the sealed chamber —
/// which each class opens in its own way.
/// </summary>
public static class EmberveilRuinsLocation
{
    /// <summary>Might a Warrior needs to break the east wall.</summary>
    public const int WallMight = 15;

    /// <summary>Perception an Archer needs to find the vent shaft.</summary>
    public const int ShaftPerception = 12;

    private static string ClassCacheFlag => Story.ByClass(WorldFlags.RuinsCacheWarrior, WorldFlags.RuinsCacheMage, WorldFlags.RuinsCacheArcher);

    /// <summary>Shows the location once. Returns false when the player quits.</summary>
    public static bool Visit()
    {
        string description = "The greatest academy of the Emberveil Kingdom, two centuries after it burned. Above ground: a maze of broken walls open to the sky. Below: lecture halls the fire sealed rather than destroyed. Vents in the rock breathe heat like something asleep.";
        if (Game.Has(WorldFlags.EmberSealBroken)) description += " Far below, a door that was shut for two hundred years stands open, and the whole ruin feels like it is holding its breath.";

        LocationMenu.Banner("Emberveil Ruins", description);
        Gossip.Tell(Gossip.Ruins, 1);

        var options = new List<MenuOption>
        {
            new("1", "Lira's camp", "the hermit at the ruin's edge", Lira.Visit),
            new("2", "Search the outer vents", "sprites, beetles, hounds and worse (levels 3–12)", Vents),
        };
        if (QuestLog.Started(QuestLog.EchoesInTheRuins))
            options.Add(new MenuOption("3", "Descend into the academy", Game.Has(WorldFlags.EmberSealBroken) ? "the way down is known" : "quest: Echoes in the Ruins (recommended level 5+)", Descend));
        return LocationMenu.Show(options);
    }

    private static void Vents()
    {
        Encounters.Fight("The Outer Vents",
            "Heat-shimmer over cracked black rock. Then one of the shimmers turns to look at you.",
            Encounters.RuinsGroup(Game.Hero.Level));
    }

    // ------------------------------------------------------------------ the descent

    /// <summary>Asks whether to keep going. Returns false to retreat.</summary>
    private static bool PressOn(string prompt)
    {
        while (true)
        {
            var hero = Game.Hero;
            Ui.Line();
            HeroScreens.WriteVitals(hero);
            int choice = Ui.Choose(new List<string> { prompt, "Open your pack first", "Retreat to the surface" });
            if (choice == 0) return true;
            if (choice == 2) return false;
            HeroScreens.Pack();
        }
    }

    private static void Descend()
    {
        var hero = Game.Hero;

        // --- Level 1: the collapsed surface ---
        Ui.Header("The Collapsed Surface");
        if (!Game.Has(WorldFlags.RuinsSurfaceCleared))
        {
            Ui.Narrate("You pick your way between walls that end in mid-air. Someone carved 'KNOWLEDGE IS A FLAME' over an arch. Someone else — later, shakily — scratched 'YES' underneath.");
            var fight = Encounters.Fight("The Collapsed Surface",
                "Two sprites spill out of a broken chimney like embers out of a kicked fire.",
                Bestiary.Group(Bestiary.EmberSprite, Bestiary.EmberSprite), new BattleOptions { CanFlee = true });
            if (fight != BattleOutcome.Victory) return;
            Game.Set(WorldFlags.RuinsSurfaceCleared);
            Ui.Header("The Collapsed Surface");
            Ui.Narrate("Behind the chimney, a stair leads down. The air coming up it is cooler than the air outside. That should be reassuring, and is not.");
        }
        else
        {
            Ui.Narrate("The surface is quiet. The stair down waits behind the broken chimney where your Keeper's hands cleared it.");
            if (Rng.Chance(0.25))
            {
                var fight = Encounters.Fight("The Collapsed Surface", "A lone sprite has drifted back to the warm stones.",
                    Bestiary.Group(Bestiary.EmberSprite));
                if (fight == BattleOutcome.Defeat) return;
                Ui.Header("The Collapsed Surface");
            }
        }
        if (!PressOn("Go down the stair")) return;

        // --- Level 2: the lecture halls ---
        Ui.Header("The Lecture Halls");
        if (!Game.Has(WorldFlags.RuinsHallsCleared))
        {
            Ui.Narrate("The halls are intact. Benches in rows. Slates still on the desks. At the front of the largest hall, a figure of ash and dull fire stands at a lectern, gesturing at a board that crumbled a century ago.");
            Ui.Narrate("It is giving a lecture. It has been giving this lecture for two hundred years.");
            bool listened = false;
            if (!Game.Has(WorldFlags.WraithLectureHeard))
            {
                int choice = Ui.Choose(new List<string>
                {
                    "Sit down on a bench and listen.",
                    "Attack while its back is turned."
                });
                if (choice == 0)
                {
                    listened = true;
                    Game.Set(WorldFlags.WraithLectureHeard);
                    Ui.Line();
                    Ui.Vision("\"...and so the Compact is not a treaty, whatever the heralds say. It is a mechanism. Seven seals, seven kingdoms, each seal fed by the land it sits in. Remove a kingdom and the seal starves. Starve enough seals and what they hold shut will open.\"");
                    Ui.Vision("\"What do they hold shut? An excellent question, and the one this academy was founded to never, ever answer in writing. See me after.\"");
                    Ui.Narrate("The wraith turns to take questions. It sees you. For a moment its face is a person's face — puzzled, polite — and then it remembers that it is on fire.");
                    Rewards.GiveXp(40);
                    Rewards.GiveSupply(SupplyCatalog.AcademyPage);
                    Game.World.Log($"{hero.Name} sat through a dead professor's lecture on the Ancient Compact.");
                    Ui.Pause();
                }
            }

            var fight = listened
                ? Encounters.Fight("The Lecture Halls", "\"Class,\" says the wraith, in a voice like a furnace door, \"is dismissed.\"",
                    Bestiary.Group(Bestiary.AshWraith), new BattleOptions { CanFlee = true })
                : Encounters.Fight("The Lecture Halls", "It wheels on you mid-sentence. A sprite, drawn by the noise, comes skittering down the aisle.",
                    Bestiary.Group(Bestiary.AshWraith, Bestiary.EmberSprite), new BattleOptions { CanFlee = true });
            if (fight != BattleOutcome.Victory) return;
            Game.Set(WorldFlags.RuinsHallsCleared);
            Ui.Header("The Lecture Halls");
            Ui.Narrate("The ash settles on the lectern. You have the uncomfortable feeling of having interrupted someone.");
        }
        else
        {
            Ui.Narrate("The great hall is empty now. The lectern stands in a drift of fine ash. Nobody is teaching.");
        }
        if (!PressOn("Take the deep stair")) return;

        // --- Level 3: the Court's dig ---
        Ui.Header("The Deep Stair");
        if (!Game.Has(WorldFlags.RuinsDigTeamDefeated))
        {
            Ui.Narrate("Lanterns. New ones — good glass, clean oil. Timber props shoring up a side tunnel that someone has been cutting toward the sealed chamber, patiently, for a long time. Crates stencilled with nothing at all.");
            Ui.Narrate("And one guard: black plate chased with gold, a mask without features, standing so still you took it for a statue.");
            var fight = Encounters.Fight("The Deep Stair",
                "It does not challenge you. It does not speak. It draws, and comes on.",
                Bestiary.Group(Bestiary.CourtSentinel), new BattleOptions { CanFlee = true });
            if (fight != BattleOutcome.Victory) return;
            Game.Set(WorldFlags.RuinsDigTeamDefeated);
            Ui.Header("The Deep Stair");
            Ui.Narrate("Their tunnel stops a hand's breadth short of the chamber wall, where the stone turns glassy and the picks simply skated off. Two years of digging, and the seal held.");
        }
        else
        {
            Ui.Narrate("The Court's dig is abandoned: props, dead lanterns, a tunnel that ends in glassy stone.");
        }
        if (!PressOn("Approach the sealed chamber")) return;

        Seal();
    }

    private static void Seal()
    {
        var hero = Game.Hero;
        Ui.Header("The Sealed Chamber");

        if (Game.Has(WorldFlags.EmberSealBroken))
        {
            Ui.Narrate($"The seal is already broken{(Story.OtherHero(WorldFlags.EmberSealBroken) is string other ? $" — {other}'s work" : "")}. The chamber stands open.");
            if (!Game.Has(ClassCacheFlag))
            {
                Ui.Narrate("But the academy hid more than one thing down here, and not all of it behind the same door.");
                if (TryClassRoute()) ClassCache();
            }
            else
            {
                Ui.Narrate("You have already taken what this place kept for someone like you.");
            }
            if (Game.Has(WorldFlags.EmberArtifactLeft) && !Game.Has(WorldFlags.CinderHandDefeated))
            {
                Ui.Narrate("The Emberveil Focus still sits on its plinth, turning slowly, patient as a held breath.");
                if (Ui.Confirm("Take the Focus after all?")) TakeFocus();
            }
            Game.Save();
            Ui.Pause();
            return;
        }

        Ui.Narrate("A door of fused black glass, without handle or hinge. Lines of dull red light crawl across it in patterns that hurt to follow. This is the seal: the last working thing the Emberveil mages ever made.");
        if (!TryClassRoute())
        {
            Ui.Pause();
            return;
        }

        Game.Set(WorldFlags.EmberSealBroken);
        Game.World.Log($"{hero.Name} broke into the sealed chamber beneath the Emberveil academy.");
        Ui.Pause();

        Ui.Header("The Sealed Chamber");
        Ui.Narrate("A round room, perfectly preserved. Shelves of instruments. A workbench. And at the centre, on a plinth, a lens of amber the size of two cupped hands, turning slowly in the air with nothing holding it.");
        Ui.Narrate("The workbench is wrong. Everything else here is two hundred years old. The tools on the bench are new: fine steel picks, a jeweller's loupe, a ledger. Someone got a hand through — a crack, a gap, long enough to work at the seal from inside before it closed on them. There is a glove on the floor. There is still a hand in it.");
        Ui.Narrate("The ledger's cover is stamped in black and gold: a crown with no top.");
        if (!Game.Has(WorldFlags.AshlandsCourtFound))
        {
            Game.Set(WorldFlags.AshlandsCourtFound);
            Ui.World("Someone organised, wealthy and very patient wants what is in this room.");
        }
        if (!Game.Has(WorldFlags.HasCourtSigil))
        {
            Game.Set(WorldFlags.HasCourtSigil);
            Ui.Gain("Key item: Court Sigil");
        }
        Ui.Line();
        Ui.Vision("The amber lens turns toward you. Not toward the hero. Toward you.");
        Ui.Vision("It knows what you are. Something in it is glad, the way a dog is glad at a familiar step on the stair — and you have never been here before.");
        Ui.Line();
        Ui.Line("  The Emberveil Focus. What do you do with it?", ConsoleColor.White);
        int choice = Ui.Choose(new List<string>
        {
            "Take it. Whatever the Court wants this badly, they should not have.",
            "Leave it. It has been safe here for two hundred years; your pack is not safer than this room was."
        });
        if (choice == 0) TakeFocus();
        else
        {
            Game.Set(WorldFlags.EmberArtifactLeft);
            Ui.Narrate("You leave it turning in the dark. On the stair you look back once. It has turned to watch you go.");
            Ui.World("The Focus remains in the chamber — behind a seal that is now broken. The Court's diggers are a hand's breadth away.");
            Game.World.Log($"{hero.Name} left the Emberveil Focus on its plinth.");
        }

        ClassCache();
        Ui.Line();
        Ui.Note("  (Lira will want to hear about this.)");
        Game.Save();
        Ui.Pause();
    }

    private static void TakeFocus()
    {
        Game.World.Flags.Remove(WorldFlags.EmberArtifactLeft);
        Game.Set(WorldFlags.EmberArtifactSecured);
        Ui.Narrate("It settles into your hands like it was waiting to be asked. It is warm, and very slightly heavier than it should be, as if it contained a great deal of something.");
        Ui.Gain("Key item: The Emberveil Focus");
        Ui.World("The Keeper holds the Focus. Whoever comes looking for its power will find the well dry.");
        Game.World.Log($"{Game.Hero.Name} took the Emberveil Focus.");
    }

    /// <summary>Each class gets into the chamber's secrets its own way.</summary>
    private static bool TryClassRoute()
    {
        var hero = Game.Hero;
        Ui.Line();
        switch (hero)
        {
            case Mage:
                if (!hero.LearnedSkills.Contains(SkillBook.Unbinding))
                {
                    Ui.Narrate("You can feel the shape of the working — a knot tied by better mages than you — but you do not know where to pull. Lira would.");
                    Ui.Note("  (Ask Lira to teach you the counter-spell.)");
                    return false;
                }
                Ui.Narrate("You speak the Unbinding the way Lira taught it: not a word of force, but a word of permission. The red lines stop crawling. They listen. Then, one by one, they go out.");
                Ui.Narrate("The glass does not shatter. It sighs, and is simply a doorway.");
                return true;

            case Warrior:
                Ui.Narrate("No handle, no hinge, and no patience for riddles. But the east wall beside it took the worst of the old fire: the stone there is cracked like a dropped plate.");
                if (hero.Might < WallMight)
                {
                    Ui.Narrate("You set your shoulder to it and heave until your vision swims. It shifts — a finger's width — and settles back.");
                    Ui.Note($"  (You need Might {WallMight}. You have {hero.Might}. Come back stronger.)");
                    return false;
                }
                Ui.Narrate("You set your shoulder to it. Something in the wall says no. You disagree. On the third heave two hundred years of masonry lose the argument, and you walk in through the dust.");
                return true;

            default:
                Ui.Narrate("No way through for someone your size. But rooms need air, even sealed ones, and mages who work with fire need it more than most.");
                if (hero.Perception < ShaftPerception)
                {
                    Ui.Narrate("You search the north tower for an hour. There is a draught — you can feel it on your cheek — but you cannot find where it comes from.");
                    Ui.Note($"  (You need Perception {ShaftPerception}. You have {hero.Perception}. Come back sharper.)");
                    return false;
                }
                Ui.Narrate("There: a gap in the north tower wall where one stone is a slightly different grey. Behind it, a ventilation shaft, barely shoulder-wide, dropping into the dark. You go down it like a letter down a chute.");
                return true;
        }
    }

    /// <summary>The class-specific reward, once per class across the whole save.</summary>
    private static void ClassCache()
    {
        var hero = Game.Hero;
        if (Game.Has(ClassCacheFlag)) return;
        Game.Set(ClassCacheFlag);
        Ui.Line();
        switch (hero)
        {
            case Mage:
                Ui.Narrate("Leaning in the corner, exactly where a tired lecturer would leave it: a staff of fire-blackened ash-wood with a band of amber. A brass tag reads PROPERTY OF THE CHAIR OF APPLIED FLAME. DO NOT BORROW.");
                Rewards.GiveItem("last_lecture_staff");
                break;
            case Warrior:
                Ui.Narrate("In the rubble of the wall you broke: an armoury niche, and in it a breastplate set with a scale the size of a shield — dragonshard, taken as a trophy from the cult. It weighs almost nothing. It turns a test-blow like water off a roof.");
                hero.Guard += 3;
                hero.BonusMaxHealth += 20;
                hero.Restore(20);
                Ui.Gain("Dragonshard Plate: Guard +3, maximum Health +20 (permanent).");
                break;
            default:
                Ui.Narrate("Halfway down the shaft, wedged on a ledge beside the bones of someone who had the same idea two centuries ago: a bow carved from ash-wood so old it has gone the colour of smoke. The string is new. It should not be.");
                Rewards.GiveItem("ashcarved_bow");
                break;
        }
        Ui.World($"Each class finds something different down here. Another of your heroes could return for theirs.");
    }
}

/// <summary>
/// Lira the Ashen: ninety years at the edge of the ruins, and not entirely in the present.
/// </summary>
public static class Lira
{
    private static bool WillSpeak =>
        Game.Hero is Mage || Game.Tier >= ReputationTier.Known || Game.Has(WorldFlags.DrenMorrowAllied) || Game.Has(WorldFlags.LiraMet);

    public static void Visit()
    {
        var hero = Game.Hero;
        Ui.Header("Lira's Camp");
        Ui.Narrate("A lean-to of salvaged doors against the outer wall. Bundles of dried herbs. Stacks of slates covered in tiny, furious handwriting. A kettle that is always almost boiling though there is no fire under it.");

        if (!WillSpeak)
        {
            Ui.Narrate("An old woman watches you from the doorway — small, upright, her hair the exact grey of the ash. She looks at your hands, not your face. Then she goes back inside and shuts the door, which is a real door, and has a real bolt.");
            Ui.Note("  (Lira only speaks to mages on sight. Others need the Ashlands to know them — Known reputation — or a friend who can vouch for the Keeper.)");
            Ui.Pause();
            return;
        }

        if (!Game.Has(WorldFlags.LiraMet))
        {
            Game.Set(WorldFlags.LiraMet);
            Ui.Say("Lira", hero is Mage
                ? "There you are. You're late; Magister Oren started without you. No — no, that was... Never mind. You've the craft on you. Sit. Mind the slates."
                : Game.Has(WorldFlags.DrenMorrowAllied)
                    ? "The courier's friend. He said someone would come. He says a great many things, that boy, but that one was true. Sit. Mind the slates."
                    : "You're the one the town talks about. The one who is more than one. I can see it — you're crowded, behind the eyes. Sit. Mind the slates.");
            Ui.World("Lira has let the Keeper in. She will speak to any hero you send from now on.");
        }

        var tree = new DialogueTree();
        tree.Add(new DialogueNode("hub", () => { Ui.Say("Lira", HubLine()); Gossip.Tell(Gossip.Lira); },
            new DialogueOption("\"What's under the academy?\"", "quest") { Condition = () => !QuestLog.Started(QuestLog.EchoesInTheRuins) },
            new DialogueOption("\"I reached the sealed chamber.\"", "chamber") { Condition = () => QuestLog.ObjectivesMet(QuestLog.EchoesInTheRuins) },
            new DialogueOption("\"Teach me the counter-spell.\"", "teach") { Condition = () => hero is Mage && QuestLog.Started(QuestLog.EchoesInTheRuins) && !hero.LearnedSkills.Contains(SkillBook.Unbinding) },
            new DialogueOption("Show her the Court Sigil.", "sigil") { Condition = () => Game.Has(WorldFlags.HasCourtSigil) && !Game.Has(WorldFlags.AshlandsCourtExposed) },
            new DialogueOption("\"I've found some of the academy's pages.\"", "pages") { Condition = () => QuestLog.IsActive(QuestLog.WhatTheAcademyKnew) },
            new DialogueOption("\"Do you need anything from down there?\"", "pages_offer") { Condition = () => QuestLog.Started(QuestLog.EchoesInTheRuins) && !QuestLog.Started(QuestLog.WhatTheAcademyKnew) },
            new DialogueOption("\"Can you do anything with Fire Essence?\"", "coldfire"),
            new DialogueOption("\"Tell me about the war.\"", "war"),
            new DialogueOption("\"Tell me about the Compact.\"", "compact") { Condition = () => QuestLog.IsDone(QuestLog.WhatTheAcademyKnew) || Game.Has(WorldFlags.WraithLectureHeard) },
            new DialogueOption("Leave her to her slates.", null)));

        tree.Add(new DialogueNode("quest", () =>
        {
            Ui.Say("Lira", "A room. The Chair of Applied Flame's own workroom, sealed the night of the fire. I was sixteen, in the sub-basement, cutting feverwort. I heard the seal go up. It sounded like a bell being swallowed.");
            Ui.Say("Lira", "There's a thing in it. The Focus. I've kept people away from it for ninety years. But the last two, someone's been digging — careful people, with good lanterns. They'll get through eventually. Careful people do.");
            Ui.Say("Lira", "So you'll get there first. " + Story.ByClass(
                "You're no mage. Good — the seal expects mages. The east wall beside it cracked in the fire. Be strong enough and it'll remember it's only a wall.",
                "The seal answers to the old discipline. I'll give you the Unbinding. Don't force it. You don't break a seal like that, you excuse it.",
                "You're no mage. Good — the seal expects mages. There's an air shaft from the north tower. Hidden. Be sharp enough and you'll find where the draught comes from."));
            Rewards.AcceptQuest(QuestLog.EchoesInTheRuins);
            if (hero is Mage) Teach();
        }, new DialogueOption("\"I'll go down.\"", "hub")));

        tree.Add(new DialogueNode("teach", Teach, new DialogueOption("\"I have it.\"", "hub")));

        tree.Add(new DialogueNode("chamber", Chamber, new DialogueOption("\"I'll climb it.\"", "hub")));

        tree.Add(new DialogueNode("sigil", () =>
        {
            Ui.Narrate("She takes the disc, turns it to the light, and for a moment looks every one of her hundred and six years.");
            Ui.Say("Lira", "The Hollow Crown. Yes. So that's who has the good lanterns. They call themselves a Court — the Hollow Court. Old families. The ones who lost kingdoms and decided the lesson was to stop needing kingdoms.");
            Ui.Say("Lira", "They want the Focus. I don't know what for. But anything wanted that patiently is wanted for something large.");
            Game.Set(WorldFlags.AshlandsCourtExposed);
            Game.Set(WorldFlags.AshlandsCourtFound);
            Ui.World("The Keeper knows the name now: the Hollow Court.");
            Game.World.Log($"Lira the Ashen named the Hollow Court for {hero.Name}.");
        }, new DialogueOption("\"The Hollow Court.\"", "hub")));

        tree.Add(new DialogueNode("pages_offer", () =>
        {
            Ui.Say("Lira", "Pages. The wraiths carry them — they were scholars, they died holding their work, and the work is still in there with them, in the ash. Three pages would let me finish something I started in... what year is it? No, don't tell me.");
            Rewards.AcceptQuest(QuestLog.WhatTheAcademyKnew);
        }, new DialogueOption("\"I'll keep an eye out.\"", "hub")));

        tree.Add(new DialogueNode("pages", Pages, new DialogueOption("\"Of course.\"", "hub")));

        tree.Add(new DialogueNode("coldfire", Coldfire, new DialogueOption("\"Good to know.\"", "hub")));

        tree.Add(new DialogueNode("war", () =>
        {
            Ui.Say("Lira", "The dragon cult said the academy was stealing fire that belonged to something older. The academy said the cult were peasants worshipping a lizard. They were both right, which is the worst way for a war to start.");
            Ui.Say("Lira", "The cult woke their dragon. The Chair of Applied Flame went up to the plateau to meet it. Whatever she did there killed it on the wing — it's still up there, you've seen the bones — and the backwash came down the mountain and through every lecture hall we had.");
            Ui.Say("Lira", "Magister Oren owes me four silver, incidentally. He'll say he doesn't. Don't believe him.");
        }, new DialogueOption("\"...I'll tell him.\"", "hub")));

        tree.Add(new DialogueNode("compact", () =>
        {
            Ui.Say("Lira", "Seven seals. Seven kingdoms to feed them. That was the Compact — not a peace treaty, a lock. Emberveil's seal was the Focus. When the kingdom died the seal should have starved. It didn't. Something has been feeding it.");
            Ui.Say("Lira", "I think it was waiting. Seals were made by someone, you know. Not by kings. Kings only signed. Someone older stood behind them and held the pen.");
            Ui.Narrate("She looks at you — not at the hero. At you — for an uncomfortably long time.");
            Ui.Say("Lira", "You're crowded behind the eyes, and the Focus is glad to see you. I'm an old woman. I'll leave the arithmetic to you.");
        }, new DialogueOption("Say nothing.", "hub")));

        tree.Run("hub");
        Game.Save();
    }

    private static string HubLine()
    {
        if (Game.Has(WorldFlags.CinderHandDefeated)) return "It's quieter. The ruin, I mean. As if something up on the plateau stopped leaning on it. Was that you? It was. I can tell by how tired you look.";
        if (Game.Has(WorldFlags.EmberArtifactSecured)) return "You have it with you. I can hear it — like a kettle in another room. Keep it away from them. Anything they want that badly is something they should not have.";
        if (Game.Has(WorldFlags.EmberSealBroken)) return "The seal's open. Ninety years. I thought I'd feel something more than tired.";
        if (QuestLog.IsActive(QuestLog.EchoesInTheRuins)) return "Still up here? The room is down. Down is that way. Mind the professor on the second level; he hates latecomers.";
        return "Well? Ask. I've been talking to slates for ninety years; you'll do for a change.";
    }

    private static void Teach()
    {
        var hero = Game.Hero;
        if (hero is not Mage || hero.LearnedSkills.Contains(SkillBook.Unbinding)) return;
        Ui.Line();
        Ui.Narrate("She makes you say it forty times. Thirty-nine of them are wrong. On the fortieth the kettle goes abruptly cold, and she nods.");
        hero.LearnedSkills.Add(SkillBook.Unbinding);
        var skill = SkillBook.Get(SkillBook.Unbinding);
        Ui.Line($"  New skill: {skill.Name} — {skill.Description}", ConsoleColor.Cyan);
    }

    private static void Chamber()
    {
        var hero = Game.Hero;
        Ui.Narrate("You tell her: the workbench, the new tools, the glove with the hand still in it, the lens that turned to look.");
        if (Game.Has(WorldFlags.EmberArtifactSecured))
            Ui.Say("Lira", "Good. You took it. Then they'll come for you instead of for my wall, which I confess is a relief. Keep it close.");
        else
            Ui.Say("Lira", "You left it. Hm. I'd have done the same, sixty years ago. But the door's open now, and they've only a hand's breadth of stone left to cut. Think on it. It's not too late to go back down.");
        Rewards.CompleteQuest(QuestLog.EchoesInTheRuins);

        Ui.Line();
        Ui.Say("Lira", "Now. They'll have felt the seal go — they've someone up on the plateau who'd feel it. The Ashen Throne. That's where their lanterns come from; I've watched the lights go up and down that mountain for two years.");
        Ui.Say("Lira", "There's a pilgrim stair on the east face. The cult cut it, to climb up and visit their dragon. I'll show you where it starts. What you do at the top is your own affair. But if I were young, and crowded behind the eyes, I'd go and cut the hand off the arm.");
        Rewards.AcceptQuest(QuestLog.HollowCrown);
        Ui.World("The Ashen Throne can now be reached — by any of your heroes.");
        if (!Game.Has(WorldFlags.AshlandsCourtExposed) && Game.Has(WorldFlags.HasCourtSigil))
            Ui.Note("  (You are carrying a Court Sigil. Lira might recognise it.)");
    }

    private static void Pages()
    {
        var hero = Game.Hero;
        int carried = hero.SupplyCount(SupplyCatalog.AcademyPage);
        int needed = 3 - Game.World.Count(QuestLog.PagesDelivered);
        if (carried == 0)
        {
            Ui.Say("Lira", "You've none on you. The wraiths below carry them. Any of your... selves... can bring them; I can't tell you apart anyway.");
            return;
        }

        int given = Math.Min(carried, needed);
        hero.UseSupply(SupplyCatalog.AcademyPage, given);
        Game.World.Add(QuestLog.PagesDelivered, given);
        Ui.Narrate($"You hand over {given} scorched page{(given == 1 ? "" : "s")}. She reads them standing up, lips moving.");

        if (!QuestLog.ObjectivesMet(QuestLog.WhatTheAcademyKnew))
        {
            Ui.Say("Lira", $"Yes. Yes, this is Oren's hand. {3 - Game.World.Count(QuestLog.PagesDelivered)} more and I'll have the whole argument.");
            return;
        }

        Ui.Say("Lira", "There. Ninety years I've been missing the middle of that proof. It's about the Compact. Ask me about the Compact — I can finally answer properly.");
        Rewards.CompleteQuest(QuestLog.WhatTheAcademyKnew);
        if (hero is Mage && !hero.LearnedSkills.Contains(SkillBook.Emberlance))
        {
            Ui.Say("Lira", "And this, on the back — someone was scribbling war-spells in the margins during a lecture. Shameful. Here. Learn it.");
            hero.LearnedSkills.Add(SkillBook.Emberlance);
            var skill = SkillBook.Get(SkillBook.Emberlance);
            Ui.Line($"  New skill: {skill.Name} — {skill.Description}", ConsoleColor.Cyan);
        }
        else
        {
            Ui.Say("Lira", "There's a war-spell scribbled on the back. Wasted on you, but it'll be here if one of yours with the craft comes by. Take these instead.");
            Rewards.GiveSupply(SupplyCatalog.FireEssence, 3);
            Rewards.GiveSupply(SupplyCatalog.GreaterHealingPotion, 1);
        }
    }

    private static void Coldfire()
    {
        var hero = Game.Hero;
        Ui.Say("Lira", "Fire Essence? Oh, yes. Any fool can make a blade hot. The trick we were proudest of was turning a flame inside out. Coldfire. It burns like frost, and the things in this ruin hate it — the sprites, the poor professors. Bring me three embers and a weapon.");

        if (hero is Mage && !hero.LearnedSkills.Contains(SkillBook.Emberlance) && QuestLog.IsDone(QuestLog.WhatTheAcademyKnew))
        {
            Ui.Say("Lira", "Oh — and you've the craft. There's a war-spell on the back of those pages your other self brought me. Here. Learn it.");
            hero.LearnedSkills.Add(SkillBook.Emberlance);
            var skill = SkillBook.Get(SkillBook.Emberlance);
            Ui.Line($"  New skill: {skill.Name} — {skill.Description}", ConsoleColor.Cyan);
        }

        var candidates = hero.Weapons.OfType<Weapon>().Where(w => w.Type != WeaponType.STAFF && w.Element == Element.NONE).ToList();
        int essence = hero.SupplyCount(SupplyCatalog.FireEssence);
        int needed = Contracts.Done(Contracts.UnfinishedLecture) ? 0 : 3;
        if (needed == 0) Ui.World("For what your Keeper's mage did for Magister Oren, Lira tempers coldfire for nothing.");
        Ui.Note($"  (Coldfire tempering: {needed} Fire Essence. You have {essence}. A coldfire weapon deals ice damage — strong against sprites and wraiths, and it ignores a wraith's resistance to plain steel.)");
        if (essence < needed || candidates.Count == 0) return;
        if (!Ui.Confirm("Have a weapon tempered now?")) return;

        int choice = Ui.Choose(candidates.Select(ItemForge.Describe).ToList(), "Never mind");
        if (choice < 0) return;
        var weapon = candidates[choice];
        if (needed > 0) hero.UseSupply(SupplyCatalog.FireEssence, needed);
        weapon.Element = Element.ICE;
        if (!weapon.Name.StartsWith("Coldfire ")) weapon.Name = "Coldfire " + weapon.Name;
        Ui.Narrate("She cups the embers in her bare hands and breathes on them — in, not out. The glow turns blue, then white, then is somehow darker than the air around it. When she runs her thumb down the weapon, frost follows.");
        Ui.Good($"It is now the {weapon.Name}.");
    }
}
