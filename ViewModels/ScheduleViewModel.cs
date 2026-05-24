using System.Collections.ObjectModel;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    // SchedulePage: активные цели, сгруппированные по дням недели.
    internal class ScheduleViewModel : ViewModelBase
    {
        private readonly AppService appService;

        public ObservableCollection<ScheduleDaySection> WeekSections { get; } = new();

        public ScheduleViewModel()
        {
            appService = AppService.GetInstance();
            _ = RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            await appService.EnsureGameDataLoadedAsync();
            WeekSections.Clear();

            List<Goal> activeGoals = appService.GetActiveGoals();
            List<ScheduleDaySection> sections = new()
            {
                BuildDaySection("Sunday", activeGoals, goal => goal.IsSunday),
                BuildDaySection("Monday", activeGoals, goal => goal.IsMonday),
                BuildDaySection("Tuesday", activeGoals, goal => goal.IsTuesday),
                BuildDaySection("Wednesday", activeGoals, goal => goal.IsWednesday),
                BuildDaySection("Thursday", activeGoals, goal => goal.IsThursday),
                BuildDaySection("Friday", activeGoals, goal => goal.IsFriday),
                BuildDaySection("Saturday", activeGoals, goal => goal.IsSaturday)
            };

            foreach (ScheduleDaySection section in sections)
            {
                WeekSections.Add(section);
            }
        }

        private static ScheduleDaySection BuildDaySection(string dayName, List<Goal> goals, Func<Goal, bool> filter)
        {
            ScheduleDaySection section = new() { DayName = dayName };
            foreach (Goal goal in goals.Where(filter))
            {
                section.Goals.Add(goal);
            }

            return section;
        }
    }

    // Один день недели и список целей на этот день.
    internal class ScheduleDaySection
    {
        public string DayName { get; set; } = string.Empty;
        public ObservableCollection<Goal> Goals { get; } = new();
    }
}
