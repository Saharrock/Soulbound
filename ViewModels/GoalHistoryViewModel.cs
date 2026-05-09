using System.Collections.ObjectModel;
using System.Globalization;
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
        public ICommand TrialDeleteGoalCommand { get; }
        public ICommand ChangeWeekdaysCommand { get; }
        public ICommand ChangeStaminaCommand { get; }
        public ICommand PostponeGoalCommand { get; }

        public GoalHistoryViewModel()
        {
            appService = AppService.GetInstance();
            CompleteGoalCommand = new Command<Goal>(async goal => await CompleteGoalAsync(goal));
            TrialDeleteGoalCommand = new Command<Goal>(async goal => await TrialDeleteGoalAsync(goal));
            ChangeWeekdaysCommand = new Command<Goal>(async goal => await ChangeWeekdaysAsync(goal));
            ChangeStaminaCommand = new Command<Goal>(async goal => await ChangeStaminaAsync(goal));
            PostponeGoalCommand = new Command<Goal>(async goal => await PostponeGoalAsync(goal));
            _ = RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            await appService.EnsureGameDataLoadedAsync();
            await appService.ApplyAbandonedGoalRulesAsync();
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

        private async Task TrialDeleteGoalAsync(Goal? goalToDelete)
        {
            if (goalToDelete == null)
            {
                return;
            }

            if (goalToDelete.IsCompleted || !goalToDelete.IsTrialWindow)
            {
                return;
            }

            bool succeeded = await appService.DeleteGoalWithoutTraceAsync(goalToDelete);
            if (succeeded)
            {
                ActiveGoals.Remove(goalToDelete);
                FinishedGoals.Remove(goalToDelete);
            }
        }

        private async Task ChangeWeekdaysAsync(Goal? goalToChange)
        {
            if (goalToChange == null || goalToChange.IsCompleted || (!goalToChange.IsTrialWindow && !goalToChange.IsMiddleWindow))
            {
                return;
            }

            if (Application.Current?.MainPage == null)
            {
                return;
            }

            string? input = await Application.Current.MainPage.DisplayPromptAsync(
                "Change weekdays",
                "Enter days separated by comma. Use numbers 1-7 (Mon-Sun) or names (Mon,Tue...).",
                accept: "Save",
                cancel: "Cancel",
                placeholder: "1,3,5");

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            if (!TryParseWeekdaysInput(input, out bool mon, out bool tue, out bool wed, out bool thu, out bool fri, out bool sat, out bool sun))
            {
                await Application.Current.MainPage.DisplayAlert("Invalid input", "Use values like 1,3,5 or Mon,Wed,Fri.", "OK");
                return;
            }

            bool succeeded = await appService.UpdateGoalWeekdaysAsync(goalToChange, mon, tue, wed, thu, fri, sat, sun);
            if (!succeeded)
            {
                await Application.Current.MainPage.DisplayAlert("Not saved", "Could not update weekdays.", "OK");
            }

            await RefreshAsync();
        }

        private async Task ChangeStaminaAsync(Goal? goalToChange)
        {
            if (goalToChange == null || goalToChange.IsCompleted || !goalToChange.IsMiddleWindow)
            {
                return;
            }

            if (Application.Current?.MainPage == null)
            {
                return;
            }

            string? input = await Application.Current.MainPage.DisplayPromptAsync(
                "Change energy cost",
                $"Enter stamina cost from 1 to {Goal.MaxStaminaCostPerGoal}.",
                accept: "Save",
                cancel: "Cancel",
                initialValue: goalToChange.ResolvedStaminaCost.ToString(CultureInfo.InvariantCulture),
                keyboard: Microsoft.Maui.Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            if (!int.TryParse(input, out int staminaCost))
            {
                await Application.Current.MainPage.DisplayAlert("Invalid number", "Enter a whole number.", "OK");
                return;
            }

            bool succeeded = await appService.UpdateGoalStaminaCostAsync(goalToChange, staminaCost);
            if (!succeeded)
            {
                await Application.Current.MainPage.DisplayAlert("Not saved", "Could not update stamina cost.", "OK");
            }

            await RefreshAsync();
        }

        private async Task PostponeGoalAsync(Goal? goalToPostpone)
        {
            if (goalToPostpone == null || goalToPostpone.IsCompleted || !goalToPostpone.IsFinalWindow)
            {
                return;
            }

            if (Application.Current?.MainPage == null)
            {
                return;
            }

            string oldDateText = goalToPostpone.Deadline.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            string? input = await Application.Current.MainPage.DisplayPromptAsync(
                "Postpone goal",
                $"Enter new deadline (yyyy-MM-dd). It cannot be earlier than {oldDateText}.",
                accept: "Save",
                cancel: "Cancel",
                initialValue: oldDateText);

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            if (!DateTime.TryParseExact(input.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime newDate))
            {
                await Application.Current.MainPage.DisplayAlert("Invalid date", "Use format yyyy-MM-dd.", "OK");
                return;
            }

            bool succeeded = await appService.PostponeGoalAsync(goalToPostpone, newDate);
            if (!succeeded)
            {
                await Application.Current.MainPage.DisplayAlert("Not saved", "New date must be same or later than current deadline.", "OK");
            }

            await RefreshAsync();
        }

        private static bool TryParseWeekdaysInput(
            string raw,
            out bool mon,
            out bool tue,
            out bool wed,
            out bool thu,
            out bool fri,
            out bool sat,
            out bool sun)
        {
            mon = false;
            tue = false;
            wed = false;
            thu = false;
            fri = false;
            sat = false;
            sun = false;

            string[] parts = raw.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
            {
                return false;
            }

            foreach (string part in parts)
            {
                string token = part.Trim().ToLowerInvariant();
                switch (token)
                {
                    case "1":
                    case "mon":
                    case "monday":
                        mon = true;
                        break;
                    case "2":
                    case "tue":
                    case "tuesday":
                        tue = true;
                        break;
                    case "3":
                    case "wed":
                    case "wednesday":
                        wed = true;
                        break;
                    case "4":
                    case "thu":
                    case "thursday":
                        thu = true;
                        break;
                    case "5":
                    case "fri":
                    case "friday":
                        fri = true;
                        break;
                    case "6":
                    case "sat":
                    case "saturday":
                        sat = true;
                        break;
                    case "7":
                    case "sun":
                    case "sunday":
                        sun = true;
                        break;
                    default:
                        return false;
                }
            }

            return mon || tue || wed || thu || fri || sat || sun;
        }
    }
}
