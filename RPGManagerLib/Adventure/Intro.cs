using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.UI;
using RPGManagerLib.Worlds;

namespace RPGManagerLib.Adventure;

/// <summary>
/// The opening: the Keeper wakes, bonds a first hero, and arrives in Cinder's Rest.
/// </summary>
public static class Intro
{
    public static void Play()
    {
        Ui.Header("The Keeper Wakes");
        Ui.Vision("You have no body. You have never needed one.");
        Ui.Vision("You have been asleep — if that is the word — for longer than the kingdoms below have had names. Something has thinned the Veil enough to wake you. It smells of smoke.");
        Ui.Line();
        Ui.Narrate("Below you lie the Ashlands: the bones of the Emberveil Kingdom, which burned itself to death two centuries ago and has not finished smouldering. Grey sky. Grey ground. A handful of stubborn lights.");
        Ui.Line();
        Ui.Vision("You cannot lift a stone in this world. You cannot speak a word in it. But you can hold on to a mortal — see through their eyes, lean on their choices — and through them, do almost anything.");
        Ui.Vision("And you remember, dimly, that you have done this before. That last time it ended badly. You do not remember how.");
        Ui.Pause();

        var hero = CharacterFactory.CreateCharacter();
        CharacterFactory.GiveStartingSupplies(hero);
        Game.Roster.TryAdd(hero);
        Game.World.Log($"The Keeper woke in the Ashlands and bonded with {hero.Name} the {hero.CharacterType}.");
        Ui.Pause();

        Ui.Header("Cinder's Rest");
        Ui.Narrate($"{hero.Name} walks out of the ash-fall and into the only town for three days in any direction: forty souls, a hot spring that stinks of sulphur, and buildings made of volcanic stone and other people's scrap.");
        Ui.Narrate("The inn has a sign with no picture on it, just a word burned into the wood: DUSTFALL. Inside, a woman with grey in her braid and a stiff left leg is drying a mug. She looks up, and keeps looking a moment longer than is polite.");
        Ui.Line();
        Ui.Say("Sera", "Huh. You've got the look.");
        Ui.Say(hero.Name, "What look?");
        Ui.Say("Sera", "Like there's someone standing just behind your eyes, reading over your shoulder. Saw it once before, in the east. Fellow who had it tipped well and died badly.");
        Ui.Say("Sera", hero switch
        {
            Warrior => "You carry yourself like you know which end of that goes in the other person. Good. I've got dogs that need killing.",
            Mage => "And a staff. Wonderful. Don't set fire to anything in here — this country's had enough of that. But if you can aim it, I've got dogs that need killing.",
            _ => "Light on your feet, eyes on the exits. I used to walk into rooms like that. I've got dogs that need killing, if you're the useful sort."
        });
        Ui.Line();
        Ui.World("People here will not remember a hero. They will remember you — the Keeper. Whatever one of your heroes does, all of them will live with.");
        Game.Set(WorldFlags.IntroSeen);
        Game.Save();
        Ui.Pause();
    }
}
