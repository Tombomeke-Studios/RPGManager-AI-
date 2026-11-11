using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Combat;
using RPGManagerLib.Dialogue;
using RPGManagerLib.Items;
using RPGManagerLib.Quests;
using RPGManagerLib.UI;
using RPGManagerLib.Worlds;

namespace RPGManagerLib.Adventure;

/// <summary>
/// Small text helpers shared by the location scripts.
/// </summary>
public static class Story
{
    /// <summary>Picks a line by the active hero's class.</summary>
    public static string ByClass(string warrior, string mage, string archer) => Game.Hero switch
    {
        Warrior => warrior,
        Mage => mage,
        _ => archer
    };

    /// <summary>
    /// If a different hero than the active one set the flag, returns that hero's name.
    /// This is what lets NPCs say "your other one did that".
    /// </summary>
    public static string? OtherHero(string flag)
    {
        string? name = Game.World.DoneBy(flag);
        return name != null && name != Game.Hero.Name ? name : null;
    }
}

/// <summary>
/// Cinder's Rest: the starter hub. The Dustfall Inn (Sera, rest, supplies, bounties, the vault),
/// Korin's smithy, and the scavenger-haunted ground outside the gates.
/// </summary>
public static class CindersRestLocation
{
    private static readonly string[] InnStock =
    {
        SupplyCatalog.HealingPotion, SupplyCatalog.ManaDraught, SupplyCatalog.EmberSalve,
        SupplyCatalog.Firebomb, SupplyCatalog.SmokeBomb
    };

    private static readonly string[] SmithyStock =
    {
        "basic_dagger", "basic_sword", "basic_spear", "basic_axe", "broad_sword", "great_sword", "battle_axe",
        "simple_bow", "hunting_bow", "small_quiver", "medium_quiver",
        "basic_staff", "emberglass_staff", "rimewood_staff"
    };

    private static readonly string[] SmithyRareStock = { "great_axe", "war_bow", "big_quiver", "cinderheart_staff" };

    /// <summary>Shows the town once. Returns false when the player quits.</summary>
    public static bool Visit()
    {
        string description = "Volcanic stone, salvaged iron, and a hot spring that smells like a struck match. Ash dusts every sill. On the horizon the Emberveil Ruins glow like a coal that will not go out.";
        if (Game.Has(WorldFlags.AshlandsComplete)) description += " People nod as you pass. A few of them even smile, which in the Ashlands is practically a parade.";
        else if (Prices.RoadIsOpen) description += " There is a caravan in the square — the first in months — and children staring at the oxen.";
        else if (Game.Has(WorldFlags.AshlandsScavengerBounty)) description += " The gates stand open during the day now. Nobody thanks you. They just stop flinching.";

        LocationMenu.Banner("Cinder's Rest", description);

        return LocationMenu.Show(new List<MenuOption>
        {
            new("1", "The Dustfall Inn", "Sera Voss — work, rest, supplies, the Keeper's Vault", Inn),
            new("2", "Ashwell's Smithy", "Korin Ashwell — weapons and forge work", Smithy),
            new("3", "Hunt beyond the gates", "Ash Scavengers (levels 1–3)", Hunt),
        });
    }

    private static void Hunt()
    {
        var hero = Game.Hero;
        string intro = hero.Level >= 4
            ? "The packs keep their distance from you now. You have to go looking — and when you find them they fight like cornered things."
            : "Past the gate the world is the colour of an old hearth. Then part of it moves, and has teeth.";
        Encounters.Fight("Beyond the Gates", intro, Encounters.ScavengerPack(hero.Level));
    }

    // ------------------------------------------------------------------ the inn

    private static void Inn()
    {
        while (true)
        {
            var hero = Game.Hero;
            Ui.Header("The Dustfall Inn");
            Ui.Narrate("Low beams, a long table scarred by forty years of knives, and the best-informed bar in the Ashlands. Sera Voss runs it from a stool she pretends not to need.");
            if (Game.Has(WorldFlags.DrenMorrowAllied)) Ui.Narrate("Dren Morrow has the corner by the fire, leg up on a chair, watching the door.");

            int restCost = RestCost();
            bool leave = false;
            var options = new List<MenuOption>
            {
                new("1", "Talk to Sera", "work, news, questions", Sera.Talk),
                new("2", "Rest", restCost == 0 ? "the stable is free" : $"{restCost} gold — full health and mana", Rest),
                new("3", "Buy supplies", "potions, salves, bombs", () => Shop.BuySupplies("Dustfall Supplies", SupplyStock())),
                new("4", "Bounty board", "standing contracts", BountyBoard),
                new("5", "Keeper's Vault", "share equipment between your heroes", HeroScreens.Vault),
            };
            if (Game.Has(WorldFlags.DrenMorrowAllied)) options.Add(new MenuOption("6", "Talk to Dren", "the courier you saved", Dren.TalkAtInn));
            options.Add(new MenuOption("0", "Leave", "", () => leave = true));
            Ui.Menu(options);
            if (leave) return;
        }
    }

    private static List<string> SupplyStock()
    {
        var stock = InnStock.ToList();
        if (Game.Tier >= ReputationTier.Known) stock.Insert(1, SupplyCatalog.GreaterHealingPotion);
        return stock;
    }

    private static int RestCost()
    {
        int cost = 4 + Game.Hero.Level * 2;
        return Game.Hero.Gold >= cost ? cost : 0;
    }

    private static void Rest()
    {
        var hero = Game.Hero;
        int cost = RestCost();
        hero.Gold -= cost;
        hero.RestoreFully();
        Ui.Line();
        Ui.Narrate(cost > 0
            ? "A real bed, a bowl of something hot, and water from the spring that almost doesn't taste of eggs. You sleep like the dead and wake like the living."
            : "\"No coin? Stable's free. The mule doesn't snore much.\" The straw is warm, at least. You wake with ash in your hair and nothing broken.");
        Ui.Good($"{hero.Name} is fully restored.");
        Game.Save();
        Ui.Pause();
    }

    private static void BountyBoard()
    {
        Ui.Header("Bounty Board");
        Ui.Narrate("A plank by the door, bristling with nails. Most of the notices are in Sera's handwriting.");

        var bounties = new List<string> { QuestLog.BountyScavengers };
        if (Game.Has(WorldFlags.ScorchedPathOpen)) bounties.Add(QuestLog.BountyBandits);
        if (QuestLog.Started(QuestLog.CommandersPrice)) bounties.Add(QuestLog.BountySprites);

        foreach (string id in bounties.Where(QuestLog.ObjectivesMet).ToList())
        {
            Ui.Narrate("You tear a notice off its nail and slap it on the bar. Sera counts out coins without looking up.");
            Rewards.CompleteQuest(id);
        }

        var options = new List<string>();
        var ids = new List<string>();
        foreach (string id in bounties)
        {
            var quest = QuestLog.Get(id);
            if (QuestLog.IsActive(id))
            {
                Ui.Line($"  In progress: {quest.Title} — {quest.Objectives[0].Describe(Game.World, id)}", ConsoleColor.Cyan);
            }
            else
            {
                options.Add($"{quest.Title} — {quest.Gold} gold, {quest.Xp} XP");
                ids.Add(id);
            }
        }

        if (options.Count == 0)
        {
            Ui.Note("You have taken every notice on the board.");
            Ui.Pause();
            return;
        }

        int choice = Ui.Choose(options, "Leave the board");
        if (choice >= 0) Rewards.AcceptQuest(ids[choice]);
        Game.Save();
        Ui.Pause();
    }

    // ------------------------------------------------------------------ the smithy

    private static void Smithy()
    {
        while (true)
        {
            Ui.Header("Ashwell's Smithy");
            Ui.Narrate("One forge, one anvil, twenty years of hammer-marks. A second, smaller apron hangs on a peg by the door. Nobody has touched it in six months.");

            bool leave = false;
            var stock = SmithyStock.ToList();
            if (Game.Tier >= ReputationTier.Trusted) stock.AddRange(SmithyRareStock);

            Ui.Menu(new List<MenuOption>
            {
                new("1", "Talk to Korin", "", Korin.Talk),
                new("2", "Buy", Game.Tier >= ReputationTier.Trusted ? "including the good stock" : "weapons, bows, staffs, quivers", () => Shop.BuyEquipment("Ashwell's Smithy — Buy", stock)),
                new("3", "Sell", "weapons and materials", () => Shop.Sell("Ashwell's Smithy — Sell")),
                new("4", "Forge work", "sharpen and reforge", Shop.Smithing),
                new("0", "Leave", "", () => leave = true),
            });
            if (leave) return;
        }
    }
}

/// <summary>
/// Sera Voss: innkeeper, ex-adventurer, and the spine of the main quest chain.
/// </summary>
public static class Sera
{
    public static void Talk()
    {
        Ui.Header("Sera Voss");
        var tree = new DialogueTree();

        tree.Add(new DialogueNode("hub", Greeting,
            new DialogueOption("\"Got any work?\"", "hub") { Effect = Work },
            new DialogueOption("\"Tell me about the Ashlands.\"", "ashlands"),
            new DialogueOption("\"Heard anything lately?\"", "rumours"),
            new DialogueOption("\"What's your story?\"", "story"),
            new DialogueOption("Show her the Court Sigil.", "sigil") { Condition = () => Game.Has(WorldFlags.HasCourtSigil) && !Game.Has(WorldFlags.AshlandsCourtExposed) },
            new DialogueOption("\"What do you know about Thresh?\"", "thresh") { Condition = () => Game.Has(WorldFlags.ScorchedPathPatrolCleared) && !Game.Has(WorldFlags.ThreshDefeated) },
            new DialogueOption("\"That's all for now.\"", null)));

        tree.Add(new DialogueNode("ashlands", () =>
        {
            Ui.Say("Sera", "Two hundred years ago this was Emberveil. Fire mages. Richest kingdom on the continent, to hear the ballads tell it. Then they went to war with a dragon cult, and both sides won so hard there was nothing left.");
            Ui.Say("Sera", "Now it's this. Nobody wants the land, so nobody fights us for it. That's the whole appeal.");
            Ui.Say("Sera", "Town's safe enough. The Scorched Path is the only road out and the bandits know it. The ruins are east — don't. And the plateau past the ruins is the Ashen Throne, where a dead dragon is still deciding whether to land.");
        }, new DialogueOption("\"Go on.\"", "hub")));

        tree.Add(new DialogueNode("rumours", Rumours, new DialogueOption("\"Hm.\"", "hub")));

        tree.Add(new DialogueNode("story", () =>
        {
            Ui.Say("Sera", "Fifteen years with a pack on my back. The Mistwood, the eastern border, places that don't have names because nobody came back to name them.");
            Ui.Say("Sera", "Then something in a ditch in the east did this to my leg, and I found out I like having a roof.");
            Ui.Say("Sera", Story.ByClass(
                "You fight like the border regulars did. Economical. They mostly died of not being paid.",
                "I'll ask you about those ruins someday. Not because I want to go. Because I want to know what I've been living next to.",
                "You scout before you sit down. I noticed. I used to do that. Still do, honestly — it's just a shorter walk now."));
        }, new DialogueOption("\"Fair enough.\"", "hub")));

        tree.Add(new DialogueNode("sigil", () =>
        {
            Ui.Narrate("You set the black-and-gold disc on the bar. Sera does not touch it.");
            if (Game.Has(WorldFlags.DrenMorrowAllied))
            {
                Ui.Say("Sera", "A crown with no top. I've seen that. East, years ago — on a letter that got three people killed. And your friend in the corner flinched when you took it out.");
                Ui.Say("Sera", "The Hollow Court. That's what they call themselves. I didn't know they were here. I didn't know they were anywhere near here.");
                Game.Set(WorldFlags.AshlandsCourtExposed);
                Ui.World("The Keeper knows the name now: the Hollow Court.");
                Game.World.Log($"Sera Voss named the Hollow Court for {Game.Hero.Name}.");
            }
            else
            {
                Ui.Say("Sera", "A crown with no top. I've seen that... somewhere. East. On something I wasn't supposed to read.");
                Ui.Say("Sera", "I can't put a name to it. The hermit out by the ruins might — Lira. She's older than the ash and remembers all of it. If she'll talk to you.");
            }
        }, new DialogueOption("Put it away.", "hub")));

        tree.Add(new DialogueNode("thresh", () =>
        {
            Ui.Say("Sera", "Commander Thresh. Border guard, twelve years. The garrison was disbanded with the men still in the field — no pay, no orders, nothing. He walked forty of them west because nobody hunts deserters out here.");
            Ui.Say("Sera", "He's not cruel. That's the problem. Cruel men are sloppy. He runs that road like a supply depot.");
            if (Game.Tier >= ReputationTier.Known)
                Ui.Say("Sera", "And desperate men don't have matching boots. His do. Somebody's outfitting them. " + Story.ByClass(
                    "He'll hear a soldier out, I think. He misses being one.",
                    "He doesn't like mages. Thinks academy types look down on everyone. If you talk, bring something better than talk.",
                    "He had a scout once he swears was the best he ever saw. Shoot straight with him — he respects the craft."));
        }, new DialogueOption("\"Useful.\"", "hub")));

        tree.Run("hub");
        Game.Save();
    }

    private static void Greeting()
    {
        var hero = Game.Hero;
        if (Game.Has(WorldFlags.AshlandsComplete))
            Ui.Say("Sera", $"The Keeper's {hero.CharacterType.ToLowerInvariant()}. Sit. You've earned the good chair. There's only one.");
        else if (Game.Has(WorldFlags.AshlandsCourtExposed))
            Ui.Say("Sera", "Keep your voice down and tell me what you've found. I don't like knowing their name. I liked not knowing it better.");
        else if (Story.OtherHero(WorldFlags.AshlandsScavengerBounty) is string other && hero.Level <= 3)
            Ui.Say("Sera", $"Another one with the look. You're with {other}, aren't you — same someone behind the eyes. Fine. {other} was good for it, so you get credit at my bar.");
        else if (Game.Tier >= ReputationTier.Trusted)
            Ui.Say("Sera", $"{hero.Name}. Good. I was hoping it'd be one of yours through that door.");
        else if (Game.Tier >= ReputationTier.Known)
            Ui.Say("Sera", "There you are. People have started asking after you. I tell them you're taller.");
        else
            Ui.Say("Sera", "Still alive, then. What'll it be?");
    }

    private static void Rumours()
    {
        if (Game.Has(WorldFlags.AshlandsCourtExposed))
        {
            Ui.Say("Sera", "Every stranger who's come through here in two years — I've been going back over them. The 'surveyor'. The 'wool buyer'. Nobody buys wool in the Ashlands. We don't have sheep.");
            return;
        }
        switch (Game.Tier)
        {
            case ReputationTier.Stranger:
                Ui.Say("Sera", "Rumours are for regulars. Do something worth gossiping about and I'll return the favour.");
                break;
            case ReputationTier.Known:
                Ui.Say("Sera", "Here's one. Bandits who are hungry rob whoever's closest. Thresh's lot rob on a schedule. Patrol routes. Shift changes. That's not hunger. That's a contract.");
                break;
            default:
                Ui.Say("Sera", "Someone outside is paying for that road to stay closed. I've seen this shape before, in the east — a militia nobody could explain, and then one morning a whole valley belonged to people nobody had ever met.");
                Ui.Say("Sera", "Korin's noticed something too. Ask him about his bulk orders.");
                break;
        }
    }

    /// <summary>Runs whichever step of the main chain Sera can advance right now.</summary>
    private static void Work()
    {
        var hero = Game.Hero;
        Ui.Line();

        // --- hand-ins first ---
        if (QuestLog.ObjectivesMet(QuestLog.HollowCrown))
        {
            AshenThroneLocation.Epilogue();
            return;
        }

        if (QuestLog.ObjectivesMet(QuestLog.CommandersPrice))
        {
            if (Game.Has(WorldFlags.ThreshDefeated))
                Ui.Say("Sera", "Thresh is dead. I know — one of his men came in an hour ago and ordered the cheapest thing I sell, and cried into it. I'm not sorry. But I'm not celebrating either.");
            else if (Game.Has(WorldFlags.ThreshAlly))
                Ui.Say("Sera", "You turned him. You actually turned him. Half this town wanted his head on the gate and you've got him reading the Court's mail instead. I can't decide if that's wise or just very, very funny.");
            else
                Ui.Say("Sera", "They just... left. Packed up in the night. Korin's furious — he wanted a hanging. Me, I've buried enough people. A road with nobody on it suits me fine.");
            Ui.World("The Scorched Path is open. Caravans will return — and prices at the inn drop for every hero you guide.");
            Rewards.CompleteQuest(QuestLog.CommandersPrice);
            Ui.Pause();
            return;
        }

        if (QuestLog.ObjectivesMet(QuestLog.ScorchedRoad))
        {
            Ui.Say("Sera", "A trader came in this morning with all his fingers and most of his goods. First one in a month. That's you.");
            Rewards.CompleteQuest(QuestLog.ScorchedRoad);
            Ui.Line();
            Ui.Say("Sera", "One more thing. That trader saw a man lying off the road past the second bend. Alive, he thinks. Not one of Thresh's — wrong clothes. Too clean.");
            Ui.Say("Sera", "Could be a traveller. Could be bait. Either way I'd want to know before someone else does.");
            Rewards.AcceptQuest(QuestLog.StrangersBurden);
            Ui.Pause();
            return;
        }

        if (QuestLog.ObjectivesMet(QuestLog.TeethAtTheGate))
        {
            Ui.Say("Sera", "Five? I heard the howling stop. Old Marrit hung her washing outside the wall today, first time since spring.");
            Game.Set(WorldFlags.AshlandsScavengerBounty);
            Game.Set(WorldFlags.KorinSharpenOwed);
            Rewards.CompleteQuest(QuestLog.TeethAtTheGate);
            Ui.Say("Sera", "Go see Korin. He said he'd put an edge on something for whoever dealt with those dogs. He'll pretend he didn't say it. Make him.");
            Ui.World("Cinder's Rest will remember this — no matter which of your heroes walks in next.");
            Ui.Pause();
            return;
        }

        // --- then offers ---
        if (!QuestLog.Started(QuestLog.TeethAtTheGate))
        {
            Ui.Say("Sera", "Ash Scavengers. Feral dogs, more or less — a pack's been working the gates. They took a goat last week and tried for a child yesterday.");
            Ui.Say("Sera", "Kill five and the rest will remember they're afraid of us. Fifty gold, and you'll have done one useful thing in the Ashlands, which is one more than most.");
            Rewards.AcceptQuest(QuestLog.TeethAtTheGate);
            Ui.Note("  (Hunt beyond the gates. Rest here when you need to — it's cheap.)");
            Ui.Pause();
            return;
        }

        if (QuestLog.IsDone(QuestLog.TeethAtTheGate) && !QuestLog.Started(QuestLog.ScorchedRoad))
        {
            Ui.Say("Sera", "Dogs were the easy part. The Scorched Path is the only road out of here, and there's a patrol on it — bandits, taking a 'toll' from anyone who can't outrun them.");
            Ui.Say("Sera", "Go out there and break that patrol. There'll be an Enforcer leading it — real armour, carries a potion. Don't let him drink it in peace.");
            Rewards.AcceptQuest(QuestLog.ScorchedRoad);
            Game.Set(WorldFlags.ScorchedPathOpen);
            Ui.World("The Scorched Path is now open for travel — for all of your heroes.");
            Ui.Note("  (Bring potions. A better weapon from Korin wouldn't hurt either.)");
            Ui.Pause();
            return;
        }

        if (QuestLog.IsDone(QuestLog.StrangersBurden) && !QuestLog.Started(QuestLog.CommandersPrice))
        {
            if (Game.Has(WorldFlags.DrenMorrowAllied))
                Ui.Say("Sera", "Your stray's been talking. Says the people he carried letters for are interested in the ruins. Says there's a hermit out there, Lira, who's been keeping them out for years.");
            else
                Ui.Say("Sera", "Korin got paid for your stranger. By 'guild representatives'. In the Ashlands. We don't have guilds. We barely have representatives.");
            Ui.Say("Sera", "Two things, then. First: the ruins. There's a hermit at the edge — Lira the Ashen. If anyone knows what's worth wanting in there, she does. She's... particular about who she talks to.");
            Ui.Say("Sera", "Second: Thresh. The patrol you broke was a finger. He's the hand. His war-camp is off the Path, north side. End his hold on that road. How is your business.");
            Rewards.AcceptQuest(QuestLog.CommandersPrice);
            Ui.World("The Emberveil Ruins can now be reached from the Scorched Path.");
            Ui.Pause();
            return;
        }

        // --- otherwise, a nudge ---
        if (QuestLog.IsActive(QuestLog.TeethAtTheGate))
            Ui.Say("Sera", $"The dogs are still out there. {QuestLog.Get(QuestLog.TeethAtTheGate).Objectives[0].Describe(Game.World, QuestLog.TeethAtTheGate)}.");
        else if (QuestLog.IsActive(QuestLog.ScorchedRoad))
            Ui.Say("Sera", "The patrol's still on the Path. You'll find them easily enough. They'll find you first.");
        else if (QuestLog.IsActive(QuestLog.StrangersBurden))
            Ui.Say("Sera", "The man by the road. Past the second bend on the Scorched Path. Go and look.");
        else if (QuestLog.IsActive(QuestLog.HollowCrown))
            Ui.Say("Sera", "The plateau. I know. I've been watching lights up there for two years and telling myself it was weather. Come back alive and tell me I was a fool.");
        else if (QuestLog.IsActive(QuestLog.CommandersPrice))
            Ui.Say("Sera", "Thresh is still on his hill. And the hermit is still at the ruins, if you haven't been. I've nothing else — the board by the door has the small stuff.");
        else if (Game.Has(WorldFlags.AshlandsComplete))
            Ui.Say("Sera", "Work? You've done the work. There's a road north into the Mistwood, when you're ready — and when whoever's writing this world gets round to it. Until then, the board's by the door.");
        else
            Ui.Say("Sera", "Nothing new from me. The board by the door has the small stuff.");
        Ui.Pause();
    }
}

/// <summary>
/// Korin Ashwell: the blacksmith. Gruff, fair, and slow to trust.
/// </summary>
public static class Korin
{
    public static void Talk()
    {
        Ui.Header("Korin Ashwell");
        var tree = new DialogueTree();

        tree.Add(new DialogueNode("hub", Greeting,
            new DialogueOption("\"About the hammer...\"", "hammer_return") { Condition = () => QuestLog.ObjectivesMet(QuestLog.ApprenticesHammer) },
            new DialogueOption("\"Whose apron is that?\"", "apprentice") { Condition = () => Game.Has(WorldFlags.ScorchedPathPatrolCleared) && !QuestLog.Started(QuestLog.ApprenticesHammer) },
            new DialogueOption("\"Sera says you've had strange orders.\"", "buyer") { Condition = () => Game.Tier >= ReputationTier.Trusted },
            new DialogueOption("\"What happened to the man I brought you?\"", "dren") { Condition = () => Game.Has(WorldFlags.DrenMorrowBetrayed) },
            new DialogueOption("\"How did you end up out here?\"", "past"),
            new DialogueOption("\"I'll let you work.\"", null)));

        tree.Add(new DialogueNode("apprentice", () =>
        {
            bool willing = Game.Hero is Warrior || Game.Tier >= ReputationTier.Known;
            Ui.Narrate("Korin's hammer stops mid-swing. He sets it down carefully, the way you set down something hot.");
            Ui.Say("Korin", "Tam's. My apprentice. Fourteen. Good hands. Went with a caravan six months back to fetch iron, and Thresh's people hit it on the Path.");
            if (!willing)
            {
                Ui.Say("Korin", Story.ByClass("", "And that's all I'm saying to someone who mutters at fire for a living.", "And that's all I'm saying to someone I can't hear walk up behind me."));
                Ui.Note("  (Korin needs to trust the Keeper more — Known reputation — before he will ask this of you.)");
                return;
            }
            Ui.Say("Korin", "He had a hammer. I made it for him. It'll be in whatever's left of their old camp off the road — they burned it when they moved up the hill. I can't go. I'd not come back, and then who shoes the mule.");
            Ui.Say("Korin", "Bring it home and I'll make you something worth carrying. Not a favour. A trade.");
            Rewards.AcceptQuest(QuestLog.ApprenticesHammer);
        }, new DialogueOption("\"I'll look.\"", "hub")));

        tree.Add(new DialogueNode("hammer_return", () =>
        {
            Ui.Narrate("You hold out the hammer. The haft is scorched. The head is fine — small, well-balanced, with a T punched into the cheek.");
            Ui.Narrate("Korin takes it. He doesn't say anything for long enough that you consider leaving. Then he hangs it on the peg with the apron, and turns to the back of the shop.");
            Ui.Say("Korin", "Said I'd make you something. Made it weeks ago, in case. Didn't want to jinx it by saying.");
            Game.Set(WorldFlags.HammerReturned);
            Rewards.CompleteQuest(QuestLog.ApprenticesHammer);
            Rewards.GiveItem(Story.ByClass("masterwork_sword", "masterwork_staff", "masterwork_bow"));
            Ui.World("Korin will not forget this. Neither will his prices — for any of your heroes.");
        }, new DialogueOption("\"Thank you.\"", "hub")));

        tree.Add(new DialogueNode("buyer", () =>
        {
            Ui.Say("Korin", "Strange. Aye. Fellow in a good cloak, comes every second month. Buys blades by the crate. Says it's for a mining operation.");
            Ui.Say("Korin", "There's no mine in the Ashlands. There's nothing in the ground here but dead mages. And you don't arm miners with thirty matched swords.");
            Ui.Say("Korin", "Pays in gold with no mint mark. Smooth coins. I took it, because it was gold. I've been thinking I shouldn't have.");
            if (!Game.Has(WorldFlags.AshlandsCourtFound))
            {
                Game.Set(WorldFlags.AshlandsCourtFound);
                Ui.World("Someone is quietly arming the Ashlands. The Keeper has found the first thread.");
                Game.World.Log($"Korin told {Game.Hero.Name} about the cloaked buyer.");
            }
        }, new DialogueOption("\"Keep the next crate short.\"", "hub")));

        tree.Add(new DialogueNode("dren", () =>
        {
            Ui.Say("Korin", "Kept him in the coal store overnight. Morning comes, two men at my door. Polite. Clean boots. 'Guild representatives.' Paid me for my trouble and walked him out between them.");
            Ui.Say("Korin", "He didn't look grateful to be collected. I've been turning that over. I don't like how it sits.");
        }, new DialogueOption("\"Neither do I.\"", "hub")));

        tree.Add(new DialogueNode("past", () =>
        {
            if (Game.Tier >= ReputationTier.Honored)
            {
                Ui.Say("Korin", "Had a foundry in the west. Forty men. Took a contract from a lord to arm his levy, and the lord used them on his own tenants. My steel. My mark on every blade they found in those people.");
                Ui.Say("Korin", "So I came somewhere nobody buys enough to do harm with. And now there's a man in a good cloak buying crates. Funny how it finds you.");
            }
            else
            {
                Ui.Say("Korin", "Business in the west didn't work out. That's all there is to say about it, and more than I say to most.");
            }
        }, new DialogueOption("\"Understood.\"", "hub")));

        tree.Run("hub");
        Game.Save();
    }

    private static void Greeting()
    {
        var hero = Game.Hero;
        if (Game.Has(WorldFlags.HammerReturned))
            Ui.Say("Korin", $"{hero.Name}. Fire's hot, kettle's on. What do you need?");
        else if (hero is Archer && Game.Tier == ReputationTier.Stranger)
            Ui.Say("Korin", "Sneaky types always bring trouble. Buy something or don't. Prices are what they are for people I can't hear coming.");
        else if (hero is Archer && Game.Tier == ReputationTier.Known && !Game.Has(WorldFlags.ScorchedPathPatrolCleared))
            Ui.Say("Korin", "You're still here. Most sneaky types aren't.");
        else if (Story.OtherHero(WorldFlags.AshlandsScavengerBounty) is string other)
            Ui.Say("Korin", $"You're with {other}. Same... whatever it is, behind the eyes. {other} dealt with those dogs, so I suppose that vouches for you. Don't make me regret the arithmetic.");
        else if (Game.Has(WorldFlags.AshlandsScavengerBounty))
            Ui.Say("Korin", "You're the one who dealt with those dogs? Good." + (Game.Has(WorldFlags.KorinSharpenOwed) ? " I said I'd put an edge on something. Forge work, when you're ready." : ""));
        else if (hero is Mage)
            Ui.Say("Korin", "Mage. Hm. I'll sell to you. Keep your hands where I can see them and your fire where I can't.");
        else
            Ui.Say("Korin", "If it's steel you want, it's on the wall. If it's talk, I'm busy.");
    }
}

/// <summary>
/// Dren Morrow: abandoned Court courier. This class holds his conversations at the inn once he has been saved.
/// </summary>
public static class Dren
{
    public static void TalkAtInn()
    {
        Ui.Header("Dren Morrow");
        var hero = Game.Hero;
        var tree = new DialogueTree();

        tree.Add(new DialogueNode("hub", () =>
        {
            if (Story.OtherHero(WorldFlags.DrenMorrowAllied) is string saviour)
                Ui.Say("Dren", $"You're — no, you're not {saviour}. But you stand like {saviour}. Sera says that's a thing with you people. I don't ask. {saviour} carried me three miles; you can have whatever I know.");
            else
                Ui.Say("Dren", Story.ByClass(
                    "Oh. It's you. Good. I mean — good. I still jump when the door opens. You'd think I'd stop.",
                    "Sit, sit. You people read, don't you? I've been wanting to tell someone who'd understand the half of it.",
                    "I didn't hear you come in. Nobody hears you come in. I'm choosing to find that reassuring."));
        },
            new DialogueOption("\"Who were you working for?\"", "court"),
            new DialogueOption("\"What do they want in the ruins?\"", "ruins"),
            new DialogueOption("\"Tell me about your handler.\"", "handler"),
            new DialogueOption("\"What do you know about Thresh?\"", "thresh") { Condition = () => !Game.Has(WorldFlags.ThreshDefeated) },
            new DialogueOption("\"Rest that leg.\"", null)));

        tree.Add(new DialogueNode("court", () =>
        {
            Ui.Say("Dren", "The Hollow Court. I wasn't supposed to know the name. I heard it once, through a door, and then spent three months pretending I hadn't.");
            Ui.Say("Dren", "They're not a gang. Gangs are loud. These people are — organised. Wealthy. Everyone I met only knew the person above them. I only knew a codename.");
            if (!Game.Has(WorldFlags.AshlandsCourtExposed))
            {
                Game.Set(WorldFlags.AshlandsCourtExposed);
                Game.Set(WorldFlags.AshlandsCourtFound);
                Ui.World("The Keeper knows the name now: the Hollow Court.");
                Game.World.Log($"Dren Morrow named the Hollow Court for {hero.Name}.");
            }
        }, new DialogueOption("\"Go on.\"", "hub")));

        tree.Add(new DialogueNode("ruins", () =>
        {
            Ui.Say("Dren", "Something sealed up under the old academy. The letters called it 'the Focus'. They've been trying to get at it for two years and something keeps stopping them.");
            Ui.Say("Dren", "There's a hermit out there — Lira. The Court's people talked about her the way you'd talk about a wasp nest over the door. Tell her I sent you. Actually — don't. Tell her the Court wants in. That's the same thing and she'll like it better.");
            if (hero is Mage) Ui.Say("Dren", "You'd know better than me, but — one letter said the seal 'only yields to the old discipline or to brute idiocy'. I assume you're the first one.");
        }, new DialogueOption("\"Useful.\"", "hub")));

        tree.Add(new DialogueNode("handler", () =>
        {
            Ui.Say("Dren", "Codename 'the Cinder Hand'. Never saw a face — always a hood, always gloves. One glove, really. The right hand was always in this... metal thing. A gauntlet. It was warm. You could feel it across a table.");
            Ui.Say("Dren", "Favoured the left side when walking. Old wound, I'd guess, under the ribs. I notice things like that. It's why I was a good courier and a terrible fighter.");
            Ui.Note("  (If you ever face the Cinder Hand, you will know where to strike first.)");
        }, new DialogueOption("\"I'll remember that.\"", "hub")));

        tree.Add(new DialogueNode("thresh", () =>
        {
            Ui.Say("Dren", "Thresh? He doesn't know. That's the joke. He thinks he's working for a merchant guild that wants the road. He's never heard the word 'Court' in his life.");
            Ui.Say("Dren", "I carried his pay. Same smooth coins they paid me in. If someone told him who really owns him... well. He's a proud man. I wouldn't want to be standing near the Court's contact that day.");
        }, new DialogueOption("\"Interesting.\"", "hub")));

        tree.Run("hub");
        Game.Save();
    }
}
