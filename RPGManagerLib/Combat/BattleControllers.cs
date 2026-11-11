using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Items;
using RPGManagerLib.UI;

namespace RPGManagerLib.Combat;

/// <summary>
/// Lets the player drive the hero from the console: shows the battlefield and reads a choice each turn.
/// </summary>
public class ConsoleBattleController : IBattleController
{
    public HeroAction ChooseAction(Battle battle)
    {
        while (true)
        {
            ShowBattlefield(battle);
            var hero = battle.Hero;
            string weapon = hero.ActiveWeapon?.Name ?? "bare hands";

            Ui.Line();
            Ui.Write("  [1] ", ConsoleColor.DarkYellow); Ui.Line($"Attack ({weapon})");
            Ui.Write("  [2] ", ConsoleColor.DarkYellow); Ui.Line("Skills");
            Ui.Write("  [3] ", ConsoleColor.DarkYellow); Ui.Line("Supplies");
            Ui.Write("  [4] ", ConsoleColor.DarkYellow); Ui.Line("Defend");
            if (battle.Options.CanFlee)
            {
                Ui.Write("  [5] ", ConsoleColor.DarkYellow); Ui.Line("Flee");
            }

            string input = Ui.Prompt();
            switch (input)
            {
                case "1":
                    int target = PickTarget(battle);
                    if (target >= 0) return new HeroAction(HeroActionKind.Attack, null, target);
                    break;
                case "2":
                    var skillAction = PickSkill(battle);
                    if (skillAction != null) return skillAction;
                    break;
                case "3":
                    var supplyAction = PickSupply(battle);
                    if (supplyAction != null) return supplyAction;
                    break;
                case "4":
                    return new HeroAction(HeroActionKind.Defend);
                case "5" when battle.Options.CanFlee:
                    return new HeroAction(HeroActionKind.Flee);
                default:
                    Ui.Note("Type the number of an action.");
                    break;
            }
        }
    }

    private static void ShowBattlefield(Battle battle)
    {
        var hero = battle.Hero;
        Ui.Sub($"Round {battle.Round}");

        for (int i = 0; i < battle.Enemies.Count; i++)
        {
            var enemy = battle.Enemies[i];
            if (!enemy.IsAlive) continue;
            Ui.Write($"   {i + 1}. {enemy.Name,-26}", ConsoleColor.White);
            Ui.Write(Ui.Bar(enemy.Health, enemy.MaxHealth, 10), Ui.HealthColor(enemy.Health, enemy.MaxHealth));
            Ui.Write($" {enemy.Health,3:0}/{enemy.MaxHealth:0} ");
            if (enemy.Untargetable) Ui.Write("[Phased] ", ConsoleColor.DarkGray);
            if (enemy.Telegraphing) Ui.Write("[WINDING UP] ", ConsoleColor.Yellow);
            Ui.Line(enemy.StatusLine(), ConsoleColor.Magenta);
        }

        Ui.Line();
        Ui.Write($"   {hero.Name,-14}", ConsoleColor.Cyan);
        Ui.Write("HP ");
        Ui.Write(Ui.Bar(hero.Health, hero.MaxHealth, 14), Ui.HealthColor(hero.Health, hero.MaxHealth));
        Ui.Write($" {hero.Health:0}/{hero.MaxHealth:0}   ");
        Ui.Write($"{hero.ResourceName} ", ConsoleColor.DarkCyan);
        Ui.Write($"{battle.Resource:0}/{battle.MaxResource:0} ");
        Ui.Line(hero.StatusLine(), ConsoleColor.Magenta);
    }

    private static int PickTarget(Battle battle)
    {
        var living = battle.Living;
        if (living.Count == 1) return battle.Enemies.IndexOf(living[0]);

        while (true)
        {
            string input = Ui.Prompt("Target number (ENTER = first, 0 = back) ");
            if (input == "0") return -1;
            if (input == "") return battle.Enemies.IndexOf(living[0]);
            if (int.TryParse(input, out int n) && n >= 1 && n <= battle.Enemies.Count && battle.Enemies[n - 1].IsAlive)
                return n - 1;
            Ui.Note("Pick one of the numbered enemies above.");
        }
    }

    private static HeroAction? PickSkill(Battle battle)
    {
        var skills = SkillBook.For(battle.Hero);
        if (skills.Count == 0)
        {
            Ui.Note("You know no skills yet.");
            return null;
        }

        Ui.Line();
        for (int i = 0; i < skills.Count; i++)
        {
            var skill = skills[i];
            bool usable = battle.CanUse(skill, out string reason);
            string cost = skill.Cost > 0 ? $"{skill.Cost} {battle.Hero.ResourceName}" : "free";
            Ui.Write($"  [{i + 1}] ", usable ? ConsoleColor.DarkYellow : ConsoleColor.DarkGray);
            Ui.Write($"{skill.Name,-16}", usable ? ConsoleColor.White : ConsoleColor.DarkGray);
            Ui.Write($"{cost,-10}", usable ? ConsoleColor.DarkCyan : ConsoleColor.DarkGray);
            Ui.Line(usable ? skill.Description : $"({reason})", ConsoleColor.DarkGray);
        }
        Ui.Line("  [0] Back", ConsoleColor.DarkGray);

        while (true)
        {
            string input = Ui.Prompt();
            if (input is "0" or "") return null;
            if (!int.TryParse(input, out int n) || n < 1 || n > skills.Count)
            {
                Ui.Note("Pick a skill number, or 0 to go back.");
                continue;
            }

            var chosen = skills[n - 1];
            if (!battle.CanUse(chosen, out string why))
            {
                Ui.Note($"Can't use {chosen.Name}: {why}.");
                continue;
            }

            if (chosen.Target != SkillTarget.Enemy) return new HeroAction(HeroActionKind.Skill, chosen.Id);
            int target = PickTarget(battle);
            return target < 0 ? null : new HeroAction(HeroActionKind.Skill, chosen.Id, target);
        }
    }

    private static HeroAction? PickSupply(Battle battle)
    {
        var supplies = battle.UsableSupplies()
            .Where(s => s.Id != SupplyCatalog.ManaDraught || battle.Hero is Mage)
            .ToList();
        if (supplies.Count == 0)
        {
            Ui.Note("Nothing in your pack will help here.");
            return null;
        }

        int choice = Ui.Choose(
            supplies.Select(s => $"{s.Name} x{battle.Hero.SupplyCount(s.Id)} — {s.Description}").ToList(),
            "Back");
        return choice < 0 ? null : new HeroAction(HeroActionKind.Supply, supplies[choice].Id);
    }
}

/// <summary>
/// A simple automatic player used by the balance simulator: heals when low, defends against
/// telegraphed blows, otherwise spends its resource on the best available skill.
/// </summary>
public class BotBattleController : IBattleController
{
    public HeroAction ChooseAction(Battle battle)
    {
        var hero = battle.Hero;
        var living = battle.Living;
        var skills = SkillBook.For(hero).Where(s => battle.CanUse(s, out _)).ToList();
        double healthRatio = hero.Health / hero.MaxHealth;

        if (living.Any(e => e.Telegraphing))
        {
            var evade = skills.FirstOrDefault(s => s.Special == SkillSpecial.Evade);
            return evade != null ? new HeroAction(HeroActionKind.Skill, evade.Id) : new HeroAction(HeroActionKind.Defend);
        }

        if (healthRatio < 0.4)
        {
            if (hero.SupplyCount(SupplyCatalog.GreaterHealingPotion) > 0) return new HeroAction(HeroActionKind.Supply, SupplyCatalog.GreaterHealingPotion);
            if (hero.SupplyCount(SupplyCatalog.HealingPotion) > 0) return new HeroAction(HeroActionKind.Supply, SupplyCatalog.HealingPotion);
            var heal = skills.FirstOrDefault(s => s.Special == SkillSpecial.Heal);
            if (heal != null) return new HeroAction(HeroActionKind.Skill, heal.Id);
        }

        if (living.All(e => e.Untargetable)) return new HeroAction(HeroActionKind.Defend);

        var targetEnemy = living.Where(e => !e.Untargetable).OrderBy(e => e.Health).First();
        int target = battle.Enemies.IndexOf(targetEnemy);

        if (hero is Archer && battle.Resource < 1) return new HeroAction(HeroActionKind.Defend);

        if (living.Count >= (hero is Archer ? 3 : 2))
        {
            var area = skills.FirstOrDefault(s => s.Target == SkillTarget.AllEnemies && s.Special == SkillSpecial.None);
            if (area != null) return new HeroAction(HeroActionKind.Skill, area.Id);
        }

        var ward = skills.FirstOrDefault(s => s.Special == SkillSpecial.Ward);
        if (ward != null && !hero.Has(StatusType.Ward) && healthRatio < 0.8) return new HeroAction(HeroActionKind.Skill, ward.Id);

        var best = skills
            .Where(s => s.Target == SkillTarget.Enemy)
            .OrderByDescending(s => s.IsSpell
                ? (s.BaseDamage + hero.Arcana) * (s.Special == SkillSpecial.Unbinding && (targetEnemy.Tags.Contains("spirit") || targetEnemy.Tags.Contains("court")) ? 2 : 1) * targetEnemy.DamageMultiplier(s.Element, true) / s.Cost
                : s.WeaponMultiplier * 30)
            .FirstOrDefault();
        if (best != null) return new HeroAction(HeroActionKind.Skill, best.Id, target);

        return new HeroAction(HeroActionKind.Attack, null, target);
    }
}
