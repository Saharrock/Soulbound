using System.Collections.Generic;

namespace Soulbound.Models
{
    public class Goal
    {
        public string Id { get; set; } = String.Empty; //number of goal
        public string Title { get; set; } = String.Empty;     // Goal Name
        public string Description { get; set; } = String.Empty; //Goal description
        public int GoalTime { get; set; } //Time of goal duration  in hours
        public int ProgressPoints { get; set; }
        /// <summary>
        /// When set, this XP value is used instead of calculating from days until deadline.
        /// </summary>
        public int? CustomProgressPoints { get; set; }
        //Dates
        public DateTime CreatedAt { get; set; } = DateTime.Now; //Date of goal ending
        public DateTime EndDate { get; set; } = DateTime.Now;
        public DateTime Deadline { get; set; } = DateTime.Now;

        // Categories
        public bool IsPhysical { get; set; } = false;
        public bool IsMental { get; set; } = false;
        public bool IsIntellectual { get; set; } = false;

        //Status 
        public bool IsCompleted { get; set; } = false; // Completed/No
        public bool IsAbandoned { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public bool IsOverduePenaltyApplied { get; set; } = false;

        //WeekDays
        public bool IsSunday { get; set; } = false;
        public bool IsMonday { get; set; } = false;
        public bool IsTuesday { get; set; } = false;
        public bool IsWednesday { get; set; } = false;
        public bool IsThursday { get; set; } = false;
        public bool IsFriday { get; set; } = false;
        public bool IsSaturday { get; set; } = false;

        public string ScheduleText
        {
            get
            {
                List<string> days = new();
                if (IsMonday) days.Add("Mon");
                if (IsTuesday) days.Add("Tue");
                if (IsWednesday) days.Add("Wed");
                if (IsThursday) days.Add("Thu");
                if (IsFriday) days.Add("Fri");
                if (IsSaturday) days.Add("Sat");
                if (IsSunday) days.Add("Sun");
                return days.Count == 0 ? "No days selected" : string.Join(", ", days);
            }
        }
    }

}
