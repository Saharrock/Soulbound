using System.Collections.ObjectModel;
using System.Windows.Input;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    class GoalHistoryViewModel : ViewModelBase
    {
        private readonly LocalDataService dataService;

        public ObservableCollection<Goal> ActiveGoals { get; } = new();
        public ObservableCollection<Goal> FinishedGoals { get; } = new();

        public ICommand CompleteGoalCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand ToggleExpandCommand { get; }

        public GoalHistoryViewModel()
        {
            dataService = LocalDataService.GetInstance();
            CompleteGoalCommand = new Command<Goal>(async goal => await CompleteGoalAsync(goal));
            DeleteItemCommand = new Command<Goal>(async goal => await DeleteGoalAsync(goal));
            ToggleExpandCommand = new Command<Goal>(ToggleExpand);
            Init();
        }

        public void Init()
        {
            ActiveGoals.Clear();
            FinishedGoals.Clear();

            foreach (var goal in dataService.GetActiveGoals())
            {
                ActiveGoals.Add(goal);
            }

            foreach (var goal in dataService.GetFinishedGoals())
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

            var successed = await dataService.MarkGoalAsCompletedAsync(goalToComplete);
            if (successed)
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

            var successed = await dataService.RemoveGoalAsync(goalToDelete);
            if (successed)
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
