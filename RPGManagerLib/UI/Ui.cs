namespace RPGManagerLib.UI;

/// <summary>
/// Thrown when console input runs out (piped input reached its end), so the game can save and exit cleanly.
/// </summary>
public class InputEndedException : Exception
{
    public InputEndedException() : base("Console input ended.") { }
}

/// <summary>
/// One entry in a keyed menu.
/// </summary>
/// <param name="Key">What the player types to choose it.</param>
/// <param name="Label">Main text.</param>
/// <param name="Hint">Optional dimmed explanation.</param>
/// <param name="Action">What runs when chosen.</param>
public record MenuOption(string Key, string Label, string Hint, Action Action);

/// <summary>
/// Console presentation helpers: colours, word wrapping, menus, bars and pacing.
/// Every screen in the game writes through this class so the look stays consistent.
/// </summary>
public static class Ui
{
    /// <summary>Maximum text width used for word wrapping.</summary>
    public const int Width = 78;

    /// <summary>When true nothing is written and no pauses happen (used by the balance simulator).</summary>
    public static bool Silent { get; set; }

    /// <summary>When true, short pauses are inserted between combat lines.</summary>
    public static bool Pacing { get; set; } = true;

    private static bool Interactive => !Console.IsInputRedirected;

    /// <summary>Clears the screen when attached to a real console.</summary>
    public static void Clear()
    {
        if (Silent) return;
        try
        {
            if (!Console.IsOutputRedirected) Console.Clear();
            else Console.WriteLine();
        }
        catch (IOException) { Console.WriteLine(); }
    }

    /// <summary>Writes text without a newline in an optional colour.</summary>
    public static void Write(string text, ConsoleColor? color = null)
    {
        if (Silent) return;
        if (color.HasValue) SetColor(color.Value);
        Console.Write(text);
        if (color.HasValue) ResetColor();
    }

    /// <summary>Writes one line in an optional colour.</summary>
    public static void Line(string text = "", ConsoleColor? color = null)
    {
        if (Silent) return;
        Write(text, color);
        Console.WriteLine();
    }

    /// <summary>Clears the screen and prints a big title banner.</summary>
    public static void Header(string title)
    {
        Clear();
        Line();
        Line($"  ══════ {title.ToUpperInvariant()} " + new string('═', Math.Max(4, Width - title.Length - 12)), ConsoleColor.DarkYellow);
        Line();
    }

    /// <summary>Prints a small section heading.</summary>
    public static void Sub(string title)
    {
        Line();
        Line($"  ── {title} " + new string('─', Math.Max(4, Width - title.Length - 8)), ConsoleColor.DarkCyan);
    }

    /// <summary>Narration: wrapped grey prose.</summary>
    public static void Narrate(string text)
    {
        foreach (string line in Wrap(text, Width - 2)) Line("  " + line, ConsoleColor.Gray);
    }

    /// <summary>Lore or vision text: wrapped, in a distinct colour.</summary>
    public static void Vision(string text)
    {
        foreach (string line in Wrap(text, Width - 6)) Line("    " + line, ConsoleColor.Magenta);
    }

    /// <summary>A line of dialogue with a coloured speaker name.</summary>
    public static void Say(string speaker, string text)
    {
        if (Silent) return;
        string prefix = $"  {speaker}: ";
        var lines = Wrap("\"" + text + "\"", Width - prefix.Length).ToList();
        for (int i = 0; i < lines.Count; i++)
        {
            if (i == 0) Write(prefix, ConsoleColor.Yellow);
            else Write(new string(' ', prefix.Length));
            Line(lines[i], ConsoleColor.White);
        }
    }

    /// <summary>Positive outcome (green).</summary>
    public static void Good(string text) => WrapColored(text, ConsoleColor.Green);

    /// <summary>Negative outcome (red).</summary>
    public static void Bad(string text) => WrapColored(text, ConsoleColor.Red);

    /// <summary>Rewards: gold, XP, items (yellow).</summary>
    public static void Gain(string text) => WrapColored(text, ConsoleColor.Yellow);

    /// <summary>Quiet system note (dark grey).</summary>
    public static void Note(string text) => WrapColored(text, ConsoleColor.DarkGray);

    /// <summary>The Keeper's shared-world callouts (cyan).</summary>
    public static void World(string text) => WrapColored("» " + text, ConsoleColor.Cyan);

    private static void WrapColored(string text, ConsoleColor color)
    {
        foreach (string line in Wrap(text, Width - 2)) Line("  " + line, color);
    }

    /// <summary>
    /// Shows numbered options and returns the chosen index, or -1 when the player picks the back option.
    /// </summary>
    /// <param name="options">The choices, shown as [1]..[n].</param>
    /// <param name="back">Label for the [0] option; null to force a choice.</param>
    public static int Choose(IReadOnlyList<string> options, string? back = null)
    {
        Line();
        for (int i = 0; i < options.Count; i++)
        {
            Write($"  [{i + 1}] ", ConsoleColor.DarkYellow);
            var lines = Wrap(options[i], Width - 6).ToList();
            for (int l = 0; l < lines.Count; l++) Line((l == 0 ? "" : "      ") + lines[l]);
        }
        if (back != null)
        {
            Write("  [0] ", ConsoleColor.DarkGray);
            Line(back, ConsoleColor.DarkGray);
        }

        while (true)
        {
            string input = Prompt();
            if (back != null && (input == "0" || input == "")) return -1;
            if (int.TryParse(input, out int n) && n >= 1 && n <= options.Count) return n - 1;
            Note("Type the number of your choice.");
        }
    }

    /// <summary>Shows a keyed menu, runs the chosen action, and returns.</summary>
    public static void Menu(IReadOnlyList<MenuOption> options)
    {
        Line();
        int width = options.Max(o => o.Label.Length) + 2;
        foreach (var o in options)
        {
            Write($"  [{o.Key}] ", ConsoleColor.DarkYellow);
            Write(o.Label.PadRight(width));
            Line(o.Hint.Length > 0 ? "— " + o.Hint : "", ConsoleColor.DarkGray);
        }

        while (true)
        {
            string input = Prompt().ToLowerInvariant();
            var chosen = options.FirstOrDefault(o => o.Key.ToLowerInvariant() == input);
            if (chosen != null)
            {
                chosen.Action();
                return;
            }
            Note("Type one of the keys shown in brackets.");
        }
    }

    /// <summary>Asks a yes/no question.</summary>
    public static bool Confirm(string question)
    {
        while (true)
        {
            Write($"  {question} ", ConsoleColor.White);
            Write("[y/n] ", ConsoleColor.DarkYellow);
            string input = ReadLine().Trim().ToLowerInvariant();
            if (input is "y" or "yes") return true;
            if (input is "n" or "no") return false;
        }
    }

    /// <summary>Shows the input prompt and returns the trimmed line.</summary>
    public static string Prompt(string label = "")
    {
        Write($"\n  {label}> ", ConsoleColor.DarkYellow);
        return ReadLine().Trim();
    }

    /// <summary>Reads one line of input; throws <see cref="InputEndedException"/> when input has ended.</summary>
    public static string ReadLine()
    {
        string? line = Console.ReadLine();
        if (line == null) throw new InputEndedException();
        return line;
    }

    /// <summary>Waits for ENTER.</summary>
    public static void Pause(string message = "Press ENTER to continue...")
    {
        if (Silent) return;
        Line();
        Write("  " + message, ConsoleColor.DarkGray);
        ReadLine();
    }

    /// <summary>A short dramatic pause between lines (only on a real console).</summary>
    public static void Beat(int milliseconds = 220)
    {
        if (Silent || !Pacing || !Interactive) return;
        Thread.Sleep(milliseconds);
    }

    /// <summary>Builds a text bar such as ██████░░░░.</summary>
    public static string Bar(double current, double max, int width = 12)
    {
        if (max <= 0) return new string('░', width);
        int filled = (int)Math.Round(Math.Clamp(current / max, 0, 1) * width);
        if (current > 0 && filled == 0) filled = 1;
        return new string('█', filled) + new string('░', width - filled);
    }

    /// <summary>Picks a colour for a health value.</summary>
    public static ConsoleColor HealthColor(double current, double max)
    {
        double ratio = max <= 0 ? 0 : current / max;
        return ratio > 0.6 ? ConsoleColor.Green : ratio > 0.3 ? ConsoleColor.Yellow : ConsoleColor.Red;
    }

    /// <summary>Word-wraps text to the given width.</summary>
    public static IEnumerable<string> Wrap(string text, int width)
    {
        foreach (string paragraph in text.Split('\n'))
        {
            string current = "";
            foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (current.Length == 0) current = word;
                else if (current.Length + 1 + word.Length <= width) current += " " + word;
                else
                {
                    yield return current;
                    current = word;
                }
            }
            yield return current;
        }
    }

    private static void SetColor(ConsoleColor color)
    {
        try { Console.ForegroundColor = color; } catch (IOException) { }
    }

    private static void ResetColor()
    {
        try { Console.ResetColor(); } catch (IOException) { }
    }
}
