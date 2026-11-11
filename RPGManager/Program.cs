using RPGManagerLib.Adventure;
using RPGManagerLib.Combat;
using RPGManagerLib.UI;

namespace RPGManager
{
    /// <summary>
    /// Console entry point for the RPG Manager demo application.
    /// </summary>
    /// <remarks>
    /// Displays a styled intro screen and launches the main game menu.
    /// </remarks>
    internal class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        /// <param name="args">Optional command line arguments (unused).</param>
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            if (args.Contains("--balance"))
            {
                BalanceSimulator.Run();
                return;
            }

            Game.Load();
            StartupScreens.Show(Game.Roster.Heroes);
            GameMenu.Start();
        }
    }
}
