using System.Text.Json;
using System.Text.Json.Serialization;
using RPGManagerLib.Characters.Heroes;
using RPGManagerLib.Worlds;

namespace RPGManagerLib.Saves
{
    /// <summary>
    /// Reads and writes save files as JSON.
    /// </summary>
    /// <remarks>
    /// The full game (roster + shared world) is stored in <c>keeper.json</c>. The older
    /// <c>characters.json</c> list format is still supported and is migrated automatically on first load.
    /// The save folder defaults to <c>Save/</c> and can be overridden with RPGMANAGER_SAVE_DIR.
    /// </remarks>
    public static class SaveManager
    {
        private static readonly string SaveFolder =
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("RPGMANAGER_SAVE_DIR"))
                ? "Save"
                : Environment.GetEnvironmentVariable("RPGMANAGER_SAVE_DIR")!;
        private const string SaveFileName = "characters.json";
        private const string GameFileName = "keeper.json";
        private static readonly string SaveFile = Path.Combine(SaveFolder, SaveFileName);
        private static readonly string GameFile = Path.Combine(SaveFolder, GameFileName);

        private static JsonSerializerOptions Options(bool indented) => new()
        {
            WriteIndented = indented,
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Saves the whole game: every hero, which one is active, and the shared world state.
        /// </summary>
        /// <param name="heroes">The roster.</param>
        /// <param name="active">The active hero (may be null).</param>
        /// <param name="world">The shared world.</param>
        public static void SaveGame(IReadOnlyList<Character> heroes, Character? active, WorldState world)
        {
            Directory.CreateDirectory(SaveFolder);
            var save = new SaveGame
            {
                Characters = heroes.Select(c => new CharacterSaveData(c)).ToList(),
                ActiveIndex = active == null ? 0 : Math.Max(0, heroes.ToList().IndexOf(active)),
                World = new WorldStateSaveData(world)
            };
            File.WriteAllText(GameFile, JsonSerializer.Serialize(save, Options(true)));
        }

        /// <summary>
        /// Loads the whole game. Falls back to migrating an old character-only save.
        /// </summary>
        /// <returns>The save, or null when there is nothing to load or the file is unreadable.</returns>
        public static SaveGame? LoadGame()
        {
            try
            {
                if (File.Exists(GameFile))
                {
                    string json = File.ReadAllText(GameFile);
                    if (!string.IsNullOrWhiteSpace(json))
                        return JsonSerializer.Deserialize<SaveGame>(json, Options(false));
                }

                if (File.Exists(SaveFile))
                {
                    string json = File.ReadAllText(SaveFile);
                    if (string.IsNullOrWhiteSpace(json)) return null;
                    var characters = JsonSerializer.Deserialize<List<CharacterSaveData>>(json, Options(false));
                    if (characters != null && characters.Count > 0) return new SaveGame { Characters = characters };
                }
            }
            catch (Exception ex) when (ex is JsonException or IOException or NotSupportedException)
            {
                Console.WriteLine("Save file could not be read. Starting a new game.");
            }
            return null;
        }

        /// <summary>True when a save exists on disk.</summary>
        public static bool HasSave() => File.Exists(GameFile) || File.Exists(SaveFile);

        /// <summary>Deletes the full-game save (used when starting over).</summary>
        public static void DeleteGame()
        {
            if (File.Exists(GameFile)) File.Delete(GameFile);
            if (File.Exists(SaveFile)) File.Delete(SaveFile);
        }

        /// <summary>
        /// Loads a list of characters from the legacy character-only save file.
        /// </summary>
        /// <returns>The saved characters, or an empty list when the file is missing or invalid.</returns>
        public static List<Character> LoadCharacters()
        {
            try
            {
                if (!File.Exists(SaveFile))
                    return new List<Character>();

                string json = File.ReadAllText(SaveFile);

                if (string.IsNullOrWhiteSpace(json))
                    return new List<Character>();

                var saveDataList = JsonSerializer.Deserialize<List<CharacterSaveData>>(json, Options(false))
                    ?? new List<CharacterSaveData>();

                return saveDataList.Select(sd => sd.ToCharacter()).ToList();
            }
            catch (JsonException)
            {
                Console.WriteLine("Save file contains invalid JSON. Starting with empty character list.");
                return new List<Character>();
            }
            catch (IOException)
            {
                Console.WriteLine("Error reading save file.");
                return new List<Character>();
            }
            catch (Exception)
            {
                Console.WriteLine("Unexpected error while loading save file.");
                return new List<Character>();
            }
        }

        /// <summary>
        /// Saves a list of characters to the legacy character-only save file.
        /// </summary>
        /// <param name="characters">The characters to save.</param>
        public static void SaveCharacters(List<Character> characters)
        {
            Directory.CreateDirectory(SaveFolder);
            var saveData = characters.Select(c => new CharacterSaveData(c)).ToList();
            File.WriteAllText(SaveFile, JsonSerializer.Serialize(saveData, Options(true)));
            Console.WriteLine($"Saved to: {SaveFile}");
        }
    }
}
