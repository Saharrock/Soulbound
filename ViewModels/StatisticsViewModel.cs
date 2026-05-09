using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class StatisticsViewModel : ViewModelBase
    {
        /// <summary>
        /// Only used to scale the colourful lifetime bars downward so they seldom look "finished".
        /// </summary>
        public const int LifetimeStaminaVisualizationCap = 75000;

        private readonly AppService appService;
        private const double MaxChartBarWidth = 220;

        private string volumeTierCaption = string.Empty;
        private string volumeCapacityCaption = string.Empty;
        private double physicalGrowthProgress;
        private double intellectualGrowthProgress;
        private double mentalGrowthProgress;
        private string physicalGrowthMeter = string.Empty;
        private string intellectualGrowthMeter = string.Empty;
        private string mentalGrowthMeter = string.Empty;

        public string VolumeTierCaption
        {
            get => volumeTierCaption;
            set { volumeTierCaption = value; OnPropertyChanged(); }
        }

        public string VolumeCapacityCaption
        {
            get => volumeCapacityCaption;
            set { volumeCapacityCaption = value; OnPropertyChanged(); }
        }

        public double PhysicalGrowthProgress
        {
            get => physicalGrowthProgress;
            set { physicalGrowthProgress = value; OnPropertyChanged(); }
        }

        public double IntellectualGrowthProgress
        {
            get => intellectualGrowthProgress;
            set { intellectualGrowthProgress = value; OnPropertyChanged(); }
        }

        public double MentalGrowthProgress
        {
            get => mentalGrowthProgress;
            set { mentalGrowthProgress = value; OnPropertyChanged(); }
        }

        public string PhysicalGrowthMeter
        {
            get => physicalGrowthMeter;
            set { physicalGrowthMeter = value; OnPropertyChanged(); }
        }

        public string IntellectualGrowthMeter
        {
            get => intellectualGrowthMeter;
            set { intellectualGrowthMeter = value; OnPropertyChanged(); }
        }

        public string MentalGrowthMeter
        {
            get => mentalGrowthMeter;
            set { mentalGrowthMeter = value; OnPropertyChanged(); }
        }

        public ObservableCollection<HistoryRecord> HistoryRecords { get; } = new();
        public ObservableCollection<CategorySummaryItem> CategorySummaries { get; } = new();
        public ObservableCollection<ActiveGoalStatItem> ActiveGoalStats { get; } = new();

        public StatisticsViewModel()
        {
            appService = AppService.GetInstance();
            ExplainHistoryStripCommand = new Command<HistoryRecord>(ExecuteExplainHistoryStrip);
            _ = RefreshAsync();
        }

        /// <summary>
        /// Wired to TapGestureRecognizer on the coloured strip beside each timeline row (simple DisplayAlert pattern for Android-safe UX).
        /// </summary>
        public Command<HistoryRecord> ExplainHistoryStripCommand { get; }

        private async void ExecuteExplainHistoryStrip(HistoryRecord? record)
        {
            if (record == null)
            {
                return;
            }

            Page? presenter = Shell.Current?.CurrentPage ?? Application.Current?.MainPage;
            if (presenter == null)
            {
                return;
            }

            await presenter.DisplayAlert("Timeline detail", record.BuildTeacherFriendlyStatusStory(), "OK");
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

            ApplyVolumePresentation(appService.GetProgress());
        }

        private void ApplyVolumePresentation(PetProgress progress)
        {
            int gate = Math.Max(1, progress.PointsPerStatForCurrentLevel);
            VolumeTierCaption = $"Growth level {progress.Level} — {progress.Rank}";
            VolumeCapacityCaption =
                $"Credit appears only after a goal fully closes (not during workouts alone). Credit equals stamina spent across that goal—including every workout stamp and your final Done—and each flagged pillar earns an equal slice. Every pillar needs {gate} simultaneously to bump you from growth level {progress.Level} toward {progress.Level + 1}. Overflow credit stays instead of wiping to zero.";
            PhysicalGrowthMeter = $"{progress.PhysicalPoints}/{gate}";
            IntellectualGrowthMeter = $"{progress.IntellectualPoints}/{gate}";
            MentalGrowthMeter = $"{progress.MentalPoints}/{gate}";
            PhysicalGrowthProgress = Math.Min(1.0, Math.Max(0.0, progress.PhysicalPoints / (double)gate));
            IntellectualGrowthProgress = Math.Min(1.0, Math.Max(0.0, progress.IntellectualPoints / (double)gate));
            MentalGrowthProgress = Math.Min(1.0, Math.Max(0.0, progress.MentalPoints / (double)gate));
        }

        private static ActiveGoalStatItem BuildActiveGoalStat(Goal goal)
        {
            return new ActiveGoalStatItem
            {
                Title = goal.Title,
                DeadlineText = goal.Deadline.ToString("dd MMM yyyy"),
                CategoriesText = FormatCategories(goal),
                TimelineProgress = ComputeTimelineProgress(goal)
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

            Dictionary<string, int> workoutMarksByCategory = new()
            {
                { "Physical", 0 },
                { "Intellectual", 0 },
                { "Mental", 0 }
            };

            foreach (HistoryRecord record in records)
            {
                if (!staminaByCategory.ContainsKey(record.Category))
                {
                    continue;
                }

                staminaByCategory[record.Category] += Math.Max(0, record.StaminaSpent);

                if (record.ResultStatus == HistoryRecord.StatusWorkout)
                {
                    workoutMarksByCategory[record.Category]++;
                    continue;
                }

                bool isSuccessfulGoalClose =
                    record.ResultStatus == HistoryRecord.StatusCompleted ||
                    record.ResultStatus == HistoryRecord.StatusCompletedLate;

                if (isSuccessfulGoalClose)
                {
                    completedCountByCategory[record.Category]++;
                }
            }

            List<CategorySummaryItem> result = new();
            foreach (string key in staminaByCategory.Keys)
            {
                int stamina = staminaByCategory[key];
                int finishedGoalsCount = completedCountByCategory[key];
                int workoutsLoggedCount = workoutMarksByCategory[key];
                double width = CalculateLifetimeBarWidthPixels(stamina);

                result.Add(new CategorySummaryItem
                {
                    CategoryName = key,
                    StaminaTotalSpent = stamina,
                    CompletedGoalsFinished = finishedGoalsCount,
                    WorkoutMarksLogged = workoutsLoggedCount,
                    BarColorHex = GetCategoryColor(key),
                    BarWidth = width
                });
            }

            return result;
        }

        private static double CalculateLifetimeBarWidthPixels(int staminaTotal)
        {
            if (staminaTotal < 1)
            {
                return 0;
            }

            double ratio = staminaTotal / (double)LifetimeStaminaVisualizationCap;
            if (ratio > 1)
            {
                ratio = 1;
            }

            double pixels = ratio * MaxChartBarWidth;
            if (pixels > 0 && pixels < 8)
            {
                pixels = 8;
            }

            return pixels;
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
        public int StaminaTotalSpent { get; set; }
        public int CompletedGoalsFinished { get; set; }
        public int WorkoutMarksLogged { get; set; }
        public string BarColorHex { get; set; } = "#888888";
        public double BarWidth { get; set; }

        public string LifetimeMetricsLine =>
            $"{CompletedGoalsFinished} goals closed · {WorkoutMarksLogged} workouts logged · {StaminaTotalSpent} stamina spent";

        public string LifetimeVisualizationCaption =>
            $"This bar only measures against a {StatisticsViewModel.LifetimeStaminaVisualizationCap:N0} stamina ruler so it stays roomy for years of play.";
    }

    internal class ActiveGoalStatItem
    {
        public string Title { get; set; } = string.Empty;
        public string DeadlineText { get; set; } = string.Empty;
        public string CategoriesText { get; set; } = string.Empty;
        public double TimelineProgress { get; set; }
        public string ProgressPercentText => $"{(int)Math.Round(TimelineProgress * 100)}% along timeline";
    }
}
