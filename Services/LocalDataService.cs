using Soulbound.Models;

namespace Soulbound.Services
{
    class LocalDataService
    {
        private const int ProgressPerDay = 10;

        private static LocalDataService? instance;
        public static LocalDataService GetInstance()
        {
            instance ??= new LocalDataService();
            return instance;
        }

        private readonly List<Goal> goals = new();
        private readonly List<GoalPack> packs = new();
        private readonly PetProgress petProgress = new();
        private readonly List<PetOption> pets = new();
        private int selectedPetIndex;
        private int lastGoalId;

        private LocalDataService()
        {
            CreatePets();
            CreatePacks();
            CreateFakeData();
        }

        public List<Goal> GetGoals() => goals;
        public List<Goal> GetActiveGoals() => goals.Where(g => !g.IsCompleted).ToList();
        public List<Goal> GetFinishedGoals() => goals.Where(g => g.IsCompleted).ToList();
        public PetProgress GetPetProgress() => petProgress;
        public int GetCurrentPetIndex() => selectedPetIndex;

        public List<PetOption> GetPets() => pets;

        public PetOption MovePetLeft()
        {
            selectedPetIndex = (selectedPetIndex - 1 + pets.Count) % pets.Count;
            return pets[selectedPetIndex];
        }

        public PetOption MovePetRight()
        {
            selectedPetIndex = (selectedPetIndex + 1) % pets.Count;
            return pets[selectedPetIndex];
        }

        public PetOption GetCurrentPet() => pets[selectedPetIndex];

        public void ConfirmPetSelection(string petName)
        {
            var currentPet = pets[selectedPetIndex];
            petProgress.PetImage = currentPet.Image;
            petProgress.PetName = string.IsNullOrWhiteSpace(petName) ? currentPet.DefaultName : petName.Trim();
        }

        public List<GoalPack> GetGoalPacks() => packs;

        public async Task<bool> AddPackGoalsAsync(string packTitle)
        {
            var selectedPack = packs.FirstOrDefault(p => p.Title == packTitle);
            if (selectedPack == null)
            {
                return await Task.FromResult(false);
            }

            foreach (var packGoal in selectedPack.Goals)
            {
                var clonedGoal = new Goal
                {
                    Title = packGoal.Title,
                    Description = packGoal.Description,
                    Notes = packGoal.Notes,
                    EndDate = DateTime.Today.AddDays(7),
                    CreatedAt = DateTime.Now,
                    GoalTime = (int)TimeSpan.FromDays(7).TotalHours,
                    IsPhysical = packGoal.IsPhysical,
                    IsIntellectual = packGoal.IsIntellectual,
                    IsMental = packGoal.IsMental,
                    IsMonday = true,
                    IsTuesday = true,
                    IsWednesday = true,
                    IsThursday = true,
                    IsFriday = true
                };
                clonedGoal.ProgressPoints = CalculateGoalProgressByDays(clonedGoal);
                await AddGoalAsync(clonedGoal);
            }

            return await Task.FromResult(true);
        }

        public async Task<bool> AddGoalAsync(Goal goal)
        {
            lastGoalId++;
            goal.Id = lastGoalId.ToString();
            goal.IsCompleted = false;
            goal.ProgressPoints = CalculateGoalProgressByDays(goal);
            goals.Add(goal);
            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveGoalAsync(Goal goalToDelete)
        {
            goals.Remove(goalToDelete);
            return await Task.FromResult(true);
        }

        public async Task<bool> MarkGoalAsCompletedAsync(Goal goalToComplete)
        {
            if (goalToComplete.IsCompleted)
            {
                return await Task.FromResult(false);
            }

            goalToComplete.IsCompleted = true;

            if (goalToComplete.IsPhysical)
            {
                petProgress.PhysicalPoints += goalToComplete.ProgressPoints;
            }
            if (goalToComplete.IsIntellectual)
            {
                petProgress.IntellectualPoints += goalToComplete.ProgressPoints;
            }
            if (goalToComplete.IsMental)
            {
                petProgress.MentalPoints += goalToComplete.ProgressPoints;
            }

            TryLevelUp();
            return await Task.FromResult(true);
        }

        public List<Goal> GetTodayGoals()
        {
            var today = DateTime.Today.DayOfWeek;
            return goals.Where(g => !g.IsCompleted && IsGoalScheduledForDay(g, today)).ToList();
        }

        public List<ScheduleDayGroup> GetScheduleGroups()
        {
            var activeGoals = GetActiveGoals();
            return new List<ScheduleDayGroup>
            {
                new() { DayName = "Monday", Goals = activeGoals.Where(g => g.IsMonday).ToList() },
                new() { DayName = "Tuesday", Goals = activeGoals.Where(g => g.IsTuesday).ToList() },
                new() { DayName = "Wednesday", Goals = activeGoals.Where(g => g.IsWednesday).ToList() },
                new() { DayName = "Thursday", Goals = activeGoals.Where(g => g.IsThursday).ToList() },
                new() { DayName = "Friday", Goals = activeGoals.Where(g => g.IsFriday).ToList() },
                new() { DayName = "Saturday", Goals = activeGoals.Where(g => g.IsSaturday).ToList() },
                new() { DayName = "Sunday", Goals = activeGoals.Where(g => g.IsSunday).ToList() }
            };
        }

        private int CalculateGoalProgressByDays(Goal goal)
        {
            var days = Math.Max(1, (goal.EndDate.Date - goal.CreatedAt.Date).Days);
            return days * ProgressPerDay;
        }

        private void TryLevelUp()
        {
            var pointsForCurrentLevel = petProgress.PointsPerStatForCurrentLevel;
            if (petProgress.PhysicalPoints >= pointsForCurrentLevel &&
                petProgress.IntellectualPoints >= pointsForCurrentLevel &&
                petProgress.MentalPoints >= pointsForCurrentLevel)
            {
                petProgress.Level++;
                petProgress.PhysicalPoints = 0;
                petProgress.IntellectualPoints = 0;
                petProgress.MentalPoints = 0;
            }
        }

        private static bool IsGoalScheduledForDay(Goal goal, DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => goal.IsSunday,
                DayOfWeek.Monday => goal.IsMonday,
                DayOfWeek.Tuesday => goal.IsTuesday,
                DayOfWeek.Wednesday => goal.IsWednesday,
                DayOfWeek.Thursday => goal.IsThursday,
                DayOfWeek.Friday => goal.IsFriday,
                DayOfWeek.Saturday => goal.IsSaturday,
                _ => false
            };
        }

        private void CreatePets()
        {
            pets.Add(new PetOption { Image = "pet_fox.svg", DefaultName = "Fox" });
            pets.Add(new PetOption { Image = "pet_wolf.svg", DefaultName = "Wolf" });
            pets.Add(new PetOption { Image = "pet_rabbit.svg", DefaultName = "Rabbit" });
            pets.Add(new PetOption { Image = "pet_cat.svg", DefaultName = "Cat" });
            selectedPetIndex = 0;
        }

        private void CreatePacks()
        {
            packs.Add(new GoalPack
            {
                Title = "Physical",
                Goals = new List<Goal>
                {
                    new() { Title = "Morning run", Description = "Run for 20 minutes", Notes = "Keep stable pace", IsPhysical = true },
                    new() { Title = "Push-ups", Description = "Do 3 sets", Notes = "Track repetitions", IsPhysical = true }
                }
            });

            packs.Add(new GoalPack
            {
                Title = "Intellectual",
                Goals = new List<Goal>
                {
                    new() { Title = "Read a chapter", Description = "Read 20 pages", Notes = "Write 3 key ideas", IsIntellectual = true },
                    new() { Title = "Practice coding", Description = "Solve 1 task", Notes = "Focus on clean code", IsIntellectual = true }
                }
            });

            packs.Add(new GoalPack
            {
                Title = "Mental",
                Goals = new List<Goal>
                {
                    new() { Title = "Meditation", Description = "Meditate 10 minutes", Notes = "No distractions", IsMental = true },
                    new() { Title = "Journal", Description = "Write evening reflection", Notes = "Note main feeling", IsMental = true }
                }
            });
        }

        private void CreateFakeData()
        {
            goals.Add(new Goal
            {
                Id = "1",
                Title = "Morning Run",
                Description = "Run for 20 minutes",
                Notes = "Stay hydrated",
                IsPhysical = true,
                EndDate = DateTime.Today.AddDays(7),
                IsMonday = true,
                IsWednesday = true,
                IsFriday = true,
                ProgressPoints = 70
            });

            goals.Add(new Goal
            {
                Id = "2",
                Title = "Read a chapter",
                Description = "Read 20 pages of a book",
                Notes = "Write one takeaway",
                IsIntellectual = true,
                EndDate = DateTime.Today.AddDays(7),
                IsTuesday = true,
                IsThursday = true,
                IsSaturday = true,
                ProgressPoints = 70
            });

            lastGoalId = goals.Count;
        }
    }

    class PetOption
    {
        public string Image { get; set; } = string.Empty;
        public string DefaultName { get; set; } = string.Empty;
    }
}
