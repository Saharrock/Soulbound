

using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.Maui.Storage;
using Soulbound.Models;

namespace Soulbound.Services
{
    // Единый сервис приложения (Singleton). Хранит gameData в RAM,
    // синхронизирует с Firebase Auth + Realtime Database, содержит всю бизнес-логику.
    // ViewModels обращаются только сюда.
    internal sealed class AppService
    {
        private const int StaminaPerGoalCompletion = 10;
        public const int WeeklyStaminaCap = 100;
        public const string DefaultPetImage = "cat.png";
        // Подпапка в AppData для локальных фото целей
        private const string GoalPhotosFolderName = "goal_photos";

        private static AppService? instance;
        private FirebaseAuthClient? auth;
        private FirebaseClient? client;
        // Вся игровая информация текущего пользователя в памяти
        private GameData gameData = new();

        public AuthCredential? LoginAuthUser { get; private set; }
        public AuthUser? FullDetailsLoggedInUser { get; private set; }

        // Доступные аватары питомца на карусели
        private readonly string[] petImages =
        {
            "cat.png",
            "fox.png",
            "rabbit.png",
            "wolf.png"
        };

        // Lazy Singleton — один AppService на всё приложение.
        public static AppService GetInstance()
        {
            instance ??= new AppService();
            return instance;
        }

        private AppService()
        {
            Init();
        }

        // === Firebase ===

        // Подключение Firebase Auth (email) и Realtime Database.
        // DB-клиент передаёт IdToken пользователя в каждый запрос.
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

        // === Auth ===

        // Регистрация: Firebase Auth → profile в users/{uid}/profile → пустой gameData → save.
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

        // Вход: sign-in → загрузка userName из profile → LoadGameDataAsync.
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

        // Выход: SignOut, очистка пользователя и gameData в памяти (облако не трогаем).
        public bool Logout()
        {
            try
            {
                auth?.SignOut();
                LoginAuthUser = null;
                FullDetailsLoggedInUser = null;
                gameData = CreateDefaultGameData();
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

        // Список имён файлов питомцев для PetSelectionPage.
        public string[] GetPetImages()
        {
            return petImages;
        }

        public int GoalCompletionStaminaCost => StaminaPerGoalCompletion;

        public static int MaxStaminaCostPerGoalPublic => Goal.MaxStaminaCostPerGoal;

        // === Чтение целей / истории ===

        // Цели одной категории (Physical/Intellectual/Mental) для popup Statistics.
        // Пересчитывает counters, сортирует: активные первыми, по названию.
        public List<Goal> GetGoalsForLifetimeCategory(string category)
        {
            static bool Matches(Goal g, string c) => c switch
            {
                "Physical" => g.IsPhysical,
                "Intellectual" => g.IsIntellectual,
                "Mental" => g.IsMental,
                _ => false
            };

            List<Goal> goals = gameData.Goals.Where(g => Matches(g, category)).ToList();
            foreach (Goal goal in goals)
            {
                UpdateWorkoutCounters(goal);
            }

            return goals
                .OrderBy(g => g.IsCompleted ? 1 : 0)
                .ThenBy(g => g.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // Lazy load gameData из Firebase + проверка недельного сброса stamina.
        // Вызывается при открытии экранов.
        public async Task EnsureGameDataLoadedAsync() //вызывается каждый раз при открытии экрана. "Скачались ли данные из инета?"
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

            await EnsureDailyStaminaAsync(); //не наступило ли воскресенье, чтобы обновить энергию
        }

        // PetProgress (питомец, stamina, onboarding).
        public PetProgress GetProgress()
        {
            return gameData.Character;
        }

        // Незавершённые цели с актуальными WorkoutStats и Schedule %.
        public List<Goal> GetActiveGoals()
        {
            List<Goal> goals = gameData.Goals.Where(g => !g.IsCompleted).ToList();
            foreach (Goal goal in goals)
            {
                UpdateWorkoutCounters(goal);
            }

            return goals;
        }

        // Текст для Main Room: средний Schedule % по активным целям.
        public string SummarizeAverageScheduleAdherence(IReadOnlyList<Goal> activeGoals) //Красивый текст для экрана 
        {
            if (activeGoals == null || activeGoals.Count == 0)
            {
                return string.Empty;
            }

            int average = GetAverageScheduleAdherencePercent(activeGoals);
            return $"Schedule adherence (avg): {average}%";
        }

        // Средний Schedule % по списку целей (целое число).
        public int GetAverageScheduleAdherencePercent(IReadOnlyList<Goal>? goals) //Среднее арифметическое
        {
            if (goals == null || goals.Count == 0)
            {
                return 0;
            }

            int total = 0;
            foreach (Goal goal in goals)
            {
                total += ComputeScheduleAdherencePercent(goal);
            }

            return total / goals.Count;
        }

        // Schedule % одной цели: CompletedWorkouts / запланированных слотов до сегодня (или deadline).
        private static int ComputeScheduleAdherencePercent(Goal goal) //Успеваемость по одной цели
        {
            // Горизонт — сегодня или deadline, если он уже прошёл
            DateTime horizon = DateTime.Today < goal.Deadline.Date ? DateTime.Today : goal.Deadline.Date;
            int sessionsScheduledToDate = CountScheduledWorkoutDays(goal, goal.CreatedAt.Date, horizon);
            if (sessionsScheduledToDate < 1)
            {
                return 100;
            }

            int value = (int)Math.Round(100.0 * goal.CompletedWorkouts / sessionsScheduledToDate);
            return Math.Clamp(value, 0, 100);
        }

        // Завершённые цели (Done).
        public List<Goal> GetFinishedGoals()
        {
            List<Goal> goals = gameData.Goals.Where(g => g.IsCompleted).ToList();
            foreach (Goal goal in goals)
            {
                UpdateWorkoutCounters(goal);
            }

            return goals;
        }

        // Журнал для Statistics timeline.
        public List<HistoryRecord> GetHistoryRecords()
        {
            return gameData.History;
        }



        // === Workouts ===

        // Активные цели, у которых сегодня тренировочный день и workout ещё не отмечен.
        public List<Goal> FilterGoalsAwaitingWorkoutToday(IReadOnlyList<Goal> activeGoals) 
        {
            string todayIso = LocalDateIso(DateTime.Today);
            DayOfWeek dow = DateTime.Today.DayOfWeek;

            List<Goal> result = new();
            foreach (Goal goal in activeGoals)
            {
                if (IsGoalScheduledForDay(goal, dow) && !WorkoutRecordedOnDate(goal.Id, todayIso))
                {
                    result.Add(goal);
                }
            }

            return result;
        }

        public List<Goal> GetTodayGoalsAwaitingWorkout()
        {
            return FilterGoalsAwaitingWorkoutToday(GetActiveGoals());
        }

        public async Task<bool> RecordWorkoutForTodayAsync(Goal goal) //Я сделал тренировку
        {
            await EnsureWeeklyPeriodAsync();

            Goal? persisted = gameData.Goals.FirstOrDefault(g => g.Id == goal.Id);
            if (goal.IsCompleted || persisted == null || persisted.IsCompleted) //Жива ли цель
            {
                return false;
            }

            if (!IsGoalScheduledForDay(goal, DateTime.Today.DayOfWeek)) //Правильный ли день
            {
                return false;
            }

            string todayIso = LocalDateIso(DateTime.Today);

            if (WorkoutRecordedOnDate(goal.Id, todayIso)) //Защита от случайного дабл-клика
            {
                return false;
            }

            int staminaCost = goal.ResolvedStaminaCost; 

            if (gameData.Character.Stamina < staminaCost) //Хватает ли энергии
            {
                return false;
            }

            gameData.Character.Stamina = Math.Max(0, gameData.Character.Stamina - staminaCost); //отнятие стамины

            gameData.WorkoutSessions.Add(new WorkoutSession
            {
                GoalId = goal.Id,
                SessionDateIso = todayIso 
            }); //сохранение тренировки


            // Запись в журнал для Statistics
            gameData.History.Insert(0, new HistoryRecord
            {
                TaskName = goal.Title,
                Category = GetCategoryLabel(goal),
                ResultStatus = HistoryRecord.StatusWorkout,
                TotalStaminaInvested = 0,
                StaminaSpent = staminaCost,
                DateFinished = DateTime.Now
            });

            persisted.TotalStaminaSpentAcrossGoal += staminaCost;
            UpdateWorkoutCounters(persisted);

            await SaveGameDataAsync();
            return true;
        }

        // === Питомец ===

        // Сохранить выбор питомца и завершить onboarding.
        public async Task UpdatePetSelectionAsync(string petImage, string petName)
        {
            gameData.Character.SelectedPetImage = string.IsNullOrWhiteSpace(petImage) ? DefaultPetImage : petImage;
            gameData.Character.PetName = string.IsNullOrWhiteSpace(petName) ? "Pet" : petName.Trim();
            gameData.Character.PetOnboardingComplete = true; //выбрал питомца
            await SaveGameDataAsync();
        }

        public bool HasCompletedPetOnboarding() //выбирал ли пиомца (Проверка при входе в приложение)
        {
            return gameData.Character.PetOnboardingComplete;
        }

        // Проверка и сброс stamina по воскресеньям (alias).
        public async Task EnsureDailyStaminaAsync()
        {
            await EnsureWeeklyPeriodAsync();
        }

        // === CRUD целей ===

        // Создать цель: auto-id, PlannedWorkouts, counters, save в Firebase.
        public async Task<bool> AddGoalAsync(Goal goal)
        {
            if (!goal.IsPhysical && !goal.IsIntellectual && !goal.IsMental)
            {
                return false;
            }

            gameData.LastGoalId++;
            goal.Id = gameData.LastGoalId.ToString();
            goal.IsCompleted = false;
            goal.Deadline = goal.EndDate.Date;
            goal.EndDate = goal.Deadline;
            goal.StaminaCost = NormalizeStaminaCostForStorage(goal.StaminaCost);
            goal.PlannedWorkouts = Math.Max(1, CountScheduledWorkoutDays(goal, goal.CreatedAt.Date, goal.Deadline.Date));
            goal.CompletedWorkouts = 0;
            goal.MissedWorkouts = 0;
            goal.TotalStaminaSpentAcrossGoal = 0;

            UpdateWorkoutCounters(goal);
            gameData.Goals.Add(goal);
            await SaveGameDataAsync();
            return true;
        }

        // Удалить только активную цель: workouts, локальные фото, запись в Goals.
        public async Task<bool> RemoveGoalAsync(Goal goalRef)
        {
            Goal? goal = gameData.Goals.FirstOrDefault(g => g.Id == goalRef.Id);
            if (goal == null || goal.IsCompleted)
            {
                return false;
            }

            RemoveWorkoutsForGoal(goal.Id);
            TryDeleteGoalPhotoDirectory(goal.Id);
            gameData.Goals.Remove(goal);
            await SaveGameDataAsync();
            return true;
        }

        // === Фото целей (локально + путь в Firebase) ===

        // Сохранить фото в AppData/goal_photos/{id}/, добавить GoalPhotoRef.
        public async Task<bool> AddGoalPhotoFromStreamAsync(Goal goalRef, Stream sourceStream, string fileExtension)
        {
            Goal? goal = gameData.Goals.FirstOrDefault(g => g.Id == goalRef.Id);
            if (goal == null || goal.IsCompleted)
            {
                return false;
            }

            string ext = string.IsNullOrWhiteSpace(fileExtension) ? ".jpg" : fileExtension.Trim();
            if (!ext.StartsWith('.'))
            {
                ext = "." + ext;
            }

            ext = SanitizeFileExtension(ext);

            goal.AttachedPhotos ??= new ObservableCollection<GoalPhotoRef>();
            string dir = Path.Combine(FileSystem.AppDataDirectory, GoalPhotosFolderName, goal.Id);
            Directory.CreateDirectory(dir);

            string fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{ext}";
            string fullPath = Path.Combine(dir, fileName);

            await using (FileStream fs = File.Create(fullPath))
            {
                await sourceStream.CopyToAsync(fs);
            }

            string relative = Path.GetRelativePath(FileSystem.AppDataDirectory, fullPath);
            var photoRef = new GoalPhotoRef { RelativePath = relative, Owner = goal };
            goal.AttachedPhotos.Add(photoRef);

            await SaveGameDataAsync();
            return true;
        }

        // Удалить ссылку и файл с диска.
        public async Task<bool> RemoveGoalPhotoAsync(GoalPhotoRef photo)
        {
            if (string.IsNullOrWhiteSpace(photo.RelativePath))
            {
                return false;
            }

            Goal? owner = photo.Owner ?? gameData.Goals.FirstOrDefault(g =>
                g.AttachedPhotos.Any(p => ReferenceEquals(p, photo) || p.RelativePath == photo.RelativePath));

            if (owner == null || owner.IsCompleted)
            {
                return false;
            }

            owner.AttachedPhotos ??= new ObservableCollection<GoalPhotoRef>();

            GoalPhotoRef? inList = owner.AttachedPhotos.FirstOrDefault(p =>
                ReferenceEquals(p, photo) ||
                string.Equals(p.RelativePath, photo.RelativePath, StringComparison.OrdinalIgnoreCase));
            if (inList == null)
            {
                return false;
            }

            owner.AttachedPhotos.Remove(inList);
            TryDeletePhotoFileIfExists(inList.RelativePath);
            await SaveGameDataAsync();
            return true;
        }

        // Done: −stamina, IsCompleted, History, удаление WorkoutSessions цели, save.
        public async Task<bool> MarkGoalAsCompletedAsync(Goal goalRef)
        {
            await EnsureWeeklyPeriodAsync();

            Goal? goal = gameData.Goals.FirstOrDefault(g => g.Id == goalRef.Id);
            if (goal == null || goal.IsCompleted)
            {
                return false;
            }

            int staminaCost = goal.ResolvedStaminaCost;
            if (gameData.Character.Stamina < staminaCost)
            {
                return false;
            }

            goal.IsCompleted = true;
            DateTime completionDate = DateTime.Today;
            goal.IsCompletedLate = completionDate > goal.Deadline.Date;

            gameData.Character.Stamina = Math.Max(0, gameData.Character.Stamina - staminaCost);
            goal.TotalStaminaSpentAcrossGoal += staminaCost;
            int staminaInvestedTotal = goal.TotalStaminaSpentAcrossGoal;

            gameData.History.Insert(0, new HistoryRecord
            {
                TaskName = goal.Title,
                Category = GetCategoryLabel(goal),
                ResultStatus = goal.IsCompletedLate ? HistoryRecord.StatusCompletedLate : HistoryRecord.StatusCompleted,
                TotalStaminaInvested = staminaInvestedTotal,
                StaminaSpent = staminaCost,
                DateFinished = DateTime.Now
            });

            // Sessions больше не нужны — цель закрыта
            RemoveWorkoutsForGoal(goal.Id);

            gameData.Character.CompletedGoalsLifetime = gameData.Goals.Count(static g => g.IsCompleted);

            await SaveGameDataAsync();
            return true;
        }

        // === Firebase sync ===

        // PUT всего gameData в users/{uid}/gameData.
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

        // GET gameData из Firebase + нормализация после загрузки.
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
                gameData.Character.SelectedPetImage = DefaultPetImage;
            }

            NormalizeLoadedGoals();
            NormalizeAttachedPhotosOwners();
            SyncCompletedGoalsLifetime();
        }

        // Пересчёт CompletedGoalsLifetime по числу Done-целей.
        private void SyncCompletedGoalsLifetime()
        {
            gameData.Character.CompletedGoalsLifetime = gameData.Goals.Count(static g => g.IsCompleted);
        }

        // Обрезает deadline/endDate до даты без времени у активных целей.
        private void NormalizeLoadedGoals()
        {
            foreach (Goal goal in gameData.Goals)
            {
                if (goal.IsCompleted)
                {
                    continue;
                }

                goal.Deadline = goal.Deadline.Date;
                goal.EndDate = goal.EndDate.Date;
            }
        }

        // После десериализации — связать GoalPhotoRef.Owner с целью для UI.
        private void NormalizeAttachedPhotosOwners()
        {
            foreach (Goal goal in gameData.Goals)
            {
                goal.AttachedPhotos ??= new ObservableCollection<GoalPhotoRef>();
                foreach (GoalPhotoRef photo in goal.AttachedPhotos)
                {
                    photo.Owner = goal;
                }
            }
        }

        // Удалить всю папку фото цели при RemoveGoalAsync.
        private void TryDeleteGoalPhotoDirectory(string goalId)
        {
            if (string.IsNullOrWhiteSpace(goalId))
            {
                return;
            }

            string dir = Path.Combine(FileSystem.AppDataDirectory, GoalPhotosFolderName, goalId);
            if (!Directory.Exists(dir))
            {
                return;
            }

            try
            {
                Directory.Delete(dir, recursive: true);
            }
            catch
            {
            }
        }

        // Удалить один файл фото; путь не выходит за пределы AppData.
        private void TryDeletePhotoFileIfExists(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            string full = Path.GetFullPath(Path.Combine(FileSystem.AppDataDirectory,
                relativePath.Replace('/', Path.DirectorySeparatorChar)));
            string root = Path.GetFullPath(FileSystem.AppDataDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
            {
                return;
            }

            try
            {
                File.Delete(full);
            }
            catch
            {
            }
        }

        // Безопасное расширение файла (.jpg по умолчанию).
        private static string SanitizeFileExtension(string ext)
        {
            if (string.IsNullOrEmpty(ext) || ext.Length < 2 || ext[0] != '.')
            {
                return ".jpg";
            }

            ReadOnlySpan<char> span = ext.AsSpan(1);
            foreach (char c in span)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    return ".jpg";
                }
            }

            return ext.Length > 12 ? ".jpg" : ext.ToLowerInvariant();
        }

        // Пустое состояние для нового пользователя или после Logout.
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

        // Если auth/client null — вызвать Init().
        private void EnsureInitialized()
        {
            if (auth == null || client == null)
            {
                Init();
            }
        }

        // Первая выбранная категория цели для History (Physical > Intellectual > Mental).
        private static string GetCategoryLabel(Goal goal)
        {
            if (goal.IsPhysical)
            {
                return "Physical";
            }

            if (goal.IsIntellectual)
            {
                return "Intellectual";
            }

            if (goal.IsMental)
            {
                return "Mental";
            }

            return "Other";
        }

        // Есть ли WorkoutSession на (goalId, дата).
        private bool WorkoutRecordedOnDate(string goalId, string dateIso)
        {
            return gameData.WorkoutSessions.Exists(
                session => session.GoalId == goalId && session.SessionDateIso == dateIso);
        }

        // === Schedule math ===

        // Пересчёт CompletedWorkouts, MissedWorkouts, ScheduleAdherenceLine для одной цели.
        private void UpdateWorkoutCounters(Goal goal)
        {
            if (goal.PlannedWorkouts < 1)
            {
                goal.PlannedWorkouts = Math.Max(1, CountScheduledWorkoutDays(goal, goal.CreatedAt.Date, goal.Deadline.Date));
            }

            goal.CompletedWorkouts = gameData.WorkoutSessions.Count(session => session.GoalId == goal.Id);

            DateTime untilDate = DateTime.Today;
            if (goal.Deadline.Date < untilDate)
            {
                untilDate = goal.Deadline.Date;
            }

            int expectedSoFar = CountScheduledWorkoutDays(goal, goal.CreatedAt.Date, untilDate);
            goal.MissedWorkouts = Math.Max(0, expectedSoFar - goal.CompletedWorkouts);
            goal.ScheduleAdherenceLine = FormatScheduleAdherenceLine(goal, expectedSoFar);
        }

        private static string FormatScheduleAdherenceLine(Goal goal, int sessionsScheduledToDate)
        {
            if (sessionsScheduledToDate < 1)
            {
                return "Schedule —";
            }

            int percent = (int)Math.Round(100.0 * goal.CompletedWorkouts / sessionsScheduledToDate);
            percent = Math.Clamp(percent, 0, 100);
            return $"Schedule {percent}%";
        }

        // Удалить все WorkoutSession цели (при Done или Remove).
        private void RemoveWorkoutsForGoal(string goalId)
        {
            gameData.WorkoutSessions.RemoveAll(session => session.GoalId == goalId);
        }

        private static string LocalDateIso(DateTime localDate)
        {
            return localDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        // Запланирована ли цель на этот день недели.
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

        // Сколько календарных дней между from и to включительно попадают в расписание цели.
        private static int CountScheduledWorkoutDays(Goal goal, DateTime fromDate, DateTime toDate)
        {
            DateTime start = fromDate.Date;
            DateTime end = toDate.Date;
            if (end < start)
            {
                return 0;
            }

            int count = 0;
            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                if (IsGoalScheduledForDay(goal, date.DayOfWeek))
                {
                    count++;
                }
            }

            return count;
        }

        private static int NormalizeStaminaCostForStorage(int raw)
        {
            return raw < 1 ? StaminaPerGoalCompletion : Math.Clamp(raw, 1, Goal.MaxStaminaCostPerGoal);
        }

        // === Stamina ===

        // Проверить воскресный сброс; при изменении — save.
        private async Task EnsureWeeklyPeriodAsync()
        {
            bool staminaChanged = ApplySundayMidnightStaminaResetIfNeeded(DateTime.Now);
            if (staminaChanged)
            {
                await SaveGameDataAsync();
            }
        }

        // Сброс Stamina до 100 в воскресенье 00:00 local. Возвращает true, если значение изменилось.
        private bool ApplySundayMidnightStaminaResetIfNeeded(DateTime nowLocal)
        {
            DateTime thisWeekSunday = GetSundayMidnightLocal(nowLocal);
            PetProgress character = gameData.Character;
            bool changed = false;

            if (character.LastSundayStaminaReset == default)
            {
                character.LastSundayStaminaReset = thisWeekSunday;
                character.Stamina = WeeklyStaminaCap;
                changed = true;
            }
            else if (thisWeekSunday > character.LastSundayStaminaReset.Date)
            {
                // Наступило новое воскресенье — новая неделя
                character.LastSundayStaminaReset = thisWeekSunday;
                character.Stamina = WeeklyStaminaCap;
                changed = true;
            }

            return changed;
        }

        // Полночь воскресенья той недели, в которой лежит referenceLocal.
        private static DateTime GetSundayMidnightLocal(DateTime referenceLocal)
        {
            DateTime day = referenceLocal.Date;
            int dowSundayIndexed = (int)day.DayOfWeek;
            return day.AddDays(-dowSundayIndexed);
        }

        // Агрегат всего игрового состояния — сериализуется в Firebase как gameData.
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
