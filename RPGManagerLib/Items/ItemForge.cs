using RPGManagerLib.Items.Staffs;
using RPGManagerLib.Items.Weapons;
using RPGManagerLib.Items.Weapons.Bows;
using RPGManagerLib.Items.Weapons.Melee.Axes;
using RPGManagerLib.Items.Weapons.Melee.Daggers;
using RPGManagerLib.Items.Weapons.Melee.Spears;
using RPGManagerLib.Items.Weapons.Melee.Swords;
using RPGManagerLib.Items.Weapons.Quivers;
using RPGManagerLib.Weapons.Quivers;

namespace RPGManagerLib.Items;

/// <summary>
/// Creates equipment by id. Named loot and quest rewards are ordinary weapon classes with custom names
/// and numbers, so they save and load through the existing weapon save data unchanged.
/// </summary>
public static class ItemForge
{
    /// <summary>Creates a fresh item from its id.</summary>
    /// <exception cref="ArgumentException">Thrown for an unknown id.</exception>
    public static IEquipable Create(string id) => id switch
    {
        "basic_sword" => new BasicSword(),
        "broad_sword" => new BroadSword(),
        "great_sword" => new GreatSword(),
        "basic_axe" => new BasicAxe(),
        "battle_axe" => new BattleAxe(),
        "great_axe" => new GreatAxe(),
        "basic_dagger" => new BasicDagger(),
        "basic_spear" => new BasicSpear(),
        "simple_bow" => new SimpleBow(),
        "hunting_bow" => new HuntingBow(),
        "war_bow" => new WarBow(),
        "basic_staff" => new BasicStaff(),
        "small_quiver" => new SmallQuiver(),
        "medium_quiver" => new MediumQuiver(),
        "big_quiver" => new BigQuiver(),

        // Loot
        "rusty_sword" => Custom(new BasicSword(), "Rusty Sword", 11, Rarity.COMMON),
        "notched_dagger" => Custom(new BasicDagger(), "Notched Dagger", 10, Rarity.COMMON),
        "enforcer_blade" => Custom(new BasicSword(), "Enforcer's Blade", 17, Rarity.UNCOMMON),
        "enforcer_axe" => Custom(new BasicAxe(), "Enforcer's Axe", 22, Rarity.UNCOMMON),
        "bandit_shortbow" => Custom(new SimpleBow(), "Bandit Shortbow", 13, Rarity.UNCOMMON),
        "sentinel_blade" => Custom(new BasicSword(), "Sentinel Blade", 20, Rarity.RARE),
        "sentinel_pike" => Custom(new BasicSpear(), "Sentinel Pike", 21, Rarity.RARE),
        "garrison_longsword" => Custom(new BasicSword(), "Garrison Longsword", 22, Rarity.RARE),

        // Shop staffs
        "emberglass_staff" => Custom(new BasicStaff(), "Emberglass Staff", 6, Rarity.UNCOMMON, Element.FIRE),
        "rimewood_staff" => Custom(new BasicStaff(), "Rimewood Staff", 6, Rarity.UNCOMMON, Element.ICE),
        "cinderheart_staff" => Custom(new BasicStaff(), "Cinderheart Staff", 10, Rarity.RARE, Element.FIRE),

        // Quest rewards
        "masterwork_sword" => Custom(new BasicSword(), "Ashwell Masterwork Blade", 19, Rarity.RARE),
        "masterwork_bow" => Custom(new HuntingBow(), "Ashwell Masterwork Bow", 18, Rarity.RARE),
        "masterwork_staff" => Custom(new BasicStaff(), "Ashwell Iron-shod Staff", 9, Rarity.RARE),
        "last_lecture_staff" => Custom(new BasicStaff(), "Staff of the Last Lecture", 9, Rarity.EPIC, Element.FIRE),
        "ashcarved_bow" => Custom(new HuntingBow(), "Ash-carved Bow", 21, Rarity.RARE),
        "cinder_brand" => Custom(new BasicSword(), "Brand of the Cinder Hand", 20, Rarity.EPIC, Element.FIRE),
        "thronewing_bow" => Custom(new HuntingBow(), "Throne-wing Bow", 19, Rarity.EPIC),
        "hollow_scepter" => Custom(new BasicStaff(), "Hollow Scepter", 11, Rarity.EPIC, Element.LIGHTNING),

        _ => throw new ArgumentException($"Unknown item id: {id}")
    };

    private static Weapon Custom(Weapon weapon, string name, int damage, Rarity rarity, Element element = Element.NONE)
    {
        weapon.Name = name;
        weapon.DamageAmount = damage;
        weapon.Rarity = rarity;
        weapon.Element = element;
        return weapon;
    }

    /// <summary>Gold value of an item; shops sell at this price and buy at a fraction of it.</summary>
    public static int ValueOf(IEquipable item) => item switch
    {
        Staff staff => staff.GetEffectiveDamage() * 18 + (int)staff.Rarity * 30 + (staff.Element != Element.NONE ? 20 : 0),
        Weapon weapon => weapon.GetEffectiveDamage() * 6 + (int)weapon.Rarity * 30 + (weapon.Element != Element.NONE ? 40 : 0),
        Quiver quiver => quiver.Capacity * 4,
        _ => 10
    };

    /// <summary>One-line description used in shops and the inventory.</summary>
    public static string Describe(IEquipable item)
    {
        string size = item.InventorySpaceAmount == InventorySpaceAmount.LARGE ? "2 slots" : "1 slot";
        return item switch
        {
            Staff staff => $"{staff.Name} [{staff.Rarity}] spell power +{staff.GetEffectiveDamage()}{ElementTag(staff.Element, true)}, {size}",
            Weapon weapon => $"{weapon.Name} [{weapon.Rarity}] {weapon.Type.ToString().ToLowerInvariant()}, damage {weapon.GetEffectiveDamage()}{ElementTag(weapon.Element, false)}, {size}",
            Quiver quiver => $"{quiver.Name} [{quiver.Rarity}] {quiver.Capacity} arrows per fight, {size}",
            _ => item.Name
        };
    }

    private static string ElementTag(Element element, bool staff)
    {
        if (element == Element.NONE) return "";
        string name = element == Element.ICE && !staff ? "coldfire" : element.ToString().ToLowerInvariant();
        return staff ? $", +25% {name} spells" : $", {name}";
    }
}
