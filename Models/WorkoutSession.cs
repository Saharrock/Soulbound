namespace Soulbound.Models
{
    /// <summary>
    /// One logged workout ("Mark workout") for a goal on a calendar day.
    /// </summary>
    public sealed class WorkoutSession
    {
        public string GoalId { get; set; } = string.Empty;

        /// <summary>Local calendar date, yyyy-MM-dd.</summary>
        public string SessionDateIso { get; set; } = string.Empty;
    }
}
