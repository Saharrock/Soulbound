using System.Windows.Input;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
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

        private string newNotes = string.Empty;
        public string NewNotes
        {
            get => newNotes;
            set { newNotes = value; OnPropertyChanged(); }
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

        public ICommand AddGoalCommand { get; }

        public ICommand AddPhysicalPackCommand { get; }

        public ICommand AddIntellectPackCommand { get; }

        public CreateGoalViewModel()
        {
            appService = AppService.GetInstance();
            AddGoalCommand = new Command(async () => await AddGoalAsync());
            AddPhysicalPackCommand = new Command(async () => await AddPhysicalPackAsync());
            AddIntellectPackCommand = new Command(async () => await AddIntellectPackAsync());
        }

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
                Notes = NewNotes.Trim(),
                EndDate = SelectedDate,
                Deadline = SelectedDate,
                CreatedAt = DateTime.Now,
                GoalTime = Math.Max(24, (SelectedDate - DateTime.Now).Days * 24),
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

        private async Task AddIntellectPackAsync()
        {
            await appService.EnsureGameDataLoadedAsync();
            await appService.AddQuickIntellectPackAsync();
            MessageForUser = "Intellect quick start added.";
        }

        private async Task AddPhysicalPackAsync()
        {
            await appService.EnsureGameDataLoadedAsync();
            await appService.AddQuickPhysicalPackAsync();
            MessageForUser = "Physical quick start added.";
        }

        private void ResetForm()
        {
            NewTitle = string.Empty;
            NewDescription = string.Empty;
            NewNotes = string.Empty;
            NewIsPhysical = false;
            NewIsIntellectual = false;
            NewIsMental = false;
            SelectedDate = DateTime.Today.AddDays(1);
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
