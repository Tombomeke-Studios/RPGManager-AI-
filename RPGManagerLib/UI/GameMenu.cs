using RPGManagerLib.Adventure;

namespace RPGManagerLib.UI
{
    /// <summary>
    /// The top-level game loop: loads the session, plays the opening for a new Keeper,
    /// then keeps showing the active hero's current location until the player quits.
    /// </summary>
    public static class GameMenu
    {
        /// <summary>
        /// Runs the game until the player quits or console input ends. Always saves on the way out.
        /// </summary>
        public static void Start()
        {
            if (Game.Roster.Heroes.Count == 0) Game.Load();

            try
            {
                if (Game.Roster.Heroes.Count == 0) Intro.Play();

                bool running = true;
                while (running)
                {
                    running = Game.Hero.LocationId switch
                    {
                        Travel.ScorchedPath => ScorchedPathLocation.Visit(),
                        Travel.EmberveilRuins => EmberveilRuinsLocation.Visit(),
                        Travel.AshenThrone => AshenThroneLocation.Visit(),
                        _ => CindersRestLocation.Visit()
                    };
                }
            }
            catch (InputEndedException)
            {
                // Piped input ran out: fall through to the save below.
            }

            Game.Save();
            Ui.Line();
            Ui.Vision("You loosen your hold. The ash keeps falling. Tavaryn will be there when you return.");
            Ui.Line();
        }
    }

    /// <summary>
    /// Shared frame for location screens: a status line on top, and Travel / Keeper's menu at the bottom.
    /// </summary>
    public static class LocationMenu
    {
        /// <summary>Prints the location title, its description and the active hero's vitals.</summary>
        public static void Banner(string title, string description)
        {
            var hero = Game.Hero;
            Ui.Header(title);
            Ui.Narrate(description);
            Ui.Line();
            Ui.Write($"  {hero.Name} the {hero.CharacterType}", ConsoleColor.Cyan);
            Ui.Line($"   Gold {hero.Gold}", ConsoleColor.Yellow);
            HeroScreens.WriteVitals(hero);
        }

        /// <summary>Shows the location's own options plus the common ones.</summary>
        /// <returns>False when the player chose to quit the game.</returns>
        public static bool Show(List<MenuOption> options)
        {
            bool keepPlaying = true;
            options.Add(new MenuOption("t", "Travel", "leave for another place", Travel.Menu));
            options.Add(new MenuOption("k", "Keeper's menu", "hero, pack, journal, roster, save", () => keepPlaying = HeroScreens.KeeperMenu()));
            Ui.Menu(options);
            return keepPlaying;
        }
    }
}
