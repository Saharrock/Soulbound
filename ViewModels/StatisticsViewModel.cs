using System.Collections.ObjectModel;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class StatisticsViewModel : ViewModelBase
    {
        private readonly AppService appService;
        private const double MaxChartBarWidth = 220;

        public ObservableCollection<HistoryRecord> HistoryRecords { get; } = new();
        public ObservableCollection<CategorySummaryItem> CategorySummaries { get; } = new();
        public ObservableCollection<ActiveGoalStatItem> ActiveGoalStats { get; } = new();

        public StatisticsViewModel()
        {
            appService = AppService.GetInstance();
            _ = RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            await appService.EnsureGameDataLoadedAsync();
            await appService.EnsureDailyStaminaAsync();
            HistoryRecords.Clear();
            CategorySummaries.Clear();
            ActiveGoalStats.Clear();

            List<HistoryRecord> records = appService.GetHistoryRecords();
            foreach (HistoryRecord item in records)
            {
                HistoryRecords.Add(item);
            }

            List<CategorySummaryItem> summaries = BuildCategorySummaries(records);
            foreach (CategorySummaryItem summary in summaries)
            {
                CategorySummaries.Add(summary);
            }

            foreach (Goal goal in appService.GetActiveGoals())
            {
                ActiveGoalStats.Add(BuildActiveGoalStat(goal));
            }
        }

        private static ActiveGoalStatItem BuildActiveGoalStat(Goal goal)
        {
            return new ActiveGoalStatItem
            {
                Title = goal.Title,
                DeadlineText = goal.Deadline.ToString("dd MMM yyyy"),
                CategoriesText = FormatCategories(goal),
                TimelineProgress = ComputeTimelineProgress(goal),
                TimelineHint = "Progress along your deadline window. Mark done in Main room or Goal history when you finish."
            };
        }

        private static string FormatCategories(Goal goal)
        {
            List<string> parts = new();
            if (goal.IsPhysical) parts.Add("Physical");
            if (goal.IsIntellectual) parts.Add("Intellectual");
            if (goal.IsMental) parts.Add("Mental");

            return parts.Count == 0 ? "Mixed" : string.Join(", ", parts);
        }

        private static double ComputeTimelineProgress(Goal goal)
        {
            DateTime start = goal.CreatedAt.Date;
            DateTime end = goal.Deadline.Date;
            DateTime today = DateTime.Today;
            if (today >= end)
            {
                return 1.0;
            }

            if (today <= start)
            {
                return 0.0;
            }

            double span = (end - start).TotalDays;
            if (span <= 0)
            {
                return 1.0;
            }

            return Math.Min(1.0, Math.Max(0.0, (today - start).TotalDays / span));
        }

        private static List<CategorySummaryItem> BuildCategorySummaries(List<HistoryRecord> records)
        {
            Dictionary<string, int> staminaByCategory = new()
            {
                { "Physical", 0 },
                { "Intellectual", 0 },
                { "Mental", 0 }
            };

            Dictionary<string, int> completedCountByCategory = new()
            {
                { "Physical", 0 },
                { "Intellectual", 0 },
                { "Mental", 0 }
            };

            foreach (HistoryRecord record in records)
            {
                if (record.ResultStatus != "Completed" || !staminaByCategory.ContainsKey(record.Category))
                {
                    continue;
                }

                staminaByCategory[record.Category] += Math.Max(0, record.StaminaSpent);
                completedCountByCategory[record.Category]++;
            }

            int maxStamina = staminaByCategory.Values.DefaultIfEmpty(0).Max();
            List<CategorySummaryItem> result = new();
            foreach (string key in staminaByCategory.Keys)
            {
                int stamina = staminaByCategory[key];
                int count = completedCountByCategory[key];
                double width = 0;
                if (maxStamina > 0 && stamina > 0)
                {
                    width = Math.Max(10, (stamina / (double)maxStamina) * MaxChartBarWidth);
                }

                result.Add(new CategorySummaryItem
                {
                    CategoryName = key,
                    StaminaTotal = stamina,
                    CompletedCount = count,
                    BarColorHex = GetCategoryColor(key),
                    BarWidth = width
                });
            }

            return result;
        }

        private static string GetCategoryColor(string category)
        {
            return category switch
            {
                "Physical" => "#e03c31",
                "Intellectual" => "#3c6fe0",
                "Mental" => "#9b4de0",
                _ => "#888888"
            };
        }
    }

    internal class CategorySummaryItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public int StaminaTotal { get; set; }
        public int CompletedCount { get; set; }
        public string BarColorHex { get; set; } = "#888888";
        public double BarWidth { get; set; }
        public string MetricsLine => $"{CompletedCount} completed · {StaminaTotal} stamina (all time)";
    }

    internal class ActiveGoalStatItem
    {
        public string Title { get; set; } = string.Empty;
        public string DeadlineText { get; set; } = string.Empty;
        public string CategoriesText { get; set; } = string.Empty;
        public double TimelineProgress { get; set; }
        public string TimelineHint { get; set; } = string.Empty;
        public string ProgressPercentText => $"{(int)Math.Round(TimelineProgress * 100)}% along timeline";
    }
}
