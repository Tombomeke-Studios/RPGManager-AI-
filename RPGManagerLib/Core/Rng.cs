namespace RPGManagerLib.Core;

/// <summary>
/// Single shared random source for the whole game.
/// Set the RPGMANAGER_SEED environment variable to get reproducible runs (handy for playtesting).
/// </summary>
public static class Rng
{
    private static Random random = Create();

    private static Random Create()
    {
        string? seed = Environment.GetEnvironmentVariable("RPGMANAGER_SEED");
        return int.TryParse(seed, out int value) ? new Random(value) : new Random();
    }

    /// <summary>Replaces the random source with a seeded one.</summary>
    public static void Reseed(int seed) => random = new Random(seed);

    /// <summary>Returns an integer between <paramref name="min"/> and <paramref name="max"/>, both inclusive.</summary>
    public static int Next(int min, int max) => random.Next(min, max + 1);

    /// <summary>Returns a value in [0, 1).</summary>
    public static double NextDouble() => random.NextDouble();

    /// <summary>Returns true with the given probability (0..1).</summary>
    public static bool Chance(double probability) => random.NextDouble() < probability;

    /// <summary>Picks one random element from a list.</summary>
    public static T Pick<T>(IReadOnlyList<T> items) => items[random.Next(items.Count)];
}
