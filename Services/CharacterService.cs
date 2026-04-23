using Soulbound.Models;

namespace Soulbound.Services
{
    /// <summary>
    /// Character (pet) progress: stamina, XP, level, and pet selection preview.
    /// Stamina daily restore and stat changes are handled here; persistence goes through DatabaseService.
    /// </summary>
    public sealed class CharacterService
    {
        private static CharacterService? instance;

        private readonly DatabaseService database;

        private readonly List<PetOption> petTemplates = new();

        private int sessionPetIndex;

        public static CharacterService GetInstance()
        {
            if (instance == null)
            {
                instance = new CharacterService();
            }

            return instance;
        }

        private CharacterService()
        {
            database = DatabaseService.GetInstance();
            FillPetTemplates();

            AppPersistedData data = database.GetPersistedData();
            sessionPetIndex = data.SelectedPetIndex;

            if (sessionPetIndex < 0 || sessionPetIndex >= petTemplates.Count)
            {
                sessionPetIndex = 0;
            }
        }

        private void FillPetTemplates()
        {
            petTemplates.Add(new PetOption { Image = "fox.png", DefaultName = "Fox" });
            petTemplates.Add(new PetOption { Image = "wolf.png", DefaultName = "Wolf" });
            petTemplates.Add(new PetOption { Image = "rabbit.png", DefaultName = "Rabbit" });
            petTemplates.Add(new PetOption { Image = "cat.png", DefaultName = "Cat" });
        }

        public List<PetOption> GetPetTemplates()
        {
            return petTemplates;
        }

        public PetOption GetCurrentPetTemplate()
        {
            return petTemplates[sessionPetIndex];
        }

        public PetOption MovePetLeft()
        {
            sessionPetIndex = sessionPetIndex - 1;
            if (sessionPetIndex < 0)
            {
                sessionPetIndex = petTemplates.Count - 1;
            }

            return petTemplates[sessionPetIndex];
        }

        public PetOption MovePetRight()
        {
            sessionPetIndex = sessionPetIndex + 1;
            if (sessionPetIndex >= petTemplates.Count)
            {
                sessionPetIndex = 0;
            }

            return petTemplates[sessionPetIndex];
        }

        /// <summary>
        /// Saves chosen pet name and image to persisted character data.
        /// </summary>
        public void ConfirmPetSelection(string petName)
        {
            PetOption current = petTemplates[sessionPetIndex];
            AppPersistedData data = database.GetPersistedData();
            data.Character.PetImage = PetImageHelper.GetSafePetImageFileName(current.Image);

            if (string.IsNullOrWhiteSpace(petName))
            {
                data.Character.PetName = current.DefaultName;
            }
            else
            {
                data.Character.PetName = petName.Trim();
            }

            data.SelectedPetIndex = sessionPetIndex;
            database.Save();
        }

        public PetProgress GetProgress()
        {
            return database.GetPersistedData().Character;
        }

        /// <summary>
        /// If the calendar day changed since last login, refill stamina to 100.
        /// </summary>
        public void EnsureDailyStamina()
        {
            PetProgress progress = GetProgress();

            if (progress.LastLoginDate.Date != DateTime.Today)
            {
                progress.Stamina = 100;
                progress.LastLoginDate = DateTime.Today;
                database.Save();
            }
        }

        public void SpendStaminaForTaskCompletion(int amount)
        {
            PetProgress progress = GetProgress();
            progress.Stamina -= amount;
        }

        public void AddProgressFromCompletedGoal(Goal goal)
        {
            PetProgress progress = GetProgress();

            if (goal.IsPhysical)
            {
                progress.PhysicalPoints += goal.ProgressPoints;
            }

            if (goal.IsIntellectual)
            {
                progress.IntellectualPoints += goal.ProgressPoints;
            }

            if (goal.IsMental)
            {
                progress.MentalPoints += goal.ProgressPoints;
            }
        }

        /// <summary>
        /// Subtracts penalty points from stats that match the goal categories.
        /// </summary>
        public void ApplyPenaltyByGoalCategories(Goal goal, int penaltyPoints)
        {
            PetProgress progress = GetProgress();

            if (goal.IsPhysical)
            {
                int newValue = progress.PhysicalPoints - penaltyPoints;
                if (newValue < 0)
                {
                    newValue = 0;
                }

                progress.PhysicalPoints = newValue;
            }

            if (goal.IsIntellectual)
            {
                int newValue = progress.IntellectualPoints - penaltyPoints;
                if (newValue < 0)
                {
                    newValue = 0;
                }

                progress.IntellectualPoints = newValue;
            }

            if (goal.IsMental)
            {
                int newValue = progress.MentalPoints - penaltyPoints;
                if (newValue < 0)
                {
                    newValue = 0;
                }

                progress.MentalPoints = newValue;
            }
        }

        /// <summary>
        /// When all three stats reach the threshold for the current level, level up and reset stats.
        /// </summary>
        public void TryLevelUp()
        {
            PetProgress progress = GetProgress();
            int pointsForCurrentLevel = progress.PointsPerStatForCurrentLevel;

            if (progress.PhysicalPoints >= pointsForCurrentLevel &&
                progress.IntellectualPoints >= pointsForCurrentLevel &&
                progress.MentalPoints >= pointsForCurrentLevel)
            {
                progress.Level++;
                progress.PhysicalPoints = 0;
                progress.IntellectualPoints = 0;
                progress.MentalPoints = 0;
            }
        }
    }
}
