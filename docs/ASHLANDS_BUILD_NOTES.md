# The Ashlands — Playable Build Notes

This is the AI-built clone: the whole Ashlands arc, playable start to finish. This file says how to run it, what is in it, and — most usefully if you are deciding what you want in your own version — **where it departs from the original design and why**.

## Run it

```bash
dotnet run --project RPGManager
```

Saves go to `Save/keeper.json` (override the folder with `RPGMANAGER_SAVE_DIR`). An old `characters.json` is migrated on first load. To start over, delete the `Save` folder.

Developer switches:

| Switch | What it does |
|---|---|
| `dotnet run --project RPGManager -- --balance` | Plays every key fight 400 times per class with a bot and prints win rate, health left, potions used, rounds |
| `RPGMANAGER_SEED=7` | Fixed random seed, for reproducible runs |
| `RPGMANAGER_AUTOBATTLE=1` | The bot fights for you (used for scripted playtests) |

A full run of the main story takes roughly 25–30 fights and ends around level 9.

## What is in the build

| System | Where | Notes |
|---|---|---|
| Combat | `Combat/` | Turn-based, hero vs. group. Status effects on one shared list, ticked once per round. Enemy behaviour enum + once-per-fight traits. Bosses telegraph heavy attacks a turn ahead. |
| Class skills | `Combat/Skill.cs` | Skills describe intent; `Battle` resolves it. Warrior spends **Fury** (builds in the fight), Mage spends **Mana** (carries between fights), Archer spends **Arrows** (per-fight budget = quiver size). |
| Levelling | `Characters/Heroes/` | XP table as an array, abstract `OnLevelUp()` per class, four stats: Might, Guard, Arcana, Perception. Cap 10. |
| Roster | `CharacterRoster` | Slot rules as a dictionary. New heroes start two levels under your strongest. |
| World state | `Worlds/WorldState.cs` | Flags, counters, reputation (tier computed), capped chronicle, the shared vault, and **who** set each flag. |
| Quests | `Quests/` | `Quest` is data, `QuestLog` owns state, objectives are small classes. Progress lives in world counters. |
| Dialogue | `Dialogue/` | Node dictionary; options carry a condition and an effect. Trees are hardcoded in C#. |
| Travel | `Adventure/Travel.cs` | Four places, unlocked by world state, with road encounters. |
| Economy | `UI/Shop.cs`, `Adventure/Game.cs` (`Prices`) | Price is computed for the buyer every time. Sharpen, reforge, coldfire tempering. |
| Content | `Adventure/` | One file per location; NPC conversations live beside the place you meet them. |
| Save | `Saves/` | `SaveGame` = roster + active hero + world. |

## Where this departs from the design docs

**Roster slots open early (level 3 and level 6), not at level 10 / region end.**
The shared world is the game's identity, so the build puts it in your hands in the first half hour instead of after the credits. The catch-up level stops the second hero from being a chore. *If you prefer the original pacing, it is one dictionary in `CharacterRoster`.*

**Quests belong to the Keeper, not to a hero.**
The design said quests taken are per-hero. Here any hero can advance or hand in any quest. It is simpler, and it makes "send the right hero for the job" a real choice — but it does mean a second hero has no early quests of their own, only bounties. Worth deciding deliberately.

**Death is not permanent.**
Lore says a dead hero severs the bond. In play that would delete hours of progress, so a beaten hero wakes at the inn, lighter in the purse. The lore hook is kept ("the bond does not break — you hold on") and turned into a story beat: see the Ashen Throne below.

**NPCs name your other heroes.**
`WorldState.Deeds` records which hero set each flag, so Korin can say "You're with Vex — Vex dealt with those dogs". This is cheap and it is the single thing that sells the mechanic best.

**Three ways into the sealed chamber, three separate rewards.**
Per the design, each class opens the chamber differently (Unbinding spell / Might 15 / Perception 12). Added: the class reward is tracked per class across the save, so after one hero breaks the seal the others can still go down and claim theirs.

**Coldfire.** Fire Essence was a material "for later", and fire weapons are useless against the fire enemies of the Ashlands. So Lira turns the ember inside out: three Fire Essence makes any weapon deal ice damage, which is how a Warrior or Archer gets past a Wraith's resistance to plain steel.

**Class resources and Defend.** Not in the design. Fury / Mana / Arrows give each class a different rhythm, and Defend refunds some of the class resource, so it is never a wasted turn.

**Thresh at the stair.** If you never deal with Thresh, he holds the pilgrim stair at the end and dies never knowing who paid him. If he is your ally, the Court sends Sentinels after him ("Hollow Reprisal"); save him and he fights beside you in the finale.

## Lore added

- **The Cinder Hand** — the Court's handler in the Ashlands, Dren's abandoned contact, final boss. A tired, ordinary woman with a gauntlet tuned to the Focus. Calls herself "a clerk with a glove".
- **The Eighth Throne** — the Empty Throne is not symbolic. The Court is keeping it for the Keeper.
- **The Compact is a lock, not a treaty** — seven seals, each fed by its kingdom. The Emberveil Focus is the Ashlands' seal. Learned from a dead professor's lecture and from Lira.
- **The Keeper was here before** — sitting on the Ashen Throne returns a memory: the mage who burned the sky to kill the dragon was the Keeper's hero. Emberveil "did not destroy itself. It had help." This reframes the non-permanent death above: the Keeper's flaw is that it cannot let go.

All of it is optional to keep. The first two only need the tent documents changed; the last is one scene.

## Choices that carry across heroes

| Choice | Consequence |
|---|---|
| Help Dren | He is at the inn for everyone; names the Court; vouches for you with Lira; tells you the boss's weak side (she starts Exposed) |
| Turn Dren in | 150 gold; he is gone for everyone; the Court's papers later say where they sent him |
| Kill / turn / ally Thresh | Road opens either way (cheaper supplies, fewer ambushes); only an ally fights at the Throne |
| Take / leave the Focus | Taken: the boss's mid-fight power-up fails and she is Exposed. Left: she heals and hits harder |
| Sit / walk away | Sit: +3 main stat, −15 health, and the memory. Walk away: reputation |

## Things I would look at next

- Balance is on the forgiving side. No scripted run lost a fight. Raising enemy attack ~15% in `Bestiary.cs` would make potions matter more.
- Gold piles up late; there is not enough to spend it on.
- Armour does not exist as an item (only the Warrior's Dragonshard relic), and weapon durability is still unused.
- Dialogue trees are code. They are now stable enough to see what a data format would need.
