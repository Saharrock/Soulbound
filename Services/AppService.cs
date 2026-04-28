using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Database;
using Firebase.Database.Query;
using Soulbound.Models;

namespace Soulbound.Services
{
    internal sealed class AppService
    {
        private const int StaminaPerGoalCompletion = 15;
        private const int OverduePenaltyPoints = 5;
        private const int LateDeletePenaltyPoints = 3;

        private static AppService? instance;
        private FirebaseAuthClient? auth;
        private FirebaseClient? client;
        private GameData gameData = new();

        public AuthCredential? LoginAuthUser { get; private set; }
        public AuthUser? FullDetailsLoggedInUser { get; private set; }

        private readonly string[] petImages = new[]
        {
            "dotnet_bot.png",
            "pet_egg.png",
            "pet_cat.png",
            "pet_dog.png"
        };

        public static AppService GetInstance()
        {
            instance ??= new AppService();
            return instance;
        }

        private AppService()
        {
            Init();
        }

        public void Init()
        {
            var config = new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyCawxBWp5-fLxVmoluIXkvHZaYG4rMdgOA",
                AuthDomain = "soulbound-cf78d.firebaseapp.com",
                Providers = new FirebaseAuthProvider[] { new EmailProvider() },
                UserRepository = new FileUserRepository("appUserData")
            };

            auth = new FirebaseAuthClient(config);
            client = new FirebaseClient(
                "https://soulbound-cf78d-default-rtdb.europe-west1.firebasedatabase.app/",
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(auth.User.Credential.IdToken)
                });
        }

        public async Task<bool> TryRegister(string userName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            EnsureInitialized();

            try
            {
                var response = await auth!.CreateUserWithEmailAndPasswordAsync(email, password);
                LoginAuthUser = response.AuthCredential;

                FullDetailsLoggedInUser = new AuthUser
                {
                    Email = response.User.Info.Email,
                    Id = response.User.Uid,
                    UserName = userName.Trim()
                };

                await client!
                    .Child("users")
                    .Child(FullDetailsLoggedInUser.Id)
                    .Child("profile")
                    .PutAsync(new { userName = FullDetailsLoggedInUser.UserName, email });

                gameData = CreateDefaultGameData();
                await SaveGameDataAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TryLogin(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            EnsureInitialized();

            try
            {
                var authUser = await auth!.SignInWithEmailAndPasswordAsync(email, password);
                LoginAuthUser = authUser.AuthCredential;

                string uid = auth.User.Uid;
                string name = await client!
                    .Child("users")
                    .Child(uid)
                    .Child("profile")
                    .Child("userName")
                    .OnceSingleAsync<string>() ?? email;

                FullDetailsLoggedInUser = new AuthUser
                {
                    Email = auth.User.Info.Email,
                    Id = uid,
                    UserName = name
                };

                await LoadGameDataAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Logout()
        {
            try
            {
                auth?.SignOut();
                LoginAuthUser = null;
                FullDetailsLoggedInUser = null;
                gameData = new GameData();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetUserFullName()
        {
            return FullDetailsLoggedInUser?.UserName ?? string.Empty;
        }

        public string[] GetPetImages()
        {
            return petImages;
        }

        public async Task EnsureGameDataLoadedAsync()
        {
            if (FullDetailsLoggedInUser == null)
            {
                gameData = CreateDefaultGameData();
                return;
            }

            if (gameData.Goals.Count == 0 && gameData.History.Count == 0 && gameData.LastGoalId == 0)
            {
                await LoadGameDataAsync();
            }
        }

        public PetProgress GetProgress()
        {
            return gameData.Character;
        }

        public List<Goal> GetActiveGoals()
        {
            return gameData.Goals.Where(g => !g.IsCompleted).ToList();
        }

        public List<Goal> GetFinishedGoals()
        {
            return gameData.Goals.Where(g => g.IsCompleted).ToList();
        }

        public List<HistoryRecord> GetHistoryRecords()
        {
            return gameData.History;
        }

        public List<Goal> GetTodayGoals()
        {
            DayOfWeek today = DateTime.Today.DayOfWeek;
            return GetActiveGoals().Where(goal => IsGoalScheduledForDay(goal, today)).ToList();
        }

        public async Task UpdatePetSelectionAsync(string petImage, string petName)
        {
            gameData.Character.SelectedPetImage = string.IsNullOrWhiteSpace(petImage) ? "dotnet_bot.png" : petImage;
            gameData.Character.PetName = string.IsNullOrWhiteSpace(petName) ? "Pet" : petName.Trim();
            await SaveGameDataAsync();
        }

        public async Task EnsureDailyStaminaAsync()
        {
            if (gameData.Character.LastLoginDate.Date != DateTime.Today)
            {
                gameData.Character.Stamina = 100;
                gameData.Character.LastLoginDate = DateTime.Today;
                await SaveGameDataAsync();
            }
        }

        public async Task<bool> AddGoalAsync(Goal goal)
        {
            gameData.LastGoalId++;
            goal.Id = gameData.LastGoalId.ToString();
            goal.IsCompleted = false;
            goal.Deadline = goal.EndDate;
            if (goal.CustomProgressPoints.HasValue)
            {
                goal.ProgressPoints = goal.CustomProgressPoints.Value;
            }
            else
            {
                int days = Math.Max(1, (goal.Deadline.Date - goal.CreatedAt.Date).Days);
                goal.ProgressPoints = days * 10;
            }

            gameData.Goals.Add(goal);
            await SaveGameDataAsync();
            return true;
        }

        public async Task<bool> RemoveGoalAsync(Goal goalToDelete)
        {
            int penaltyPoints = TryApplyLateDeletePenalty(goalToDelete);
            if (penaltyPoints > 0)
            {
                gameData.History.Insert(0, new HistoryRecord
                {
                    TaskName = goalToDelete.Title,
                    Category = GetCategoryLabel(goalToDelete),
                    ResultStatus = "Penalty",
                    XpChange = -penaltyPoints,
                    StaminaSpent = 0,
                    DateFinished = DateTime.Now
                });
            }

            gameData.Goals.Remove(goalToDelete);
            await SaveGameDataAsync();
            return true;
        }

        public async Task<bool> MarkGoalAsCompletedAsync(Goal goalToComplete)
        {
            if (goalToComplete.IsCompleted || gameData.Character.Stamina < 10)
            {
                return false;
            }

            goalToComplete.IsCompleted = true;
            gameData.Character.Stamina = Math.Max(0, gameData.Character.Stamina - StaminaPerGoalCompletion);
            AddProgressFromGoal(goalToComplete);

            gameData.History.Insert(0, new HistoryRecord
            {
                TaskName = goalToComplete.Title,
                Category = GetCategoryLabel(goalToComplete),
                ResultStatus = "Completed",
                XpChange = goalToComplete.ProgressPoints,
                StaminaSpent = StaminaPerGoalCompletion,
                DateFinished = DateTime.Now
            });

            TryLevelUp();
            await SaveGameDataAsync();
            return true;
        }

        public async Task ApplyDeadlinePenaltiesAsync()
        {
            foreach (Goal goal in gameData.Goals)
            {
                if (goal.IsCompleted || goal.IsOverduePenaltyApplied || DateTime.Now <= goal.Deadline)
                {
                    continue;
                }

                ApplyPenaltyByGoalCategories(goal, OverduePenaltyPoints);
                goal.IsOverduePenaltyApplied = true;

                gameData.History.Insert(0, new HistoryRecord
                {
                    TaskName = goal.Title,
                    Category = GetCategoryLabel(goal),
                    ResultStatus = "Failed",
                    XpChange = -OverduePenaltyPoints,
                    StaminaSpent = 0,
                    DateFinished = DateTime.Now
                });
            }

            await SaveGameDataAsync();
        }

        public async Task AddQuickIntellectPackAsync()
        {
            await AddGoalAsync(CreateQuickStartGoal("Reading (30 min)", false, true, false, 24, 20));
            await AddGoalAsync(CreateQuickStartGoal("Learn something new (IT)", false, true, false, 48, 25));
        }

        public async Task AddQuickPhysicalPackAsync()
        {
            await AddGoalAsync(CreateQuickStartGoal("Morning workout", true, false, false, 12, 15));
            await AddGoalAsync(CreateQuickStartGoal("Walk outdoors", true, false, false, 24, 10));
        }

        public async Task SaveGameDataAsync()
        {
            if (FullDetailsLoggedInUser == null)
            {
                return;
            }

            EnsureInitialized();

            await client!
                .Child("users")
                .Child(FullDetailsLoggedInUser.Id)
                .Child("gameData")
                .PutAsync(gameData);
        }

        public async Task LoadGameDataAsync()
        {
            if (FullDetailsLoggedInUser == null)
            {
                gameData = CreateDefaultGameData();
                return;
            }

            EnsureInitialized();

            GameData? cloudData = await client!
                .Child("users")
                .Child(FullDetailsLoggedInUser.Id)
                .Child("gameData")
                .OnceSingleAsync<GameData>();

            gameData = cloudData ?? CreateDefaultGameData();
            gameData.Goals ??= new List<Goal>();
            gameData.History ??= new List<HistoryRecord>();
            gameData.Character ??= new PetProgress();
            if (string.IsNullOrWhiteSpace(gameData.Character.SelectedPetImage))
            {
                gameData.Character.SelectedPetImage = "dotnet_bot.png";
            }
        }

        private static GameData CreateDefaultGameData()
        {
            return new GameData
            {
                Goals = new List<Goal>(),
                History = new List<HistoryRecord>(),
                Character = new PetProgress(),
                LastGoalId = 0
            };
        }

        private void EnsureInitialized()
        {
            if (auth == null || client == null)
            {
                Init();
            }
        }

        private static string GetCategoryLabel(Goal goal)
        {
            if (goal.IsPhysical) return "Physical";
            if (goal.IsIntellectual) return "Intellectual";
            if (goal.IsMental) return "Mental";
            return "Other";
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

        private static Goal CreateQuickStartGoal(string title, bool isPhysical, bool isIntellectual, bool isMental, int hoursFromNow, int xp)
        {
            DateTime now = DateTime.Now;
            DateTime deadline = now.AddHours(hoursFromNow);
            return new Goal
            {
                Title = title,
                Description = "Quick start",
                Notes = string.Empty,
                CreatedAt = now,
                EndDate = deadline,
                Deadline = deadline,
                GoalTime = hoursFromNow,
                CustomProgressPoints = xp,
                IsPhysical = isPhysical,
                IsIntellectual = isIntellectual,
                IsMental = isMental,
                IsSunday = true,
                IsMonday = true,
                IsTuesday = true,
                IsWednesday = true,
                IsThursday = true,
                IsFriday = true,
                IsSaturday = true
            };
        }

        private int TryApplyLateDeletePenalty(Goal goalToDelete)
        {
            if (goalToDelete.IsCompleted)
            {
                return 0;
            }

            TimeSpan totalLifetime = goalToDelete.Deadline - goalToDelete.CreatedAt;
            if (totalLifetime.TotalMinutes <= 0)
            {
                return 0;
            }

            TimeSpan passedLifetime = DateTime.Now - goalToDelete.CreatedAt;
            if (passedLifetime <= TimeSpan.FromTicks(totalLifetime.Ticks / 2))
            {
                return 0;
            }

            ApplyPenaltyByGoalCategories(goalToDelete, LateDeletePenaltyPoints);
            return LateDeletePenaltyPoints;
        }

        private void AddProgressFromGoal(Goal goal)
        {
            if (goal.IsPhysical) gameData.Character.PhysicalPoints += goal.ProgressPoints;
            if (goal.IsIntellectual) gameData.Character.IntellectualPoints += goal.ProgressPoints;
            if (goal.IsMental) gameData.Character.MentalPoints += goal.ProgressPoints;
        }

        private void ApplyPenaltyByGoalCategories(Goal goal, int penaltyPoints)
        {
            if (goal.IsPhysical) gameData.Character.PhysicalPoints = Math.Max(0, gameData.Character.PhysicalPoints - penaltyPoints);
            if (goal.IsIntellectual) gameData.Character.IntellectualPoints = Math.Max(0, gameData.Character.IntellectualPoints - penaltyPoints);
            if (goal.IsMental) gameData.Character.MentalPoints = Math.Max(0, gameData.Character.MentalPoints - penaltyPoints);
        }

        private void TryLevelUp()
        {
            int points = gameData.Character.PointsPerStatForCurrentLevel;
            if (gameData.Character.PhysicalPoints >= points &&
                gameData.Character.IntellectualPoints >= points &&
                gameData.Character.MentalPoints >= points)
            {
                gameData.Character.Level++;
                gameData.Character.PhysicalPoints = 0;
                gameData.Character.IntellectualPoints = 0;
                gameData.Character.MentalPoints = 0;
            }
        }

        private class GameData
        {
            public List<Goal> Goals { get; set; } = new();
            public List<HistoryRecord> History { get; set; } = new();
            public PetProgress Character { get; set; } = new();
            public int LastGoalId { get; set; }
        }
    }
}
