using System.Windows.Input;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    // CreateGoalPage: форма новой цели → AddGoalAsync.
    internal class CreateGoalViewModel : ViewModelBase
    {
        private readonly AppService appService;

        private string messageForUser = string.Empty;
        public string MessageForUser
        {
            get => messageForUser;
            set { messageForUser = value; OnPropertyChanged(); }
        }

        private string newTitle = string.Empty;
        public string NewTitle
        {
            get => newTitle;
            set { newTitle = value; OnPropertyChanged(); }
        }

        private string newDescription = string.Empty;
        public string NewDescription
        {
            get => newDescription;
            set { newDescription = value; OnPropertyChanged(); }
        }

        public DateTime TodayDate => DateTime.Today.AddDays(1);

        private DateTime selectedDate = DateTime.Today.AddDays(1);
        public DateTime SelectedDate
        {
            get => selectedDate;
            set { selectedDate = value; OnPropertyChanged(); }
        }

        private bool newIsPhysical;
        public bool NewIsPhysical { get => newIsPhysical; set { newIsPhysical = value; OnPropertyChanged(); } }

        private bool newIsIntellectual;
        public bool NewIsIntellectual { get => newIsIntellectual; set { newIsIntellectual = value; OnPropertyChanged(); } }

        private bool newIsMental;
        public bool NewIsMental { get => newIsMental; set { newIsMental = value; OnPropertyChanged(); } }

        private bool isSunday;
        public bool IsSunday { get => isSunday; set { isSunday = value; OnPropertyChanged(); } }
        private bool isMonday;
        public bool IsMonday { get => isMonday; set { isMonday = value; OnPropertyChanged(); } }
        private bool isTuesday;
        public bool IsTuesday { get => isTuesday; set { isTuesday = value; OnPropertyChanged(); } }
        private bool isWednesday;
        public bool IsWednesday { get => isWednesday; set { isWednesday = value; OnPropertyChanged(); } }
        private bool isThursday;
        public bool IsThursday { get => isThursday; set { isThursday = value; OnPropertyChanged(); } }
        private bool isFriday;
        public bool IsFriday { get => isFriday; set { isFriday = value; OnPropertyChanged(); } }
        private bool isSaturday;
        public bool IsSaturday { get => isSaturday; set { isSaturday = value; OnPropertyChanged(); } }

        private int newStaminaCost = Goal.FallbackStaminaCost;

        public int NewStaminaCost
        {
            get => newStaminaCost;
            set
            {
                int v = Math.Clamp(value, 1, Goal.MaxStaminaCostPerGoal);
                if (newStaminaCost == v)
                {
                    return;
                }

                newStaminaCost = v;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NewStaminaCostSlider));
                OnPropertyChanged(nameof(NewStaminaCostLabel));
            }
        }

        public double NewStaminaCostSlider
        {
            get => newStaminaCost;
            set => NewStaminaCost = (int)Math.Round(Math.Clamp(value, 1.0, Goal.MaxStaminaCostPerGoal));
        }

        public string NewStaminaCostLabel =>
            $"Stamina each workout + final Done: {NewStaminaCost} (weekly pool, max {Goal.MaxStaminaCostPerGoal} per goal)";

        public ICommand AddGoalCommand { get; }

        public CreateGoalViewModel()
        {
            appService = AppService.GetInstance();
            AddGoalCommand = new Command(async () => await AddGoalAsync());
        }

        // Валидация полей, сбор Goal, вызов AppService.AddGoalAsync.
        private async Task AddGoalAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTitle) || string.IsNullOrWhiteSpace(NewDescription))
            {
                MessageForUser = "Fill in title and description.";
                return;
            }

            if (!NewIsPhysical && !NewIsIntellectual && !NewIsMental)
            {
                MessageForUser = "Select at least one goal type.";
                return;
            }

            bool hasAnyDaySelected = IsSunday || IsMonday || IsTuesday || IsWednesday ||
                                    IsThursday || IsFriday || IsSaturday;
            if (!hasAnyDaySelected)
            {
                MessageForUser = "Select at least one day.";
                return;
            }

            Goal goal = new Goal
            {
                Title = NewTitle.Trim(),
                Description = NewDescription.Trim(),
                EndDate = SelectedDate,
                Deadline = SelectedDate,
                CreatedAt = DateTime.Now,
                StaminaCost = NewStaminaCost,
                IsPhysical = NewIsPhysical,
                IsIntellectual = NewIsIntellectual,
                IsMental = NewIsMental,
                IsSunday = IsSunday,
                IsMonday = IsMonday,
                IsTuesday = IsTuesday,
                IsWednesday = IsWednesday,
                IsThursday = IsThursday,
                IsFriday = IsFriday,
                IsSaturday = IsSaturday
            };

            await appService.EnsureGameDataLoadedAsync();
            bool succeeded = await appService.AddGoalAsync(goal);
            MessageForUser = succeeded ? "Goal created." : "Failed to create goal.";
            if (succeeded)
            {
                ResetForm();
            }
        }

        private void ResetForm()
        {
            NewTitle = string.Empty;
            NewDescription = string.Empty;
            NewIsPhysical = false;
            NewIsIntellectual = false;
            NewIsMental = false;
            SelectedDate = DateTime.Today.AddDays(1);
            NewStaminaCost = Goal.FallbackStaminaCost;
            IsSunday = false;
            IsMonday = false;
            IsTuesday = false;
            IsWednesday = false;
            IsThursday = false;
            IsFriday = false;
            IsSaturday = false;
        }
    }
}
