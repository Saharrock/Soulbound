using Microsoft.Maui.Graphics;

using Newtonsoft.Json;



namespace Soulbound.Models

{

    // Одна запись в журнале (Statistics → timeline). Создаётся при workout или Done.

    public class HistoryRecord

    {

        public const string StatusCompleted = "Completed";

        public const string StatusCompletedLate = "CompletedLate";

        public const string StatusWorkout = "Workout";



        public string TaskName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty; // Physical / Intellectual / Mental

        public string ResultStatus { get; set; } = string.Empty;



        // При Done — всего stamina на цель; при workout — 0

        public int TotalStaminaInvested { get; set; }



        // Списано stamina за это конкретное действие

        public int StaminaSpent { get; set; }

        public DateTime DateFinished { get; set; } = DateTime.Now;



        [JsonIgnore]

        public string StatusText => ResultStatus switch

        {

            StatusCompleted => "Completed",

            StatusCompletedLate => "Completed (late)",

            StatusWorkout => "Workout",

            _ => ResultStatus

        };



        [JsonIgnore]

        public Color StatusColor => ResultStatus switch

        {

            StatusCompleted => Color.FromArgb("#7ed957"),

            StatusCompletedLate => Color.FromArgb("#f5a623"),

            StatusWorkout => Color.FromArgb("#54a8ff"),

            _ => Colors.Gray

        };



        [JsonIgnore]

        public string ResourcesLine => FormatTimelineResourcesSummary();



        // Подпись под статусом в timeline, напр. "Workout logged / -10 stamina"

        public string FormatTimelineResourcesSummary()

        {

            string detail = ResultStatus switch

            {

                StatusWorkout => "Workout logged",

                StatusCompleted or StatusCompletedLate => FormatStaminaInvestedLine(TotalStaminaInvested),

                _ => "—"

            };



            string energyPart = StaminaSpent > 0

                ? " / -" + StaminaSpent + " stamina"

                : " / stamina: 0";



            return detail + energyPart;

        }



        // Текст для DisplayAlert при тапе на строку timeline

        public string BuildTimelineTapDetail()

        {

            string when = DateFinished.ToString("dd MMM yyyy HH:mm", System.Globalization.CultureInfo.CurrentCulture);

            return $"{StatusText}\nStamina on this row: {StaminaSpent}\n{when}\n\n{BuildStatusExplanation()}";

        }



        public string BuildStatusExplanation()

        {

            return ResultStatus switch

            {

                StatusCompleted =>

                    $"{TaskName}: finished on or before the deadline. Timeline rows show stamina spent on workouts and on Done.",

                StatusCompletedLate =>

                    $"{TaskName}: finished after the deadline date (still counts as done).",

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

    }

}


