using System.Text.Json;
using Soulbound.Models;

namespace Soulbound.Services
{
    /// <summary>
    /// Loads and saves application state to a JSON file under AppDataDirectory.
    /// All game data reads/writes go through this service (local persistence layer).
    /// </summary>
    public sealed class DatabaseService
    {
        private static DatabaseService? instance;

        private readonly string filePath;

        private AppPersistedData data = new();

        public static DatabaseService GetInstance()
        {
            if (instance == null)
            {
                instance = new DatabaseService();
            }

            return instance;
        }

        private DatabaseService()
        {
            filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "soulbound_data.json");
        }

        /// <summary>
        /// Reads JSON from disk or creates empty state and optional seed goals.
        /// </summary>
        public void Initialize()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    var readOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    AppPersistedData? loaded = JsonSerializer.Deserialize<AppPersistedData>(json, readOptions);
                    if (loaded != null)
                    {
                        data = loaded;
                    }
                    else
                    {
                        data = new AppPersistedData();
                    }
                }
                catch
                {
                    data = new AppPersistedData();
                }
            }
            else
            {
                data = new AppPersistedData();
            }

            if (data.Goals == null)
            {
                data.Goals = new List<Goal>();
            }

            if (data.History == null)
            {
                data.History = new List<HistoryRecord>();
            }

            if (data.Character == null)
            {
                data.Character = new PetProgress();
            }

            if (data.Goals.Count == 0)
            {
                SeedInitialGoals();
            }
        }

        public AppPersistedData GetPersistedData()
        {
            return data;
        }

        /// <summary>
        /// Writes the current in-memory state to disk.
        /// </summary>
        public void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = null
            };

            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Adds sample goals when the database file is new or empty.
        /// </summary>
        private void SeedInitialGoals()
        {
            data.Goals.Add(new Goal
            {
                Id = "1",
                Title = "Morning Run",
                Description = "Run for 20 minutes",
                Notes = "Stay hydrated",
                IsPhysical = true,
                EndDate = DateTime.Today.AddDays(7),
                Deadline = DateTime.Today.AddDays(7),
                IsMonday = true,
                IsWednesday = true,
                IsFriday = true,
                ProgressPoints = 70
            });

            data.Goals.Add(new Goal
            {
                Id = "2",
                Title = "Read a chapter",
                Description = "Read 20 pages of a book",
                Notes = "Write one takeaway",
                IsIntellectual = true,
                EndDate = DateTime.Today.AddDays(7),
                Deadline = DateTime.Today.AddDays(7),
                IsTuesday = true,
                IsThursday = true,
                IsSaturday = true,
                ProgressPoints = 70
            });

            data.LastGoalId = data.Goals.Count;
            Save();
        }
    }
}
