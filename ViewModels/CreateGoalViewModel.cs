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
                        Console.WriteLine("Funny)");
                        return;
                    }

                    result = result / 100;
                    int month = result % 100;
                    if (month > 12)
                    {
                        Console.WriteLine("months are only 12)");
                        return;
                    }
                    result = result / 100;
                    int days = result % 100;
                    if (days > 31)
                    {
                        Console.WriteLine("its can't be more than 31 day");
                        return;
                    }

                    newGoalTime = years * 8760 + month * 730 + days * 24;
                    newTimeToComplete = $"{years}Y {month}M {days}D";
                    
                    OnPropertyChanged();
                }
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
        #endregion


        #region Constructor
        public CreateGoalViewModel()
        {
            Goals = new ObservableCollection<Goal>(LocalDataService.GetInstance().GetGoals());
            AddGoalCommand = new Command(AddGoal); // Currently this is a sync function , we will change it to async later

        }
        #endregion

        public void AddGoal()
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
                    CreatedAt = DateTime.Now,
                };
                Goals.Add(newGoal);
                // We must also update the servie
                LocalDataService.GetInstance().AddGoal(newGoal); // Currently this is a sync function , we will change it to async later
                                                                 //Clean The fields
                NewTitle = "";
                NewTimeToComplete = "";
            }
        }
    }
}
