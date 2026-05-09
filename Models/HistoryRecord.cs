using Microsoft.Maui.Graphics;

namespace Soulbound.Models
{
    /// <summary>
    /// One activity row showing how stamina and growth credits moved when something happened on a goal.
    /// </summary>
    public class HistoryRecord
    {
        public const string StatusCompleted = "Completed";
        public const string StatusCompletedLate = "CompletedLate";
        public const string StatusAbandoned = "Abandoned";
        public const string StatusFailed = "Failed";
        public const string StatusPenalty = "Penalty";
        public const string StatusWorkout = "Workout";

        public string TaskName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string ResultStatus { get; set; } = string.Empty;

        public int XpChange { get; set; }

        public int StaminaSpent { get; set; }

        public DateTime DateFinished { get; set; } = DateTime.Now;

        public string StatusText => ResultStatus switch
        {
            StatusCompleted => "Completed",
            StatusCompletedLate => "Completed (late)",
            StatusAbandoned => "Abandoned",
            StatusFailed => "Failed",
            StatusPenalty => "Penalty",
            StatusWorkout => "Workout",
            _ => ResultStatus
        };

        public Color StatusColor => ResultStatus switch
        {
            StatusCompleted => Color.FromArgb("#7ed957"),
            StatusCompletedLate => Color.FromArgb("#f5a623"),
            StatusAbandoned => Color.FromArgb("#9e9e9e"),
            StatusFailed => Color.FromArgb("#ff4444"),
            StatusPenalty => Color.FromArgb("#ff9f43"),
            StatusWorkout => Color.FromArgb("#54a8ff"),
            _ => Colors.Gray
        };

        public Color CategoryBarColor => Category switch
        {
            "Physical" => Color.FromArgb("#e03c31"),
            "Intellectual" => Color.FromArgb("#3c6fe0"),
            "Mental" => Color.FromArgb("#9b4de0"),
            _ => Color.FromArgb("#888888")
        };

        /// <summary>
        /// One line for stamina spent and optional growth credits from finished goals.
        /// </summary>
        public string ResourcesLine
        {
            get
            {
                string growthPart;
                if (ResultStatus == StatusWorkout)
                {
                    growthPart = XpChange == 0 ? "Workout logged" : FormatGrowthPart(XpChange);
                }
                else
                {
                    growthPart = FormatGrowthPart(XpChange);
                }

                string energyPart;
                if (StaminaSpent > 0)
                {
                    energyPart = " / -" + StaminaSpent.ToString() + " stamina";
                }
                else
                {
                    energyPart = " / stamina: 0";
                }

                return growthPart + energyPart;
            }
        }

        /// <summary>
        /// Longer wording for tapping the colour strip inside Statistics so you can articulate what happened.
        /// Kept deliberately plain for demonstrations.
        /// </summary>
        public string BuildTeacherFriendlyStatusStory()
        {
            return ResultStatus switch
            {
                StatusCompleted =>
                    $"{TaskName}: finished on schedule. The stamina you spent throughout this goal credited your growth gauges all at once when you closed it.",
                StatusCompletedLate =>
                    $"{TaskName}: marked complete after its deadline date. Growth credit still counted from stamina spent overall, though your discipline score was treated as a late finish.",
                StatusAbandoned =>
                    $"{TaskName}: marked abandoned automatically because workouts were missed badly enough against the planned cadence.",
                StatusFailed =>
                    $"{TaskName}: stayed overdue long enough after the deadline for the automatic penalty pulse to fire.",
                StatusPenalty =>
                    $"{TaskName}: you removed the goal midway through its second half, so a late-forfeit penalty was applied.",
                StatusWorkout =>
                    $"{TaskName}: one scheduled workout was marked in the main room. This spends stamina and nudges your weekly balance, but only finishing the goal moves the big growth bars.",
                _ =>
                    $"{TaskName}: recorded as {StatusText}."
            };
        }

        private static string FormatGrowthPart(int change)
        {
            if (change > 0)
            {
                return "+" + change.ToString() + " growth credit";
            }

            if (change < 0)
            {
                return change.ToString() + " growth credit";
            }

            return "No growth credit";
        }
    }
}
