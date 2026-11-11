using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Combat;
using RPGManagerLib.Items;
using RPGManagerLib.Quests;
using RPGManagerLib.UI;
using RPGManagerLib.Worlds;

namespace RPGManagerLib.Adventure;

/// <summary>
/// The Scorched Path: the bandit road. Patrols, the broken wagon, Dren Morrow, the burned camp,
/// and Commander Thresh's war-camp.
/// </summary>
public static class ScorchedPathLocation
{
    private static bool ThreshResolved =>
        Game.Has(WorldFlags.ThreshDefeated) || Game.Has(WorldFlags.ThreshNegotiated);

    private static bool DrenResolved =>
        Game.Has(WorldFlags.DrenMorrowAllied) || Game.Has(WorldFlags.DrenMorrowBetrayed);

    /// <summary>Shows the location once. Returns false when the player quits.</summary>
    public static bool Visit()
    {
        string description = "The old trade road, sunk between canyon walls burned black two centuries ago. Sound carries strangely here. So do arrows.";
        if (Game.Has(WorldFlags.ThreshDefeated)) description += " The bandit watch-fires on the north ridge are cold. A few leaderless stragglers still haunt the rocks.";
        else if (Game.Has(WorldFlags.ThreshNegotiated)) description += " The north ridge is empty. Thresh's people left their fire-pits swept and their latrines filled in, like soldiers.";
        else if (Game.Has(WorldFlags.ScorchedPathPatrolCleared)) description += " The toll-barricade lies in pieces where it was broken. Up on the north ridge, watch-fires still burn.";

        LocationMenu.Banner("The Scorched Path", description);

        var options = new List<MenuOption>();
        int key = 1;
        void Add(string label, string hint, Action action) => options.Add(new MenuOption((key++).ToString(), label, hint, action));

        if (QuestLog.IsActive(QuestLog.ScorchedRoad) && !Game.Has(WorldFlags.ScorchedPathPatrolCleared))
            Add("Confront the bandit patrol", "quest: The Scorched Road (two fights, no rest between)", ConfrontPatrol);
        if (QuestLog.IsActive(QuestLog.StrangersBurden) && Game.Has(WorldFlags.DrenMorrowFound) && !DrenResolved)
            Add("Return to the injured man", "quest: The Stranger's Burden", DrenChoice);
        if (QuestLog.IsActive(QuestLog.HollowReprisal) && !Game.Has(WorldFlags.ThreshSaved) && !Game.Has(WorldFlags.ThreshLost))
            Add("Run to Thresh's camp", "quest: Hollow Reprisal — Sentinels are attacking", DefendThresh);
        if (QuestLog.IsActive(QuestLog.CommandersPrice) && !ThreshResolved)
            Add("Climb to Thresh's war-camp", "quest: The Commander's Price (recommended level 6+)", ThreshCamp);
        if (QuestLog.IsActive(QuestLog.ApprenticesHammer) && !Game.Has(WorldFlags.HasApprenticeHammer))
            Add("Search the burned camp", "quest: The Apprentice's Hammer", BurnedCamp);
        if (!Game.Has(WorldFlags.WagonLooted))
            Add("Investigate the broken wagon", "something glints under the canvas", Wagon);
        if (Game.Has(WorldFlags.ThreshAlly) && !Game.Has(WorldFlags.ThreshLost))
            Add("Visit Thresh", "your man on the ridge", VisitThresh);
        Add("Patrol the road", Prices.RoadIsOpen ? "stragglers and scavengers" : "bandits (levels 2–5)", Patrol);

        return LocationMenu.Show(options);
    }

    private static void Patrol()
    {
        var hero = Game.Hero;
        if (Prices.RoadIsOpen)
        {
            Encounters.Fight("The Scorched Path",
                "With Thresh gone the road belongs to whoever is hungriest. Today that is not you.",
                Core.Rng.Chance(0.5) ? Encounters.ScavengerPack(3) : Bestiary.Group(Bestiary.Bandit, Bestiary.BanditBowman));
            return;
        }
        Encounters.Fight("The Scorched Path",
            "You walk the road openly, the way honest travellers do. It does not take long for someone to come and explain the toll.",
            Encounters.BanditGroup(hero.Level));
    }

    // ------------------------------------------------------------------ quest 2

    private static void ConfrontPatrol()
    {
        Ui.Header("The Toll");
        Ui.Narrate("They have dragged a wagon-bed across the narrowest part of the canyon and called it a barricade. Two bandits lounge on it. Further back, a bigger man in a real breastplate is eating an apple with a knife.");
        Ui.Say("Bandit", "Road's closed. Road's open again for ten gold. Twenty if you've got a face like that.");
        Ui.Narrate("This will be two fights back to back: the lookouts, then the Enforcer and whoever is left. There will be no time to rest between them.");
        if (!Ui.Confirm("Step up to the barricade?")) return;

        var first = Encounters.Fight("The Toll — Lookouts",
            "You decline to pay. The two on the barricade seem almost relieved; they were bored.",
            Bestiary.Group(Bestiary.Bandit, Bestiary.Bandit),
            new BattleOptions { CanFlee = false });
        if (first != BattleOutcome.Victory) return;

        var second = Encounters.Fight("The Toll — The Enforcer",
            "The big man tosses the apple core away, wipes the knife on his thigh, and draws something much longer. A bowman has found a perch behind him.",
            Bestiary.Group(Bestiary.BanditEnforcer, Bestiary.BanditBowman),
            new BattleOptions { CanFlee = false });
        if (second != BattleOutcome.Victory) return;

        Game.Set(WorldFlags.ScorchedPathPatrolCleared);
        Game.World.Log($"{Game.Hero.Name} broke the bandit patrol on the Scorched Path.");
        Ui.Header("The Toll");
        Ui.Narrate("You kick the wagon-bed off the road. It takes three tries and is not as satisfying as you hoped, but the road is open.");
        Ui.Narrate("In the Enforcer's belt-pouch: a pay-chit, stamped, for 'services to the Guild'. The coins with it are smooth — no king's head, no mint mark.");
        Ui.World("The patrol is broken. Sera will want to hear about it — from whichever hero tells her.");
        Game.Save();
        Ui.Pause();
    }

    // ------------------------------------------------------------------ quest 3

    /// <summary>Arrival event: the hero finds Dren Morrow by the road.</summary>
    public static void FindDren()
    {
        Game.Set(WorldFlags.DrenMorrowFound);
        Ui.Header("The Stranger's Burden");
        Ui.Narrate("Past the second bend, a cart lies on its side with a dead horse still in the traces. A man sits against the wheel with one leg stretched out wrong, holding a knife in a hand that is shaking too hard to use it.");
        Ui.Narrate("He is perhaps twenty-six. His clothes were expensive a week ago.");
        Ui.Say("Stranger", Story.ByClass(
            "Please don't. Please — I'll tell you everything, whatever it is you want, I'll tell you. Just don't.",
            "You're — a mage? Oh, thank the dead gods. An educated person. You won't just kill me for my boots. Will you?",
            "You're not going to shoot me, are you? I didn't see you until you wanted me to. That's not a good sign, is it?"));
        Ui.Say("Stranger", "Dren. Dren Morrow. I'm a courier. I carry letters, that's all, I don't open them. Bandits hit the cart. The man I was meeting never came. Nobody's coming. I've a cracked rib and my leg's open to the bone.");
        Ui.Narrate("There is a satchel under the cart. Its seal is black wax, stamped with something you can't make out — a ring? A crown?");
        DrenChoice();
    }

    private static void DrenChoice()
    {
        var hero = Game.Hero;
        bool hasPotion = hero.SupplyCount(SupplyCatalog.HealingPotion) > 0 || hero.SupplyCount(SupplyCatalog.GreaterHealingPotion) > 0;
        bool canMend = hero is Mage && hero.Level >= 2 && hero.Mana >= 25;

        Ui.Line();
        Ui.Line("  What do you do?", ConsoleColor.White);
        var options = new List<string>();
        if (canMend) options.Add("Help him: knit the wound with a Mend spell (25 mana) and get him to Cinder's Rest.");
        else if (hasPotion) options.Add("Help him: spend a Healing Potion and get him to Cinder's Rest.");
        else options.Add("Help him — (you need a Healing Potion; a Mage of level 2+ could cast Mend)");
        options.Add("Turn him in: march him to Korin as a suspicious outsider. Someone will pay for a man like this.");
        options.Add("Leave him for now. The road isn't going anywhere, and neither is he.");

        int choice = Ui.Choose(options);
        if (choice == 0)
        {
            if (!canMend && !hasPotion)
            {
                Ui.Note("You have nothing to treat him with. Sera sells potions.");
                Ui.Pause();
                return;
            }
            if (canMend) hero.Mana -= 25;
            else if (!hero.UseSupply(SupplyCatalog.HealingPotion)) hero.UseSupply(SupplyCatalog.GreaterHealingPotion);
            HelpDren();
        }
        else if (choice == 1) BetrayDren();
        else
        {
            Ui.Narrate("You leave him the shade of the cart. He watches you go with the expression of a man doing arithmetic on how long he has.");
            Ui.Pause();
        }
    }

    private static void HelpDren()
    {
        var hero = Game.Hero;
        Ui.Line();
        Ui.Narrate(hero is Mage
            ? "The flesh closes under your hands, slowly, like a book being shut. Dren stares at it."
            : "He drinks it in three desperate swallows. Colour comes back into his face like a tide.");
        Ui.Narrate("It is three miles to Cinder's Rest. He leans on you for all of them, and talks for most of them, because he is the kind of man who talks when he is frightened.");
        Ui.Say("Dren", "They call themselves the Court. I wasn't meant to know even that. There's something in the ruins they want — the letters called it 'the Focus'. And there's an old woman out there who keeps stopping them. Lira. If you want to hurt them, start with her.");
        Ui.Narrate("Sera takes one look at him, says \"Corner by the fire, and you'll pay for the stew when you can stand,\" and that is that.");

        Game.Set(WorldFlags.DrenMorrowAllied);
        Game.World.Log($"{hero.Name} saved Dren Morrow and brought him to the Dustfall Inn.");
        hero.LocationId = Travel.CindersRest;
        Rewards.CompleteQuest(QuestLog.StrangersBurden);
        Rewards.GiveReputation(15);
        Ui.Line();
        Ui.World("Dren Morrow is now at the Dustfall Inn — for every hero you guide. He remembers who carried him, and he knows things. Lira the Ashen will speak to anyone the Keeper sends.");
        Ui.Note("  (Sera has more to say. So does Dren.)");
        Game.Save();
        Ui.Pause();
    }

    private static void BetrayDren()
    {
        var hero = Game.Hero;
        Ui.Line();
        Ui.Narrate("You haul him up. He understands before you say anything, and something goes out of his face.");
        Ui.Say("Dren", "No — listen — you don't know who's looking for me. If they find me they won't— please. Please.");
        Ui.Narrate("Korin locks him in the coal store and counts a purse into your hand without meeting your eyes: the town's bounty for suspicious outsiders, plus something extra \"for the trouble\".");
        Ui.Narrate("By the next morning, Dren Morrow is gone. Two polite men with clean boots collected him at dawn.");

        Game.Set(WorldFlags.DrenMorrowBetrayed);
        Game.World.Log($"{hero.Name} handed Dren Morrow over. Strangers collected him by morning.");
        hero.LocationId = Travel.CindersRest;
        Rewards.Give(gold: 150);
        Rewards.CompleteQuest(QuestLog.StrangersBurden);
        Rewards.GiveReputation(10);
        Ui.Line();
        Ui.World("Dren Morrow is gone from the Ashlands — for every hero you guide. Whatever he knew left with him. You will have to learn it the hard way.");
        Ui.Note("  (Sera has more to say.)");
        Game.Save();
        Ui.Pause();
    }

    // ------------------------------------------------------------------ side content

    private static void Wagon()
    {
        Ui.Header("The Broken Wagon");
        Ui.Narrate("A merchant's wagon, axle snapped, canvas slashed. Whoever robbed it was in a hurry: there is a strongbox wedged under the seat that nobody had the leverage to pull out.");
        Ui.Narrate("You have the leverage. You also, it turns out, have company — something has been denning in the wagon-bed, and it has a family.");
        if (!Ui.Confirm("Go for the strongbox?")) return;

        var outcome = Encounters.Fight("The Broken Wagon",
            "The canvas erupts. The big one comes first, the others right behind it.",
            Bestiary.Group(Bestiary.ScavengerAlpha, Bestiary.AshScavenger, Bestiary.AshScavenger));
        if (outcome != BattleOutcome.Victory) return;

        Game.Set(WorldFlags.WagonLooted);
        Ui.Header("The Broken Wagon");
        Ui.Narrate("The strongbox gives on the second heave. Inside: a merchant's emergency fund, and the kind of supplies a careful man packs for a road he doesn't trust.");
        Rewards.Give(gold: 60);
        Rewards.GiveSupply(SupplyCatalog.HealingPotion, 2);
        Rewards.GiveSupply(SupplyCatalog.Firebomb, 1);
        Rewards.GiveSupply(SupplyCatalog.Scraps, 3);
        Game.Save();
        Ui.Pause();
    }

    private static void BurnedCamp()
    {
        Ui.Header("The Burned Camp");
        Ui.Narrate("Thresh's people camped here before they moved up the ridge, and burned what they couldn't carry. Charred tent-poles stand like a dead orchard. Not everyone left: a few of them use the place as a cache.");
        var outcome = Encounters.Fight("The Burned Camp",
            "Three shapes look up from a half-buried crate. They were not expecting anyone. Neither, frankly, were you.",
            Bestiary.Group(Bestiary.Bandit, Bestiary.BanditBowman, Bestiary.Bandit));
        if (outcome != BattleOutcome.Victory) return;

        Game.Set(WorldFlags.HasApprenticeHammer);
        Game.World.Log($"{Game.Hero.Name} found the apprentice's hammer in the burned camp.");
        Ui.Header("The Burned Camp");
        Ui.Narrate("You find it in the ashes of the cook-fire, where someone had been using it to knock in tent pegs: a small smith's hammer, the haft scorched, a neat letter T punched into the cheek of the head.");
        Ui.Narrate("Nearby, half burned, a caravan manifest. One line near the bottom: 'T. — apprentice, Ashwell's — 2 crates bar iron.' Someone has drawn a line through it.");
        Ui.Gain("Key item: Tam's Hammer");
        Ui.Note("  (Korin Ashwell will want this.)");
        Game.Save();
        Ui.Pause();
    }

    // ------------------------------------------------------------------ Thresh

    private static void ThreshCamp()
    {
        var hero = Game.Hero;
        bool knowsCourt = Game.Has(WorldFlags.AshlandsCourtExposed);
        bool hasSigil = Game.Has(WorldFlags.HasCourtSigil);

        Ui.Header("Thresh's War-Camp");
        Ui.Narrate("It is not a bandit camp. It is a field fortification: ditch, palisade, a single gate with a clear killing ground in front of it. Whoever laid it out did so from memory.");
        Game.Set(WorldFlags.ThreshCampFound);

        bool admitted = hero is Warrior || Game.Tier >= ReputationTier.Known;
        if (admitted)
        {
            Ui.Say("Sentry", hero is Warrior
                ? "Halt. ...You walk like regular army. Commander'll want a look at you. Keep your hands off the steel."
                : "The innkeeper's errand-runner. We've heard. Commander says anyone with the nerve to walk up here gets five minutes.");
        }
        else
        {
            Ui.Say("Sentry", "Nobody comes up this hill uninvited. Turn round or get carried down.");
            if (!Ui.Confirm("Force the gate? (a fight, and Thresh will not talk afterwards)")) return;
            var gate = Encounters.Fight("The Gate", "The sentries do not waste breath on a second warning.",
                Bestiary.Group(Bestiary.Bandit, Bestiary.BanditBowman), new BattleOptions { CanFlee = true });
            if (gate != BattleOutcome.Victory) return;
            FightThresh("Thresh is waiting inside the gate with his sword already drawn. \"You killed my men at my own door. There's nothing to say.\"");
            return;
        }

        Ui.Line();
        Ui.Narrate("Commander Thresh is a broad, grey, tidy man mending a strap at a camp table. He does not get up.");
        Ui.Say("Thresh", Story.ByClass(
            "A soldier. Good. I can talk to a soldier. Say your piece.",
            "A mage. Academy types always think they're the cleverest thing on the hill. Say your piece and say it plainly.",
            "I didn't see you until the gate. I had a scout like that once. Best I ever had. Say your piece."));

        while (true)
        {
            var options = new List<string>();
            var actions = new List<Func<bool>>();

            if (knowsCourt || hasSigil)
            {
                options.Add(hasSigil
                    ? "Put the Court Sigil on his table. \"This is your 'Guild'.\""
                    : "\"The Guild that pays you doesn't exist. You work for the Hollow Court.\"");
                actions.Add(() => Negotiate(hasSigil));
            }
            else
            {
                options.Add("\"Who's paying for all this?\"");
                actions.Add(() =>
                {
                    Ui.Say("Thresh", "A merchant guild that wants the road held. They pay on time, which is more than the crown ever did. I don't ask their names and they don't ask mine.");
                    Ui.Note("  (If you knew who really pays him — or had proof — this conversation could go differently.)");
                    return false;
                });
            }
            options.Add("\"Why are you doing this?\"");
            actions.Add(() =>
            {
                Ui.Say("Thresh", "Twelve years on the eastern border. One morning the paymaster didn't come. Then the supply wagons didn't come. Then a rider came and said the garrison was disbanded, and rode off before anyone could ask him what that meant.");
                Ui.Say("Thresh", "Forty men, in the field, with nothing. I walked them here. I keep them fed. I know what it's called, what I do. I've made my peace with it.");
                Ui.Narrate("He has not, quite. You can see the seam.");
                return false;
            });
            options.Add("Draw your weapon. \"Your hold on the road ends today.\"");
            actions.Add(() =>
            {
                FightThresh("Thresh sets down the strap and stands. \"So be it. Clear a space,\" he tells his men. \"Nobody interferes. Whoever walks away, walks away.\"");
                return true;
            });
            options.Add("Leave while you still can.");
            actions.Add(() => true);

            int choice = Ui.Choose(options);
            if (actions[choice]()) return;
        }
    }

    /// <summary>Tries to turn Thresh. Returns true when the scene is over.</summary>
    private static bool Negotiate(bool hasSigil)
    {
        var hero = Game.Hero;
        Ui.Line();
        if (hasSigil)
            Ui.Narrate("The black-and-gold disc lands on the table between you. Thresh looks at it for a long moment. Then he reaches into his own coat and sets a coin beside it. Smooth. No mint mark. The same gold.");
        else
            Ui.Narrate("You tell him what Dren Morrow told you: the couriers, the smooth coins, the handler with the burning gauntlet. Thresh takes a coin from his coat and turns it over. Smooth. No mint mark.");

        // Soldiers get the benefit of the doubt; mages have to have earned more.
        int needed = hero switch { Warrior => 0, Mage => 45, _ => 30 };
        bool proof = hasSigil && Game.Has(WorldFlags.AshlandsCourtExposed);
        if (proof) needed -= 15;

        if (Game.World.GetReputation() < needed)
        {
            Ui.Say("Thresh", hero is Mage
                ? "A clever story from a clever mouth. I've been talked into things by your sort before. Come back when this country vouches for you — or come back with a blade."
                : "Maybe. Or maybe you're the innkeeper's knife, saying whatever gets me off this hill. I don't know you well enough to bet forty lives on it.");
            Ui.Note($"  (Thresh does not trust the Keeper enough yet: reputation {Game.World.GetReputation()}, needs {needed}. A Warrior would be heard out regardless.)");
            return false;
        }

        Ui.Say("Thresh", "A 'merchant guild'. Clean boots. Never a name. And I never asked, because asking might have stopped the gold.");
        Ui.Narrate("He closes his fist on the coin. When he speaks again his voice is very quiet, and every man in earshot stands a little straighter.");
        Ui.Say("Thresh", "I deserted a crown that forgot me. I will not be the kept dog of people who won't even show their faces. Sergeant! Strike the camp. We're off this road by dawn.");

        Game.Set(WorldFlags.ThreshNegotiated);
        Game.World.Log($"{hero.Name} showed Commander Thresh who really paid him. He pulled his men off the Scorched Path.");
        Ui.World("The Scorched Path is open. No blood spilled — some in Cinder's Rest will thank you for that, and some never will.");

        if (Game.Tier >= ReputationTier.Trusted)
        {
            Ui.Line();
            Ui.Say("Thresh", "You've a name in this country. People I've robbed speak well of you, which is a strange thing to hear. So I'll ask it straight: what do you want from me, beyond gone?");
            int choice = Ui.Choose(new List<string>
            {
                "\"Stay. Watch the Court for me. They think you still belong to them.\"",
                "\"Just go. Take your men somewhere they can be soldiers again.\""
            });
            if (choice == 0)
            {
                Game.Set(WorldFlags.ThreshAlly);
                Ui.Say("Thresh", "Hm. A deserter, a bandit, and now a spy. My mother would be so proud. ...Aye. We'll hold the ridge and keep our eyes open. Take this — it's regular issue, and it's seen better service than it's getting.");
                Rewards.GiveItem("garrison_longsword");
                Ui.World("Commander Thresh is now the Keeper's ally. Every hero you guide can call on him — and the Court will find out.");
                Game.World.Log($"Commander Thresh agreed to watch the Hollow Court for the Keeper.");
            }
            else
            {
                Ui.Say("Thresh", "Somewhere they can be soldiers again. ...I'd like that. Luck to you. You'll need more of it than I did.");
            }
        }
        else
        {
            Ui.Note("  (With Trusted reputation, Thresh might have been persuaded to stay and work against the Court.)");
        }

        Game.Save();
        Ui.Pause();
        return true;
    }

    private static void FightThresh(string intro)
    {
        var outcome = Encounters.Fight("Commander Thresh", intro,
            Bestiary.Group(Bestiary.Thresh, Bestiary.BanditEnforcer),
            new BattleOptions { CanFlee = false });
        if (outcome != BattleOutcome.Victory) return;

        Game.Set(WorldFlags.ThreshDefeated);
        Game.World.Log($"{Game.Hero.Name} killed Commander Thresh. His bandits scattered.");
        Ui.Header("Commander Thresh");
        Ui.Narrate("He goes down on one knee, then the other, and stays there, breathing like a bellows.");
        Ui.Say("Thresh", "Good fight. Tell them — tell my men — the Guild's gold is in the strongbox. Split it even. Then go home. If any of them still have one.");
        Ui.Narrate("In the strongbox, under the smooth unmarked coins, is a single folded order: 'Hold the road until the work in the ruins is finished. — the Hand.'");
        if (!Game.Has(WorldFlags.AshlandsCourtFound))
        {
            Game.Set(WorldFlags.AshlandsCourtFound);
            Ui.World("Someone was using Thresh. Whoever 'the Hand' is, their business is in the Emberveil Ruins.");
        }
        Ui.World("The Scorched Path is open. The bandits are leaderless; a few will linger, but the road belongs to travellers again.");
        Game.Save();
        Ui.Pause();
    }

    private static void VisitThresh()
    {
        Ui.Header("The Ridge");
        Ui.Narrate("The camp is smaller now and better hidden. Thresh keeps a fire that makes no smoke.");
        if (Game.Has(WorldFlags.CinderHandDefeated))
            Ui.Say("Thresh", "The Hand's dead, then. Good. There'll be another — there's always another with people like that. But not this season. Sit. I've got something that's nearly tea.");
        else if (Game.Has(WorldFlags.ThreshSaved))
            Ui.Say("Thresh", "I owe you forty lives twice over now. When you climb to the Throne, send word. I'm not letting you do that alone.");
        else if (Game.Has(WorldFlags.EmberSealBroken))
            Ui.Say("Thresh", "Their riders went up to the plateau yesterday, fast. Whatever you did in those ruins, they felt it. Watch the stair — that's where I'd put my sentries.");
        else
            Ui.Say("Thresh", "Their contact still thinks I'm bought. He asked me to keep 'curious people' away from the ruins. So that's where I'd go, if I were curious.");
        Ui.Say("Thresh", Story.ByClass(
            "Keep your guard up on the backswing. You drop it. Everyone does.",
            "I've decided you're the exception to academy types. Don't prove me wrong.",
            "My old scout used to say: the second arrow matters more than the first. Never knew what she meant. Maybe you do."));
        Ui.Pause();
    }

    /// <summary>Arrival event: word reaches the hero that Sentinels are moving on Thresh.</summary>
    public static void ReprisalBegins()
    {
        Ui.Header("Hollow Reprisal");
        Ui.Narrate("A boy in a bandit's too-big jerkin comes sliding down the scree, bleeding from the scalp.");
        Ui.Say("Runner", "Commander sent me — the black-and-gold ones — they came up the back of the ridge, they don't talk, they just— he says if the Keeper's people are coming, now would be the hour!");
        Ui.World("The Court has learned that Thresh turned. What your heroes do next decides whether he lives.");
        Rewards.AcceptQuest(QuestLog.HollowReprisal);
        Game.Save();
        Ui.Pause();
    }

    private static void DefendThresh()
    {
        Ui.Header("Hollow Reprisal");
        Ui.Narrate("The palisade is burning. Thresh stands in the gap with his back to his wounded, shield arm hanging wrong, and two masked figures in black and gold walking toward him at exactly the same pace.");
        var outcome = Encounters.Fight("Hollow Reprisal",
            "You hit them from behind. Neither Sentinel makes a sound. One simply turns.",
            Bestiary.Group(Bestiary.CourtSentinel, Bestiary.CourtSentinel),
            new BattleOptions { CanFlee = false, AllyName = "Commander Thresh", AllyDamage = 16 });

        Ui.Header("Hollow Reprisal");
        if (outcome == BattleOutcome.Victory)
        {
            Game.Set(WorldFlags.ThreshSaved);
            Game.World.Log($"{Game.Hero.Name} saved Commander Thresh from the Court's Sentinels.");
            Ui.Narrate("Thresh sits down hard on an upturned bucket and laughs until it turns into coughing.");
            Ui.Say("Thresh", "Twelve years on the border and nobody ever came back for me. Not once. ...When you go up to that plateau, I'm coming. Don't argue.");
            Ui.World("Thresh will fight beside whichever hero climbs the Ashen Throne.");
            Rewards.CompleteQuest(QuestLog.HollowReprisal);
        }
        else
        {
            Game.Set(WorldFlags.ThreshLost);
            Game.World.Log("The Court's Sentinels killed Commander Thresh.");
            Ui.Narrate("By the time anyone can return to the ridge, it is quiet. The Sentinels have gone. They left the camp exactly as it fell, and Thresh where he stood, facing the right way.");
            Ui.World("Commander Thresh is dead. The Court does not leave loose ends.");
            QuestLog.Complete(QuestLog.HollowReprisal, Game.Hero);
        }
        Game.Save();
        Ui.Pause();
    }
}
