using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Core;
using RPGManagerLib.Items;
using RPGManagerLib.UI;

namespace RPGManagerLib.Combat;

/// <summary>
/// Developer tool: plays each key fight many times with a simple bot and prints win rates.
/// Run with <c>dotnet run --project RPGManager -- --balance</c>.
/// </summary>
public static class BalanceSimulator
{
    private record Scenario(string Name, int Level, string[] Enemies, int Potions = 2, bool Upgraded = false, BattleOptions? Options = null);

    private static readonly Scenario[] Scenarios =
    {
        new("2 scavengers", 1, new[] { Bestiary.AshScavenger, Bestiary.AshScavenger }),
        new("3 scavengers", 2, new[] { Bestiary.AshScavenger, Bestiary.AshScavenger, Bestiary.AshScavenger }),
        new("2 bandits", 3, new[] { Bestiary.Bandit, Bestiary.Bandit }),
        new("enforcer + bowman", 3, new[] { Bestiary.BanditEnforcer, Bestiary.BanditBowman }),
        new("enforcer + bowman (geared)", 4, new[] { Bestiary.BanditEnforcer, Bestiary.BanditBowman }, 3, true),
        new("2 sprites", 5, new[] { Bestiary.EmberSprite, Bestiary.EmberSprite }, 2, true),
        new("wraith + sprite", 5, new[] { Bestiary.AshWraith, Bestiary.EmberSprite }, 3, true),
        new("sentinel (calls 1)", 6, new[] { Bestiary.CourtSentinel }, 3, true),
        new("Thresh + enforcer", 6, new[] { Bestiary.Thresh, Bestiary.BanditEnforcer }, 4, true),
        new("Thresh + enforcer", 7, new[] { Bestiary.Thresh, Bestiary.BanditEnforcer }, 4, true),
        new("2 sentinels", 8, new[] { Bestiary.CourtSentinel, Bestiary.CourtSentinel }, 4, true),
        new("Cinder Hand", 8, new[] { Bestiary.CinderHand }, 4, true),
        new("Cinder Hand", 9, new[] { Bestiary.CinderHand }, 4, true),
        new("Cinder Hand (Focus held)", 9, new[] { Bestiary.CinderHand }, 4, true, new BattleOptions { FocusDenied = true }),
    };

    /// <summary>Runs every scenario for every class and prints a table.</summary>
    public static void Run(int runs = 400)
    {
        Console.WriteLine($"{"Scenario",-30}{"Lvl",4}  {"Warrior",-20}{"Mage",-20}{"Archer",-20}");
        foreach (var scenario in Scenarios)
        {
            Console.Write($"{scenario.Name,-30}{scenario.Level,4}  ");
            foreach (string heroClass in new[] { "Warrior", "Mage", "Archer" })
            {
                int wins = 0;
                double healthLeft = 0, rounds = 0, potions = 0;
                Ui.Silent = true;
                for (int i = 0; i < runs; i++)
                {
                    var hero = Build(heroClass, scenario);
                    var options = scenario.Options == null ? new BattleOptions() : new BattleOptions { FocusDenied = scenario.Options.FocusDenied };
                    options.CanFlee = false;
                    var result = new Battle(hero, Bestiary.Group(scenario.Enemies), new BotBattleController(), options).Run();
                    rounds += result.Rounds;
                    potions += scenario.Potions - hero.Supplies.Values.Sum();
                    if (result.Outcome != BattleOutcome.Victory) continue;
                    wins++;
                    healthLeft += hero.Health / hero.MaxHealth;
                }
                Ui.Silent = false;
                string cell = $"{100.0 * wins / runs,3:0}% hp{(wins == 0 ? 0 : 100 * healthLeft / wins),3:0} p{potions / runs:0.0} r{rounds / runs:0}";
                Console.Write($"{cell,-20}");
            }
            Console.WriteLine();
        }
    }

    private static Character Build(string heroClass, Scenario scenario)
    {
        Character hero = heroClass switch
        {
            "Mage" => new Mage("Sim"),
            "Archer" => new Archer("Sim"),
            _ => new Warrior("Sim")
        };
        hero.SetLevel(scenario.Level);

        string[] gear = (heroClass, scenario.Upgraded) switch
        {
            ("Warrior", false) => new[] { "basic_sword" },
            ("Warrior", true) => new[] { scenario.Level >= 7 ? "battle_axe" : "broad_sword" },
            ("Mage", false) => new[] { "basic_staff" },
            ("Mage", true) => new[] { "rimewood_staff" },
            (_, false) => new[] { "simple_bow", "small_quiver", "basic_dagger" },
            _ => new[] { "hunting_bow", "medium_quiver", "basic_dagger" },
        };
        foreach (string id in gear) hero.Weapons.Add(ItemForge.Create(id));
        if (hero is Mage && scenario.Level >= 6) hero.LearnedSkills.Add(SkillBook.Unbinding);

        hero.AddSupply(scenario.Level >= 7 ? SupplyCatalog.GreaterHealingPotion : SupplyCatalog.HealingPotion, scenario.Potions);
        return hero;
    }
}
