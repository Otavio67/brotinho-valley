using System;
using System.IO;
using UnityEngine;
namespace Brotinho
{
    public sealed class SaveSystem
    {
        public const int CurrentVersion = 1;
        readonly string path;
        readonly Func<GameState, bool> validator;
        bool recovered;
        public bool WriteBlocked { get; private set; }
        public string LastError { get; private set; }
        public SaveSystem(string directory, Func<GameState, bool> validator = null) { path = Path.Combine(directory, "brotinho-save.json"); this.validator = validator; }
        public bool Exists => File.Exists(path) || File.Exists(path + ".bak");
        public GameState Load()
        {
            WriteBlocked = false; LastError = null; recovered = false;
            if (!Exists) return GameState.New();
            // A future-version primary is never silently replaced by an older backup.
            try { return Read(path); }
            catch (FutureSaveException) { WriteBlocked = true; LastError = "save_newer_version"; return GameState.New(); }
            catch (Exception e) when (e is IOException || e is ArgumentException || e is InvalidDataException || e is UnauthorizedAccessException) { LastError = "save_recovered"; }
            try { var state = Read(path + ".bak"); recovered = true; return state; }
            catch (FutureSaveException) { WriteBlocked = true; LastError = "save_newer_version"; return GameState.New(); }
            catch (Exception) { WriteBlocked = true; LastError = "save_corrupt"; return GameState.New(); }
        }
        GameState Read(string file)
        {
            string json = File.ReadAllText(file);
            // JsonUtility otherwise silently supplies defaults for arbitrary objects.
            if (!json.Contains("\"version\"") || !json.Contains("\"inventory\"") || !json.Contains("\"savedUtc\"")) throw new InvalidDataException("Invalid save envelope.");
            var state = JsonUtility.FromJson<GameState>(json);
            if (state == null) throw new InvalidDataException("Empty save.");
            if (state.version > CurrentVersion) throw new FutureSaveException();
            // Version 0 migrations deliberately keep the initial game schema.
            if (state.version < 0 || state.inventory == null || state.crops == null || state.buildings == null || state.animals == null || state.workers == null || state.orders == null || state.land == null || state.market == null || state.claimedAchievements == null || state.dailyClaimed == null || state.coins < 0 || state.gems < 0 || state.xp < 0 || double.IsNaN(state.elapsed) || double.IsInfinity(state.elapsed) || state.elapsed < 0 || state.tickRemainder < 0 || state.tickRemainder >= 1) throw new InvalidDataException("Invalid game state.");
            if (!SaveStateValidator.Basic(state) || validator != null && !validator(state)) throw new InvalidDataException("Invalid saved entities.");
            state.version = CurrentVersion; return state;
        }
        public bool Save(GameState state, long utcNow)
        {
            if (WriteBlocked) return false;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)); state.savedUtc = utcNow; state.version = CurrentVersion;
                string temp = path + ".tmp"; File.WriteAllText(temp, JsonUtility.ToJson(state, true));
                // Validate before rotating, keeping the previous primary as a recovery point.
                Read(temp);
                if (File.Exists(path)) File.Replace(temp, path, recovered ? null : path + ".bak"); else File.Move(temp, path);
                recovered = false;
                LastError = null; return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is InvalidDataException)
            { LastError = "save_failed"; return false; }
        }
        public void Reset()
        {
            // This method is called only after the in-game reset confirmation or isolated test setup.
            foreach (string suffix in new[] { "", ".bak", ".tmp" }) if (File.Exists(path + suffix)) File.Delete(path + suffix);
            WriteBlocked = false; LastError = null;
        }
        sealed class FutureSaveException : Exception { }
    }
}
