using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class MainRoomViewModel : ViewModelBase
    {
        private readonly AppService appService;

        public ObservableCollection<Goal> TodayGoals { get; } = new();

        private int level;
        public int Level
        {
            get => level;
            set { level = value; OnPropertyChanged(); }
        }

        private string rank = "Beginner";
        public string Rank
        {
            get => rank;
            set { rank = value; OnPropertyChanged(); }
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

        public double StaminaProgress => Math.Min(1.0, Math.Max(0.0, Stamina / 100.0));

        public string StaminaText => $"{Stamina}/100";

        public MainRoomViewModel()
        {
            appService = AppService.GetInstance();
            _ = RefreshDataAsync();
        }

        public async Task RefreshDataAsync()
        {
            await appService.EnsureGameDataLoadedAsync();
            await appService.EnsureDailyStaminaAsync();
            await appService.ApplyDeadlinePenaltiesAsync();

            PetProgress progress = appService.GetProgress();
            int required = progress.PointsPerStatForCurrentLevel;

            Level = progress.Level;
            Rank = progress.Rank;
            PetName = string.IsNullOrWhiteSpace(progress.PetName) ? "Your Pet" : progress.PetName;
            PetAvatar = ImageSource.FromFile(string.IsNullOrWhiteSpace(progress.SelectedPetImage) ? "dotnet_bot.png" : progress.SelectedPetImage);
            PhysicalValue = Math.Min(1.0, (double)progress.PhysicalPoints / required);
            IntellectualValue = Math.Min(1.0, (double)progress.IntellectualPoints / required);
            MentalValue = Math.Min(1.0, (double)progress.MentalPoints / required);
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
            foreach (Goal goal in appService.GetTodayGoals())
            {
                TodayGoals.Add(goal);
            }
        }
    }
}
