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


        private bool isTimeHelpVisible;
        public bool IsTimeHelpVisible
        {
            get { return isTimeHelpVisible; }
            set
            {
                isTimeHelpVisible = value;
                OnPropertyChanged();
            }
        }


        public int newGoalTime;
        private string newTimeToComplete;
        public string NewTimeToComplete
        {
            get { return newTimeToComplete; }
            set
            {
                if (value != null && value.Length == 6 && int.TryParse(value, out int result))
                {

                    int years = result % 100;
                    if (years > 10)
                    {
                        MessageForUser = "Funny";
                        return;
                    }

                    result = result / 100;
                    int month = result % 100;
                    if (month > 12)
                    {
                        MessageForUser = "months are only 12 ;)";
                        return;
                    }
                    result = result / 100;
                    int days = result % 100;
                    if (days > 31)
                    {
                        MessageForUser = "There is no more than 31 day >.<";
                        return;
                    }

                    newGoalTime = years * 8760 + month * 730 + days * 24;
                    newTimeToComplete = $"{years}Y {month}M {days}D";

                    OnPropertyChanged();
                }
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
        public ICommand ToggleTimeHelpCommand { get; }
        #endregion


        #region Constructor
        public CreateGoalViewModel()
        {
            Goals = new ObservableCollection<Goal>(LocalDataService.GetInstance().GetGoals());
            AddGoalCommand = new Command(async () => await AddGoalAsync()); // Currently this is a sync function , we will change it to async later
            ToggleTimeHelpCommand = new Command(() =>
            {
                IsTimeHelpVisible = !IsTimeHelpVisible;
            });

        }
        public 
        #endregion

        async Task AddGoalAsync()
        {
            if (NewTitle != null && NewTimeToComplete != null && NewTitle != "" && NewTimeToComplete != "")
            {
                Random randomId = new Random();
                Goal newGoal = new Goal()
                {
                    Id = randomId.Next(1, 100).ToString(),
                    Title = NewTitle,
                    TimeToComplete = NewTimeToComplete,
                    GoalTime = newGoalTime,
                    IsPhysical = NewIsPhysical,
                    IsMental = newIsMental,
                    IsIntellectual = newIsIntellectual,
                    CreatedAt = DateTime.Now,
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
                                                                 //Clean The fields
                NewTitle = "";
                NewTimeToComplete = "";
            }
        }


    }
}
