using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
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

        private string lifetimeSummaryCaption = string.Empty;
        private double activeGoalsDisciplineProgress;
        private string activeGoalsDisciplineMeter = string.Empty;
        private double finishedGoalsDisciplineProgress;
        private string finishedGoalsDisciplineMeter = string.Empty;

        public string LifetimeSummaryCaption
        {
            get => lifetimeSummaryCaption;
            set { lifetimeSummaryCaption = value; OnPropertyChanged(); }
        }

        public double ActiveGoalsDisciplineProgress
        {
            get => activeGoalsDisciplineProgress;
            set { activeGoalsDisciplineProgress = value; OnPropertyChanged(); }
        }

        public string ActiveGoalsDisciplineMeter
        {
            get => activeGoalsDisciplineMeter;
            set { activeGoalsDisciplineMeter = value; OnPropertyChanged(); }
        }

        public double FinishedGoalsDisciplineProgress
        {
            get => finishedGoalsDisciplineProgress;
            set { finishedGoalsDisciplineProgress = value; OnPropertyChanged(); }
        }

        public string FinishedGoalsDisciplineMeter
        {
            get => finishedGoalsDisciplineMeter;
            set { finishedGoalsDisciplineMeter = value; OnPropertyChanged(); }
        }

        public ObservableCollection<HistoryRecord> HistoryRecords { get; } = new();
        public ObservableCollection<CategorySummaryItem> CategorySummaries { get; } = new();
        public ObservableCollection<ActiveGoalStatItem> ActiveGoalStats { get; } = new();

        private const int MaxLifetimeWorkloadPopupChars = 4200;

        public StatisticsViewModel()
        {
            appService = AppService.GetInstance();
            ExplainHistoryStripCommand = new Command<HistoryRecord>(ExecuteExplainHistoryStrip);
            ExplainLifetimeWorkloadRowCommand = new Command<CategorySummaryItem>(ExecuteExplainLifetimeWorkloadRow);
        }

        /// <summary>
        /// Wired to TapGestureRecognizer on the coloured strip beside each timeline row (simple DisplayAlert pattern for Android-safe UX).
        /// </summary>
        public Command<HistoryRecord> ExplainHistoryStripCommand { get; }

        /// <summary>Tap a lifetime category row to list goals tagged with that pillar.</summary>
        public Command<CategorySummaryItem> ExplainLifetimeWorkloadRowCommand { get; }

        private static Page? TryGetPresenterPage()
        {
            return Shell.Current?.CurrentPage ?? Application.Current?.MainPage;
        }

        private async void ExecuteExplainLifetimeWorkloadRow(CategorySummaryItem? row)
        {
            if (row == null || string.IsNullOrWhiteSpace(row.CategoryName))
            {
                return;
            }

            Page? presenter = TryGetPresenterPage();
            if (presenter == null)
            {
                return;
            }

            await appService.EnsureGameDataLoadedAsync();
            List<Goal> goals = appService.GetGoalsForLifetimeCategory(row.CategoryName);
            string body = BuildLifetimeWorkloadGoalsDetail(row, goals);
            string title = $"{row.CategoryName} · your goals ({goals.Count})";
            await presenter.DisplayAlert(title, body, "OK");
        }

        private static string BuildLifetimeWorkloadGoalsDetail(CategorySummaryItem summary, List<Goal> goals)
        {
            var sb = new StringBuilder();
            sb.AppendLine(summary.LifetimeMetricsLine);
            sb.AppendLine("(History totals for this pillar; below is goal-by-goal snapshot.)");
            sb.AppendLine();

            if (goals.Count == 0)
            {
                sb.Append("No goals in this category yet.");
                return sb.ToString();
            }

            for (int i = 0; i < goals.Count; i++)
            {
                string block = BuildSingleGoalLifetimeDetail(goals[i]);
                int projected = sb.Length + (i > 0 ? Environment.NewLine.Length * 3 + 8 : 0) + block.Length;
                if (projected > MaxLifetimeWorkloadPopupChars && i > 0)
                {
                    int remaining = goals.Count - i;
                    sb.AppendLine();
                    sb.AppendLine(new string('-', 16));
                    sb.Append("(Scroll not available in this dialog.) ");
                    sb.Append(remaining == 1
                        ? "One more goal is omitted — open Goal history for everything."
                        : $"{remaining} more goals omitted — open Goal history for everything.");
                    break;
                }

                if (i > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine(new string('-', 16));
                }

                sb.Append(block);
            }

            string result = sb.ToString().TrimEnd();
            if (result.Length > MaxLifetimeWorkloadPopupChars)
            {
                ReadOnlySpan<char> span = result.AsSpan(0, MaxLifetimeWorkloadPopupChars - 80);
                return $"{span.TrimEnd()}...{Environment.NewLine}(Truncated - open Goal history for the rest.)";
            }

            return result;
        }

        private static string BuildSingleGoalLifetimeDetail(Goal g)
        {
            var sb = new StringBuilder();
            sb.AppendLine(g.Title);
            sb.Append("Status · ").AppendLine(FormatGoalLifetimeStatusHeading(g));

            sb.Append("Categories · ").AppendLine(FormatGoalPillarLabels(g, "None set"));
            sb.Append("Created ").Append(g.CreatedAt.ToString("dd MMM yyyy", CultureInfo.CurrentCulture));
            sb.Append(" · deadline ").AppendLine(g.Deadline.ToString("dd MMM yyyy", CultureInfo.CurrentCulture));

            sb.Append(g.WorkoutStatsText).Append(" · ").Append(g.MissedStatsText);
            sb.Append(" · ").AppendLine(g.ScheduleAdherenceLine);
            sb.Append("Stamina / workout · ").Append(g.ResolvedStaminaCost.ToString(CultureInfo.CurrentCulture));
            sb.Append(" · total spent · ").AppendLine(g.TotalStaminaSpentAcrossGoal.ToString(CultureInfo.CurrentCulture));

            if (!g.IsCompleted)
            {
                sb.AppendLine($"Schedule · {g.ScheduleText}");
            }

            sb.AppendLine();
            sb.Append(string.IsNullOrWhiteSpace(g.Description) ? "(No description)" : g.Description.Trim());
            return sb.ToString();
        }

        private static string FormatGoalLifetimeStatusHeading(Goal g)
        {
            if (g.IsCompleted)
            {
                return g.IsCompletedLate ? "Done (after deadline)" : "Done on time";
            }

            return "Active";
        }

        private static string FormatGoalPillarLabels(Goal g, string whenNone)
        {
            List<string> parts = new();
            if (g.IsPhysical)
            {
                parts.Add("Physical");
            }

            if (g.IsIntellectual)
            {
                parts.Add("Intellectual");
            }

            if (g.IsMental)
            {
                parts.Add("Mental");
            }

            return parts.Count == 0 ? whenNone : string.Join(", ", parts);
        }

        private async void ExecuteExplainHistoryStrip(HistoryRecord? record)
        {
            if (record == null)
            {
                return;
            }

            Page? presenter = TryGetPresenterPage();
            if (presenter == null)
            {
                return;
            }

            await presenter.DisplayAlert("Timeline entry", record.BuildTimelineTapDetail(), "OK");
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

            List<Goal> activeGoals = appService.GetActiveGoals();
            List<Goal> finishedGoals = appService.GetFinishedGoals();

            foreach (Goal goal in activeGoals)
            {
                ActiveGoalStats.Add(BuildActiveGoalStat(goal));
            }

            ApplyDisciplinePresentation(appService.GetProgress(), activeGoals, finishedGoals);
        }

        private void ApplyDisciplinePresentation(PetProgress progress, List<Goal> activeGoals, List<Goal> finishedGoals)
        {
            LifetimeSummaryCaption = $"Goals finished (lifetime): {progress.CompletedGoalsLifetime}";

            if (activeGoals.Count == 0)
            {
                ActiveGoalsDisciplineProgress = 0;
                ActiveGoalsDisciplineMeter = "No active goals";
            }
            else
            {
                int avgActive = appService.GetAverageScheduleAdherencePercent(activeGoals);
                ActiveGoalsDisciplineProgress = avgActive / 100.0;
                ActiveGoalsDisciplineMeter =
                    $"Avg schedule adherence {avgActive}% · {activeGoals.Count} active (same % as on goal cards)";
            }

            if (finishedGoals.Count == 0)
            {
                FinishedGoalsDisciplineProgress = 0;
                FinishedGoalsDisciplineMeter = "No completed goals yet";
            }
            else
            {
                int avgDone = appService.GetAverageScheduleAdherencePercent(finishedGoals);
                FinishedGoalsDisciplineProgress = avgDone / 100.0;
                FinishedGoalsDisciplineMeter =
                    $"Avg schedule adherence {avgDone}% · {finishedGoals.Count} completed";
            }
        }

        private static ActiveGoalStatItem BuildActiveGoalStat(Goal goal)
        {
            return new ActiveGoalStatItem
            {
                Title = goal.Title,
                DeadlineText = goal.Deadline.ToString("dd MMM yyyy"),
                CategoriesText = FormatGoalPillarLabels(goal, "Mixed"),
                ScheduleAdherenceLine = goal.ScheduleAdherenceLine,
                TimelineProgress = ComputeTimelineProgress(goal)
            };
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
            string[] categories = { "Physical", "Intellectual", "Mental" };

            Dictionary<string, int> staminaByCategory = categories.ToDictionary(c => c, _ => 0);
            Dictionary<string, int> completedCountByCategory = categories.ToDictionary(c => c, _ => 0);
            Dictionary<string, int> workoutMarksByCategory = categories.ToDictionary(c => c, _ => 0);

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
            foreach (string key in categories)
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
    }

    internal class ActiveGoalStatItem
    {
        public string Title { get; set; } = string.Empty;
        public string DeadlineText { get; set; } = string.Empty;
        public string CategoriesText { get; set; } = string.Empty;
        public string ScheduleAdherenceLine { get; set; } = string.Empty;
        public double TimelineProgress { get; set; }
        public string ProgressPercentText => $"{(int)Math.Round(TimelineProgress * 100)}% along timeline";
    }
}
