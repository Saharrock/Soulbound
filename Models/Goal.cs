using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Soulbound.Models
{
    public class Goal
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
        public DateTime Deadline { get; set; } = DateTime.Now;

        public bool IsPhysical { get; set; }
        public bool IsMental { get; set; }
        public bool IsIntellectual { get; set; }

        /// <summary>Stamina per workout mark and for the final Done tap.</summary>
        public int StaminaCost { get; set; } = 15;

        /// <summary>Running total of stamina spent on this goal while active (workouts + Done).</summary>
        public int TotalStaminaSpentAcrossGoal { get; set; }

        public bool IsCompleted { get; set; }
        public bool IsCompletedLate { get; set; }

        public int PlannedWorkouts { get; set; }
        public int CompletedWorkouts { get; set; }
        public int MissedWorkouts { get; set; }

        public bool IsSunday { get; set; }
        public bool IsMonday { get; set; }
        public bool IsTuesday { get; set; }
        public bool IsWednesday { get; set; }
        public bool IsThursday { get; set; }
        public bool IsFriday { get; set; }
        public bool IsSaturday { get; set; }

        /// <summary>Local-only photo paths (relative to app data). Synced in RTDB as metadata only — binary files stay on device.</summary>
        [JsonProperty("attachedPhotos")]
        public ObservableCollection<GoalPhotoRef> AttachedPhotos { get; set; } = new();

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
        public const int MaxStaminaCostPerGoal = 15;

        public int ResolvedStaminaCost =>
            StaminaCost < 1 ? FallbackStaminaCost : Math.Clamp(StaminaCost, 1, MaxStaminaCostPerGoal);

        public string WorkoutStatsText => $"Workouts: {CompletedWorkouts}/{Math.Max(1, PlannedWorkouts)}";

        public string MissedStatsText => $"Missed: {MissedWorkouts}";

        /// <summary>UI line e.g. &quot;Schedule 82%&quot; — workouts vs expected slots through today or deadline.</summary>
        [JsonProperty("disciplineUiLine")]
        public string ScheduleAdherenceLine { get; set; } = "—";

        public string FinalStatusText =>
            IsCompleted ? (IsCompletedLate ? "Done (late)" : "Done") : "Active";
    }

    /// <summary>Local goal attachment; file bytes live under <see cref="Microsoft.Maui.Storage.FileSystem.AppDataDirectory"/>.</summary>
    public sealed class GoalPhotoRef
    {
        [JsonProperty("relativePath")]
        public string RelativePath { get; set; } = string.Empty;

        [JsonIgnore]
        public Goal? Owner { get; set; }
    }
}
