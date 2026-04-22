using System.Text.Json.Serialization;

namespace Soulbound.Models
{
    public class PetProgress
    {
        private int stamina = 100;

        public string PetName { get; set; } = string.Empty;
        public string PetImage { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public int PhysicalPoints { get; set; }
        public int IntellectualPoints { get; set; }
        public int MentalPoints { get; set; }
        public DateTime LastLoginDate { get; set; } = DateTime.Today;

        public int Stamina
        {
            get => stamina;
            set
            {
                if (value < 0)
                {
                    stamina = 0;
                    return;
                }

                if (value > 100)
                {
                    stamina = 100;
                    return;
                }

                stamina = value;
            }
        }

        [JsonIgnore]
        public int PointsPerStatForCurrentLevel => 100 + (Level - 1) * 50;

        [JsonIgnore]
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
    }
}
