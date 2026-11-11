# RPG Manager – Development Roadmap

![Status: Active](https://img.shields.io/badge/Status-Active-success)
![Version: v0.1.2](https://img.shields.io/badge/Version-v0.1.2-blue)
![Platform: .NET 8](https://img.shields.io/badge/Platform-.NET%208-512BD4)

Welcome to the official development roadmap for **RPG Manager**, a text-based RPG engine built in C# by Tombomeke Studios.

---

## Vision

RPG Manager is built around one core idea: you manage a **roster of heroes**, not a single character. Each hero has their own class, level, and inventory — but they all share the same living world. What one hero does changes how NPCs talk to the others, unlocks paths, and shifts reputations across every region of Tavaryn.

The long-term goal is a fully playable, story-driven console RPG where:
- Multiple hero classes each have unique strengths that open different paths
- The world remembers what you've done — across all your characters
- Combat, quests, and exploration grow in depth with each release
- The architecture stays clean enough to extend indefinitely

---

## Milestones

| Version | Milestone | Status | Target |
|:---|:---|:---|:---|
| **v0.1** | Core Engine — classes, weapons, save system | Done | Oct 2025 |
| **v0.2** | World & Travel — locations, regions, travel loop | In Progress | May 2026 |
| **v0.3** | Combat System — turn-based fights, enemies, loot | Planned | Jul 2026 |
| **v0.4** | Spells & Leveling — mana system, XP, character roster | Planned | Sep 2026 |
| **v0.5** | Shared World State — reputation, world flags, NPC memory | Planned | Nov 2026 |
| **v0.6** | NPCs & Dialogue — dialogue trees, class reactions, quests | Planned | Q1 2027 |
| **v0.7** | Quests & Economy — quest system, shops, gold loop | Planned | Q2 2027 |
| **v1.0** | Full CLI Release — complete first story arc, polished UI | Planned | Q4 2027 |

---

## Feature Board

### Currently Working On

- [ ] **Spell System:** Mana costs, spell casting in combat, elemental damage
- [ ] **World & Location Travel:** Travel between locations, encounter rolls on the road
- [ ] **NPC Foundation:** Interaction structure ready for dialogue trees
- [x] **Weapons Refactor (`feature/weapons`):** generalized weapon categories + per-family variants (bow/staff/sword/axe/dagger/quiver)

### Active Branches

- `feature/combat`
- `feature/leveling`
- `feature/world-state`
- `feature/npc-dialogue`
- `feature/travel`
- `feature/quests`
- `feature/economy`
- `feature/spells`
- `feature/weapons`

### Characters & Classes

- [x] Character creation and naming
- [x] Warrior, Mage, and Archer base classes
- [x] Per-class default equipment
- [ ] Leveling and XP system
- [ ] Per-class stat bonuses on level-up
- [ ] Character roster with unlock conditions (e.g., reach level 10 → unlock slot 2)
- [ ] Ability trees per class

### Combat

- [ ] Enemy class with health, damage, element, and loot table
- [ ] Turn-based combat loop (player turn / enemy turn)
- [ ] Player actions in combat: attack, cast spell, use item, flee
- [ ] Elemental resistances and weakness multipliers
- [ ] Status effects: burn, freeze, poison, stun
- [ ] Win/lose outcomes with XP and loot rewards

### Weapons & Equipment

- [x] Weapon rarity and upgrade scaling
- [x] Melee and ranged weapon separation
- [x] Inventory slot capacity enforcement
- [ ] Proper `Inventory` class replacing ad-hoc item handling
- [ ] Elemental damage modifiers in combat
- [ ] Durability degradation during combat

### Spells & Magic

- [x] Base `Spell` class with `Cast()`
- [x] `Fireball` prototype
- [ ] Mana cost and cooldown per spell
- [ ] Mage spell book (list of learned spells)
- [ ] Mana regeneration per combat turn
- [ ] Full implementations: Fireball, IceShard, LightningBolt, HealingLight
- [ ] Status effects tied to spell elements

### World & Exploration

- [x] World and Location base architecture
- [ ] Travel system between locations
- [ ] Random encounter rolls during travel
- [ ] Level-gated region entry
- [ ] Starter region: the Ashlands (3–4 connected locations)
- [ ] Random world events during exploration

### Shared World State

- [ ] Global flag system (world events, quest completions)
- [ ] Per-region reputation system with tiers
- [ ] Per-NPC relationship tracking
- [ ] World event log (human-readable history)
- [ ] All character actions feed into shared world state

### NPCs & Dialogue

- [ ] Dialogue tree system (nodes with conditions and actions)
- [ ] NPC reactions based on reputation tier
- [ ] Class-specific dialogue options
- [ ] Dialogue actions: give quest, set world flag, modify reputation
- [ ] Quest giver NPCs

### Quests

- [ ] Quest class with objectives and rewards
- [ ] Objective types: Kill, Collect, Reach, Talk, Deliver
- [ ] Quest progress tracked in world state (shared across all characters)
- [ ] Class-exclusive quests
- [ ] Quest log in the game menu
- [ ] Starter quests in the Ashlands

### Economy

- [ ] Gold drops from enemies
- [ ] Shop class with buy/sell
- [ ] Reputation-based price discounts
- [ ] BlackSmith fully wired to trade system

### Save System

- [x] JSON polymorphic save/load
- [x] Overridable save directory via `RPGMANAGER_SAVE_DIR`
- [ ] Save `WorldState` alongside character data
- [ ] Save quest progress
- [ ] Auto-save after major events

---


<!-- AUTO-GENERATED BELOW – DO NOT EDIT -->

# 🧮 Project Overview (auto-generated)

> Automatically generated from RPGManagerLib source files.

_Last updated: **2026-11-11 07:07**_

### 📊 Codebase Stats
- **Namespaces:** 23
- **Classes:** 74
- **Unique Methods:** 189
- **Pending TODOs:** 3


## 🧱 RPGManagerLib.Adventure

### [AshenThroneLocation.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Adventure/AshenThroneLocation.cs)
> The Ashen Throne: the pilgrim stair, the Court's camp, the Cinder Hand, and the end of the Ashlands arc. Almost everything here changes depending on what the Keeper's heroes did earlier.

**Public Methods:**
- `Visit()`
- `Epilogue()`

### [Story.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Adventure/CindersRestLocation.cs)
> Small text helpers shared by the location scripts.

**Public Methods:**
- `ByClass()`
- `Visit()`
- `Talk()`
- `TalkAtInn()`

### [EmberveilRuinsLocation.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Adventure/EmberveilRuinsLocation.cs)
> The Emberveil Ruins: Lira's camp, the vents, and the descent to the sealed chamber — which each class opens in its own way.

**Public Methods:**
- `Visit()`

### [Game.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Adventure/Game.cs)
> The running game session: the roster, the shared world, and saving.

**Public Methods:**
- `Has()`
- `Set()`
- `Load()`
- `Save()`
- `Reset()`
- `Give()`
- `GiveXp()`
- `GiveReputation()`
- `CompleteQuest()`
- `AcceptQuest()`
- `GiveItem()`
- `GiveSupply()`
- `Fight()`
- `ScavengerPack()`
- `BanditGroup()`
- `RuinsGroup()`
- `Smithy()`
- `Supplies()`
- `Sell()`

### [Intro.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Adventure/Intro.cs)
> The opening: the Keeper wakes, bonds a first hero, and arrives in Cinder's Rest.

**Public Methods:**
- `Play()`

### [ScorchedPathLocation.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Adventure/ScorchedPathLocation.cs)
> The Scorched Path: the bandit road. Patrols, the broken wagon, Dren Morrow, the burned camp, and Commander Thresh's war-camp.

**Public Methods:**
- `Visit()`
- `FindDren()`
- `ReprisalBegins()`

### [Travel.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Adventure/Travel.cs)
> A place a hero can stand. Pure data: other systems read it and decide what happens there.

**Public Methods:**
- `Place()`
- `NameOf()`
- `IsUnlocked()`
- `Menu()`


## ⚔️ RPGManagerLib.Characters.Heroes

### [Archer.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Characters/Heroes/Archer.cs)
*Inherits from: `Character`*  
> Ranged skirmisher. Critical hits, poison and evasion; every fight is an arrow budget set by the quiver.

**Public Methods:**
- `CanWield()`

### [Character.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Characters/Heroes/Character.cs)
*Inherits from: `ICombatant`*  
> A hero bonded to the Keeper: name, vitals, stats, level, equipment and supplies.

**Public Methods:**
- `CanWield()`
- `SlotsFor()`
- `TryAddItem()`
- `SupplyCount()`
- `AddSupply()`
- `UseSupply()`
- `GainExperience()`
- `SetLevel()`
- `RestoreFully()`
- `TakeDamage()`
- `Restore()`
- `RestoreMana()`
- `Heal()`
- `Damage()`
- `TravelTo()`

### [CharacterRoster.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Characters/Heroes/CharacterRoster.cs)
> The Keeper's bonded heroes. Owns the rules a raw list cannot enforce: how many slots are unlocked, who is active, and how strong a newly bonded hero starts.

**Public Methods:**
- `TryAdd()`
- `Load()`
- `SetActive()`

### [Mage.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Characters/Heroes/Mage.cs)
*Inherits from: `Character`*  
> Spellcaster. Fragile, but spells ignore armour tricks and hit elemental weaknesses. Mana carries over between fights.

**Public Methods:**
- `CanWield()`
- `CastSpell()`

### [Warrior.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Characters/Heroes/Warrior.cs)
*Inherits from: `Character`*  
> Frontline fighter. Tough, hits hard with melee weapons, and builds Fury as the fight goes on.

**Public Methods:**
- `CanWield()`


## 📜 RPGManagerLib.Characters.NPCs

### [BlackSmith.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Characters/NPCs/BlackSmith.cs)
*Inherits from: `NPC`*  
**Public Methods:**
- `Interact()`
- `Trade()`

### [NPC.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Characters/NPCs/NPC.cs)
**Public Methods:**
- `Interact()`
- `Trade()`


## 🧙 RPGManagerLib.Combat

### [BalanceSimulator.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Combat/BalanceSimulator.cs)
> Developer tool: plays each key fight many times with a simple bot and prints win rates. Run with <c>dotnet run --project RPGManager -- --balance</c>.

**Public Methods:**
- `Run()`

### [BattleOptions.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Combat/Battle.cs)
> How a fight ended.</summary> public enum BattleOutcome { Victory, Defeat, Fled } <summary>What the hero can do on a turn.</summary> public enum HeroActionKind { Attack, Skill, Supply, Defend, Flee } <summary>A chosen hero action.</summary> <param name="Kind">The action type.</param> <param name="Id">Skill id or supply id, when relevant.</param> <param name="Target">Index into <see cref="Battle.Enemies"/>.</param> public record HeroAction(HeroActionKind Kind, string? Id = null, int Target = 0); <summary>Per-fight rules set by whoever starts the battle.</summary> public class BattleOptions { <summary>Story fights cannot be fled.</summary> public bool CanFlee { get; set; } = true; <summary>Name of an ally fighting alongside the hero, if any.</summary> public string? AllyName { get; set; } <summary>Average damage the ally deals each round.</summary> public double AllyDamage { get; set; } <summary>The Cinder Hand cannot draw on the Focus (the Keeper holds it).</summary> public bool FocusDenied { get; set; } <summary>Rounds the first enemy starts Exposed (a known weakness).</summary> public int OpeningExposedTurns { get; set; } } <summary>What came out of a fight.</summary> public class BattleResult { public BattleOutcome Outcome { get; set; } public int Xp { get; set; } public int Gold { get; set; } public int Rounds { get; set; } public List<string> Loot { get; } = new(); public List<Enemy> Slain { get; } = new(); } <summary>Supplies the hero's decisions: the console for players, a bot for the balance simulator.</summary> public interface IBattleController { HeroAction ChooseAction(Battle battle); } <summary> One turn-based fight between a hero and a group of enemies. Each round: hero acts, ally acts, enemies act, then effects tick in a single pass.

**Public Methods:**
- `HeroAction()`
- `CanUse()`
- `UsableSupplies()`
- `Run()`

### [ConsoleBattleController.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Combat/BattleControllers.cs)
*Inherits from: `IBattleController`*  
> Lets the player drive the hero from the console: shows the battlefield and reads a choice each turn.

**Public Methods:**
- `ChooseAction()`

### [Bestiary.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Combat/Bestiary.cs)
> Every enemy of the Ashlands. Each call builds a fresh instance at full health.

**Public Methods:**
- `Create()`
- `Group()`

### [Enemy.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Combat/Enemy.cs)
*Inherits from: `ICombatant`*  
> Broad turn-by-turn temperament. Keeps fights feeling different without real AI.

**Public Methods:**
- `LootEntry()`
- `Ready()`
- `TakeDamage()`
- `Restore()`
- `DamageMultiplier()`

### [Skill.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Combat/Skill.cs)
> Who a skill is aimed at.</summary> public enum SkillTarget { Enemy, AllEnemies, Self } <summary>Behaviour that goes beyond "deal damage, maybe apply a status".</summary> public enum SkillSpecial { None, Execute, Heal, Ward, Evade, WarCry, Unbinding, Pierce } <summary> A class ability. Skills only describe intent — cost, numbers, element, status — and the battle loop resolves what that means against resistances and active effects.

**Public Methods:**
- `Get()`
- `For()`
- `UnlockedAt()`

### [StatusEffect.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Combat/StatusEffect.cs)
> Every temporary condition a combatant can be under.

**Public Methods:**
- `Has()`
- `Apply()`
- `Remove()`
- `StatusLine()`


## 🏹 RPGManagerLib.Core

### [Rng.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Core/Rng.cs)
> Single shared random source for the whole game. Set the RPGMANAGER_SEED environment variable to get reproducible runs (handy for playtesting).

**Public Methods:**
- `Reseed()`
- `Next()`
- `NextDouble()`
- `Chance()`


## 🐉 RPGManagerLib.Dialogue

### [DialogueOption.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Dialogue/Dialogue.cs)
> One thing the player can say. Conditions and effects are small functions so each option can check flags, reputation or class and change the world without the runner knowing how.

**Public Methods:**
- `Add()`
- `Run()`


## 🏰 RPGManagerLib.Exceptions

### [CharacterException.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Exceptions/CharacterException.cs)
*Inherits from: `Exception`*  
> Base exception type for character-related validation issues.

_No unique public methods found._

### [InvalidWeaponException.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Exceptions/InvalidWeaponException.cs)
*Inherits from: `Exception`*  
> Thrown when an input or selection does not correspond to any known weapon type.

_No unique public methods found._

### [NegativeDamageException.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Exceptions/NegativeDamageException.cs)
*Inherits from: `CharacterException`*  
> Thrown when a negative value is supplied for damage.

_No unique public methods found._

### [NegativeHealException.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Exceptions/NegativeHealException.cs)
*Inherits from: `CharacterException`*  
> Thrown when a negative value is supplied for healing.

_No unique public methods found._

### [OverhealException.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Exceptions/OverhealException.cs)
*Inherits from: `CharacterException`*  
> Thrown when healing would exceed the maximum allowed health.

_No unique public methods found._

### [OverkillException.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Exceptions/OverkillException.cs)
*Inherits from: `CharacterException`*  
> Thrown when damage would drop health beyond the permitted lower bound.

_No unique public methods found._


## 🧭 RPGManagerLib.Items

### [ItemForge.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/ItemForge.cs)
> Creates equipment by id. Named loot and quest rewards are ordinary weapon classes with custom names and numbers, so they save and load through the existing weapon save data unchanged.

**Public Methods:**
- `Create()`
- `ValueOf()`
- `Describe()`

### [SupplyCatalog.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Supplies.cs)
> What a supply is for.

**Public Methods:**
- `Supply()`
- `Get()`
- `Exists()`


## 🪄 RPGManagerLib.Items.Staffs

### [BasicStaff.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Staffs/BasicStaff.cs)
*Inherits from: `Staff`*  
> Represents a staff weapon that can be used in combat, providing basic damage and durability attributes.

_No unique public methods found._

### [Staff.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Staffs/Staff.cs)
*Inherits from: `Weapon`*  
> Represents a staff weapon that can be used in combat, providing basic damage and durability attributes.

_No unique public methods found._

### [WindStaff.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Staffs/WindStaff.cs)
*Inherits from: `Staff`*  
_No unique public methods found._


## 🧰 RPGManagerLib.Items.Weapons

### [Weapon.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Weapon.cs)
*Inherits from: `IEquipable`*  
> Base type for all weapons that can be equipped by a character.

**Public Methods:**
- `GetRarityMultiplier()`
- `GetEffectiveDamage()`
- `GetEffectiveDurability()`
- `UpgradeWeapon()`


## 🎯 RPGManagerLib.Items.Weapons.Bows

### [Bow.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Bows/Bow.cs)
*Inherits from: `Weapon`*  
> Represents an abstract base class for all bow weapons, providing common properties and behaviors for derived bow types.

_No unique public methods found._

### [HuntingBow.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Bows/HuntingBow.cs)
*Inherits from: `Bow`*  
_No unique public methods found._

### [SimpleBow.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Bows/SimpleBow.cs)
*Inherits from: `Bow`*  
> A basic bow with common stats suitable for early gameplay.

_No unique public methods found._

### [WarBow.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Bows/WarBow.cs)
*Inherits from: `Bow`*  
_No unique public methods found._


## 🧱 RPGManagerLib.Items.Weapons.Melee.Axes

### [Axe.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Melee/Axes/Axe.cs)
*Inherits from: `Weapon`*  
> Represents a melee weapon of type axe with predefined damage, durability, rarity, and inventory space attributes.

_No unique public methods found._

### [BasicAxe.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Melee/Axes/BasicAxe.cs)
*Inherits from: `Axe`*  
> Initializes a new instance of the Axe class with predefined attributes for a basic axe weapon.

_No unique public methods found._

### [BattleAxe.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Melee/Axes/BattleAxe.cs)
*Inherits from: `Axe`*  
_No unique public methods found._

### [GreatAxe.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Melee/Axes/GreatAxe.cs)
*Inherits from: `Axe`*  
_No unique public methods found._


## ⚔️ RPGManagerLib.Items.Weapons.Melee.Daggers

### [BasicDagger.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Melee/Daggers/BasicDagger.cs)
*Inherits from: `Dagger`*  
> Initializes a new <see cref="Dagger"/> with default values.

_No unique public methods found._

### [Dagger.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Melee/Daggers/Dagger.cs)
*Inherits from: `Weapon`*  
> A fast melee weapon with low damage and short cooldown.

_No unique public methods found._


## 📜 RPGManagerLib.Items.Weapons.Melee.Spears

### [BasicSpear.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Melee/Spears/BasicSpear.cs)
*Inherits from: `Weapon`*  
> A basic spear with balanced damage and durability, suitable for close combat.

_No unique public methods found._

### [Spear.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Melee/Spears/Spear.cs)
*Inherits from: `Weapon`*  
> A reach melee weapon with solid durability and moderate cooldown.

_No unique public methods found._


## 🧙 RPGManagerLib.Items.Weapons.Melee.Swords

### [BasicSword.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Melee/Swords/BasicSword.cs)
*Inherits from: `Sword`*  
> Initializes a new <see cref="Sword"/> with default values.

_No unique public methods found._

### [BroadSword.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Melee/Swords/BroadSword.cs)
*Inherits from: `Sword`*  
_No unique public methods found._

### [GreatSword.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Melee/Swords/GreatSword.cs)
*Inherits from: `Sword`*  
_No unique public methods found._

### [Sword.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Melee/Swords/Sword.cs)
*Inherits from: `Weapon`*  
> A versatile melee weapon with balanced damage and durability, suitable for close combat.

_No unique public methods found._


## 🏹 RPGManagerLib.Items.Weapons.Quivers

### [BigQuiver.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Quivers/BigQuiver.cs)
*Inherits from: `Quiver`*  
_No unique public methods found._

### [MediumQuiver.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Quivers/MediumQuiver.cs)
*Inherits from: `Quiver`*  
_No unique public methods found._

### [SmallQuiver.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Quivers/SmallQuiver.cs)
*Inherits from: `Quiver`*  
> Represents a small quiver designed to hold arrows, suitable for basic inventory needs.

_No unique public methods found._


## 🐉 RPGManagerLib.Locations

### [Location.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Locations/Location.cs)
> Adds a non-player character (NPC) to the current list of NPCs.

**Public Methods:**
- `AddNPC()`
- `RemoveNPC()`
- `GetNPCs()`


## 🏰 RPGManagerLib.Quests

### [KillObjective.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Quests/Quest.cs)
*Inherits from: `IQuestObjective`*  
> One thing a quest asks for. Each objective type knows how to read its own progress from the world.

**Public Methods:**
- `CounterKey()`
- `OnAccept()`
- `IsComplete()`
- `Describe()`

### [QuestLog.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Quests/QuestLog.cs)
> Every quest in the Ashlands and the rules for accepting and completing them. Quest state lives in <see cref="WorldState"/>: quests belong to the Keeper, so any hero can advance them.

**Public Methods:**
- `Get()`
- `Status()`
- `IsActive()`
- `IsDone()`
- `Started()`
- `Active()`
- `Completed()`
- `Accept()`
- `ObjectivesMet()`
- `Complete()`


## 🧭 RPGManagerLib.Saves

### [CharacterSaveData.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Saves/CharacterSaveData.cs)
> Serializable snapshot of a character for saving and loading.

**Public Methods:**
- `FromEquipable()`
- `ToCharacter()`

### [EquipableSaveData.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Saves/EquipableSaveData.cs)
> Base class for all saveable equipable items. Uses polymorphic JSON serialization to handle different item types cleanly.

**Public Methods:**
- `ToEquipable()`

### [QuiverSaveData.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Saves/QuiverSaveData.cs)
*Inherits from: `EquipableSaveData`*  
> Represents the save data for a quiver, including its capacity and other equipable properties.

**Public Methods:**
- `ToEquipable()`

### [WorldStateSaveData.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Saves/SaveGame.cs)
> Serializable snapshot of the shared world: flags, counters, reputation, quests, chronicle and vault.

**Public Methods:**
- `ToWorldState()`

### [SaveManager.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Saves/SaveManager.cs)
> Reads and writes save files as JSON.

**Public Methods:**
- `SaveGame()`
- `HasSave()`
- `DeleteGame()`
- `LoadCharacters()`
- `SaveCharacters()`

### [WeaponSaveData.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Saves/WeaponSaveData.cs)
*Inherits from: `EquipableSaveData`*  
> Represents the data required to serialize and reconstruct a weapon, including its type, damage, durability, level, and elemental affinity.

**Public Methods:**
- `ToEquipable()`


## 🪄 RPGManagerLib.Spells

### [Fireball.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Spells/Fireball.cs)
*Inherits from: `Spell`*  
_No unique public methods found._

### [Spell.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Spells/Spell.cs)
**Public Methods:**
- `Cast()`

**TODOs:**
- [ ] Move spell metadata toward unlockable/progression-aware content once leveling and spell acquisition are added.
- [ ] Integrate combat logging, target validation, and status-effect resolution with CombatManager instead of direct console-only flow.


## 🧰 RPGManagerLib.UI

### [CharacterFactory.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/UI/CharacterFactory.cs)
> Provides functionality to create character instances based on user input.

**Public Methods:**
- `CreateCharacter()`
- `GiveStartingSupplies()`
- `CreateDefaultWeaponsWarrior()`
- `CreateDefaultWeaponsArcher()`
- `CreateDefaultWeaponsMage()`

### [GameMenu.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/UI/GameMenu.cs)
> The top-level game loop: loads the session, plays the opening for a new Keeper, then keeps showing the active hero's current location until the player quits.

**Public Methods:**
- `Start()`
- `Banner()`
- `Show()`

### [HeroScreens.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/UI/HeroScreens.cs)
> The Keeper's menu: hero sheet, pack, journal, chronicle, standing and roster.

**Public Methods:**
- `KeeperMenu()`
- `Sheet()`
- `WriteVitals()`
- `Pack()`
- `Journal()`
- `Standing()`
- `WorldSummary()`
- `Chronicle()`
- `Roster()`
- `BondNewHero()`
- `Vault()`

### [MenuSystem.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/UI/MenuSystem.cs)
> Minimal console menu helper that maps string keys to labeled actions with optional hints.

**Public Methods:**
- `AddOption()`
- `Show()`

### [Shop.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/UI/Shop.cs)
> Buying, selling and smithing screens. Shops are just stock lists; the price depends on who is asking.

**Public Methods:**
- `BuyEquipment()`
- `Sell()`
- `BuySupplies()`
- `SharpenLevel()`
- `ApplySharpen()`
- `Smithing()`

### [InputEndedException.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/UI/Ui.cs)
*Inherits from: `Exception`*  
> Thrown when console input runs out (piped input reached its end), so the game can save and exit cleanly.

**Public Methods:**
- `MenuOption()`
- `Clear()`
- `Write()`
- `Line()`
- `Header()`
- `Sub()`
- `Narrate()`
- `Vision()`
- `Say()`
- `Good()`
- `Bad()`
- `Gain()`
- `Note()`
- `World()`
- `Choose()`
- `Menu()`
- `Confirm()`
- `Prompt()`
- `ReadLine()`
- `Pause()`
- `Beat()`
- `Bar()`
- `HealthColor()`
- `Wrap()`


## 🎯 RPGManagerLib.Weapons.Quivers

### [Quiver.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Items/Weapons/Quivers/Quiver.cs)
*Inherits from: `IEquipable`*  
> Represents an abstract base class for quivers, which are used to store and manage arrows in an inventory system.

_No unique public methods found._


## 🧱 RPGManagerLib.Worlds

### [World.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Worlds/World.cs)
> Unlocks the current instance, allowing access to its features.

**Public Methods:**
- `AddLocation()`
- `RemoveLocation()`
- `Unlock()`
- `Lock()`

**TODOs:**
- [ ] Evolve this into region/world data that can participate in travel routes, encounter tables, and reputation/world-flag checks.

### [WorldFlags.cs](https://github.com/tombomeke-ehb/RPGManager/main/RPGManagerLib/Worlds/WorldState.cs)
> How the people of a region see the Keeper. Always computed from the raw score, never stored.

**Public Methods:**
- `Has()`
- `Set()`
- `Count()`
- `Add()`
- `GetReputation()`
- `AddReputation()`
- `Tier()`
- `TierFor()`
- `Log()`

