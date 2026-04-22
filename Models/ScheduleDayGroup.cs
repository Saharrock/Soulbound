namespace Soulbound.Models
{
    public class ScheduleDayGroup
    {
        public string DayName { get; set; } = string.Empty;
        public List<Goal> Goals { get; set; } = new();
    }
}
