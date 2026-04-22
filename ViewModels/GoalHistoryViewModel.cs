using System.Collections.ObjectModel;
using System.Windows.Input;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class GoalHistoryViewModel : ViewModelBase
    {
        private readonly TaskService taskService;

        private readonly CharacterService characterService;

        public ObservableCollection<Goal> ActiveGoals { get; } = new();

        public ObservableCollection<Goal> FinishedGoals { get; } = new();

        public ICommand CompleteGoalCommand { get; }

        public ICommand DeleteItemCommand { get; }

        public ICommand ToggleExpandCommand { get; }

        public GoalHistoryViewModel()
        {
            taskService = TaskService.GetInstance();
            characterService = CharacterService.GetInstance();
            CompleteGoalCommand = new Command<Goal>(async goal => await CompleteGoalAsync(goal));
            DeleteItemCommand = new Command<Goal>(async goal => await DeleteGoalAsync(goal));
            ToggleExpandCommand = new Command<Goal>(ToggleExpand);
            Init();
        }

        /// <summary>
        /// Refreshes the two lists from the database-backed task service.
        /// </summary>
        public void Init()
        {
            ActiveGoals.Clear();
            FinishedGoals.Clear();

            foreach (Goal goal in taskService.GetActiveGoals())
            {
                ActiveGoals.Add(goal);
            }

            foreach (Goal goal in taskService.GetFinishedGoals())
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

            characterService.EnsureDailyStamina();

            if (characterService.GetProgress().Stamina < 10)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "No stamina",
                        "Restore your character stamina before completing goals.",
                        "OK");
                }

                return;
            }

            bool succeeded = await taskService.MarkGoalAsCompletedAsync(goalToComplete);
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

            bool succeeded = await taskService.RemoveGoalAsync(goalToDelete);
            if (succeeded)
            {
                ActiveGoals.Remove(goalToDelete);
                FinishedGoals.Remove(goalToDelete);
            }
        }

        private void ToggleExpand(Goal? goal)
        {
            if (goal == null)
            {
                return;
            }

            goal.IsExpanded = !goal.IsExpanded;
            OnPropertyChanged(nameof(ActiveGoals));
            OnPropertyChanged(nameof(FinishedGoals));
        }
    }
}
