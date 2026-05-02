using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class MainRoomViewModel : ViewModelBase
    {
        private readonly AppService appService;

        public ObservableCollection<Goal> TodayGoals { get; } = new();

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

        private double physicalValue;
        public double PhysicalValue
        {
            get => physicalValue;
            set { physicalValue = value; OnPropertyChanged(); }
        }

        private double intellectualValue;
        public double IntellectualValue
        {
            get => intellectualValue;
            set { intellectualValue = value; OnPropertyChanged(); }
        }

        private double mentalValue;
        public double MentalValue
        {
            get => mentalValue;
            set { mentalValue = value; OnPropertyChanged(); }
        }

        private int activeGoalsCount;
        public int ActiveGoalsCount
        {
            get => activeGoalsCount;
            set { activeGoalsCount = value; OnPropertyChanged(); }
        }

        private string nearestDeadlineText = "No active goals";
        public string NearestDeadlineText
        {
            get => nearestDeadlineText;
            set { nearestDeadlineText = value; OnPropertyChanged(); }
        }

        private string precisionSummary = string.Empty;
        public string PrecisionSummary
        {
            get => precisionSummary;
            set { precisionSummary = value; OnPropertyChanged(); }
        }

        private string achievementSummary = string.Empty;
        public string AchievementSummary
        {
            get => achievementSummary;
            set { achievementSummary = value; OnPropertyChanged(); }
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

        public MainRoomViewModel()
        {
            appService = AppService.GetInstance();
            NavigateToStatisticsCommand = new Command(async () => await NavigateToStatisticsAsync());
            NavigateToGoalHistoryCommand = new Command(async () => await NavigateToGoalHistoryAsync());
            MarkWorkoutCommand = new Command<Goal>(async goal => await MarkWorkoutAsync(goal));
            _ = RefreshDataAsync();
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
                        $"Marking a workout costs {cost}. Your weekly stamina pool refreshes next Monday.",
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
            await appService.ApplyDeadlinePenaltiesAsync();

            PetProgress progress = appService.GetProgress();

            PetName = string.IsNullOrWhiteSpace(progress.PetName) ? "Your Pet" : progress.PetName;
            PetAvatar = ImageSource.FromFile(string.IsNullOrWhiteSpace(progress.SelectedPetImage) ? "dotnet_bot.png" : progress.SelectedPetImage);

            PrecisionSummary =
                $"Precision: {PetProgress.PrecisionLabel(progress.PrecisionScore)} ({progress.PrecisionScore})";

            AchievementSummary =
                $"Mastery: {PetProgress.AchievementLabel(progress.CompletedGoalsLifetime)} ({progress.CompletedGoalsLifetime} goals)";

            ApplyWeeklyEffortShares(progress);

            Stamina = progress.Stamina;

            List<Goal> activeGoals = appService.GetActiveGoals();
            ActiveGoalsCount = activeGoals.Count;

            Goal? nearest = null;
            foreach (Goal g in activeGoals)
            {
                if (nearest == null)
                {
                    nearest = g;
                }
                else
                {
                    if (g.EndDate < nearest.EndDate)
                    {
                        nearest = g;
                    }
                }
            }

            if (nearest == null)
            {
                NearestDeadlineText = "No active goals";
            }
            else
            {
                NearestDeadlineText = nearest.EndDate.ToString("dd/MM/yyyy");
            }

            TodayGoals.Clear();
            foreach (Goal goal in appService.GetTodayGoalsAwaitingWorkout())
            {
                TodayGoals.Add(goal);
            }
        }

        private void ApplyWeeklyEffortShares(PetProgress progress)
        {
            int p = progress.WeeklyPhysicalPoints;
            int i = progress.WeeklyIntellectualPoints;
            int m = progress.WeeklyMentalPoints;
            double denom = Math.Max(1.0, Math.Max(p, Math.Max(i, m)));
            PhysicalValue = p / denom;
            IntellectualValue = i / denom;
            MentalValue = m / denom;
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
    }
}
