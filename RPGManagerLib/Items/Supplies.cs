namespace RPGManagerLib.Items;

/// <summary>
/// What a supply is for.
/// </summary>
public enum SupplyKind { Consumable, Material }

/// <summary>
/// A stackable, non-equipable item: potions, bombs and crafting materials.
/// </summary>
/// <param name="Id">Stable id used in saves and loot tables.</param>
/// <param name="Name">Display name.</param>
/// <param name="Description">What it does.</param>
/// <param name="Price">Base buy price in gold.</param>
/// <param name="Kind">Consumable or material.</param>
/// <param name="InCombat">Usable during a fight.</param>
/// <param name="OutOfCombat">Usable from the inventory screen.</param>
public record Supply(string Id, string Name, string Description, int Price, SupplyKind Kind, bool InCombat, bool OutOfCombat);

/// <summary>
/// Every supply in the game, looked up by id.
/// </summary>
public static class SupplyCatalog
{
    public const string HealingPotion = "healing_potion";
    public const string GreaterHealingPotion = "greater_healing_potion";
    public const string ManaDraught = "mana_draught";
    public const string EmberSalve = "ember_salve";
    public const string Firebomb = "firebomb";
    public const string SmokeBomb = "smoke_bomb";
    public const string Scraps = "scraps";
    public const string FireEssence = "fire_essence";
    public const string AcademyPage = "academy_page";

    private static readonly Dictionary<string, Supply> All = new List<Supply>
    {
        new(HealingPotion, "Healing Potion", "Restores 50 health.", 20, SupplyKind.Consumable, true, true),
        new(GreaterHealingPotion, "Greater Healing Potion", "Restores 120 health.", 55, SupplyKind.Consumable, true, true),
        new(ManaDraught, "Mana Draught", "Restores 60 mana. Only a Mage gets anything from it.", 25, SupplyKind.Consumable, true, true),
        new(EmberSalve, "Ember Salve", "Cures Burn and Poison, restores 15 health.", 12, SupplyKind.Consumable, true, false),
        new(Firebomb, "Firebomb", "Throws fire over every enemy (about 35 damage each).", 30, SupplyKind.Consumable, true, false),
        new(SmokeBomb, "Smoke Bomb", "Guaranteed escape from any fight you are allowed to flee.", 15, SupplyKind.Consumable, true, false),
        new(Scraps, "Scraps", "Hide, bone and salvaged metal. Korin reforges weapons with it.", 3, SupplyKind.Material, false, false),
        new(FireEssence, "Fire Essence", "A warm, restless ember. Lira can turn it inside out into coldfire.", 14, SupplyKind.Material, false, false),
        new(AcademyPage, "Academy Page", "A scorched page of Emberveil research. Lira would want this.", 0, SupplyKind.Material, false, false),
    }.ToDictionary(s => s.Id);

    /// <summary>Looks up a supply by id.</summary>
    public static Supply Get(string id) => All[id];

    /// <summary>True when the id names a supply.</summary>
    public static bool Exists(string id) => All.ContainsKey(id);
}
