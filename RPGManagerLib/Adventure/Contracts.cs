using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Combat;
using RPGManagerLib.UI;

namespace RPGManagerLib.Adventure;

/// <summary>
/// One fight inside a contract.
/// </summary>
/// <param name="Title">Screen title.</param>
/// <param name="Intro">Narration before the fight.</param>
/// <param name="Enemies">Enemy ids. Prefix "+" for a Veteran, "!" for an Elite.</param>
/// <param name="BossName">Optional name for the first enemy.</param>
public record ContractStage(string Title, string Intro, string[] Enemies, string? BossName = null);

/// <summary>
/// A posting only one class can take. Each one changes something in the world for every hero,
/// which is the reason to keep more than one hero on the roster.
/// </summary>
public class Contract
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";

    /// <summary>The only class that can take it.</summary>
    public string HeroClass { get; init; } = "";

    /// <summary>"Easy", "Hard" or "Deadly".</summary>
    public string Difficulty { get; init; } = "";
    public int MinLevel { get; init; }
    public string Poster { get; init; } = "";
    public string Pitch { get; init; } = "";
    public List<ContractStage> Stages { get; init; } = new();

    /// <summary>The stat tested before the last fight.</summary>
    public HeroStat CheckStat { get; init; }
    public int CheckNeeded { get; init; }
    public string CheckSuccess { get; init; } = "";
    public string CheckFail { get; init; } = "";

    public string Outcome { get; init; } = "";

    /// <summary>What changes for every hero of the Keeper.</summary>
    public string WorldEffect { get; init; } = "";
    public int Gold { get; init; }
    public int Xp { get; init; }
    public int Reputation { get; init; }

    /// <summary><see cref="Items.ItemForge"/> id of a reward, if any.</summary>
    public string? Item { get; init; }

    /// <summary>What Sera tells the Keeper's other heroes afterwards.</summary>
    public string SeraRumour { get; init; } = "";

    /// <summary>World flag set when the contract is done.</summary>
    public string Flag => $"Contract:{Id}";
}

/// <summary>
/// The board of class-only postings at the Dustfall Inn, from easy to deadly.
/// </summary>
public static class Contracts
{
    public const string HoldTheGate = "hold_the_gate";
    public const string BlackGullyBoar = "black_gully_boar";
    public const string SleepingWarden = "sleeping_warden";
    public const string RestlessSpring = "restless_spring";
    public const string UnfinishedLecture = "unfinished_lecture";
    public const string InquisitorsLedger = "inquisitors_ledger";
    public const string EyesOnTheRoad = "eyes_on_the_road";
    public const string ThePaymaster = "the_paymaster";
    public const string WingsOverTheThrone = "wings_over_the_throne";

    public static readonly IReadOnlyList<Contract> All = new List<Contract>
    {
        // ------------------------------------------------------------ Warrior
        new()
        {
            Id = HoldTheGate, Title = "Hold the Gate", HeroClass = "Warrior", Difficulty = "Easy", MinLevel = 2, Poster = "Sera Voss",
            Pitch = "Cinder goblins have found the gap in the east palisade. They come at dusk, in two waves, and the militia is four farmers and a pitchfork. Somebody needs to stand in the gap. Standing there is the whole job.",
            Stages =
            {
                new("Hold the Gate — First Wave", "Dusk. The gap in the palisade is exactly one person wide, and tonight that person is you. Small shapes come chittering out of the ash.", new[] { Bestiary.CinderGoblin, Bestiary.CinderGoblin, Bestiary.GoblinSlinger }),
                new("Hold the Gate — Second Wave", "The first wave breaks. The second has a leader: a goblin in a dented kettle helm, shrieking orders nobody obeys.", new[] { "+" + Bestiary.CinderGoblin, Bestiary.CinderGoblin, Bestiary.GoblinSlinger }, "Kettle-Helm"),
            },
            CheckStat = HeroStat.Guard, CheckNeeded = 7,
            CheckSuccess = "You plant your feet and take the leader's charge square on your guard. It bounces. The goblins behind it stop to reconsider.",
            CheckFail = "The leader's charge knocks you a step back. You hold the gap, barely.",
            Outcome = "By full dark the gap is choked with kiln-tile armour and nobody is coming through it. The four farmers have watched all of it from behind a cart. One of them asks, very politely, if you would show them how to hold a spear.",
            WorldEffect = "The militia now drills at the gate you held. Sera has decided the Keeper's heroes sleep free at the Dustfall Inn — all of them, always.",
            Gold = 60, Xp = 90, Reputation = 3,
            SeraRumour = "There was a warrior here not long ago — {name}. Stood in the east gap alone for two waves of goblins. The militia won't shut up about it. That's why your bed is free, by the way."
        },
        new()
        {
            Id = BlackGullyBoar, Title = "Old Ironside", HeroClass = "Warrior", Difficulty = "Hard", MinLevel = 5, Poster = "Korin Ashwell",
            Pitch = "There's a boar in Black Gully the size of my anvil and about as soft. It's gutted two mules and a tinker. I want its hide for bellows and its tusks for handles. Bring it down and I'll remember it in my prices. — K.A.",
            Stages =
            {
                new("Black Gully", "The gully is a slot of black rock with one way in. Two of the old boar's sows are rooting at the mouth of it, and they do not care for visitors.", new[] { Bestiary.AshbackBoar, Bestiary.AshbackBoar }),
                new("Old Ironside", "Something at the back of the gully stands up, and keeps standing up. Its hide is plated with cooled slag. One tusk is broken. The other is not.", new[] { "!" + Bestiary.AshbackBoar }, "Old Ironside"),
            },
            CheckStat = HeroStat.Might, CheckNeeded = 14,
            CheckSuccess = "It charges. You do not move. You catch the tusk on your crossguard, twist, and three hundred pounds of boar goes over on its side with a look of pure astonishment.",
            CheckFail = "It charges. You get out of the way, which is the best that can be said.",
            Outcome = "It takes you the rest of the day to drag the hide back. Korin walks round it twice without speaking, which from Korin is a standing ovation.",
            WorldEffect = "Korin has hide for bellows and tusk for handles. His prices drop a tenth — for every hero the Keeper sends through his door.",
            Gold = 150, Xp = 260, Reputation = 5,
            SeraRumour = "A warrior called {name} dragged a boar-hide through my taproom last week. The whole hide. Korin's been in a good mood since. It's unsettling."
        },
        new()
        {
            Id = SleepingWarden, Title = "The Sleeping Warden", HeroClass = "Warrior", Difficulty = "Deadly", MinLevel = 9, Poster = "Lira the Ashen",
            Pitch = "The academy's great door-warden has woken under the east wall and is asking everyone for a pass. Nobody has a pass. It was made to stop sieges. Spells slide off it; it was built by mages to keep out mages. It will need to be argued with by hand. — L.",
            Stages =
            {
                new("The East Undercroft", "The undercroft is knee-deep in slag, and the slag has residents. Two beetles the size of handcarts turn toward your lantern.", new[] { Bestiary.SlagBeetle, Bestiary.SlagBeetle }),
                new("The Sleeping Warden", "It fills the corridor: fused brick, bronze bands, a face like a furnace door. \"PASS,\" it says, in a voice that drops mortar from the ceiling. \"PRESENT. YOUR. PASS.\"", new[] { "!" + Bestiary.RuinGolem }, "The Door-Warden"),
            },
            CheckStat = HeroStat.Might, CheckNeeded = 20,
            CheckSuccess = "There is a seam where the bronze bands meet. You put your whole weight into it before it finishes its sentence. Something inside the Warden goes CLUNK, and it lists to one side.",
            CheckFail = "You look for a weak point and find only brick. This is going to be the long way.",
            Outcome = "It comes apart like a kiln being dismantled. In the rubble sits its heart: a core of something that is still, after two hundred years, exactly as hot as a good forge.",
            WorldEffect = "The Warden's core now burns in Korin's forge. Reforging no longer needs Scraps — for any hero.",
            Gold = 350, Xp = 600, Reputation = 8, Item = "wardens_maul",
            SeraRumour = "A warrior of yours — {name} — took apart the thing under the academy's east wall. By hand. Lira came into town to tell me. Lira. Came into town."
        },

        // ------------------------------------------------------------ Mage
        new()
        {
            Id = RestlessSpring, Title = "The Restless Spring", HeroClass = "Mage", Difficulty = "Easy", MinLevel = 2, Poster = "Sera Voss",
            Pitch = "The hot spring has started spitting sparks, and yesterday one of the sparks got up and walked. I need someone who speaks fire to go down to the source and tell it to stop. I run an inn. I can't have the bathwater biting people.",
            Stages =
            {
                new("The Spring-Head", "Under the bath-house the spring comes up through a crack in the rock. The crack is glowing. Something small and bright is sitting in the steam, humming.", new[] { Bestiary.EmberSprite }),
                new("The Vent", "The first one was a scout. Deeper in, where the water boils, two more are widening the crack with their hands.", new[] { Bestiary.EmberSprite, Bestiary.EmberSprite }),
            },
            CheckStat = HeroStat.Arcana, CheckNeeded = 10,
            CheckSuccess = "You feel the vent's rhythm — a slow pulse, like breath — and time your working to the exhale. The sprites gutter, suddenly short of air.",
            CheckFail = "The vent's heat is erratic and you cannot find its rhythm. You will have to do this by force.",
            Outcome = "You close the crack the way you would close a book. The spring settles. By evening the bath-house smells of sulphur and soap again, and nothing in it has opinions.",
            WorldEffect = "The spring runs clean and oddly potent. Ember Salve and Mana Draughts brewed from it sell at half price — to every hero.",
            Gold = 60, Xp = 90, Reputation = 3,
            SeraRumour = "A mage came through not long ago — {name}. Went under my bath-house and had a word with the spring. It's behaved ever since. I'd never have believed I'd owe my plumbing to your lot."
        },
        new()
        {
            Id = UnfinishedLecture, Title = "The Unfinished Lecture", HeroClass = "Mage", Difficulty = "Hard", MinLevel = 6, Poster = "Lira the Ashen",
            Pitch = "Magister Oren has been giving the same lecture for two hundred years because nobody has ever answered his last question. It wants a mage. He will not hear the answer from anyone else; he was a dreadful snob. Go down. Let him finish. — L.",
            Stages =
            {
                new("The Antechamber", "Oren's teaching assistants still guard the door of the great hall, as they did in life: one in ash, one in flame, both checking for late arrivals.", new[] { Bestiary.AshWraith, Bestiary.EmberSprite }),
                new("The Unfinished Lecture", "\"— and so,\" says the figure at the lectern, \"what do the seals hold shut? Anyone? ...Anyone.\" It has asked ten thousand empty halls. Tonight there is someone in the front row.", new[] { "+" + Bestiary.AshWraith }, "Magister Oren"),
            },
            CheckStat = HeroStat.Arcana, CheckNeeded = 17,
            CheckSuccess = "You answer him. Not correctly — nobody knows the correct answer — but in the right form, cited, with a counter-argument. The wraith stops. Its fire dims to a reading-lamp glow.",
            CheckFail = "You try to answer and he talks straight over you. Two centuries of tenure. He is not going to be reasoned with.",
            Outcome = "When it is over the ash settles on the lectern in the shape of a hand, palm up, as if offering the floor. Then even that is gone. Somewhere above, for the first time, a bell that melted two hundred years ago finishes ringing.",
            WorldEffect = "The halls are quieter and the road to the ruins is safer. Lira, grateful, will temper coldfire for nothing — for any hero the Keeper sends.",
            Gold = 150, Xp = 260, Reputation = 5,
            SeraRumour = "Lira says a mage of yours — {name} — went down and let some dead professor finish his sentence. She cried, apparently. I didn't know she could."
        },
        new()
        {
            Id = InquisitorsLedger, Title = "The Inquisitor's Ledger", HeroClass = "Mage", Difficulty = "Deadly", MinLevel = 10, Poster = "an unsigned note in Dren's handwriting",
            Pitch = "The Court keeps a field scholar in the hills above the ruins: an Inquisitor. Every ward the Sentinels wear is written in that one's ledger. It is warded against blades and against theft. It is not warded against someone who can read it. I didn't write this. I was never here.",
            Stages =
            {
                new("The Hill Camp", "A neat tent, a folding desk, a single lamp. And between you and it, a Sentinel who has been standing so still that frost has formed on its pauldrons.", new[] { Bestiary.CourtSentinel }),
                new("The Inquisitor", "The figure at the desk does not look up. \"You are the anomaly from the plateau reports,\" it says, still writing. \"Do sit. This will only take a moment.\" The second Sentinel steps out of the tent.", new[] { Bestiary.CourtInquisitor, Bestiary.CourtSentinel }),
            },
            CheckStat = HeroStat.Arcana, CheckNeeded = 24,
            CheckSuccess = "You read the Inquisitor's wards before it finishes raising them — three sigils, nested, the middle one lazy. You unpick the middle one. The whole lattice sags.",
            CheckFail = "The wards go up faster than you can read them. Layered, professional, without a visible seam.",
            Outcome = "The ledger is bound in grey leather and written in a cipher that takes you an hour and a headache. Then it opens like a flower: every ward-sigil the Court issues in the Ashlands, with the counter-stroke noted neatly beside each. Scholars. They cannot help showing their working.",
            WorldEffect = "The Court's wards are an open book. Every Court soldier any of your heroes meets starts the fight already weakened.",
            Gold = 350, Xp = 600, Reputation = 8, Item = "inquisitors_rod",
            SeraRumour = "A mage called {name} came back from the hills with a grey book and a look I didn't like. Since then I hear the black-and-gold ones go down easier. I don't ask."
        },

        // ------------------------------------------------------------ Archer
        new()
        {
            Id = EyesOnTheRoad, Title = "Eyes on the Road", HeroClass = "Archer", Difficulty = "Easy", MinLevel = 2, Poster = "Sera Voss",
            Pitch = "Every ambush between here and the Path starts from the same goblin warren, and nobody can tell me where it is, because everyone who goes looking clanks. I need someone quiet with good eyes to find it, mark the runs, and come back. I used to do this sort of work. It's a young person's game. Bring your knees back intact.",
            Stages =
            {
                new("The Runs", "Goblin runs are easy to follow once you stop looking for a path and start looking for where the ash has been swept. Two slingers are perched on a boulder, supposedly on watch, actually arguing.", new[] { Bestiary.GoblinSlinger, Bestiary.GoblinSlinger }),
                new("The Warren Mouth", "The warren is a collapsed kiln. You have what you came for — and then a goblin with an armful of stolen washing walks round the corner and looks straight at you.", new[] { Bestiary.CinderGoblin, Bestiary.GoblinSlinger, Bestiary.CinderGoblin }),
            },
            CheckStat = HeroStat.Perception, CheckNeeded = 10,
            CheckSuccess = "You saw the washing-thief's shadow a full breath before it saw you. Your first arrow is already on the string.",
            CheckFail = "It sees you at the same moment you see it. It screams. Things answer.",
            Outcome = "You come back with the whole warren sketched on the inside of your quiver: every run, every bolt-hole, every boulder they like to sit on. Sera copies it out twice and nails one to the gate.",
            WorldEffect = "The goblin runs are marked. Ambushes on the roads near Cinder's Rest and the Scorched Path are half as likely — for every hero.",
            Gold = 60, Xp = 90, Reputation = 3,
            SeraRumour = "An archer was through here not long ago — {name}. Found the goblin warren I've been after for two years, and drew me a better map of it than I could have. I've nailed it to the gate. Mind you read it."
        },
        new()
        {
            Id = ThePaymaster, Title = "The Paymaster", HeroClass = "Archer", Difficulty = "Hard", MinLevel = 6, Poster = "Sera Voss",
            Pitch = "Somebody's wages come up the Scorched Path every second month in a covered cart with too many guards. Smooth coins. I want that cart stopped before it reaches whoever it's for. It'll want to be done from the rocks, at range, by someone who doesn't miss. The coin goes into the town's bounty chest. Your cut is generous.",
            Stages =
            {
                new("The Outriders", "You pick your ledge the night before. At noon the outriders come through, scanning the rocks, looking everywhere but up.", new[] { Bestiary.BanditBowman, Bestiary.Bandit, Bestiary.BanditBowman }),
                new("The Pay-Cart", "The cart stops when its outriders don't report. A man in a good cloak climbs down, sighs, and says something quiet. The two guards who step out from under the canvas are not bandits. They are just dressed like them.", new[] { Bestiary.BanditCutthroat, Bestiary.BanditEnforcer }),
            },
            CheckStat = HeroStat.Perception, CheckNeeded = 15,
            CheckSuccess = "You wait for the moment the cutthroat glances at the cloak for orders — and put a shaft through the gap in his collar before he looks back.",
            CheckFail = "The cutthroat never takes his eyes off the rocks. He knows exactly what kind of ledge an archer likes.",
            Outcome = "The man in the cloak is gone when the dust settles; you never saw him run. But the strongbox is there, and it takes two people to lift. Smooth coins. No king's head. Sera weighs them without comment and locks them in the bounty chest.",
            WorldEffect = "The town's bounty chest is full of the Court's own gold. Every bounty on the board now pays half again as much — to any hero.",
            Gold = 150, Xp = 260, Reputation = 5,
            SeraRumour = "The bounties pay better these days. That's an archer's doing — {name}. Stopped a pay-cart on the Path from four hundred paces. I counted the coins myself. Twice, for the pleasure of it."
        },
        new()
        {
            Id = WingsOverTheThrone, Title = "Wings over the Throne", HeroClass = "Archer", Difficulty = "Deadly", MinLevel = 11, Poster = "Sera Voss",
            Pitch = "There is a drake nesting in the dragon's ribcage on the plateau. Not a dragon. A drake: a dragon's stupid cousin, the size of my inn. It can't be reached with a blade and it shrugs off fire. It can be shot. I'm told the old dragon-cult hunters used to take them through the eye. I'm told a lot of things. Don't die.",
            Stages =
            {
                new("The Updraught", "The warm air that keeps the drake aloft also feeds everything else on the plateau. Two ember hounds are denning at the foot of the ribs, and they smell you long before you would like.", new[] { Bestiary.EmberHound, Bestiary.EmberHound }),
                new("Wings over the Throne", "It unfolds from the dragon's ribcage like a tent catching the wind: grey, scarred, vast, and very annoyed. The plateau goes dark under its wings.", new[] { Bestiary.AshDrake }),
            },
            CheckStat = HeroStat.Perception, CheckNeeded = 22,
            CheckSuccess = "It banks. For half a heartbeat the eye is side-on to you, wet and gold and the size of a dinner plate. You do not think. You loose.",
            CheckFail = "It never holds still long enough for the shot the old hunters talked about. This will be the long way: a hundred arrows instead of one.",
            Outcome = "It comes down across the dragon's bones like a collapsing roof, and the plateau shakes. When the dust clears, the big skull above you seems — and it must be the light — to be very slightly smiling.",
            WorldEffect = "Drake-fat salve is now sold in Cinder's Rest by the jar. Every hero of the Keeper is half as likely to be set burning.",
            Gold = 350, Xp = 650, Reputation = 8, Item = "drakebone_bow",
            SeraRumour = "I posted the drake as a joke. Nobody was meant to take it. An archer called {name} took it. The thing is dead in the dragon's ribs and I owe that archer a great deal of money."
        },
    };

    static Contracts()
    {
        foreach (var contract in All)
            Gossip.Register(new Rumour(contract.Flag, Gossip.Sera, contract.SeraRumour));
    }

    /// <summary>Has any hero completed this contract?</summary>
    public static bool Done(string id) => Game.Has($"Contract:{id}");

    /// <summary>Makes sure the contracts' rumours are registered before anyone gossips.</summary>
    public static void EnsureLoaded() { }

    /// <summary>The board at the Dustfall Inn.</summary>
    public static void Board()
    {
        while (true)
        {
            var hero = Game.Hero;
            Ui.Header("Postings for Particular Talents");
            Ui.Narrate("A second board, behind the bar, where Sera pins the jobs that cannot be done by just anyone. Each one names the kind of person it needs. Each one, done, changes something for everybody.");

            var options = All.Select(c =>
            {
                string status = Done(c.Id)
                    ? $"✔ done by {Game.World.DoneBy(c.Flag) ?? "one of yours"}"
                    : c.HeroClass == hero.CharacterType ? $"level {c.MinLevel}+" : $"needs a {c.HeroClass}";
                return $"[{c.HeroClass} · {c.Difficulty}] {c.Title} — {status}";
            }).ToList();

            int choice = Ui.Choose(options, "Leave the board");
            if (choice < 0) return;
            Read(All[choice]);
        }
    }

    private static void Read(Contract contract)
    {
        var hero = Game.Hero;
        Ui.Header(contract.Title);
        Ui.Line($"  Wanted: a {contract.HeroClass}.   Difficulty: {contract.Difficulty}.   Recommended level: {contract.MinLevel}+", ConsoleColor.DarkYellow);
        Ui.Line($"  Posted by {contract.Poster}", ConsoleColor.DarkGray);
        Ui.Line();
        Ui.Narrate("\"" + contract.Pitch + "\"");
        Ui.Line();
        Ui.Gain($"Reward: {contract.Gold} gold, {contract.Xp} XP{(contract.Item != null ? ", and something worth carrying" : "")}.");

        if (Done(contract.Id))
        {
            Ui.Line();
            Ui.Good($"Already done{(Game.World.DoneBy(contract.Flag) is string name ? $" — by {name}" : "")}.");
            Ui.World(contract.WorldEffect);
            Ui.Pause();
            return;
        }

        if (hero.CharacterType != contract.HeroClass)
        {
            Ui.Line();
            Ui.Narrate($"You read it twice. This is not work for {(hero is Archer ? "an archer" : "a " + hero.CharacterType.ToLowerInvariant())}.");
            var able = Game.Roster.Heroes.FirstOrDefault(h => h.CharacterType == contract.HeroClass);
            Ui.World(able != null
                ? $"{able.Name} could take this. (Keeper's menu → Roster.)"
                : $"None of your heroes is a {contract.HeroClass}. A new bond could change that.");
            Ui.Pause();
            return;
        }

        Ui.Line();
        if (hero.Level < contract.MinLevel) Ui.Bad($"This is meant for level {contract.MinLevel} and up. {hero.Name} is level {hero.Level}.");
        if (!Ui.Confirm("Take the posting and set out now?")) return;
        Run(contract);
    }

    private static List<Enemy> Build(ContractStage stage)
    {
        var enemies = stage.Enemies.Select(spec =>
        {
            var enemy = Bestiary.Create(spec.TrimStart('+', '!'));
            if (spec.StartsWith('!')) Bestiary.Promote(enemy, EnemyRank.Elite);
            else if (spec.StartsWith('+')) Bestiary.Promote(enemy, EnemyRank.Veteran);
            return enemy;
        }).ToList();

        if (stage.BossName != null) enemies[0].Name = stage.BossName;
        foreach (var same in enemies.GroupBy(e => e.Name).Where(g => g.Count() > 1))
        {
            int n = 0;
            foreach (var enemy in same) enemy.Name = $"{enemy.Name} {(char)('A' + n++)}";
        }
        return enemies;
    }

    private static void Run(Contract contract)
    {
        var hero = Game.Hero;
        for (int i = 0; i < contract.Stages.Count; i++)
        {
            var stage = contract.Stages[i];
            bool last = i == contract.Stages.Count - 1;
            var options = new BattleOptions();

            if (last)
            {
                int value = contract.CheckStat switch
                {
                    HeroStat.Might => hero.Might,
                    HeroStat.Guard => hero.Guard,
                    HeroStat.Arcana => hero.Arcana,
                    _ => hero.Perception
                };
                Ui.Header(stage.Title);
                Ui.Narrate(stage.Intro);
                Ui.Line();
                if (value >= contract.CheckNeeded)
                {
                    Ui.Good(contract.CheckSuccess);
                    Ui.Note($"  ({contract.CheckStat} {value} — needed {contract.CheckNeeded}. The enemy starts Exposed.)");
                    options.OpeningExposedTurns = 4;
                }
                else
                {
                    Ui.Bad(contract.CheckFail);
                    Ui.Note($"  ({contract.CheckStat} {value} — {contract.CheckNeeded} would have given you an opening.)");
                }
                Ui.Pause();
            }

            var outcome = Encounters.Fight(stage.Title, last ? "There is nothing left to do but finish it." : stage.Intro, Build(stage), options);
            if (outcome != BattleOutcome.Victory)
            {
                Ui.Line();
                Ui.Note("  The posting stays on the board. It will be there when you are ready.");
                Ui.Pause();
                return;
            }

            if (!last)
            {
                Ui.Line();
                HeroScreens.WriteVitals(hero);
                while (true)
                {
                    int choice = Ui.Choose(new List<string> { "Press on", "Open your pack first", "Turn back (the posting stays on the board)" });
                    if (choice == 0) break;
                    if (choice == 2) return;
                    HeroScreens.Pack();
                }
            }
        }

        Game.Set(contract.Flag);
        Game.World.Log($"{hero.Name} completed the posting \"{contract.Title}\".");
        Ui.Header(contract.Title);
        Ui.Narrate(contract.Outcome);
        Ui.Line();
        Ui.Line($"  ✔ Posting complete: {contract.Title}", ConsoleColor.Green);
        Rewards.Give(contract.Gold, contract.Xp, contract.Reputation);
        if (contract.Item != null) Rewards.GiveItem(contract.Item);
        Ui.Line();
        Ui.World(contract.WorldEffect);
        Game.Save();
        Ui.Pause();
    }
}
