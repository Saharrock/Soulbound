using System;
using System.Collections.Generic;
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

        public List<Goal> goals = new List<Goal>();
        private void CreateFakeData()
        {
            Goal goal1 = new Goal()
            {
                Id = "1",
                Title = "Learn swimming",
                TimeToComplete = "2d",
                IsPhysical = true,
                IsMental = true

            };
            goals.Add(goal1);
        }
        public List<Goal> GetGoals()
        {
            return goals;
        }
        public void RemoveGoal(Goal msg)
        {
            goals.Remove(msg);
        }
        public void AddGoal(Goal msg)
        {
            goals.Add(msg);
        }
    }


}
