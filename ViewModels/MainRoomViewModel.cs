using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class MainRoomViewModel : ViewModelBase
    {
        private readonly AppService appService;

        private Goal? nearestDeadlineGoal;
        private bool deadlineTickerRunning;

        public ObservableCollection<Goal> TodayGoals { get; } = new();

        private string averageScheduleSummary = string.Empty;
        public string AverageScheduleSummary
        {
            get => averageScheduleSummary;
            set { averageScheduleSummary = value; OnPropertyChanged(); }
        }

        private string petName = "Your Pet";
        public string PetName
        {
            get => petName;
            set { petName = value; OnPropertyChanged(); }
        }

        private ImageSource petAvatar = ImageSource.FromFile("dotnet_bot.png");
        public ImageSource PetAvatar
        {
            get => petAvatar;
            set { petAvatar = value; OnPropertyChanged(); }
        }

        private int activeGoalsCount;
        public int ActiveGoalsCount
        {
            get => activeGoalsCount;
            set { activeGoalsCount = value; OnPropertyChanged(); }
        }

        private string nearestDeadlineGoalTitle = "No active goals";
        public string NearestDeadlineGoalTitle
        {
            get => nearestDeadlineGoalTitle;
            set { nearestDeadlineGoalTitle = value; OnPropertyChanged(); }
        }

        private string nearestDeadlineCountdownText = string.Empty;
        public string NearestDeadlineCountdownText
        {
            get => nearestDeadlineCountdownText;
            set { nearestDeadlineCountdownText = value; OnPropertyChanged(); }
        }

        private string petSummaryLine = string.Empty;
        public string PetSummaryLine
        {
            get => petSummaryLine;
            set { petSummaryLine = value; OnPropertyChanged(); }
        }

        private int stamina;
        public int Stamina
        {
            get => stamina;
            set
            {
                stamina = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StaminaProgress));
                OnPropertyChanged(nameof(StaminaText));
            }
        }

        public double StaminaProgress => Math.Min(1.0, Math.Max(0.0, Stamina / (double)AppService.WeeklyStaminaCap));

        public string StaminaText => $"{Stamina}/{AppService.WeeklyStaminaCap}";

        public ICommand NavigateToStatisticsCommand { get; }

        public ICommand NavigateToGoalHistoryCommand { get; }

        public ICommand MarkWorkoutCommand { get; }

        public ICommand OpenHandbookCommand { get; }

        public MainRoomViewModel()
        {
            appService = AppService.GetInstance();
            NavigateToStatisticsCommand = new Command(async () => await NavigateToStatisticsAsync());
            NavigateToGoalHistoryCommand = new Command(async () => await NavigateToGoalHistoryAsync());
            MarkWorkoutCommand = new Command<Goal>(async goal => await MarkWorkoutAsync(goal));
            OpenHandbookCommand = new Command(async () => await NavigateToHandbookAsync());
            _ = RefreshDataAsync();
        }

        private static async Task NavigateToHandbookAsync()
        {
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("//HandbookPage");
            }
        }

        private async Task MarkWorkoutAsync(Goal? workoutGoal)
        {
            if (workoutGoal == null)
            {
                return;
            }

            await appService.EnsureGameDataLoadedAsync();
            await appService.EnsureDailyStaminaAsync();

            int cost = workoutGoal.ResolvedStaminaCost;

            if (appService.GetProgress().Stamina < cost)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Not enough stamina",
                        $"Marking a workout costs {cost}. Stamina refills Sundays at 00:00.",
                        "OK");
                }

                return;
            }

            bool succeeded = await appService.RecordWorkoutForTodayAsync(workoutGoal);

            if (!succeeded)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Workout not saved",
                        "Could not log this workout. It may already be marked for today, or the goal is no longer active.",
                        "OK");
                }

                await RefreshDataAsync();

                return;
            }

            await RefreshDataAsync();
        }

        public async Task RefreshDataAsync()
        {
            await appService.EnsureGameDataLoadedAsync();
            await appService.EnsureDailyStaminaAsync();

            PetProgress progress = appService.GetProgress();

            PetName = string.IsNullOrWhiteSpace(progress.PetName) ? "Your Pet" : progress.PetName;
            PetAvatar = ImageSource.FromFile(string.IsNullOrWhiteSpace(progress.SelectedPetImage) ? "dotnet_bot.png" : progress.SelectedPetImage);

            PetSummaryLine = $"Goals finished: {progress.CompletedGoalsLifetime}";

            Stamina = progress.Stamina;

            List<Goal> activeGoals = appService.GetActiveGoals();
            ActiveGoalsCount = activeGoals.Count;
            AverageScheduleSummary = appService.SummarizeAverageScheduleAdherence(activeGoals);

            Goal? nearest = null;
            foreach (Goal g in activeGoals)
            {
                if (nearest == null)
                {
                    nearest = g;
                }
                else if (g.EndDate < nearest.EndDate)
                {
                    nearest = g;
                }
            }

            nearestDeadlineGoal = nearest;
            ApplyNearestDeadlineLabels();

            TodayGoals.Clear();
            foreach (Goal goal in appService.FilterGoalsAwaitingWorkoutToday(activeGoals))
            {
                TodayGoals.Add(goal);
            }
        }

        private static async Task NavigateToStatisticsAsync()
        {
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("//StatisticsPage");
            }
        }

        private static async Task NavigateToGoalHistoryAsync()
        {
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("//GoalHistoryPage");
            }
        }

        /// <summary>Start 1s countdown refresh while Main Room is visible and a nearest deadline exists.</summary>
        public void StartDeadlineTickerIfNeeded()
        {
            if (deadlineTickerRunning || nearestDeadlineGoal == null)
            {
                return;
            }

            IDispatcher? dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                return;
            }

            deadlineTickerRunning = true;
            dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (!deadlineTickerRunning)
                {
                    return false;
                }

                MainThread.BeginInvokeOnMainThread(TickNearestDeadlineCountdown);
                return true;
            });
        }

        public void StopDeadlineTicker()
        {
            deadlineTickerRunning = false;
        }

        private void TickNearestDeadlineCountdown()
        {
            if (nearestDeadlineGoal == null)
            {
                NearestDeadlineCountdownText = string.Empty;
                return;
            }

            NearestDeadlineCountdownText = FormatDeadlineCountdown(nearestDeadlineGoal.EndDate);
        }

        private void ApplyNearestDeadlineLabels()
        {
            if (nearestDeadlineGoal == null)
            {
                NearestDeadlineGoalTitle = "No active goals";
                NearestDeadlineCountdownText = string.Empty;
                return;
            }

            NearestDeadlineGoalTitle = nearestDeadlineGoal.Title;
            NearestDeadlineCountdownText = FormatDeadlineCountdown(nearestDeadlineGoal.EndDate);
        }

        private static DateTime EndOfLocalCalendarDay(DateTime date)
        {
            return date.Date.AddDays(1).AddTicks(-1);
        }

        private static string FormatDeadlineCountdown(DateTime endDate)
        {
            DateTime deadlineEnd = EndOfLocalCalendarDay(endDate);
            DateTime now = DateTime.Now;
            if (now > deadlineEnd)
            {
                return "Overdue";
            }

            TimeSpan remaining = deadlineEnd - now;
            return $"Time left: {remaining.Days}d {remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }
}
