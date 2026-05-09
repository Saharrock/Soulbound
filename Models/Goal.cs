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

        /// <summary>Stamina spent when tapping Done. Legacy data may omit or use 0.</summary>
        public int StaminaCost { get; set; } = 15;

        /// <summary>
        /// Every stamina deduction tied to this goal while it stays active (workouts plus the final Done press).
        /// Used only when the goal is fully completed to feed growth-level pillars.
        /// </summary>
        public int TotalStaminaSpentAcrossGoal { get; set; }

        //Status 
        public bool IsCompleted { get; set; } = false; // Completed/No
        public bool IsCompletedLate { get; set; } = false;
        public bool IsAbandoned { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public bool IsOverduePenaltyApplied { get; set; } = false;
        public int PlannedWorkouts { get; set; }
        public int CompletedWorkouts { get; set; }
        public int MissedWorkouts { get; set; }

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

        public const int FallbackStaminaCost = 15;

        /// <summary>Weekly pool segment per goal (stored value may be legacy &gt; cap).</summary>
        public const int MaxStaminaCostPerGoal = 15;

        public int ResolvedStaminaCost => StaminaCost < 1 ? FallbackStaminaCost : Math.Clamp(StaminaCost, 1, MaxStaminaCostPerGoal);

        public string WorkoutStatsText => $"Workouts: {CompletedWorkouts}/{Math.Max(1, PlannedWorkouts)}";

        public string MissedStatsText => $"Missed: {MissedWorkouts}";

        public string FinalStatusText
        {
            get
            {
                if (IsAbandoned)
                {
                    return "Abandoned";
                }

                if (IsCompleted)
                {
                    return IsCompletedLate ? "Done (late)" : "Done";
                }

                return "Active";
            }
        }

        /// <summary>
        /// Whether "Done" is shown. Long goals (≥14d span): ≥7 days after creation, still before deadline.
        /// Short goals: ≥12h after creation.
        /// </summary>
        public bool IsTrialWindow => GetLifecycleProgressFraction() <= 0.20;

        public bool IsMiddleWindow
        {
            get
            {
                double p = GetLifecycleProgressFraction();
                return p > 0.20 && p < 0.80;
            }
        }

        public bool IsFinalWindow => GetLifecycleProgressFraction() >= 0.80;

        private double GetLifecycleProgressFraction()
        {
            if (IsCompleted)
            {
                return 1.0;
            }

            double totalSeconds = (Deadline - CreatedAt).TotalSeconds;
            if (totalSeconds <= 0)
            {
                return 1.0;
            }

            double elapsedSeconds = (DateTime.Now - CreatedAt).TotalSeconds;
            return Math.Clamp(elapsedSeconds / totalSeconds, 0.0, 1.0);
        }
    }

}
