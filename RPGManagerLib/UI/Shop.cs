using System.Text.RegularExpressions;
using RPGManagerLib.Adventure;
using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Items;
using RPGManagerLib.Items.Staffs;
using RPGManagerLib.Items.Weapons;
using RPGManagerLib.Worlds;

namespace RPGManagerLib.UI;

/// <summary>
/// Buying, selling and smithing screens. Shops are just stock lists; the price depends on who is asking.
/// </summary>
public static class Shop
{
    /// <summary>Buy equipment from a stock list of <see cref="ItemForge"/> ids.</summary>
    public static void BuyEquipment(string title, IReadOnlyList<string> stockIds)
    {
        while (true)
        {
            var hero = Game.Hero;
            Ui.Header(title);
            Ui.Line($"  Gold {hero.Gold}     Pack slots {hero.UsedSlots}/{Character.InventoryCapacity}", ConsoleColor.Yellow);

            var stock = stockIds.Select(ItemForge.Create).ToList();
            var options = stock.Select(item =>
            {
                int price = Prices.Smithy(ItemForge.ValueOf(item));
                string usable = item is Weapon w && !hero.CanWield(w) ? "  (not for your class)" : "";
                return $"{price,4}g  {ItemForge.Describe(item)}{usable}";
            }).ToList();

            int choice = Ui.Choose(options, "Back");
            if (choice < 0) return;

            var chosen = stock[choice];
            int cost = Prices.Smithy(ItemForge.ValueOf(chosen));
            if (hero.Gold < cost)
            {
                Ui.Bad("You can't afford that.");
            }
            else if (!hero.TryAddItem(chosen))
            {
                Ui.Bad("Your pack is full. Sell something or use the Keeper's Vault at the inn.");
            }
            else
            {
                hero.Gold -= cost;
                Ui.Good($"Bought the {chosen.Name} for {cost} gold.");
                if (chosen is Weapon bought && hero.CanWield(bought) &&
                    (hero.ActiveWeapon == null || bought.GetEffectiveDamage() > hero.ActiveWeapon.GetEffectiveDamage() || hero.ActiveWeapon.Type != bought.Type && IsPrimary(hero, bought)))
                {
                    hero.EquippedWeapon = bought;
                    Ui.Note($"  {hero.Name} takes it in hand.");
                }
                Game.Save();
            }
            Ui.Pause();
        }
    }

    private static bool IsPrimary(Character hero, Weapon weapon) =>
        (hero is Archer && weapon.Type == WeaponType.BOW) || (hero is Mage && weapon.Type == WeaponType.STAFF);

    /// <summary>Sell equipment and materials.</summary>
    public static void Sell(string title)
    {
        while (true)
        {
            var hero = Game.Hero;
            Ui.Header(title);
            Ui.Line($"  Gold {hero.Gold}", ConsoleColor.Yellow);

            var options = new List<string>();
            var actions = new List<Action>();
            foreach (var item in hero.Weapons)
            {
                int price = Prices.Sell(ItemForge.ValueOf(item));
                options.Add($"{price,4}g  {ItemForge.Describe(item)}{(item == hero.ActiveWeapon ? "  ◄ in hand" : "")}");
                actions.Add(() =>
                {
                    if (item == hero.ActiveWeapon && !Ui.Confirm("That's the weapon in your hand. Sell it anyway?")) return;
                    hero.Weapons.Remove(item);
                    if (hero.EquippedWeapon == item) hero.EquippedWeapon = null;
                    hero.Gold += price;
                    Ui.Good($"Sold the {item.Name} for {price} gold.");
                });
            }
            foreach (var supply in hero.Supplies.Keys.Select(SupplyCatalog.Get).Where(s => s.Price > 0).ToList())
            {
                int price = Prices.Sell(supply.Price);
                options.Add($"{price,4}g  {supply.Name} (you have {hero.SupplyCount(supply.Id)})");
                actions.Add(() =>
                {
                    hero.UseSupply(supply.Id);
                    hero.Gold += price;
                    Ui.Good($"Sold one {supply.Name} for {price} gold.");
                });
            }

            if (options.Count == 0)
            {
                Ui.Note("You have nothing to sell.");
                Ui.Pause();
                return;
            }

            int choice = Ui.Choose(options, "Back");
            if (choice < 0) return;
            actions[choice]();
            Game.Save();
        }
    }

    /// <summary>Buy supplies from a stock list of supply ids.</summary>
    public static void BuySupplies(string title, IReadOnlyList<string> stockIds)
    {
        while (true)
        {
            var hero = Game.Hero;
            Ui.Header(title);
            Ui.Line($"  Gold {hero.Gold}", ConsoleColor.Yellow);
            if (Prices.RoadIsOpen) Ui.World("Caravans are back on the road — prices are down.");

            var stock = stockIds.Select(SupplyCatalog.Get).ToList();
            var options = stock.Select(s =>
                $"{Prices.Supplies(s.Price, s.Id),4}g  {s.Name} — {s.Description} (you have {hero.SupplyCount(s.Id)})").ToList();

            int choice = Ui.Choose(options, "Back");
            if (choice < 0) return;

            var chosen = stock[choice];
            int cost = Prices.Supplies(chosen.Price, chosen.Id);
            if (hero.Gold < cost)
            {
                Ui.Bad("You can't afford that.");
                Ui.Pause();
                continue;
            }
            hero.Gold -= cost;
            hero.AddSupply(chosen.Id);
            Game.Save();
        }
    }

    // ------------------------------------------------------------------ smithing

    private static readonly Regex SharpenSuffix = new(@" \+(\d)$");

    /// <summary>How many times a weapon has been sharpened (read from its "+N" name suffix).</summary>
    public static int SharpenLevel(Weapon weapon)
    {
        var match = SharpenSuffix.Match(weapon.Name);
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    /// <summary>Sharpens a weapon one step: more damage and a "+N" on its name.</summary>
    public static void ApplySharpen(Weapon weapon)
    {
        int level = SharpenLevel(weapon);
        weapon.Name = SharpenSuffix.Replace(weapon.Name, "") + $" +{level + 1}";
        weapon.DamageAmount += weapon is Staff ? 1 : 2;
    }

    /// <summary>Korin's forge services: sharpen and reforge.</summary>
    public static void Smithing()
    {
        while (true)
        {
            var hero = Game.Hero;
            Ui.Header("Korin's Forge");
            Ui.Line($"  Gold {hero.Gold}     Scraps {hero.SupplyCount(SupplyCatalog.Scraps)}", ConsoleColor.Yellow);
            Ui.Note("  Sharpen: more damage, up to +3.   Reforge: raise a weapon's rarity (needs Scraps).");

            var weapons = hero.Weapons.OfType<Weapon>().ToList();
            if (weapons.Count == 0)
            {
                Ui.Note("You have no weapons to work on.");
                Ui.Pause();
                return;
            }

            bool free = Game.Has(WorldFlags.KorinSharpenOwed);
            if (free) Ui.Good("  Korin owes you one free sharpening for the scavenger bounty.");

            var options = new List<string>();
            var actions = new List<Action>();
            foreach (var weapon in weapons)
            {
                int level = SharpenLevel(weapon);
                if (level < 3)
                {
                    int cost = free ? 0 : Prices.Smithy(40 * (level + 1));
                    options.Add($"Sharpen {weapon.Name} — {(cost == 0 ? "free" : cost + "g")}");
                    actions.Add(() => Sharpen(weapon, cost));
                }

                var (gold, scraps, allowed, why) = ReforgeCost(weapon);
                if (weapon.Rarity < Rarity.EPIC)
                {
                    options.Add($"Reforge {weapon.Name} to {weapon.Rarity + 1} — {gold}g + {scraps} Scraps{(allowed ? "" : $"  ({why})")}");
                    actions.Add(() => Reforge(weapon, gold, scraps, allowed, why));
                }
            }

            if (options.Count == 0)
            {
                Ui.Note("\"Nothing more I can do with these,\" Korin says. \"They're as good as they get here.\"");
                Ui.Pause();
                return;
            }

            int choice = Ui.Choose(options, "Back");
            if (choice < 0) return;
            actions[choice]();
            Game.Save();
            Ui.Pause();
        }
    }

    private static (int Gold, int Scraps, bool Allowed, string Why) ReforgeCost(Weapon weapon)
    {
        // With the Door-Warden's core in the forge, Korin needs no Scraps.
        int scale = Contracts.Done(Contracts.SleepingWarden) ? 0 : 1;
        return weapon.Rarity switch
        {
            Rarity.COMMON => (Prices.Smithy(90), 4 * scale, true, ""),
            Rarity.UNCOMMON => (Prices.Smithy(220), 8 * scale, Game.Tier >= ReputationTier.Known, "Korin only does this for people he knows"),
            Rarity.RARE => (Prices.Smithy(480), 12 * scale, Game.Tier >= ReputationTier.Honored, "Korin only does this for the Honored"),
            _ => (0, 0, false, "")
        };
    }

    private static void Sharpen(Weapon weapon, int cost)
    {
        var hero = Game.Hero;
        if (hero.Gold < cost)
        {
            Ui.Bad("You can't afford that.");
            return;
        }
        hero.Gold -= cost;
        if (cost == 0) Game.World.Flags[WorldFlags.KorinSharpenOwed] = false;
        ApplySharpen(weapon);
        Ui.Narrate("Korin works the edge in long, unhurried strokes, tests it on his thumbnail, and grunts.");
        Ui.Good($"It is now the {weapon.Name} ({weapon.GetEffectiveDamage()} damage).");
    }

    private static void Reforge(Weapon weapon, int gold, int scraps, bool allowed, string why)
    {
        var hero = Game.Hero;
        if (!allowed)
        {
            Ui.Bad(why + ".");
            return;
        }
        if (hero.Gold < gold || hero.SupplyCount(SupplyCatalog.Scraps) < scraps)
        {
            Ui.Bad("You don't have the gold and Scraps for that.");
            return;
        }
        hero.Gold -= gold;
        hero.UseSupply(SupplyCatalog.Scraps, scraps);
        weapon.UpgradeWeapon();
        Ui.Narrate("The forge roars for most of an hour. When Korin hands the weapon back it is the same shape and a different thing.");
        Ui.Good($"{weapon.Name} is now {weapon.Rarity} ({weapon.GetEffectiveDamage()} damage).");
    }
}
