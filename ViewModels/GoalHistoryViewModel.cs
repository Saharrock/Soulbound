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

        public ObservableCollection<Goal> activeGoals;
        public ObservableCollection<Goal> ActiveGoals
        {
            get { return activeGoals; }
            set
            {
                activeGoals = value;
                OnPropertyChanged();
            }

        }
        public ObservableCollection<Goal> finishedGoals;
        public ObservableCollection<Goal> FinishedGoals
        {
            get { return finishedGoals; }
            set
            {
                finishedGoals = value;
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

        #region Commands
        public ICommand CompleteGoalCommand { get; set; }
        public ICommand DeleteItemCommand { get; set; }

        #endregion


        #region Constructor
        public GoalHistoryViewModel()
        {
            InitAsync();
            CompleteGoalCommand = new Command((item) => CompleteGoal(item));
            DeleteItemCommand = new Command((item) => DeleteItem(item)); // Currently this is a sync function , we will change it to async later

        }
        public async Task InitAsync()
        {
            Goals = new ObservableCollection<Goal>(LocalDataService.GetInstance().GetGoals());
            ActiveGoals = new ObservableCollection<Goal>(LocalDataService.GetInstance().GetActiveGoals());
            FinishedGoals = new ObservableCollection<Goal>(LocalDataService.GetInstance().GetFinishedGoals());
        }
        #endregion


        #region Functions

        public void CompleteGoal(object objGoal)
        {
            if (objGoal is not Goal goalToComplete)
                return;

            // Устанавливаем цель как выполненную
            goalToComplete.IsCompleted = true;
            OnPropertyChanged();

            // Переносим из Active в Finished
            if (ActiveGoals.Contains(goalToComplete))
            {
                ActiveGoals.Remove(goalToComplete);
                FinishedGoals.Add(goalToComplete);
            }

        }

        public void DeleteItem(object obgGoal)
        {
            Goal goalToDelete = (Goal)obgGoal;
            int elapsedDays = (DateTime.Today - goalToDelete.CreatedAt).Days;

            if (elapsedDays * 24 < goalToDelete.GoalTime / 4)
            {
                ActiveGoals.Remove(goalToDelete);
                Goals.Remove(goalToDelete); // Remove the iem from the ObservableCollection on THIS PAGE only
                
                LocalDataService.GetInstance().RemoveActiveGoal(goalToDelete);
                LocalDataService.GetInstance().RemoveGoal(goalToDelete);
                OnPropertyChanged();
            }
            else
            {
                ActiveGoals.Remove(goalToDelete);
                FinishedGoals.Add(goalToDelete);
            }

            // We must also update the servie
            // Currently this is a sync function , we will change it to async later
        }
        #endregion


    }
}
