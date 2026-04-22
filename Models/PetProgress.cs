namespace Soulbound.Models
{
    public class PetProgress
    {
        public string PetName { get; set; } = string.Empty;
        public string PetImage { get; set; } = string.Empty;
        public int Level { get; set; } = 1;
        public int PhysicalPoints { get; set; }
        public int IntellectualPoints { get; set; }
        public int MentalPoints { get; set; }

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
    }
}
