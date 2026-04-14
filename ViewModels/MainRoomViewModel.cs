using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soulbound.Services;
using Soulbound.Models;

namespace Soulbound.ViewModels
{
    class MainRoomViewModel : ViewModelBase
    {
        double physicalValue;
        public double PhysicalValue
        {
            get => physicalValue;
            set { physicalValue = value; OnPropertyChanged(); }
        }

        double mentalValue;
        public double MentalValue
        {
            get => mentalValue;
            set { mentalValue = value; OnPropertyChanged(); }
        }

        double intellectualValue;
        public double IntellectualValue
        {
            get => intellectualValue;
            set { intellectualValue = value; OnPropertyChanged(); }
        }

        int activeGoalsCount;
        public int ActiveGoalsCount
        {
            get => activeGoalsCount;
            set { activeGoalsCount = value; OnPropertyChanged(); }
        }

        string nearestDeadlineText = "No active goals";
        public string NearestDeadlineText
        {
            get => nearestDeadlineText;
            set { nearestDeadlineText = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Goal> TodayGoals { get; } = new();

        public MainRoomViewModel()
        {
            RefreshData();
        }

        public void RefreshData()
        {
            var goals = LocalDataService.GetInstance()
                .GetGoals()
                .Where(g => !g.IsCompleted && !g.IsAbandoned && !g.IsDeleted)
                .ToList();

            ActiveGoalsCount = goals.Count;
            RefreshTodayGoals(goals);

            var nearest = goals
                .Where(g => g.EndDate >= DateTime.Today)
                .OrderBy(g => g.EndDate)
                .FirstOrDefault();

            if (nearest == null)
            {
                NearestDeadlineText = ActiveGoalsCount > 0
                    ? "All goals are overdue"
                    : "No active goals";
            }
            else
            {
                int daysLeft = (nearest.EndDate.Date - DateTime.Today).Days;
                NearestDeadlineText = daysLeft == 0
                    ? $"Today: {nearest.Title}"
                    : $"{nearest.Title} - {daysLeft} day(s) left";
            }

            if (ActiveGoalsCount == 0)
            {
                PhysicalValue = 0;
                MentalValue = 0;
                IntellectualValue = 0;
                return;
            }

            PhysicalValue = goals.Count(g => g.IsPhysical) / (double)ActiveGoalsCount;
            MentalValue = goals.Count(g => g.IsMental) / (double)ActiveGoalsCount;
            IntellectualValue = goals.Count(g => g.IsIntellectual) / (double)ActiveGoalsCount;
        }

        private void RefreshTodayGoals(List<Goal> goals)
        {
            DayOfWeek today = DateTime.Today.DayOfWeek;
            var todayGoals = goals.Where(g => IsGoalScheduledForDay(g, today)).ToList();

            TodayGoals.Clear();
            foreach (var goal in todayGoals)
            {
                TodayGoals.Add(goal);
            }
            OnPropertyChanged(nameof(TodayGoals));
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
    }
}
