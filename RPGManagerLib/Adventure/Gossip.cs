using RPGManagerLib.UI;
using RPGManagerLib.Worlds;

namespace RPGManagerLib.Adventure;

/// <summary>
/// One thing somebody (or some place) has to say about a deed done by a different hero.
/// </summary>
/// <param name="Flag">The world flag the deed set.</param>
/// <param name="Speaker">Who says it: an NPC id, or "place:..." for a location.</param>
/// <param name="Line">The text. {name} is the hero who did it, {class} their class.</param>
public record Rumour(string Flag, string Speaker, string Line);

/// <summary>
/// The shared world talking back. When the active hero meets someone who knows about a deed
/// done by one of the Keeper's <em>other</em> heroes, they bring it up — once per hero.
/// "There was an adventurer through here not long ago..."
/// </summary>
public static class Gossip
{
    public const string Sera = "sera";
    public const string Korin = "korin";
    public const string Lira = "lira";
    public const string Dren = "dren";
    public const string Thresh = "thresh";
    public const string Town = "place:town";
    public const string Path = "place:path";
    public const string Ruins = "place:ruins";
    public const string Throne = "place:throne";

    private static readonly Dictionary<string, string> SpeakerNames = new()
    {
        [Sera] = "Sera", [Korin] = "Korin", [Lira] = "Lira", [Dren] = "Dren", [Thresh] = "Thresh"
    };

    private static readonly List<Rumour> Rumours = new()
    {
        new(WorldFlags.AshlandsScavengerBounty, Town, "The gate stands open in daylight. Someone thinned the packs before you arrived — a {class} called {name}, the washerwomen say."),

        new(WorldFlags.ScorchedPathPatrolCleared, Sera, "There was a {class} through here not long ago — {name}. Broke the toll on the Path and barely mentioned it. One of yours, I'd bet my good leg."),
        new(WorldFlags.ScorchedPathPatrolCleared, Korin, "Iron's coming up the road again. Some {class} called {name} cleared the barricade. If you see them, tell them the smith says thanks. Once."),
        new(WorldFlags.ScorchedPathPatrolCleared, Path, "The barricade is kindling. A {class} passed this way not long ago — the bandits in the rocks still mutter the name: {name}."),

        new(WorldFlags.DrenMorrowAllied, Sera, "The stray in my corner? A {class} carried him in. Three miles. {name}. He hasn't stopped talking about it since."),
        new(WorldFlags.DrenMorrowAllied, Korin, "Heard {name} dragged a half-dead courier into Sera's. Soft. ...The good kind of soft."),
        new(WorldFlags.DrenMorrowAllied, Lira, "The courier boy speaks of someone called {name}. He says the name the way people say grace."),

        new(WorldFlags.DrenMorrowBetrayed, Sera, "A {class} named {name} marched a wounded man to Korin's coal store. He was gone by morning. I'm not saying it was wrong. I'm saying I noticed."),
        new(WorldFlags.DrenMorrowBetrayed, Korin, "It was a {class} brought me that courier — {name}. I took the purse. Been wishing I hadn't."),

        new(WorldFlags.HammerReturned, Sera, "Korin smiled yesterday. Frightened the children. Something about a hammer, and a {class} called {name}."),
        new(WorldFlags.WagonLooted, Path, "The broken wagon has been cracked open and its den cleared out. {name} got here first."),

        new(WorldFlags.ThreshDefeated, Sera, "They say a {class} went up Thresh's hill alone and came down alone. {name}. The road's been quiet since."),
        new(WorldFlags.ThreshDefeated, Korin, "{name} killed Thresh. Tam can rest. You're with {name}, so I'll tell you what I'd tell them: about time."),
        new(WorldFlags.ThreshDefeated, Lira, "The soldier on the ridge is dead. I felt forty men stop being afraid of one thing and start being afraid of everything. A {class} did that. {name}."),
        new(WorldFlags.ThreshDefeated, Dren, "Thresh is dead? {name} did that? He paid my wages, in a way. Smooth coins. I'm not sorry. I'm just counting how many people from that part of my life are left."),
        new(WorldFlags.ThreshDefeated, Path, "Cold watch-fires on the ridge. A {class} came through not long ago and ended Commander Thresh — {name}, the stragglers call them, and spit."),

        new(WorldFlags.ThreshNegotiated, Sera, "A {class} talked forty armed men off a hill. Talked. {name}. I've been trying to work out what was said ever since."),
        new(WorldFlags.ThreshNegotiated, Korin, "{name} let Thresh walk. I'd have hanged him. ...Tam wouldn't have wanted a hanging, though. So."),
        new(WorldFlags.ThreshNegotiated, Path, "Swept fire-pits on the ridge, latrines filled in. Someone talked Thresh down — a {class} called {name}, if the caravan drivers have it right."),

        new(WorldFlags.ThreshAlly, Thresh, "You're not {name}. But you've got {name}'s shadow behind you. Fine. Same terms: I watch the Court, you keep that innkeeper off my neck."),
        new(WorldFlags.ThreshSaved, Thresh, "It was {name} came back for us when the black-and-gold came up the ridge. I don't forget that. Whatever you are to {name}, you're that to me."),

        new(WorldFlags.EmberSealBroken, Lira, "The seal's open. A {class} did it — {name}. Came up covered in two centuries of dust and asked if I had any tea."),
        new(WorldFlags.EmberSealBroken, Sera, "The glow over the ruins changed colour. Lira sent word a {class} called {name} opened something down there. I'd like it noted I was against it."),
        new(WorldFlags.EmberSealBroken, Ruins, "Fresh bootprints lead down into the academy and back up again. A {class} passed here not long ago: {name}. What they opened is still open."),

        new(WorldFlags.EmberArtifactSecured, Lira, "{name} carries the Focus. You all do, I suppose — it's the one behind your eyes that carries it. It hums when you walk in."),
        new(WorldFlags.WraithLectureHeard, Lira, "One of yours sat through Magister Oren's entire lecture. {name}. Nobody has done that since he was alive. He'll have been so pleased."),

        new(WorldFlags.CinderHandDefeated, Korin, "They say {name} killed something on the plateau that held the whole Court's leash. A {class}, with whatever they had on them. Makes a smith feel surplus."),
        new(WorldFlags.CinderHandDefeated, Lira, "The plateau is quiet. {name} went up angry and came down thoughtful. That is the right order."),
        new(WorldFlags.CinderHandDefeated, Thresh, "{name} took the Hand. I'd have paid to see it. I'd have paid in smooth coins, even."),
        new(WorldFlags.CinderHandDefeated, Dren, "The Cinder Hand is dead. {name} did it. I keep waiting to feel safe. I think it might take a while."),
        new(WorldFlags.CinderHandDefeated, Throne, "Scorch-marks on the black glass, and a cracked gauntlet nobody has dared pick up. {name} was here."),

        new(WorldFlags.AshenThroneSat, Lira, "{name} sat in the chair. I can smell it on all of you now — old smoke. Did it tell you? It tells everyone something."),
        new(WorldFlags.AshenThroneSat, Throne, "The seat is cold. Someone has already sat here: {name}."),
        new(WorldFlags.AshenThroneRefused, Sera, "{name} was offered a throne and walked home for supper. Marrit says they spat on it. I'm not correcting her."),
    };

    /// <summary>Adds a rumour at runtime (class contracts register theirs this way).</summary>
    public static void Register(Rumour rumour)
    {
        if (!Rumours.Any(r => r.Flag == rumour.Flag && r.Speaker == rumour.Speaker)) Rumours.Add(rumour);
    }

    /// <summary>
    /// Lets a speaker bring up deeds done by the Keeper's other heroes that this hero has not heard about yet.
    /// </summary>
    /// <param name="speaker">Speaker id (one of the constants on this class).</param>
    /// <param name="max">Most rumours to tell in one go.</param>
    /// <returns>How many were told.</returns>
    public static int Tell(string speaker, int max = 2)
    {
        Contracts.EnsureLoaded();
        var hero = Game.Hero;
        var world = Game.World;
        int told = 0;

        foreach (var rumour in Rumours.Where(r => r.Speaker == speaker))
        {
            if (told >= max) break;
            string? doer = world.DoneBy(rumour.Flag);
            if (!world.Has(rumour.Flag) || doer == null || doer == hero.Name) continue;

            string heardKey = $"heard:{hero.Name}:{speaker}:{rumour.Flag}";
            if (world.Count(heardKey) > 0) continue;
            world.Counters[heardKey] = 1;

            string heroClass = Game.Roster.Heroes.FirstOrDefault(h => h.Name == doer)?.CharacterType.ToLowerInvariant() ?? "adventurer";
            string line = rumour.Line.Replace("{name}", doer).Replace("{class}", heroClass);

            if (SpeakerNames.TryGetValue(speaker, out string? name)) Ui.Say(name, line);
            else Ui.World(line);
            told++;
        }
        return told;
    }
}
