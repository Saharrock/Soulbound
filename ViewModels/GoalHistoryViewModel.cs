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
    class GoalHistoryViewModel : ViewModelBase
    {

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

        #region Commands
        public ICommand DeleteItemCommand { get; set; }
        public ICommand AddGoalCommand { get; set; }
        #endregion


        #region Constructor
        public GoalHistoryViewModel()
        {
            Goals = new ObservableCollection<Goal>(LocalDataService.GetInstance().GetGoals());
            DeleteItemCommand = new Command((item) => DeleteItem(item)); // Currently this is a sync function , we will change it to async later

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
    }
}
