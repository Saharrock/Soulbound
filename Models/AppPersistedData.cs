namespace Soulbound.Models
{
    /// <summary>
    /// Root object stored on disk by DatabaseService (JSON file).
    /// </summary>
    public class AppPersistedData
    {
        public List<Goal> Goals { get; set; } = new();

        public List<HistoryRecord> History { get; set; } = new();

        public PetProgress Character { get; set; } = new();

        public int LastGoalId { get; set; }

        public int SelectedPetIndex { get; set; }
    }
}
