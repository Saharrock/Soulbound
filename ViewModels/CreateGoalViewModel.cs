using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soulbound.Models;
using Soulbound.Services;
using System.Windows.Input;

namespace Soulbound.ViewModels
{
    internal class CreateGoalViewModel : ViewModelBase
    {
        #region get set

        private string messageForUser;
        public string MessageForUser
        {
            get { return messageForUser; }
            set
            {
                if (value != null)
                {
                    messageForUser = value;
                    OnPropertyChanged();
                }
            }
        }


        private string newTitle;
        public string NewTitle
        {
            get { return newTitle; }
            set
            {
                if (value != null)
                {
                    newTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        private string newDescription;
        public string NewDescription
        {
            get { return newDescription; }
            set
            {
                if (value != null)
                {
                    
                    newDescription = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime TodayDate => DateTime.Today.AddDays(1);
        private DateTime selectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get => selectedDate;
            set
            {
                selectedDate = value;
                OnPropertyChanged();
            }
        }


        private bool newIsPhysical;
        public bool NewIsPhysical
        {
            get { return newIsPhysical; }
            set
            {
                newIsPhysical = value;
                OnPropertyChanged();
            }
        }

        private bool newIsMental;
        public bool NewIsMental
        {
            get { return newIsMental; }
            set
            {
                newIsMental = value;
                OnPropertyChanged();
            }
        }

        private bool newIsIntellectual;
        public bool NewIsIntellectual
        {
            get { return newIsIntellectual; }
            set
            {
                newIsIntellectual = value;
                OnPropertyChanged();
            }
        }

        private bool isSunday;
        public bool IsSunday
        {
            get { return isSunday; }
            set { isSunday = value; OnPropertyChanged(); }
        }

        private bool isMonday;
        public bool IsMonday
        {
            get { return isMonday; }
            set { isMonday = value; OnPropertyChanged(); }
        }

        private bool isTuesday;
        public bool IsTuesday
        {
            get { return isTuesday; }
            set { isTuesday = value; OnPropertyChanged(); }
        }

        private bool isWednesday;
        public bool IsWednesday
        {
            get { return isWednesday; }
            set { isWednesday = value; OnPropertyChanged(); }
        }

        private bool isThursday;
        public bool IsThursday
        {
            get { return isThursday; }
            set { isThursday = value; OnPropertyChanged(); }
        }

        private bool isFriday;
        public bool IsFriday
        {
            get { return isFriday; }
            set { isFriday = value; OnPropertyChanged(); }
        }

        private bool isSaturday;
        public bool IsSaturday
        {
            get { return isSaturday; }
            set { isSaturday = value; OnPropertyChanged(); }
        }


        private ObservableCollection<Goal> goals;
        public ObservableCollection<Goal> Goals
        {
            get { return goals; }
            set
            {
                goals = value;
                OnPropertyChanged();
            }
        }
        #endregion


        #region Commands

        public ICommand AddGoalCommand { get; set; }
        #endregion


        #region Constructor
        public CreateGoalViewModel()
        {
            Goals = new ObservableCollection<Goal>(LocalDataService.GetInstance().GetGoals());
            AddGoalCommand = new Command(async () => await AddGoalAsync()); // Currently this is a sync function , we will change it to async later

        }
        #endregion

        private async Task ClearMessageAfterDelayAsync()
        {
            await Task.Delay(3000);
            MessageForUser = "";
        }

        async Task AddGoalAsync()
        {
            if (NewTitle != null && NewDescription != null && NewTitle != "" && NewDescription != "")
            {
                bool hasAnyDaySelected = IsSunday || IsMonday || IsTuesday || IsWednesday || IsThursday || IsFriday || IsSaturday;
                if (!hasAnyDaySelected)
                {
                    MessageForUser = "Select at least one training day";
                    _ = ClearMessageAfterDelayAsync();
                    return;
                }

                Random randomId = new Random();
                Goal newGoal = new Goal()
                {
                    Title = newTitle,
                    Description = newDescription,
                    GoalTime = (SelectedDate - DateTime.Now).Days * 24,
                    EndDate = selectedDate,
                    IsPhysical = newIsPhysical,
                    IsMental = newIsMental,
                    IsIntellectual = newIsIntellectual,
                    CreatedAt = DateTime.Now,
                    IsSunday = IsSunday,
                    IsMonday = IsMonday,
                    IsTuesday = IsTuesday,
                    IsWednesday = IsWednesday,
                    IsThursday = IsThursday,
                    IsFriday = IsFriday,
                    IsSaturday = IsSaturday
                };
               
                // Lets try to update the DBs
                bool respond = await LocalDataService.GetInstance().AddGoalAsync(newGoal);
                if (respond == true)
                {
                    Goals.Add(newGoal);
                }  else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Failed to add Goal",
                        "Check odsf sdnk",
                        "OK"
                    );
                }
                    MessageForUser = "Goal successfully created!";
                _ = ClearMessageAfterDelayAsync();
                //Clean The fields
                NewTitle = "";
                NewDescription = "";
                NewIsPhysical = false;
                NewIsMental = false;
                NewIsIntellectual = false;
                IsSunday = false;
                IsMonday = false;
                IsTuesday = false;
                IsWednesday = false;
                IsThursday = false;
                IsFriday = false;
                IsSaturday = false;
                SelectedDate = DateTime.Today.AddDays(1);
            }
        }


    }
}
