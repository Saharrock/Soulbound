using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soulbound.Models
{
    internal class WeekGoals
    {
 
        public List<Goal> MondayGoals { get; set; } = new List<Goal>();
        public List<Goal> TuesdayGoals { get; set; } = new List<Goal>();
        public List<Goal> WednesdayGoals { get; set; } = new List<Goal>();
        public List<Goal> ThursdayGoals { get; set; } = new List<Goal>();
        public List<Goal>FridayGoals { get; set; } = new List<Goal>();
        public List<Goal>SaturdayGoals { get; set; } = new List<Goal>();
        public List<Goal> SundayGoals { get; set; } = new List<Goal>();

        public void AddGoal(Goal goal)
        {

            if (goal.IsMonday)
                MondayGoals.Add(goal);
            if (goal.IsTuesday)
                ThursdayGoals.Add(goal);
            if (goal.IsWednesday)
                WednesdayGoals.Add(goal);
            if (goal.IsThursday)
                ThursdayGoals.Add(goal);
            if (goal.IsFriday)
                FridayGoals.Add(goal);
            if (goal.IsSaturday)
                SaturdayGoals.Add(goal);
            if (goal.IsSunday)
                SundayGoals.Add(goal);
            //
        }
    }
}
