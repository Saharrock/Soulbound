namespace Soulbound.Models

{

    // Факт тренировки: цель + дата. Для Schedule % и счётчиков (не timeline).

    public sealed class WorkoutSession

    {

        public string GoalId { get; set; } = string.Empty;



        // yyyy-MM-dd — одна тренировка в день на цель

        public string SessionDateIso { get; set; } = string.Empty;

    }

}


