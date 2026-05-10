using Microsoft.Maui.Graphics;
using Newtonsoft.Json;

namespace Soulbound.Models
{
    /// <summary>One row in the activity timeline.</summary>
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

        /// <summary>
        /// On goal completion: total stamina spent on that goal (workouts + Done). Legacy rows may store old XP/penalty deltas.
        /// Stored under Firebase key <c>xpChange</c>.
        /// </summary>
        [JsonProperty("xpChange")]
        public int StaminaInvestedOrLegacyDelta { get; set; }

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

        public string ResourcesLine => FormatTimelineResourcesSummary();

        /// <summary>Single place for timeline subtitle text (used by <see cref="ResourcesLine"/>).</summary>
        public string FormatTimelineResourcesSummary()
        {
            string detail = ResultStatus switch
            {
                StatusWorkout => "Workout logged",
                StatusCompleted or StatusCompletedLate => FormatStaminaInvestedLine(StaminaInvestedOrLegacyDelta),
                _ => FormatLegacyDeltaLine(StaminaInvestedOrLegacyDelta)
            };

            string energyPart = StaminaSpent > 0
                ? " / -" + StaminaSpent + " stamina"
                : " / stamina: 0";

            return detail + energyPart;
        }

        public string BuildTimelineTapDetail()
        {
            string when = DateFinished.ToString("dd MMM yyyy HH:mm", System.Globalization.CultureInfo.CurrentCulture);
            return $"{StatusText}\nStamina on this row: {StaminaSpent}\n{when}\n\n{BuildTeacherFriendlyStatusStory()}";
        }

        public string BuildTeacherFriendlyStatusStory()
        {
            return ResultStatus switch
            {
                StatusCompleted =>
                    $"{TaskName}: finished on or before the deadline. Timeline rows show stamina spent on workouts and on Done.",
                StatusCompletedLate =>
                    $"{TaskName}: finished after the deadline date (still counts as done).",
                StatusAbandoned =>
                    $"{TaskName}: legacy row from an older app version (auto-abandon).",
                StatusFailed or StatusPenalty =>
                    $"{TaskName}: legacy penalty row from an older app version.",
                StatusWorkout =>
                    $"{TaskName}: one scheduled workout was marked; stamina was deducted from your weekly pool.",
                _ =>
                    $"{TaskName}: recorded as {StatusText}."
            };
        }

        private static string FormatStaminaInvestedLine(int totalSpent)
        {
            if (totalSpent > 0)
            {
                return $"{totalSpent} stamina invested on this goal (total)";
            }

            return "No stamina total recorded";
        }

        private static string FormatLegacyDeltaLine(int change)
        {
            if (change > 0)
            {
                return "+" + change + " (legacy)";
            }

            if (change < 0)
            {
                return change + " (legacy)";
            }

            return "—";
        }
    }
}
