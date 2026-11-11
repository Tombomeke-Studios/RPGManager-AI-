using RPGManagerLib.Combat;
using RPGManagerLib.Core;
using RPGManagerLib.Quests;
using RPGManagerLib.UI;
using RPGManagerLib.Worlds;

namespace RPGManagerLib.Adventure;

/// <summary>
/// A place a hero can stand. Pure data: other systems read it and decide what happens there.
/// </summary>
/// <param name="Id">Stable id stored on the hero.</param>
/// <param name="Name">Display name.</param>
/// <param name="Levels">Recommended level range.</param>
/// <param name="MinLevel">Level below which the game warns the player.</param>
/// <param name="EncounterChance">Chance of trouble on the way there.</param>
public record Place(string Id, string Name, string Levels, int MinLevel, double EncounterChance);

/// <summary>
/// The Ashlands map and the rules for moving around it.
/// </summary>
public static class Travel
{
    public const string CindersRest = "cinders_rest";
    public const string ScorchedPath = "scorched_path";
    public const string EmberveilRuins = "emberveil_ruins";
    public const string AshenThrone = "ashen_throne";

    public static readonly IReadOnlyList<Place> Places = new List<Place>
    {
        new(CindersRest, "Cinder's Rest", "1–3", 1, 0.15),
        new(ScorchedPath, "The Scorched Path", "2–5", 2, 0.35),
        new(EmberveilRuins, "Emberveil Ruins", "5–8", 4, 0.35),
        new(AshenThrone, "The Ashen Throne", "8–10", 7, 0.40),
    };

    /// <summary>Display name of a location id.</summary>
    public static string NameOf(string id) => Places.FirstOrDefault(p => p.Id == id)?.Name ?? "somewhere in the ash";

    /// <summary>Whether the shared world has opened the way to a place.</summary>
    public static bool IsUnlocked(string id) => id switch
    {
        CindersRest => true,
        ScorchedPath => Game.Has(WorldFlags.ScorchedPathOpen),
        EmberveilRuins => QuestLog.Started(QuestLog.CommandersPrice),
        AshenThrone => Game.Has(WorldFlags.EmberSealBroken),
        _ => false
    };

    /// <summary>Lets the player pick a destination, rolls for trouble on the road, and moves the hero.</summary>
    public static void Menu()
    {
        var hero = Game.Hero;
        Ui.Header("Travel");
        Ui.Narrate($"{hero.Name} is at {NameOf(hero.LocationId)}.");

        var open = Places.Where(p => p.Id != hero.LocationId && IsUnlocked(p.Id)).ToList();
        if (open.Count == 0)
        {
            Ui.Line();
            Ui.Narrate("Beyond the gates there is only ash and the howling of dogs. Nobody in town will point you down the road until you have proven you can survive the doorstep.");
            Ui.Note("(Sera Voss at the Dustfall Inn has work.)");
            Ui.Pause();
            return;
        }

        int choice = Ui.Choose(open.Select(p => $"{p.Name}  (levels {p.Levels})").ToList(), "Stay here");
        if (choice < 0) return;
        var destination = open[choice];

        if (hero.Level < destination.MinLevel &&
            !Ui.Confirm($"{destination.Name} is dangerous for a level {hero.Level} hero. Go anyway?")) return;

        if (!RoadEncounter(destination)) return;

        hero.LocationId = destination.Id;
        Game.Save();
        OnArrival(destination.Id);
    }

    /// <summary>Rolls a road encounter. Returns false if the hero was beaten and carried home.</summary>
    private static bool RoadEncounter(Place destination)
    {
        var hero = Game.Hero;
        double chance = destination.EncounterChance;
        bool banditRoad = destination.Id is ScorchedPath or CindersRest or EmberveilRuins;
        if (banditRoad && Prices.RoadIsOpen) chance *= 0.4;
        if (destination.Id is ScorchedPath or CindersRest && Contracts.Done(Contracts.EyesOnTheRoad)) chance *= 0.5;
        if (destination.Id == EmberveilRuins && Contracts.Done(Contracts.UnfinishedLecture)) chance *= 0.5;
        if (!Rng.Chance(chance)) return true;

        List<Enemy> enemies;
        string intro;
        switch (destination.Id)
        {
            case CindersRest:
                enemies = Encounters.ScavengerPack(hero.Level);
                intro = "Almost home. Then grey shapes peel away from the grey ground, and the pack is on you.";
                break;
            case ScorchedPath:
                enemies = hero.Level < 3 ? Encounters.ScavengerPack(hero.Level) : Encounters.BanditGroup(hero.Level);
                intro = "The canyon narrows. A pebble skitters down the black rock — and then you are not alone on the road.";
                break;
            case EmberveilRuins:
                enemies = Rng.Chance(0.5) ? Encounters.BanditGroup(hero.Level) : Encounters.RuinsGroup(hero.Level);
                intro = "The road to the ruins runs past cracked ground where the earth breathes heat. Something has been waiting in the shimmer.";
                break;
            default:
                enemies = Encounters.PlateauGroup(hero.Level);
                intro = "The climb toward the plateau is watched. You knew it would be.";
                break;
        }

        return Encounters.Fight("Ambush on the Road", intro, enemies) != BattleOutcome.Defeat;
    }

    /// <summary>Story events that fire the moment a hero arrives somewhere.</summary>
    private static void OnArrival(string id)
    {
        if (id != ScorchedPath) return;

        if (QuestLog.IsActive(QuestLog.StrangersBurden) && !Game.Has(WorldFlags.DrenMorrowFound))
            ScorchedPathLocation.FindDren();

        if (Game.Has(WorldFlags.ThreshAlly) && Game.Has(WorldFlags.AshlandsCourtFound) && Game.Has(WorldFlags.EmberSealBroken) &&
            !QuestLog.Started(QuestLog.HollowReprisal) && !Game.Has(WorldFlags.CinderHandDefeated))
            ScorchedPathLocation.ReprisalBegins();
    }
}
