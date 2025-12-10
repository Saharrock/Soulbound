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

        private string newTimeToComplete;
        public string NewTimeToComplete
        {
            get { return newTimeToComplete; }
            set
            {
                if (value != null)
                {
                    newTimeToComplete = value;
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
        public ICommand DeleteItemCommand { get; set; }
        public ICommand AddGoalCommand { get; set; }
        #endregion


        #region Constructor
        public CreateGoalViewModel()
        {
            Goals = new ObservableCollection<Goal>(LocalDataService.GetInstance().GetGoals());
            DeleteItemCommand = new Command((item) => DeleteItem(item)); // Currently this is a sync function , we will change it to async later
            AddGoalCommand = new Command(AddGoal); // Currently this is a sync function , we will change it to async later

        }
        #endregion


        #region Functions
        public void DeleteItem(object obgGoal)
        {
            Goal goalToDelete = (Goal)obgGoal;

            Goals.Remove(goalToDelete); // Remove the iem from the ObservableCollection on THIS PAGE only
            OnPropertyChanged();
            // We must also update the servie
            LocalDataService.GetInstance().RemoveGoal(goalToDelete); // Currently this is a sync function , we will change it to async later
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
