using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq;
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

        public ICommand AddQuickPackCommand { get; }
        public ICommand TogglePackPreviewCommand { get; }
        public ObservableCollection<QuickPackCardViewModel> QuickPacks { get; } = new();

        public CreateGoalViewModel()
        {
            appService = AppService.GetInstance();
            AddGoalCommand = new Command(async () => await AddGoalAsync());
            AddQuickPackCommand = new Command<QuickPackCardViewModel>(async pack => await AddQuickPackAsync(pack));
            TogglePackPreviewCommand = new Command<QuickPackCardViewModel>(TogglePackPreview);
            LoadQuickPacks();
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

        private async Task AddQuickPackAsync(QuickPackCardViewModel? pack)
        {
            if (pack == null)
            {
                return;
            }

            await appService.EnsureGameDataLoadedAsync();
            await appService.AddQuickPackAsync(pack.Id);
            MessageForUser = $"{pack.Title} added.";
            pack.IsExpanded = false;
        }

        private void ResetForm()
        {
            NewTitle = string.Empty;
            NewDescription = string.Empty;
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

        private void LoadQuickPacks()
        {
            QuickPacks.Clear();
            int staminaCostPerGoal = appService.GoalCompletionStaminaCost;
            foreach (QuickStartPackDefinition definition in appService.GetQuickStartPacks())
            {
                var tasks = definition.Tasks
                    .Select(task => new QuickPackTaskViewModel(task.Title, task.XpGain))
                    .ToList();

                QuickPacks.Add(new QuickPackCardViewModel(
                    definition.Id,
                    definition.Title,
                    definition.Description,
                    tasks,
                    staminaCostPerGoal));
            }
        }

        private void TogglePackPreview(QuickPackCardViewModel? selectedPack)
        {
            if (selectedPack == null)
            {
                return;
            }

            foreach (QuickPackCardViewModel pack in QuickPacks)
            {
                pack.IsExpanded = pack == selectedPack ? !pack.IsExpanded : false;
            }
        }
    }

    internal sealed class QuickPackCardViewModel : ViewModelBase
    {
        private bool isExpanded;
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public IReadOnlyList<QuickPackTaskViewModel> Tasks { get; }
        public int TotalXpGain { get; }
        public int EstimatedStaminaCost { get; }
        public string TotalXpText => $"Total XP gain: {TotalXpGain}";
        public string EstimatedStaminaText => $"Estimated stamina cost: {EstimatedStaminaCost}";
        public string PreviewButtonText => IsExpanded ? "Hide details" : "Preview details";

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PreviewButtonText));
            }
        }

        public QuickPackCardViewModel(
            string id,
            string title,
            string description,
            IReadOnlyList<QuickPackTaskViewModel> tasks,
            int staminaCostPerGoal)
        {
            Id = id;
            Title = title;
            Description = description;
            Tasks = tasks;
            TotalXpGain = tasks.Sum(task => task.XpGain);
            EstimatedStaminaCost = tasks.Count * staminaCostPerGoal;
        }
    }

    internal sealed class QuickPackTaskViewModel
    {
        public string Title { get; }
        public int XpGain { get; }
        public string XpText => $"+{XpGain} XP";

        public QuickPackTaskViewModel(string title, int xpGain)
        {
            Title = title;
            XpGain = xpGain;
        }
    }
}
