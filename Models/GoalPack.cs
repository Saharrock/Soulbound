namespace Soulbound.Models
{
    public class GoalPack
    {
        public string Title { get; set; } = string.Empty;
        public List<Goal> Goals { get; set; } = new();
    }
}
