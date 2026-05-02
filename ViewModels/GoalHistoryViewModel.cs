using System.Collections.ObjectModel;
using System.Windows.Input;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class GoalHistoryViewModel : ViewModelBase
    {
        private readonly AppService appService;

        public ObservableCollection<Goal> ActiveGoals { get; } = new();

        public ObservableCollection<Goal> FinishedGoals { get; } = new();

        public ICommand CompleteGoalCommand { get; }

        public ICommand DeleteItemCommand { get; }

        public GoalHistoryViewModel()
        {
            appService = AppService.GetInstance();
            CompleteGoalCommand = new Command<Goal>(async goal => await CompleteGoalAsync(goal));
            DeleteItemCommand = new Command<Goal>(async goal => await DeleteGoalAsync(goal));
            _ = RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            await appService.EnsureGameDataLoadedAsync();
            ActiveGoals.Clear();
            FinishedGoals.Clear();

            foreach (Goal goal in appService.GetActiveGoals())
            {
                ActiveGoals.Add(goal);
            }

            foreach (Goal goal in appService.GetFinishedGoals())
            {
                FinishedGoals.Add(goal);
            }
        }

        private async Task CompleteGoalAsync(Goal? goalToComplete)
        {
            if (goalToComplete == null)
            {
                return;
            }

            await appService.EnsureDailyStaminaAsync();

            int cost = goalToComplete.ResolvedStaminaCost;
            if (appService.GetProgress().Stamina < cost)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Not enough stamina",
                        $"Achievement costs {cost} stamina. Completing closes the goal and grants XP.",
                        "OK");
                }

                return;
            }

            bool succeeded = await appService.MarkGoalAsCompletedAsync(goalToComplete);
            if (succeeded)
            {
                ActiveGoals.Remove(goalToComplete);
                if (!FinishedGoals.Contains(goalToComplete))
                {
                    FinishedGoals.Add(goalToComplete);
                }
            }
        }

        private async Task DeleteGoalAsync(Goal? goalToDelete)
        {
            if (goalToDelete == null)
            {
                return;
            }

            if (goalToDelete.IsCompleted)
            {
                return;
            }

            bool succeeded = await appService.RemoveGoalAsync(goalToDelete);
            if (succeeded)
            {
                ActiveGoals.Remove(goalToDelete);
                FinishedGoals.Remove(goalToDelete);
            }
        }
    }
}
