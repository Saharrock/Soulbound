using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Soulbound.Models
{
    public class GoalCollection
    {
        public List<Goal> ActiveGoals { get; set; } = new List<Goal>();
        public List<Goal> FinishedGoals { get; set; } = new List<Goal>();

        public void AddGoal(Goal goal)
        {
            if (goal.IsCompleted)
                FinishedGoals.Add(goal);
            else
                ActiveGoals.Add(goal);
        }

        public void RemoveGoal(Goal goal)
        {
            ActiveGoals.Remove(goal);
            FinishedGoals.Remove(goal);
        }
        //to do goal packs
    }
}

