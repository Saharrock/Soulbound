using System.Collections.Generic;

using System.Collections.ObjectModel;

using Newtonsoft.Json;



namespace Soulbound.Models

{

    // Модель цели пользователя. Хранится в Firebase в gameData.Goals.

    public class Goal

    {

        // --- Идентификация и текст ---

        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;



        // --- Даты ---

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime EndDate { get; set; } = DateTime.Now;

        public DateTime Deadline { get; set; } = DateTime.Now;



        // --- Категории (столпы): можно выбрать несколько ---

        public bool IsPhysical { get; set; }

        public bool IsMental { get; set; }

        public bool IsIntellectual { get; set; }



        // Сколько stamina тратится за одну тренировку и за Done (1–10)

        public int StaminaCost { get; set; } = 10;



        // Суммарно потрачено stamina на эту цель (workouts + Done)

        public int TotalStaminaSpentAcrossGoal { get; set; }



        // --- Статус завершения ---

        public bool IsCompleted { get; set; }

        public bool IsCompletedLate { get; set; } // true, если Done после deadline



        // Счётчики тренировок (обновляет AppService.UpdateWorkoutCounters)

        public int PlannedWorkouts { get; set; }   // всего слотов за весь срок цели

        public int CompletedWorkouts { get; set; } // сколько WorkoutSession записано

        public int MissedWorkouts { get; set; }    // пропущенные слоты до сегодня/deadline



        // --- Расписание: в какие дни недели нужна тренировка ---

        public bool IsSunday { get; set; }

        public bool IsMonday { get; set; }

        public bool IsTuesday { get; set; }

        public bool IsWednesday { get; set; }

        public bool IsThursday { get; set; }

        public bool IsFriday { get; set; }

        public bool IsSaturday { get; set; }



        // Фото цели: файлы локально, в Firebase только пути (attachedPhotos)

        [JsonProperty("attachedPhotos")]

        public ObservableCollection<GoalPhotoRef> AttachedPhotos { get; set; } = new();



        // Строка расписания для UI, напр. "Mon, Wed, Fri". Не сохраняется в Firebase.

        [JsonIgnore]

        public string ScheduleText

        {

            get

            {

                List<string> days = new();

                if (IsSunday) days.Add("Sun");

                if (IsMonday) days.Add("Mon");

                if (IsTuesday) days.Add("Tue");

                if (IsWednesday) days.Add("Wed");

                if (IsThursday) days.Add("Thu");

                if (IsFriday) days.Add("Fri");

                if (IsSaturday) days.Add("Sat");

                return days.Count == 0 ? "No days selected" : string.Join(", ", days);

            }

        }



        public const int FallbackStaminaCost = 10;

        public const int MaxStaminaCostPerGoal = 10;



        // Безопасная стоимость stamina: clamp 1–10, иначе fallback 10.

        [JsonIgnore]

        public int ResolvedStaminaCost =>

            StaminaCost < 1 ? FallbackStaminaCost : Math.Clamp(StaminaCost, 1, MaxStaminaCostPerGoal);



        [JsonIgnore]

        public string WorkoutStatsText => $"Workouts: {CompletedWorkouts}/{Math.Max(1, PlannedWorkouts)}";



        [JsonIgnore]

        public string MissedStatsText => $"Missed: {MissedWorkouts}";



        // Пишет AppService, напр. "Schedule 75%"

        public string ScheduleAdherenceLine { get; set; } = "—";



        [JsonIgnore]

        public string FinalStatusText =>

            IsCompleted ? (IsCompletedLate ? "Done (late)" : "Done") : "Active";

    }



    // Ссылка на файл фото в AppData. RelativePath уходит в Firebase.

    public sealed class GoalPhotoRef

    {

        [JsonProperty("relativePath")]

        public string RelativePath { get; set; } = string.Empty;



        // Только в памяти — для UI, не сериализуется

        [JsonIgnore]

        public Goal? Owner { get; set; }

    }

}


