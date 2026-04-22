using System.Collections.ObjectModel;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    class ScheduleViewModel : ViewModelBase
    {
        private readonly LocalDataService dataService;

        public ObservableCollection<ScheduleDayGroup> DayGroups { get; } = new();

        public ScheduleViewModel()
        {
            dataService = LocalDataService.GetInstance();
            Refresh();
        }

        public void Refresh()
        {
            DayGroups.Clear();
            foreach (var group in dataService.GetScheduleGroups())
            {
                DayGroups.Add(group);
            }
        }
    }
}
