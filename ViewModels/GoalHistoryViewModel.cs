using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
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

        public ICommand DeleteGoalCommand { get; }

        public ICommand AddGoalPhotoCommand { get; }

        public ICommand RemoveGoalPhotoCommand { get; }

        public GoalHistoryViewModel()
        {
            appService = AppService.GetInstance();
            CompleteGoalCommand = new Command<Goal>(async goal => await CompleteGoalAsync(goal));
            DeleteGoalCommand = new Command<Goal>(async goal => await DeleteGoalAsync(goal));
            AddGoalPhotoCommand = new Command<Goal>(async goal => await AddGoalPhotoAsync(goal));
            RemoveGoalPhotoCommand = new Command<GoalPhotoRef>(async photo => await RemoveGoalPhotoAsync(photo));
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

        private async Task AddGoalPhotoAsync(Goal? goal)
        {
            if (goal == null || Application.Current?.MainPage == null)
            {
                return;
            }

            Page page = Application.Current.MainPage;

            string? choice = await page.DisplayActionSheet(
                "Add photo",
                "Cancel",
                null,
                "Choose from gallery",
                "Take photo");

            if (choice is null or "Cancel")
            {
                return;
            }

            try
            {
                FileResult? file = null;

                if (choice == "Take photo")
                {
                    PermissionStatus cam = await Permissions.CheckStatusAsync<Permissions.Camera>();
                    if (cam != PermissionStatus.Granted)
                    {
                        cam = await Permissions.RequestAsync<Permissions.Camera>();
                    }

                    if (cam != PermissionStatus.Granted)
                    {
                        await page.DisplayAlert("Camera", "Camera permission is needed to take a photo.", "OK");
                        return;
                    }

                    if (!MediaPicker.Default.IsCaptureSupported)
                    {
                        await page.DisplayAlert("Camera", "Camera is not supported on this device.", "OK");
                        return;
                    }

                    file = await MediaPicker.Default.CapturePhotoAsync();
                }
                else
                {
                    PermissionStatus photos = await Permissions.CheckStatusAsync<Permissions.Photos>();
                    if (photos != PermissionStatus.Granted)
                    {
                        photos = await Permissions.RequestAsync<Permissions.Photos>();
                    }

                    if (photos != PermissionStatus.Granted)
                    {
                        await page.DisplayAlert("Photos", "Photo library access is needed to pick an image.", "OK");
                        return;
                    }

                    file = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Pick a photo" });
                }

                if (file == null)
                {
                    return;
                }

                await appService.EnsureGameDataLoadedAsync();

                string ext = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(ext))
                {
                    ext = ".jpg";
                }

                await using Stream stream = await file.OpenReadAsync();
                bool ok = await appService.AddGoalPhotoFromStreamAsync(goal, stream, ext);
                if (!ok)
                {
                    await page.DisplayAlert("Photo", "Could not save the photo.", "OK");
                }
            }
            catch (FeatureNotSupportedException)
            {
                await page.DisplayAlert("Photo", "This feature is not available on this device.", "OK");
            }
            catch (PermissionException)
            {
                await page.DisplayAlert("Photo", "Permission was denied.", "OK");
            }
        }

        private async Task RemoveGoalPhotoAsync(GoalPhotoRef? photo)
        {
            if (photo == null)
            {
                return;
            }

            await appService.EnsureGameDataLoadedAsync();
            await appService.RemoveGoalPhotoAsync(photo);
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
                        $"Done costs {cost} stamina from your weekly pool.",
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
            else if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Could not complete",
                    "The goal may already be completed or no longer available.",
                    "OK");
            }
        }

        private async Task DeleteGoalAsync(Goal? goalToDelete)
        {
            if (goalToDelete == null || goalToDelete.IsCompleted)
            {
                return;
            }

            if (Application.Current?.MainPage == null)
            {
                return;
            }

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Remove goal",
                $"Remove \"{goalToDelete.Title}\"? Workouts logged for it will be cleared.",
                "Remove",
                "Cancel");

            if (!confirm)
            {
                return;
            }

            bool succeeded = await appService.RemoveGoalAsync(goalToDelete);
            if (succeeded)
            {
                ActiveGoals.Remove(goalToDelete);
            }
        }
    }
}
