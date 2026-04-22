using System.Collections.ObjectModel;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class ScheduleViewModel : ViewModelBase
    {
        private readonly TaskService taskService;

        public ObservableCollection<ScheduleDayGroup> DayGroups { get; } = new();

        public ScheduleViewModel()
        {
            taskService = TaskService.GetInstance();
            Refresh();
        }

        public void Refresh()
        {
            DayGroups.Clear();
            foreach (ScheduleDayGroup group in taskService.GetScheduleGroups())
            {
                DayGroups.Add(group);
            }
        }
    }
}
