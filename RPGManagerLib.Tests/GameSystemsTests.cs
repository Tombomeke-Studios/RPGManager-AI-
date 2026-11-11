using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Combat;
using RPGManagerLib.Core;
using RPGManagerLib.Items;
using RPGManagerLib.Items.Weapons;
using RPGManagerLib.Quests;
using RPGManagerLib.Saves;
using RPGManagerLib.UI;
using RPGManagerLib.Worlds;

namespace RPGManagerLib.Tests;

public class GameSystemsTests : IDisposable
{
    public GameSystemsTests()
    {
        WorldState.Current = new WorldState();
        Ui.Silent = true;
        Rng.Reseed(42);
    }

    public void Dispose()
    {
        Ui.Silent = false;
        WorldState.Current = new WorldState();
    }

    [Fact]
    public void GainExperience_LevelsUpAndAppliesClassGrowth()
    {
        var warrior = new Warrior("Brak");
        int might = warrior.Might;

        int gained = warrior.GainExperience(Character.XpToNextLevel[1] + Character.XpToNextLevel[2]);

        Assert.Equal(2, gained);
        Assert.Equal(3, warrior.Level);
        Assert.Equal(might + 2, warrior.Might);
        Assert.Equal(2 * Character.PointsPerLevel, warrior.UnspentPoints);
        Assert.Equal(warrior.MaxHealth, warrior.Health);
    }

    [Fact]
    public void SpendPoint_RaisesChosenStatAndUsesUpPoints()
    {
        var archer = new Archer("Wren");
        archer.SetLevel(2);
        int perception = archer.Perception;
        double health = archer.MaxHealth;

        Assert.True(archer.SpendPoint(HeroStat.Perception));
        Assert.True(archer.SpendPoint(HeroStat.Vitality));
        Assert.False(archer.SpendPoint(HeroStat.Might));

        Assert.Equal(perception + 1, archer.Perception);
        Assert.Equal(health + Character.HealthPerVitality, archer.MaxHealth);
        Assert.Equal(0, archer.UnspentPoints);
    }

    [Fact]
    public void Bestiary_IndividualsVaryButStayNearTheirTemplate()
    {
        var template = Bestiary.Exact(Bestiary.Bandit);
        var healths = Enumerable.Range(0, 40).Select(_ => Bestiary.Create(Bestiary.Bandit).MaxHealth).ToList();

        Assert.True(healths.Distinct().Count() > 3);
        Assert.All(healths, h => Assert.InRange(h, template.MaxHealth * 0.84, template.MaxHealth * 1.16));
    }

    [Fact]
    public void Bestiary_EliteIsMuchStrongerAndWorthMore()
    {
        var normal = Bestiary.Exact(Bestiary.CinderGoblin);
        var elite = Bestiary.Exact(Bestiary.CinderGoblin);
        Bestiary.Promote(elite, EnemyRank.Elite);

        Assert.Equal(EnemyRank.Elite, elite.Rank);
        Assert.Equal(Math.Round(normal.MaxHealth * Bestiary.EliteHealth), elite.MaxHealth);
        Assert.Equal(elite.MaxHealth, elite.Health);
        Assert.True(elite.Attack > normal.Attack);
        Assert.Equal(normal.Xp * 3, elite.Xp);
        Assert.StartsWith("Dread", elite.Name);
    }

    [Fact]
    public void Bestiary_WildGroupsSometimesContainVeteransOrElites()
    {
        int strong = Enumerable.Range(0, 300)
            .SelectMany(_ => Bestiary.Wild(Bestiary.AshScavenger, Bestiary.AshScavenger))
            .Count(e => e.Rank != EnemyRank.Normal);

        Assert.InRange(strong, 40, 200);
        Assert.All(Bestiary.Group(Bestiary.AshScavenger, Bestiary.AshScavenger, Bestiary.AshScavenger), e => Assert.Equal(EnemyRank.Normal, e.Rank));
    }

    [Fact]
    public void GainExperience_StopsAtMaxLevel()
    {
        var mage = new Mage("Vex");
        mage.GainExperience(1_000_000);

        Assert.Equal(Character.MaxLevel, mage.Level);
        Assert.Equal(0, mage.GainExperience(500));
    }

    [Fact]
    public void Inventory_RejectsItemsBeyondCapacity()
    {
        var warrior = new Warrior("Brak");
        for (int i = 0; i < 5; i++) Assert.True(warrior.TryAddItem(ItemForge.Create("great_sword")));

        Assert.Equal(Character.InventoryCapacity, warrior.UsedSlots);
        Assert.False(warrior.TryAddItem(ItemForge.Create("basic_dagger")));
    }

    [Fact]
    public void ActiveWeapon_PicksStrongestUsableWeaponForClass()
    {
        var archer = new Archer("Wren");
        archer.Weapons.Add(ItemForge.Create("great_axe"));
        archer.Weapons.Add(ItemForge.Create("simple_bow"));
        archer.Weapons.Add(ItemForge.Create("basic_dagger"));

        Assert.Equal(WeaponType.BOW, archer.ActiveWeapon!.Type);
    }

    [Fact]
    public void Roster_UnlocksSlotsByHighestLevel()
    {
        var roster = new CharacterRoster();
        var first = new Warrior("Brak");
        Assert.True(roster.TryAdd(first));
        Assert.False(roster.CanAdd);
        Assert.False(roster.TryAdd(new Mage("Vex")));

        first.SetLevel(CharacterRoster.SlotUnlockLevels[2]);

        Assert.True(roster.CanAdd);
        Assert.True(roster.TryAdd(new Mage("Vex")));
        Assert.Equal(2, roster.Heroes.Count);
    }

    [Fact]
    public void WorldState_ReputationTierIsComputedFromScore()
    {
        var world = new WorldState();
        Assert.Equal(ReputationTier.Stranger, world.Tier());
        world.AddReputation(30);
        Assert.Equal(ReputationTier.Known, world.Tier());
        world.AddReputation(30);
        Assert.Equal(ReputationTier.Trusted, world.Tier());
        world.AddReputation(40);
        Assert.Equal(ReputationTier.Honored, world.Tier());
    }

    [Fact]
    public void WorldState_FlagsRememberWhichHeroSetThem()
    {
        var world = new WorldState();
        world.Set(WorldFlags.EmberSealBroken, "Vex");
        world.Set(WorldFlags.EmberSealBroken, "Brak");

        Assert.True(world.Has(WorldFlags.EmberSealBroken));
        Assert.Equal("Vex", world.DoneBy(WorldFlags.EmberSealBroken));
        Assert.False(world.Has(WorldFlags.ThreshAlly));
    }

    [Fact]
    public void WorldState_LogIsCapped()
    {
        var world = new WorldState();
        for (int i = 0; i < WorldState.MaxLogEntries + 50; i++) world.Log($"entry {i}");

        Assert.Equal(WorldState.MaxLogEntries, world.EventLog.Count);
        Assert.Equal("entry 50", world.EventLog[0]);
    }

    [Fact]
    public void KillQuest_CountsOnlyKillsAfterAcceptance()
    {
        var world = WorldState.Current;
        var hero = new Warrior("Brak");
        world.Add(KillObjective.CounterKey("scavenger"), 3);

        QuestLog.Accept(QuestLog.TeethAtTheGate, hero);
        Assert.False(QuestLog.ObjectivesMet(QuestLog.TeethAtTheGate));

        world.Add(KillObjective.CounterKey("scavenger"), 5);
        Assert.True(QuestLog.ObjectivesMet(QuestLog.TeethAtTheGate));

        QuestLog.Complete(QuestLog.TeethAtTheGate, hero);
        Assert.True(QuestLog.IsDone(QuestLog.TeethAtTheGate));
    }

    [Fact]
    public void Battle_HeroBeatsWeakEnemiesAndEarnsRewards()
    {
        var warrior = new Warrior("Brak");
        warrior.Weapons.Add(ItemForge.Create("basic_sword"));

        var result = new Battle(warrior, Bestiary.Group(Bestiary.AshScavenger, Bestiary.AshScavenger), new BotBattleController()).Run();

        Assert.Equal(BattleOutcome.Victory, result.Outcome);
        Assert.Equal(30, result.Xp);
        Assert.Equal(2, result.Slain.Count);
        Assert.True(warrior.IsAlive);
    }

    [Fact]
    public void Battle_OutmatchedHeroIsDefeated()
    {
        var mage = new Mage("Vex");
        mage.Weapons.Add(ItemForge.Create("basic_staff"));

        var result = new Battle(mage, Bestiary.Group(Bestiary.CinderHand), new BotBattleController(), new BattleOptions { CanFlee = false }).Run();

        Assert.Equal(BattleOutcome.Defeat, result.Outcome);
        Assert.False(mage.IsAlive);
    }

    [Fact]
    public void Enemy_DamageMultiplier_AppliesWeaknessAndResistance()
    {
        var wraith = Bestiary.Create(Bestiary.AshWraith);

        Assert.Equal(0.5, wraith.DamageMultiplier(Element.NONE, magic: false));
        Assert.True(wraith.DamageMultiplier(Element.ICE, magic: false) > 1.0);
        Assert.True(wraith.DamageMultiplier(Element.ICE, magic: true) > wraith.DamageMultiplier(Element.ICE, magic: false));
    }

    [Fact]
    public void SkillBook_UnlocksByLevelAndByTeaching()
    {
        var mage = new Mage("Vex");
        Assert.DoesNotContain(SkillBook.For(mage), s => s.Id == SkillBook.Unbinding);
        Assert.Contains(SkillBook.For(mage), s => s.Id == "fireball");

        mage.LearnedSkills.Add(SkillBook.Unbinding);
        mage.SetLevel(5);

        Assert.Contains(SkillBook.For(mage), s => s.Id == SkillBook.Unbinding);
        Assert.Contains(SkillBook.For(mage), s => s.Id == "chain_lightning");
    }

    [Fact]
    public void SaveData_RoundTripsProgressionAndNamedLoot()
    {
        var archer = new Archer("Wren");
        archer.SetLevel(4);
        archer.Weapons.Add(ItemForge.Create("ashcarved_bow"));
        archer.EquippedWeapon = (Weapon)archer.Weapons[0];
        archer.AddSupply(SupplyCatalog.HealingPotion, 3);
        archer.LocationId = "scorched_path";

        var restored = (Archer)new CharacterSaveData(archer).ToCharacter();

        Assert.Equal(4, restored.Level);
        Assert.Equal(archer.Perception, restored.Perception);
        Assert.Equal("Ash-carved Bow", restored.EquippedWeapon!.Name);
        Assert.Equal(archer.Weapons.OfType<Weapon>().First().GetEffectiveDamage(), restored.EquippedWeapon.GetEffectiveDamage());
        Assert.Equal(3, restored.SupplyCount(SupplyCatalog.HealingPotion));
        Assert.Equal("scorched_path", restored.LocationId);
    }

    [Fact]
    public void WorldSaveData_RoundTripsFlagsQuestsAndVault()
    {
        var world = new WorldState();
        world.Set(WorldFlags.DrenMorrowAllied, "Brak");
        world.AddReputation(45);
        world.Quests[QuestLog.ScorchedRoad] = QuestStatus.Completed;
        world.Vault.Add(ItemForge.Create("sentinel_blade"));

        var restored = new WorldStateSaveData(world).ToWorldState();

        Assert.True(restored.Has(WorldFlags.DrenMorrowAllied));
        Assert.Equal("Brak", restored.DoneBy(WorldFlags.DrenMorrowAllied));
        Assert.Equal(ReputationTier.Known, restored.Tier());
        Assert.Equal(QuestStatus.Completed, restored.Quests[QuestLog.ScorchedRoad]);
        Assert.Equal("Sentinel Blade", restored.Vault.Single().Name);
    }

    [Fact]
    public void Sharpen_AddsDamageAndNameSuffix()
    {
        var sword = (Weapon)ItemForge.Create("basic_sword");
        int damage = sword.DamageAmount;

        Shop.ApplySharpen(sword);
        Shop.ApplySharpen(sword);

        Assert.Equal("Basic Sword +2", sword.Name);
        Assert.Equal(damage + 4, sword.DamageAmount);
        Assert.Equal(2, Shop.SharpenLevel(sword));
    }
}
