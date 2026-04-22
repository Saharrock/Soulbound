using Soulbound.Models;

namespace Soulbound.Services
{
    /// <summary>
    /// Goals (tasks), schedule, history log, deadline penalties, and quick-start packs.
    /// Uses DatabaseService for persistence and CharacterService for stamina and XP.
    /// </summary>
    public sealed class TaskService
    {
        private const int ProgressPerDay = 10;

        private const int StaminaPerGoalCompletion = 15;

        private const int OverduePenaltyPoints = 5;

        private const int LateDeletePenaltyPoints = 3;

        private static TaskService? instance;

        private readonly DatabaseService database;

        private readonly CharacterService character;

        private readonly List<GoalPack> packs = new();

        public static TaskService GetInstance()
        {
            if (instance == null)
            {
                instance = new TaskService();
            }

            return instance;
        }

        private TaskService()
        {
            database = DatabaseService.GetInstance();
            character = CharacterService.GetInstance();
            CreatePacks();
        }

        public List<Goal> GetGoals()
        {
            return database.GetPersistedData().Goals;
        }

        public List<Goal> GetActiveGoals()
        {
            List<Goal> result = new List<Goal>();
            foreach (Goal g in database.GetPersistedData().Goals)
            {
                if (!g.IsCompleted)
                {
                    result.Add(g);
                }
            }

            return result;
        }

        public List<Goal> GetFinishedGoals()
        {
            List<Goal> result = new List<Goal>();
            foreach (Goal g in database.GetPersistedData().Goals)
            {
                if (g.IsCompleted)
                {
                    result.Add(g);
                }
            }

            return result;
        }

        public List<HistoryRecord> GetHistoryRecords()
        {
            return database.GetPersistedData().History;
        }

        public List<GoalPack> GetGoalPacks()
        {
            return packs;
        }

        /// <summary>
        /// Copies template goals from a named pack into the active goal list.
        /// </summary>
        public async Task<bool> AddPackGoalsAsync(string packTitle)
        {
            GoalPack? selectedPack = null;
            foreach (GoalPack p in packs)
            {
                if (p.Title == packTitle)
                {
                    selectedPack = p;
                    break;
                }
            }

            if (selectedPack == null)
            {
                return await Task.FromResult(false);
            }

            foreach (Goal packGoal in selectedPack.Goals)
            {
                Goal clonedGoal = new Goal
                {
                    Title = packGoal.Title,
                    Description = packGoal.Description,
                    Notes = packGoal.Notes,
                    EndDate = DateTime.Today.AddDays(7),
                    Deadline = DateTime.Today.AddDays(7),
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
            AppPersistedData data = database.GetPersistedData();
            data.LastGoalId++;
            goal.Id = data.LastGoalId.ToString();
            goal.IsCompleted = false;
            goal.Deadline = goal.EndDate;

            if (goal.CustomProgressPoints.HasValue)
            {
                goal.ProgressPoints = goal.CustomProgressPoints.Value;
            }
            else
            {
                goal.ProgressPoints = CalculateGoalProgressByDays(goal);
            }

            data.Goals.Add(goal);
            database.Save();
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Adds two short intellectual goals with fixed XP and deadlines.
        /// </summary>
        public async Task<bool> AddQuickIntellectPackAsync()
        {
            Goal goal1 = CreateQuickStartGoal("Reading (30 min)", false, true, false, 24, 20);
            Goal goal2 = CreateQuickStartGoal("Learn something new (IT)", false, true, false, 48, 25);
            await AddGoalAsync(goal1);
            await AddGoalAsync(goal2);
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Adds two short physical goals with fixed XP and deadlines.
        /// </summary>
        public async Task<bool> AddQuickPhysicalPackAsync()
        {
            Goal goal1 = CreateQuickStartGoal("Morning workout", true, false, false, 12, 15);
            Goal goal2 = CreateQuickStartGoal("Walk outdoors", true, false, false, 24, 10);
            await AddGoalAsync(goal1);
            await AddGoalAsync(goal2);
            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveGoalAsync(Goal goalToDelete)
        {
            int penaltyPoints = TryApplyLateDeletePenalty(goalToDelete);
            if (penaltyPoints > 0)
            {
                PrependHistoryRecord(new HistoryRecord
                {
                    TaskName = goalToDelete.Title,
                    Category = GetCategoryLabel(goalToDelete),
                    ResultStatus = TaskResultStatus.Penalty,
                    XpChange = -penaltyPoints,
                    StaminaSpent = 0,
                    DateFinished = DateTime.Now
                });
            }

            database.GetPersistedData().Goals.Remove(goalToDelete);
            database.Save();
            return await Task.FromResult(true);
        }

        public async Task<bool> MarkGoalAsCompletedAsync(Goal goalToComplete)
        {
            character.EnsureDailyStamina();

            if (goalToComplete.IsCompleted)
            {
                return await Task.FromResult(false);
            }

            if (character.GetProgress().Stamina < 10)
            {
                return await Task.FromResult(false);
            }

            goalToComplete.IsCompleted = true;
            character.SpendStaminaForTaskCompletion(StaminaPerGoalCompletion);
            character.AddProgressFromCompletedGoal(goalToComplete);

            PrependHistoryRecord(new HistoryRecord
            {
                TaskName = goalToComplete.Title,
                Category = GetCategoryLabel(goalToComplete),
                ResultStatus = TaskResultStatus.Completed,
                XpChange = goalToComplete.ProgressPoints,
                StaminaSpent = StaminaPerGoalCompletion,
                DateFinished = DateTime.Now
            });

            character.TryLevelUp();
            database.Save();
            return await Task.FromResult(true);
        }

        /// <summary>
        /// For each incomplete overdue goal, subtract XP once and write a history entry.
        /// </summary>
        public void ApplyDeadlinePenalties()
        {
            foreach (Goal goal in database.GetPersistedData().Goals)
            {
                if (goal.IsCompleted)
                {
                    continue;
                }

                if (goal.IsOverduePenaltyApplied)
                {
                    continue;
                }

                if (DateTime.Now > goal.Deadline)
                {
                    character.ApplyPenaltyByGoalCategories(goal, OverduePenaltyPoints);
                    goal.IsOverduePenaltyApplied = true;

                    PrependHistoryRecord(new HistoryRecord
                    {
                        TaskName = goal.Title,
                        Category = GetCategoryLabel(goal),
                        ResultStatus = TaskResultStatus.Failed,
                        XpChange = -OverduePenaltyPoints,
                        StaminaSpent = 0,
                        DateFinished = DateTime.Now
                    });
                }
            }

            database.Save();
        }

        public List<Goal> GetTodayGoals()
        {
            DayOfWeek today = DateTime.Today.DayOfWeek;
            List<Goal> result = new List<Goal>();

            foreach (Goal g in database.GetPersistedData().Goals)
            {
                if (!g.IsCompleted)
                {
                    if (IsGoalScheduledForDay(g, today))
                    {
                        result.Add(g);
                    }
                }
            }

            return result;
        }

        public List<ScheduleDayGroup> GetScheduleGroups()
        {
            List<Goal> activeGoals = GetActiveGoals();

            List<Goal> monday = new List<Goal>();
            List<Goal> tuesday = new List<Goal>();
            List<Goal> wednesday = new List<Goal>();
            List<Goal> thursday = new List<Goal>();
            List<Goal> friday = new List<Goal>();
            List<Goal> saturday = new List<Goal>();
            List<Goal> sunday = new List<Goal>();

            foreach (Goal g in activeGoals)
            {
                if (g.IsMonday)
                {
                    monday.Add(g);
                }

                if (g.IsTuesday)
                {
                    tuesday.Add(g);
                }

                if (g.IsWednesday)
                {
                    wednesday.Add(g);
                }

                if (g.IsThursday)
                {
                    thursday.Add(g);
                }

                if (g.IsFriday)
                {
                    friday.Add(g);
                }

                if (g.IsSaturday)
                {
                    saturday.Add(g);
                }

                if (g.IsSunday)
                {
                    sunday.Add(g);
                }
            }

            List<ScheduleDayGroup> groups = new List<ScheduleDayGroup>();
            groups.Add(new ScheduleDayGroup { DayName = "Monday", Goals = monday });
            groups.Add(new ScheduleDayGroup { DayName = "Tuesday", Goals = tuesday });
            groups.Add(new ScheduleDayGroup { DayName = "Wednesday", Goals = wednesday });
            groups.Add(new ScheduleDayGroup { DayName = "Thursday", Goals = thursday });
            groups.Add(new ScheduleDayGroup { DayName = "Friday", Goals = friday });
            groups.Add(new ScheduleDayGroup { DayName = "Saturday", Goals = saturday });
            groups.Add(new ScheduleDayGroup { DayName = "Sunday", Goals = sunday });
            return groups;
        }

        private int CalculateGoalProgressByDays(Goal goal)
        {
            int days = goal.Deadline.Date.Subtract(goal.CreatedAt.Date).Days;
            if (days < 1)
            {
                days = 1;
            }

            return days * ProgressPerDay;
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
            TimeSpan halfLifetime = TimeSpan.FromTicks(totalLifetime.Ticks / 2);

            if (passedLifetime > halfLifetime)
            {
                character.ApplyPenaltyByGoalCategories(goalToDelete, LateDeletePenaltyPoints);
                return LateDeletePenaltyPoints;
            }

            return 0;
        }

        private void PrependHistoryRecord(HistoryRecord record)
        {
            database.GetPersistedData().History.Insert(0, record);
        }

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

        private static Goal CreateQuickStartGoal(string title, bool isPhysical, bool isIntellectual, bool isMental, int hoursFromNow, int xp)
        {
            DateTime now = DateTime.Now;
            DateTime deadline = now.AddHours(hoursFromNow);

            Goal goal = new Goal
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

            return goal;
        }

        private static bool IsGoalScheduledForDay(Goal goal, DayOfWeek dayOfWeek)
        {
            if (dayOfWeek == DayOfWeek.Sunday)
            {
                return goal.IsSunday;
            }

            if (dayOfWeek == DayOfWeek.Monday)
            {
                return goal.IsMonday;
            }

            if (dayOfWeek == DayOfWeek.Tuesday)
            {
                return goal.IsTuesday;
            }

            if (dayOfWeek == DayOfWeek.Wednesday)
            {
                return goal.IsWednesday;
            }

            if (dayOfWeek == DayOfWeek.Thursday)
            {
                return goal.IsThursday;
            }

            if (dayOfWeek == DayOfWeek.Friday)
            {
                return goal.IsFriday;
            }

            if (dayOfWeek == DayOfWeek.Saturday)
            {
                return goal.IsSaturday;
            }

            return false;
        }

        private void CreatePacks()
        {
            packs.Add(new GoalPack
            {
                Title = "Physical",
                Goals = new List<Goal>
                {
                    new Goal { Title = "Morning run", Description = "Run for 20 minutes", Notes = "Keep stable pace", IsPhysical = true },
                    new Goal { Title = "Push-ups", Description = "Do 3 sets", Notes = "Track repetitions", IsPhysical = true }
                }
            });

            packs.Add(new GoalPack
            {
                Title = "Intellectual",
                Goals = new List<Goal>
                {
                    new Goal { Title = "Read a chapter", Description = "Read 20 pages", Notes = "Write 3 key ideas", IsIntellectual = true },
                    new Goal { Title = "Practice coding", Description = "Solve 1 task", Notes = "Focus on clean code", IsIntellectual = true }
                }
            });

            packs.Add(new GoalPack
            {
                Title = "Mental",
                Goals = new List<Goal>
                {
                    new Goal { Title = "Meditation", Description = "Meditate 10 minutes", Notes = "No distractions", IsMental = true },
                    new Goal { Title = "Journal", Description = "Write evening reflection", Notes = "Note main feeling", IsMental = true }
                }
            });
        }
    }
}
