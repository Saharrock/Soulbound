namespace Soulbound.Models
{
    public class PetProgress
    {
        public string PetName { get; set; } = string.Empty;
        public string SelectedPetImage { get; set; } = "dotnet_bot.png";
        public int Level { get; set; } = 1;
        public int PhysicalPoints { get; set; }
        public int IntellectualPoints { get; set; }
        public int MentalPoints { get; set; }
        public DateTime LastLoginDate { get; set; } = DateTime.Today;
        /// <summary>Weekly energy pool — refilled when <see cref="WeeklyPeriodKey"/> changes.</summary>
        public int Stamina { get; set; } = 100;

        /// <summary>0–100 discipline score: workouts on schedule, timely goal completions, overdue/abandon penalised.</summary>
        public int PrecisionScore { get; set; }

        /// <summary>Lifetime goals counted toward CompletedGoalsLifetime bookkeeping.</summary>
        public int CompletedGoalsLifetime { get; set; }

        /// <summary>True once default precision has been applied for migrating saves.</summary>
        public bool PrecisionSeeded { get; set; }

        /// <summary>Weekly anchor (<c>Saturday yyyy-MM-dd</c>) matched by <see cref="Soulbound.Services.AppService"/> when stamina refills.</summary>
        public string WeeklyPeriodKey { get; set; } = string.Empty;

        public int WeeklyPhysicalPoints { get; set; }
        public int WeeklyIntellectualPoints { get; set; }
        public int WeeklyMentalPoints { get; set; }
        public int PointsPerStatForCurrentLevel => 100 + (Level - 1) * 50;
        public string Rank
        {
            get
            {
                if (Level >= 7)
                {
                    return "Advanced";
                }

                if (Level >= 4)
                {
                    return "Intermediate";
                }

                return "Beginner";
            }
        }

        public static string PrecisionLabel(int score)
        {
            score = Math.Clamp(score, 0, 100);
            return score switch
            {
                >= 95 => "Flawless",
                >= 80 => "Sharp",
                >= 60 => "Solid",
                >= 40 => "Fair",
                _ => "Loose"
            };
        }

    }
}
