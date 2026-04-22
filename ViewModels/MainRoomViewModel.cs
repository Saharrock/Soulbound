using System.Collections.ObjectModel;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    class MainRoomViewModel : ViewModelBase
    {
        private readonly LocalDataService dataService;

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

        private string petImage = string.Empty;
        public string PetImage
        {
            get => petImage;
            set { petImage = value; OnPropertyChanged(); }
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

        public MainRoomViewModel()
        {
            dataService = LocalDataService.GetInstance();
            RefreshData();
        }

        public void RefreshData()
        {
            var progress = dataService.GetPetProgress();
            var required = progress.PointsPerStatForCurrentLevel;

            Level = progress.Level;
            Rank = progress.Rank;
            PetName = string.IsNullOrWhiteSpace(progress.PetName) ? "Your Pet" : progress.PetName;
            PetImage = string.IsNullOrWhiteSpace(progress.PetImage) ? "pet_fox.svg" : progress.PetImage;
            PhysicalValue = Math.Min(1.0, (double)progress.PhysicalPoints / required);
            IntellectualValue = Math.Min(1.0, (double)progress.IntellectualPoints / required);
            MentalValue = Math.Min(1.0, (double)progress.MentalPoints / required);

            var activeGoals = dataService.GetActiveGoals();
            ActiveGoalsCount = activeGoals.Count;
            var nearest = activeGoals.OrderBy(g => g.EndDate).FirstOrDefault();
            NearestDeadlineText = nearest == null ? "No active goals" : nearest.EndDate.ToString("dd/MM/yyyy");

            TodayGoals.Clear();
            foreach (var goal in dataService.GetTodayGoals())
            {
                TodayGoals.Add(goal);
            }
        }
    }
}
