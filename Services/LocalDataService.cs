using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soulbound.Models;

namespace Soulbound.Services
{

    class LocalDataService
    {

        #region instance
        private static LocalDataService instance;
        public LocalDataService()
        {
            CreateFakeData();
        }
        public static LocalDataService GetInstance()
        {
            if (instance == null)
            {
                instance = new LocalDataService();
            }
            return instance;
        }
        #endregion


        public async Task<bool> TryLoginAsync(string userNameString, string passwordString)
        {
            if (userNameString == null || passwordString == null)
            {

                return await Task.FromResult(false);
            }
            else
            {
                if (userNameString == "eldan" && passwordString == "123")
                {
                    return await Task.FromResult(true);
                }
            }
            return await Task.FromResult(false);
        }
        public List<Goal> goals = new List<Goal>();

        private int lastGoalId = 0; // стартовое значен


        private void CreateFakeData()
        {
            Goal goal1 = new Goal()
            {
                Id = "0",
                Title = "Learn swimming",
                Description = "I want to learn swimming before my traveling to America",
                IsPhysical = true,

            };
            goals.Add(goal1);
        }
        public List<Goal> GetGoals()
        {
            return goals;
        }

        public List<Goal> GetActiveGoals()
        {
            List<Goal> activeGoals = new List<Goal>();
            foreach (Goal g in goals)
            {
                if (!g.IsCompleted)
                {
                    activeGoals.Add(g);
                }     
            }
            return activeGoals;
        }

        public List<Goal> GetFinishedGoals()
        {
            return FinishedGoals;
        }
        public void RemoveGoal(Goal goal)
        {
            goals.Remove(goal);
        }

        public void RemoveActiveGoal(Goal goal)
        {
            ActiveGoals.Remove(goal);
        }
        public List<Goal> ActiveGoals { get; set; } = new List<Goal>();
        public List<Goal> FinishedGoals { get; set; } = new List<Goal>();

        public async Task<bool> AddGoalAsync(Goal goal)
        {
            lastGoalId++;
            goal.Id = lastGoalId.ToString();
            goals.Add(goal); // общий список
            goal.IsCompleted = false;
            ActiveGoals.Add(goal);
            return true;
        }

        public async Task<bool> MakeGoalComplete(Goal goalToComplete)
        {
            ActiveGoals.Remove(goalToComplete);
            FinishedGoals.Add(goalToComplete);
            return true;
        }
    }



}
