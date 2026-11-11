using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Core;
using RPGManagerLib.Items;
using RPGManagerLib.Items.Staffs;
using RPGManagerLib.Items.Weapons;
using RPGManagerLib.Items.Weapons.Bows;
using RPGManagerLib.UI;

namespace RPGManagerLib.Combat;

/// <summary>How a fight ended.</summary>
public enum BattleOutcome { Victory, Defeat, Fled }

/// <summary>What the hero can do on a turn.</summary>
public enum HeroActionKind { Attack, Skill, Supply, Defend, Flee }

/// <summary>A chosen hero action.</summary>
/// <param name="Kind">The action type.</param>
/// <param name="Id">Skill id or supply id, when relevant.</param>
/// <param name="Target">Index into <see cref="Battle.Enemies"/>.</param>
public record HeroAction(HeroActionKind Kind, string? Id = null, int Target = 0);

/// <summary>Per-fight rules set by whoever starts the battle.</summary>
public class BattleOptions
{
    /// <summary>Story fights cannot be fled.</summary>
    public bool CanFlee { get; set; } = true;

    /// <summary>Name of an ally fighting alongside the hero, if any.</summary>
    public string? AllyName { get; set; }

    /// <summary>Average damage the ally deals each round.</summary>
    public double AllyDamage { get; set; }

    /// <summary>The Cinder Hand cannot draw on the Focus (the Keeper holds it).</summary>
    public bool FocusDenied { get; set; }

    /// <summary>Rounds the first enemy starts Exposed (a known weakness).</summary>
    public int OpeningExposedTurns { get; set; }
}

/// <summary>What came out of a fight.</summary>
public class BattleResult
{
    public BattleOutcome Outcome { get; set; }
    public int Xp { get; set; }
    public int Gold { get; set; }
    public int Rounds { get; set; }
    public List<string> Loot { get; } = new();
    public List<Enemy> Slain { get; } = new();
}

/// <summary>Supplies the hero's decisions: the console for players, a bot for the balance simulator.</summary>
public interface IBattleController
{
    HeroAction ChooseAction(Battle battle);
}

/// <summary>
/// One turn-based fight between a hero and a group of enemies.
/// Each round: hero acts, ally acts, enemies act, then effects tick in a single pass.
/// </summary>
public class Battle
{
    private readonly IBattleController controller;
    private readonly List<(Enemy Enemy, int Round)> reinforcements = new();
    private double classResource;
    private bool guaranteedCrit;
    private bool fled;

    public Character Hero { get; }
    public List<Enemy> Enemies { get; }
    public BattleOptions Options { get; }
    public int Round { get; private set; }

    /// <summary>Remaining cooldown rounds per skill id.</summary>
    public Dictionary<string, int> Cooldowns { get; } = new();

    public Battle(Character hero, List<Enemy> enemies, IBattleController controller, BattleOptions? options = null)
    {
        Hero = hero;
        Enemies = enemies;
        this.controller = controller;
        Options = options ?? new BattleOptions();
    }

    /// <summary>Enemies still standing.</summary>
    public List<Enemy> Living => Enemies.Where(e => e.IsAlive).ToList();

    /// <summary>Upper bound of the class resource.</summary>
    public double MaxResource => Hero switch
    {
        Mage => Hero.MaxMana,
        Archer archer => archer.ArrowCapacity,
        _ => 100
    };

    /// <summary>Current Fury, Mana or Arrows.</summary>
    public double Resource
    {
        get => Hero is Mage ? Hero.Mana : classResource;
        private set
        {
            if (Hero is Mage) Hero.Mana = Math.Clamp(value, 0, Hero.MaxMana);
            else classResource = Math.Clamp(value, 0, MaxResource);
        }
    }

    /// <summary>Checks whether a skill can be used right now.</summary>
    public bool CanUse(Skill skill, out string reason)
    {
        reason = "";
        if (Cooldowns.TryGetValue(skill.Id, out int cd) && cd > 0)
        {
            reason = $"ready in {cd}";
            return false;
        }
        if (Hero is Archer && skill.Target != SkillTarget.Self && Hero.ActiveWeapon is not Bow)
        {
            reason = "needs a bow";
            return false;
        }
        if (Resource < skill.Cost)
        {
            reason = $"needs {skill.Cost} {Hero.ResourceName}";
            return false;
        }
        return true;
    }

    /// <summary>Supplies the hero carries that do something in combat.</summary>
    public List<Supply> UsableSupplies() =>
        Hero.Supplies.Keys.Select(SupplyCatalog.Get)
            .Where(s => s.InCombat && (s.Id != SupplyCatalog.SmokeBomb || Options.CanFlee))
            .ToList();

    /// <summary>Runs the fight to its end.</summary>
    public BattleResult Run()
    {
        Hero.Effects.Clear();
        classResource = Hero is Archer ? MaxResource : 0;
        if (Options.OpeningExposedTurns > 0 && Enemies.Count > 0)
            Enemies[0].Apply(StatusType.Exposed, Options.OpeningExposedTurns);

        while (true)
        {
            Round++;
            JoinReinforcements();

            HeroTurn();
            if (fled) return Finish(BattleOutcome.Fled);
            if (AllEnemiesDown()) return Finish(BattleOutcome.Victory);

            AllyTurn();
            if (AllEnemiesDown()) return Finish(BattleOutcome.Victory);

            foreach (var enemy in Enemies.ToList())
            {
                if (!Hero.IsAlive) break;
                if (enemy.IsAlive) EnemyTurn(enemy);
            }

            EndOfRound();
            if (!Hero.IsAlive) return Finish(BattleOutcome.Defeat);
            if (AllEnemiesDown()) return Finish(BattleOutcome.Victory);
            if (Round >= 150) return Finish(BattleOutcome.Defeat);
        }
    }

    private bool AllEnemiesDown() => Living.Count == 0 && reinforcements.Count == 0;

    private BattleResult Finish(BattleOutcome outcome)
    {
        var result = new BattleResult { Outcome = outcome, Rounds = Round };
        Hero.Effects.Clear();
        if (outcome != BattleOutcome.Victory) return result;

        foreach (var enemy in Enemies)
        {
            result.Slain.Add(enemy);
            double factor = Hero.Level - enemy.Level >= 4 ? 0.5 : 1.0;
            result.Xp += (int)Math.Round(enemy.Xp * factor);
            result.Gold += Rng.Next(enemy.GoldMin, enemy.GoldMax);
            foreach (var entry in enemy.Loot)
                if (Rng.Chance(entry.Chance)) result.Loot.Add(entry.ItemId);
        }
        return result;
    }

    // ------------------------------------------------------------------ hero

    private void HeroTurn()
    {
        if (Hero.Has(StatusType.Stun) || Hero.Has(StatusType.Freeze))
        {
            Hero.Remove(StatusType.Stun);
            Hero.Remove(StatusType.Freeze);
            Ui.Sub($"Round {Round}");
            Say($"{Hero.Name} is reeling and loses the turn!", ConsoleColor.Red);
            return;
        }

        while (true)
        {
            var action = controller.ChooseAction(this);
            if (Resolve(action)) return;
        }
    }

    private bool Resolve(HeroAction action)
    {
        switch (action.Kind)
        {
            case HeroActionKind.Attack:
                if (!TryTarget(action.Target, out var attackTarget)) return false;
                BasicAttack(attackTarget);
                return true;

            case HeroActionKind.Skill:
                var skill = SkillBook.For(Hero).FirstOrDefault(s => s.Id == action.Id);
                if (skill == null || !CanUse(skill, out _)) return false;
                Enemy? skillTarget = null;
                if (skill.Target == SkillTarget.Enemy && !TryTarget(action.Target, out skillTarget)) return false;
                UseSkill(skill, skillTarget);
                return true;

            case HeroActionKind.Supply:
                return action.Id != null && UseSupply(action.Id);

            case HeroActionKind.Defend:
                Defend();
                return true;

            case HeroActionKind.Flee:
                if (!Options.CanFlee) return false;
                TryFlee();
                return true;
        }
        return false;
    }

    private bool TryTarget(int index, out Enemy target)
    {
        target = null!;
        if (index < 0 || index >= Enemies.Count || !Enemies[index].IsAlive) return false;
        target = Enemies[index];
        return true;
    }

    private record WeaponProfile(double Power, Element Element, bool Magic, string Name, bool IsBow);

    private WeaponProfile Profile(bool forceMelee = false)
    {
        Weapon? weapon = Hero.ActiveWeapon;
        if (forceMelee && weapon is Bow)
        {
            weapon = Hero.Weapons.OfType<Weapon>()
                .Where(w => w.Type != WeaponType.BOW && Hero.CanWield(w))
                .OrderByDescending(w => w.GetEffectiveDamage()).FirstOrDefault();
        }

        if (weapon == null) return new WeaponProfile(3 + Hero.Might, Element.NONE, false, "bare hands", false);

        int damage = weapon.GetEffectiveDamage();
        return weapon.Type switch
        {
            WeaponType.STAFF => new WeaponProfile(damage + Hero.Arcana * 0.6, weapon.Element, true, weapon.Name, false),
            WeaponType.BOW => new WeaponProfile(damage + Hero.Perception, weapon.Element, false, weapon.Name, true),
            WeaponType.DAGGER => new WeaponProfile(damage + (Hero.Might + Hero.Perception) / 2.0, weapon.Element, false, weapon.Name, false),
            _ => new WeaponProfile(damage + Hero.Might, weapon.Element, false, weapon.Name, false)
        };
    }

    private static double Variance() => 0.85 + Rng.NextDouble() * 0.30;

    private bool RollCrit(double bonus)
    {
        if (guaranteedCrit)
        {
            guaranteedCrit = false;
            return true;
        }
        return Rng.Chance(0.05 + Hero.Perception * 0.015 + bonus);
    }

    private void BasicAttack(Enemy target)
    {
        var profile = Profile();
        if (profile.IsBow)
        {
            if (Resource >= 1) Resource -= 1;
            else
            {
                Say("Out of arrows! You close in with what you have.", ConsoleColor.DarkYellow);
                profile = Profile(forceMelee: true);
            }
        }

        bool crit = RollCrit(0);
        double raw = profile.Power * Variance() * (crit ? 1.6 : 1.0);
        string verb = profile.Magic ? "sends an arcane bolt from" : profile.IsBow ? "looses an arrow from" : "strikes with";
        Say($"{Hero.Name} {verb} the {profile.Name}.");
        Strike(target, raw, profile.Element, profile.Magic, crit);
        if (Hero is Warrior) Resource += 15;
    }

    private void UseSkill(Skill skill, Enemy? target)
    {
        Resource -= skill.Cost;
        if (skill.Cooldown > 0) Cooldowns[skill.Id] = skill.Cooldown + 1;
        Say($"{Hero.Name} uses {skill.Name}!", ConsoleColor.Cyan);

        switch (skill.Special)
        {
            case SkillSpecial.Heal:
                double amount = skill.IsSpell ? 28 + Hero.Arcana * 2.2 : Hero.MaxHealth * 0.25;
                Say($"  {Hero.Name} recovers {Hero.Restore(amount):0} health.", ConsoleColor.Green);
                return;
            case SkillSpecial.Ward:
                Hero.Remove(StatusType.Ward);
                Hero.Apply(StatusType.Ward, 4, 16 + Hero.Arcana * 1.2);
                Say($"  A ward shimmers into place ({Hero.Get(StatusType.Ward)!.Potency:0} absorb).", ConsoleColor.Green);
                return;
            case SkillSpecial.Evade:
                Hero.Apply(StatusType.Evasion, 1);
                guaranteedCrit = true;
                Say("  You melt out of reach, already lining up the next shot.", ConsoleColor.Green);
                return;
            case SkillSpecial.WarCry:
                foreach (var enemy in Living) enemy.Apply(StatusType.Weaken, 3);
                Say("  The enemy line flinches. They are Weakened.", ConsoleColor.Green);
                return;
        }

        var profile = Profile();
        double staffPower = Hero.ActiveWeapon is Staff staff ? staff.GetEffectiveDamage() : 0;
        bool staffMatch = Hero.ActiveWeapon is Staff s && s.Element != Element.NONE && s.Element == skill.Element;
        var targets = skill.Target == SkillTarget.AllEnemies ? Living : new List<Enemy> { target! };

        foreach (var t in targets)
        {
            bool crit = RollCrit(skill.IsSpell ? -0.03 : skill.CritBonus);
            double raw;
            Element element;
            if (skill.IsSpell)
            {
                raw = (skill.BaseDamage + Hero.Arcana * 1.2 + staffPower) * (staffMatch ? 1.25 : 1.0);
                element = skill.Element;
                if (skill.Special == SkillSpecial.Unbinding && (t.Tags.Contains("spirit") || t.Tags.Contains("court"))) raw *= 2;
            }
            else
            {
                double multiplier = skill.WeaponMultiplier;
                if (skill.Special == SkillSpecial.Execute && t.Health / t.MaxHealth < 0.35) multiplier = 3.2;
                raw = profile.Power * multiplier;
                element = profile.Element;
            }

            raw *= Variance() * (crit ? 1.6 : 1.0);
            Strike(t, raw, element, skill.IsSpell || profile.Magic, crit, skill.Special == SkillSpecial.Pierce);

            if (skill.Status.HasValue && t.IsAlive && !t.Untargetable && Rng.Chance(skill.StatusChance))
                Inflict(t, skill.Status.Value, skill.StatusTurns);
        }
    }

    private void Inflict(Enemy target, StatusType status, int turns)
    {
        bool control = status is StatusType.Stun or StatusType.Freeze;
        if (control && target.IsBoss && Rng.Chance(0.5))
        {
            Say($"  {target.Name} shrugs off the {status.ToString().ToLowerInvariant()}.", ConsoleColor.DarkGray);
            return;
        }

        double potency = status switch
        {
            StatusType.Burn => 5 + Hero.Arcana * 0.4,
            StatusType.Poison => 4 + Hero.Perception * 0.5,
            _ => 0
        };
        target.Apply(status, turns, potency);
        string text = status switch
        {
            StatusType.Burn => "catches fire",
            StatusType.Poison => "is poisoned",
            StatusType.Freeze => "is frozen in place",
            StatusType.Stun => "is stunned",
            _ => $"is {status.ToString().ToLowerInvariant()}"
        };
        Say($"  {target.Name} {text}!", ConsoleColor.Magenta);
    }

    /// <summary>Applies a finished raw damage number to an enemy and reports it.</summary>
    private void Strike(Enemy target, double raw, Element element, bool magic, bool crit, bool ignoreGuard = false)
    {
        if (target.Untargetable)
        {
            Say($"  It passes through {target.Name} like smoke.", ConsoleColor.DarkGray);
            return;
        }

        double multiplier = target.DamageMultiplier(element, magic);
        double damage = raw * multiplier;
        if (!ignoreGuard) damage *= 10.0 / (10 + target.Guard);
        if (target.Has(StatusType.Exposed)) damage *= 1.25;
        if (target.Has(StatusType.Guarding)) damage *= 0.5;
        damage = Math.Max(1, Math.Round(damage));
        target.TakeDamage(damage);

        string note = multiplier >= 1.3 ? " It tears through — a weakness!" : multiplier <= 0.6 ? " It barely bites — resisted." : "";
        string critText = crit ? "CRITICAL! " : "";
        Say($"  {critText}{target.Name} takes {damage:0} damage.{note}", crit ? ConsoleColor.Yellow : ConsoleColor.White);
        if (!target.IsAlive) Say($"  {target.Name} falls.", ConsoleColor.Green);
    }

    private void Defend()
    {
        Hero.Apply(StatusType.Guarding, 1);
        switch (Hero)
        {
            case Warrior:
                Resource += 15;
                Say($"{Hero.Name} braces behind steel. (+15 Fury)", ConsoleColor.Cyan);
                break;
            case Mage:
                Resource += 12;
                Say($"{Hero.Name} draws inward and gathers power. (+12 Mana)", ConsoleColor.Cyan);
                break;
            case Archer:
                Resource += 4;
                Say($"{Hero.Name} keeps low and recovers spent shafts. (+4 Arrows)", ConsoleColor.Cyan);
                break;
        }
    }

    private void TryFlee()
    {
        double chance = 0.45 + Hero.Perception * 0.02 + (Hero is Archer ? 0.15 : 0);
        if (Rng.Chance(chance))
        {
            Say($"{Hero.Name} breaks away and escapes!", ConsoleColor.Yellow);
            fled = true;
        }
        else
        {
            Say($"{Hero.Name} tries to run, but they cut off the way out!", ConsoleColor.Red);
        }
    }

    private bool UseSupply(string id)
    {
        if (!SupplyCatalog.Exists(id) || !SupplyCatalog.Get(id).InCombat || Hero.SupplyCount(id) <= 0) return false;
        if (id == SupplyCatalog.ManaDraught && Hero is not Mage) return false;
        if (id == SupplyCatalog.SmokeBomb && !Options.CanFlee) return false;

        Hero.UseSupply(id);
        string name = SupplyCatalog.Get(id).Name;
        switch (id)
        {
            case SupplyCatalog.HealingPotion:
                Say($"{Hero.Name} drinks a {name} and recovers {Hero.Restore(50):0} health.", ConsoleColor.Green);
                break;
            case SupplyCatalog.GreaterHealingPotion:
                Say($"{Hero.Name} drinks a {name} and recovers {Hero.Restore(120):0} health.", ConsoleColor.Green);
                break;
            case SupplyCatalog.ManaDraught:
                Say($"{Hero.Name} drinks a {name} and recovers {Hero.RestoreMana(60):0} mana.", ConsoleColor.Green);
                break;
            case SupplyCatalog.EmberSalve:
                Hero.Remove(StatusType.Burn);
                Hero.Remove(StatusType.Poison);
                Say($"{Hero.Name} smears on {name}: burns and poison fade, {Hero.Restore(15):0} health restored.", ConsoleColor.Green);
                break;
            case SupplyCatalog.Firebomb:
                Say($"{Hero.Name} hurls a {name}!", ConsoleColor.Cyan);
                foreach (var enemy in Living) Strike(enemy, 35 * Variance(), Element.FIRE, true, false, ignoreGuard: true);
                break;
            case SupplyCatalog.SmokeBomb:
                Say($"{Hero.Name} smashes a {name} and vanishes into the haze.", ConsoleColor.Yellow);
                fled = true;
                break;
        }
        return true;
    }

    // ------------------------------------------------------------------ ally

    private void AllyTurn()
    {
        if (Options.AllyName == null || Living.Count == 0) return;
        var target = Living.OrderBy(e => e.Health).First();
        if (target.Untargetable) return;
        Say($"{Options.AllyName} wades in beside you.", ConsoleColor.Cyan);
        Strike(target, Options.AllyDamage * Variance(), Element.NONE, false, false);
    }

    // ------------------------------------------------------------------ enemies

    private void JoinReinforcements()
    {
        foreach (var (enemy, round) in reinforcements.Where(r => r.Round <= Round).ToList())
        {
            Enemies.Add(enemy);
            reinforcements.Remove((enemy, round));
            Say($"{enemy.Name} strides into the fight!", ConsoleColor.Red);
        }
    }

    private void EnemyTurn(Enemy enemy)
    {
        enemy.TurnCounter++;

        if (enemy.Untargetable)
        {
            enemy.Untargetable = false;
            Say($"{enemy.Name} tears back into the world.", ConsoleColor.DarkYellow);
        }

        if (enemy.Has(StatusType.Stun) || enemy.Has(StatusType.Freeze))
        {
            bool frozen = enemy.Has(StatusType.Freeze);
            enemy.Remove(StatusType.Stun);
            enemy.Remove(StatusType.Freeze);
            enemy.Telegraphing = false;
            Say($"{enemy.Name} is {(frozen ? "frozen" : "stunned")} and cannot act.", ConsoleColor.DarkGray);
            return;
        }

        if (RunTrait(enemy)) return;

        if (enemy.Behavior == EnemyBehavior.Random)
        {
            double roll = Rng.NextDouble();
            if (roll < 0.20)
            {
                Say($"{enemy.Name} flickers aimlessly.", ConsoleColor.DarkGray);
                return;
            }
            if (roll < 0.35)
            {
                enemy.Apply(StatusType.Guarding, 1);
                Say($"{enemy.Name} gutters low and shields itself.", ConsoleColor.DarkGray);
                return;
            }
        }

        EnemyAttack(enemy);
    }

    /// <summary>Runs once-per-fight tricks and boss scripts. Returns true when the turn was used up.</summary>
    private bool RunTrait(Enemy enemy)
    {
        double ratio = enemy.Health / enemy.MaxHealth;
        switch (enemy.Trait)
        {
            case EnemyTrait.HealOnce when !enemy.TraitUsed && ratio < 0.5:
                enemy.TraitUsed = true;
                Say($"{enemy.Name} backs off and downs a potion, recovering {enemy.Restore(enemy.MaxHealth * 0.3):0} health.", ConsoleColor.DarkYellow);
                return true;

            case EnemyTrait.PhaseOnce when !enemy.TraitUsed && ratio < 0.3:
                enemy.TraitUsed = true;
                enemy.Untargetable = true;
                enemy.Restore(enemy.MaxHealth * 0.12);
                Say($"{enemy.Name} dissolves into drifting ash — untouchable — and knits itself together.", ConsoleColor.DarkYellow);
                return true;

            case EnemyTrait.CallReinforcement when !enemy.TraitUsed && ratio < 0.4 && Living.Count + reinforcements.Count < 3:
                enemy.TraitUsed = true;
                var extra = Bestiary.Create(Bestiary.CourtSentinel);
                extra.TraitUsed = true;
                extra.Name = "Sentinel Reinforcement";
                reinforcements.Add((extra, Round + 1));
                Say($"{enemy.Name} falls back a step and sounds a short, flat horn. Boots answer from the dark.", ConsoleColor.Red);
                return true;

            case EnemyTrait.Thresh:
                return ThreshTurn(enemy, ratio);

            case EnemyTrait.CinderHand:
                return CinderHandTurn(enemy, ratio);
        }
        return false;
    }

    private bool ThreshTurn(Enemy thresh, double ratio)
    {
        if (thresh.Telegraphing)
        {
            thresh.Telegraphing = false;
            Say("Thresh drives his shield forward like a battering ram!", ConsoleColor.Red);
            EnemyAttack(thresh, 1.5, heavy: true, stunOnHit: true);
            return true;
        }
        if (!thresh.TraitUsed && ratio < 0.5)
        {
            thresh.TraitUsed = true;
            thresh.AttackBuff = 1.2;
            thresh.Remove(StatusType.Weaken);
            Say($"\"Not today.\" Thresh sets his feet, recovers {thresh.Restore(thresh.MaxHealth * 0.12):0} health, and fights like a man with nothing behind him.", ConsoleColor.Red);
            return true;
        }
        if (thresh.TurnCounter % 3 == 0)
        {
            thresh.Telegraphing = true;
            Say("! Thresh raises his shield and shifts his weight — a crushing blow is coming. (Defend or Evade!)", ConsoleColor.Yellow);
            return true;
        }
        return false;
    }

    private bool CinderHandTurn(Enemy hand, double ratio)
    {
        if (hand.Telegraphing)
        {
            hand.Telegraphing = false;
            Say("The gauntlet opens. A storm of cinders howls across the plateau!", ConsoleColor.Red);
            EnemyAttack(hand, 1.8, heavy: true, forceBurn: true);
            return true;
        }
        if (!hand.TraitUsed && ratio < 0.5)
        {
            hand.TraitUsed = true;
            if (Options.FocusDenied)
            {
                hand.Apply(StatusType.Exposed, 4);
                Say("The Cinder Hand reaches for the Emberveil Focus — and nothing answers. You hold it. The gauntlet sputters. (Exposed!)", ConsoleColor.Green);
            }
            else
            {
                hand.AttackBuff = 1.25;
                Say($"The Cinder Hand draws on the Emberveil Focus. The gauntlet blazes white; {hand.Restore(hand.MaxHealth * 0.2):0} health floods back.", ConsoleColor.Red);
            }
            return true;
        }
        if (hand.TurnCounter % 4 == 0)
        {
            hand.Telegraphing = true;
            Say("! The Cinder Hand closes the gauntlet into a fist. The air begins to glow. (Defend or Evade!)", ConsoleColor.Yellow);
            return true;
        }
        return false;
    }

    private void EnemyAttack(Enemy enemy, double multiplier = 1.0, bool heavy = false, bool stunOnHit = false, bool forceBurn = false)
    {
        if (Hero.Has(StatusType.Evasion))
        {
            Say($"{enemy.Name} strikes at empty air — {Hero.Name} is already elsewhere.", ConsoleColor.Green);
            return;
        }
        if (!heavy && Rng.Chance(0.06))
        {
            Say($"{enemy.Name} attacks and misses.", ConsoleColor.DarkGray);
            return;
        }

        bool guarding = Hero.Has(StatusType.Guarding);
        double damage = enemy.Attack * enemy.AttackBuff * multiplier * Variance();
        if (enemy.Has(StatusType.Weaken)) damage *= 0.65;
        damage *= 10.0 / (10 + Hero.Guard);
        if (guarding) damage *= 0.5;
        damage = Math.Max(1, Math.Round(damage));

        var ward = Hero.Get(StatusType.Ward);
        if (ward != null)
        {
            double absorbed = Math.Min(ward.Potency, damage);
            ward.Potency -= absorbed;
            damage -= absorbed;
            if (ward.Potency <= 0) Hero.Remove(StatusType.Ward);
            if (damage <= 0)
            {
                Say($"{enemy.Name} attacks — the ward drinks all of it.", ConsoleColor.Green);
                return;
            }
        }

        Hero.TakeDamage(damage);
        string how = enemy.AttackElement == Element.FIRE ? "scorches" : "hits";
        Say($"{enemy.Name} {how} {Hero.Name} for {damage:0}.{(guarding ? " (guarded)" : "")}", ConsoleColor.Red);
        if (Hero is Warrior) Resource += 8;
        if (!Hero.IsAlive) return;

        double burnChance = forceBurn ? 1.0 : enemy.BurnChance;
        if (burnChance > 0 && Rng.Chance(guarding ? burnChance * 0.5 : burnChance))
        {
            Hero.Apply(StatusType.Burn, 3, 3 + enemy.Level * 0.6);
            Say($"  {Hero.Name} is burning!", ConsoleColor.Magenta);
        }
        if (stunOnHit && !guarding)
        {
            Hero.Apply(StatusType.Stun, 1);
            Say($"  {Hero.Name} is knocked sprawling — stunned!", ConsoleColor.Magenta);
        }
    }

    // ------------------------------------------------------------------ upkeep

    private void EndOfRound()
    {
        Tick(Hero);
        foreach (var enemy in Living) Tick(enemy);

        if (Hero is Mage && Hero.IsAlive) Resource += 6;
        foreach (string key in Cooldowns.Keys.ToList())
            if (Cooldowns[key] > 0) Cooldowns[key]--;
    }

    private void Tick(ICombatant combatant)
    {
        foreach (var effect in combatant.Effects.ToList())
        {
            if (effect.Type is StatusType.Burn or StatusType.Poison && combatant.IsAlive)
            {
                double damage = Math.Max(1, Math.Round(effect.Potency));
                combatant.TakeDamage(damage);
                string cause = effect.Type == StatusType.Burn ? "burns" : "suffers from poison";
                Say($"{combatant.Name} {cause} for {damage:0}.", ConsoleColor.Magenta);
                if (!combatant.IsAlive) Say($"  {combatant.Name} falls.", combatant is Enemy ? ConsoleColor.Green : ConsoleColor.Red);
            }

            if (effect.Type is StatusType.Stun or StatusType.Freeze) continue;
            effect.Turns--;
            if (effect.Turns <= 0) combatant.Effects.Remove(effect);
        }
    }

    private static void Say(string text, ConsoleColor color = ConsoleColor.Gray)
    {
        if (Ui.Silent) return;
        foreach (string line in Ui.Wrap(text, Ui.Width - 4)) Ui.Line("  " + line, color);
        Ui.Beat();
    }
}
