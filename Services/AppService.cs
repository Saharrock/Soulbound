using System.Globalization;
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
        public const int WeeklyStaminaCap = 100;
        private const int OverduePenaltyPoints = 5;
        private const int LateDeletePenaltyPoints = 3;
        private const int PrecisionBonusOnTimelyCompletion = 2;
        private const int PrecisionPenaltyOverdueFail = -5;
        private const int PrecisionPenaltyLateGiveUp = -1;
        private static readonly IReadOnlyList<QuickStartPackDefinition> quickStartPacks = new List<QuickStartPackDefinition>
        {
            new QuickStartPackDefinition
            {
                Id = "intellect",
                Title = "Intellect Pack",
                Description = "Focused tasks for learning and knowledge growth.",
                IsPhysical = false,
                IsIntellectual = true,
                IsMental = false,
                Tasks = new List<QuickStartTaskDefinition>
                {
                    new QuickStartTaskDefinition { Title = "Reading (30 min)", HoursFromNow = 24, XpGain = 20 },
                    new QuickStartTaskDefinition { Title = "Learn something new (IT)", HoursFromNow = 48, XpGain = 25 }
                }
            },
            new QuickStartPackDefinition
            {
                Id = "physical",
                Title = "Physical Pack",
                Description = "Light physical activity to keep consistent momentum.",
                IsPhysical = true,
                IsIntellectual = false,
                IsMental = false,
                Tasks = new List<QuickStartTaskDefinition>
                {
                    new QuickStartTaskDefinition { Title = "Morning workout", HoursFromNow = 12, XpGain = 15 },
                    new QuickStartTaskDefinition { Title = "Walk outdoors", HoursFromNow = 24, XpGain = 10 }
                }
            }
        };

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

        public int GoalCompletionStaminaCost => StaminaPerGoalCompletion;

        public static int MaxStaminaCostPerGoalPublic => Goal.MaxStaminaCostPerGoal;

        public IReadOnlyList<QuickStartPackDefinition> GetQuickStartPacks()
        {
            return quickStartPacks;
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

        /// <summary>Active goals scheduled for today (includes already-marked).</summary>
        public List<Goal> GetTodayGoals()
        {
            DayOfWeek today = DateTime.Today.DayOfWeek;
            return GetActiveGoals().Where(goal => IsGoalScheduledForDay(goal, today)).ToList();
        }

        /// <summary>
        /// Active goals scheduled for today without a logged workout session for this calendar day.
        /// </summary>
        public List<Goal> GetTodayGoalsAwaitingWorkout()
        {
            string todayIso = LocalDateIso(DateTime.Today);
            DayOfWeek dow = DateTime.Today.DayOfWeek;

            List<Goal> list = GetActiveGoals()
                .Where(goal => IsGoalScheduledForDay(goal, dow))
                .Where(goal => !WorkoutRecordedOnDate(goal.Id, todayIso))
                .ToList();

            return list;
        }

        public async Task<bool> RecordWorkoutForTodayAsync(Goal goal)
        {
            await EnsureWeeklyPeriodAsync();

            Goal? persisted = gameData.Goals.FirstOrDefault(g => g.Id == goal.Id);
            if (goal.IsCompleted || persisted == null || persisted.IsCompleted)
            {
                return false;
            }

            if (!IsGoalScheduledForDay(goal, DateTime.Today.DayOfWeek))
            {
                return false;
            }

            string todayIso = LocalDateIso(DateTime.Today);

            if (WorkoutRecordedOnDate(goal.Id, todayIso))
            {
                return false;
            }

            int staminaCost = goal.ResolvedStaminaCost;

            if (gameData.Character.Stamina < staminaCost)
            {
                return false;
            }

            gameData.Character.Stamina = Math.Max(0, gameData.Character.Stamina - staminaCost);

            gameData.WorkoutSessions.Add(new WorkoutSession
            {
                GoalId = goal.Id,
                SessionDateIso = todayIso
            });

            gameData.History.Insert(0, new HistoryRecord
            {
                TaskName = goal.Title,
                Category = GetCategoryLabel(goal),
                ResultStatus = "Workout",
                XpChange = 0,
                StaminaSpent = staminaCost,
                DateFinished = DateTime.Now
            });

            await SaveGameDataAsync();
            return true;
        }

        public async Task UpdatePetSelectionAsync(string petImage, string petName)
        {
            gameData.Character.SelectedPetImage = string.IsNullOrWhiteSpace(petImage) ? "dotnet_bot.png" : petImage;
            gameData.Character.PetName = string.IsNullOrWhiteSpace(petName) ? "Pet" : petName.Trim();
            await SaveGameDataAsync();
        }

        public async Task EnsureDailyStaminaAsync()
        {
            await EnsureWeeklyPeriodAsync();
        }

        public async Task<bool> AddGoalAsync(Goal goal)
        {
            if (!goal.IsPhysical && !goal.IsIntellectual && !goal.IsMental)
            {
                return false;
            }

            gameData.LastGoalId++;
            goal.Id = gameData.LastGoalId.ToString();
            goal.IsCompleted = false;
            goal.Deadline = goal.EndDate;
            goal.StaminaCost = NormalizeStaminaCostForStorage(goal.StaminaCost);
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
            if (goalToDelete.IsCompleted)
            {
                return false;
            }

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
                AdjustPrecision(PrecisionPenaltyLateGiveUp);
            }

            RemoveWorkoutsForGoal(goalToDelete.Id);
            gameData.Goals.Remove(goalToDelete);
            await SaveGameDataAsync();
            return true;
        }

        public async Task<bool> MarkGoalAsCompletedAsync(Goal goalToComplete)
        {
            await EnsureWeeklyPeriodAsync();
            int staminaCost = goalToComplete.ResolvedStaminaCost;
            if (goalToComplete.IsCompleted || gameData.Character.Stamina < staminaCost)
            {
                return false;
            }

            goalToComplete.IsCompleted = true;
            gameData.Character.Stamina = Math.Max(0, gameData.Character.Stamina - staminaCost);
            AddProgressFromGoal(goalToComplete);
            AddWeeklyProgressFromGoal(goalToComplete);

            gameData.History.Insert(0, new HistoryRecord
            {
                TaskName = goalToComplete.Title,
                Category = GetCategoryLabel(goalToComplete),
                ResultStatus = "Completed",
                XpChange = goalToComplete.ProgressPoints,
                StaminaSpent = staminaCost,
                DateFinished = DateTime.Now
            });

            TryLevelUp();

            RemoveWorkoutsForGoal(goalToComplete.Id);

            gameData.Character.CompletedGoalsLifetime = gameData.Goals.Count(static g => g.IsCompleted);

            DateTime completionDate = DateTime.Now.Date;
            if (completionDate <= goalToComplete.Deadline.Date)
            {
                AdjustPrecision(PrecisionBonusOnTimelyCompletion);
            }

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

                AdjustPrecision(PrecisionPenaltyOverdueFail);
            }

            await SaveGameDataAsync();
        }

        public async Task AddQuickPackAsync(string packId)
        {
            QuickStartPackDefinition? pack = quickStartPacks.FirstOrDefault(item => item.Id == packId);
            if (pack == null)
            {
                return;
            }

            foreach (QuickStartTaskDefinition task in pack.Tasks)
            {
                Goal goal = CreateQuickStartGoal(task, pack.IsPhysical, pack.IsIntellectual, pack.IsMental);
                await AddGoalAsync(goal);
            }
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
            gameData.WorkoutSessions ??= new List<WorkoutSession>();
            gameData.Character ??= new PetProgress();
            if (string.IsNullOrWhiteSpace(gameData.Character.SelectedPetImage))
            {
                gameData.Character.SelectedPetImage = "dotnet_bot.png";
            }

            ApplyCharacterMigration(gameData);
        }

        private static void ApplyCharacterMigration(GameData data)
        {
            PetProgress c = data.Character;

            int completedGoals = data.Goals.Count(g => g.IsCompleted);
            c.CompletedGoalsLifetime = completedGoals;

            if (!c.PrecisionSeeded && c.CompletedGoalsLifetime == 0 && data.History.Count == 0)
            {
                c.PrecisionScore = 65;
            }
            else if (!c.PrecisionSeeded && c.PrecisionScore < 1)
            {
                c.PrecisionScore = 55;
            }

            if (!c.PrecisionSeeded)
            {
                c.PrecisionScore = Math.Clamp(c.PrecisionScore, 0, 100);
                c.PrecisionSeeded = true;
            }
            else
            {
                c.PrecisionScore = Math.Clamp(c.PrecisionScore, 0, 100);
            }
        }

        private static GameData CreateDefaultGameData()
        {
            return new GameData
            {
                Goals = new List<Goal>(),
                History = new List<HistoryRecord>(),
                WorkoutSessions = new List<WorkoutSession>(),
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

        private bool WorkoutRecordedOnDate(string goalId, string dateIso)
        {
            return gameData.WorkoutSessions.Exists(
                session => session.GoalId == goalId && session.SessionDateIso == dateIso);
        }

        private void RemoveWorkoutsForGoal(string goalId)
        {
            gameData.WorkoutSessions.RemoveAll(session => session.GoalId == goalId);
        }

        private static string LocalDateIso(DateTime localDate)
        {
            return localDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
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

        private static Goal CreateQuickStartGoal(QuickStartTaskDefinition task, bool isPhysical, bool isIntellectual, bool isMental)
        {
            DateTime now = DateTime.Now;
            DateTime deadline = now.AddHours(task.HoursFromNow);
            return new Goal
            {
                Title = task.Title,
                Description = "Quick start",
                CreatedAt = now,
                EndDate = deadline,
                Deadline = deadline,
                GoalTime = task.HoursFromNow,
                CustomProgressPoints = task.XpGain,
                StaminaCost = StaminaPerGoalCompletion,
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

        private static int NormalizeStaminaCostForStorage(int raw)
        {
            return raw < 1 ? StaminaPerGoalCompletion : Math.Clamp(raw, 1, Goal.MaxStaminaCostPerGoal);
        }

        private void AdjustPrecision(int delta)
        {
            gameData.Character.PrecisionScore =
                Math.Clamp(gameData.Character.PrecisionScore + delta, 0, 100);
        }

        private async Task EnsureWeeklyPeriodAsync()
        {
            string key = GetWeeklyPeriodKey(DateTime.Today);
            PetProgress character = gameData.Character;
            bool changed = false;
            if (string.IsNullOrEmpty(character.WeeklyPeriodKey))
            {
                character.WeeklyPeriodKey = key;
                character.Stamina = WeeklyStaminaCap;
                changed = true;
            }
            else if (character.WeeklyPeriodKey != key)
            {
                character.WeeklyPeriodKey = key;
                character.WeeklyPhysicalPoints = 0;
                character.WeeklyIntellectualPoints = 0;
                character.WeeklyMentalPoints = 0;
                character.Stamina = WeeklyStaminaCap;
                changed = true;
            }

            if (changed)
            {
                await SaveGameDataAsync();
            }
        }

        private static string GetWeeklyPeriodKey(DateTime localDate)
        {
            int year = ISOWeek.GetYear(localDate);
            int week = ISOWeek.GetWeekOfYear(localDate);
            return $"{year}-W{week:D2}";
        }

        private void AddProgressFromGoal(Goal goal)
        {
            if (goal.IsPhysical) gameData.Character.PhysicalPoints += goal.ProgressPoints;
            if (goal.IsIntellectual) gameData.Character.IntellectualPoints += goal.ProgressPoints;
            if (goal.IsMental) gameData.Character.MentalPoints += goal.ProgressPoints;
        }

        private void AddWeeklyProgressFromGoal(Goal goal)
        {
            if (goal.IsPhysical) gameData.Character.WeeklyPhysicalPoints += goal.ProgressPoints;
            if (goal.IsIntellectual) gameData.Character.WeeklyIntellectualPoints += goal.ProgressPoints;
            if (goal.IsMental) gameData.Character.WeeklyMentalPoints += goal.ProgressPoints;
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
            public List<WorkoutSession> WorkoutSessions { get; set; } = new();
            public PetProgress Character { get; set; } = new();
            public int LastGoalId { get; set; }
        }
    }
}
