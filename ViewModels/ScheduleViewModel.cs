using System.Collections.ObjectModel;
using Soulbound.Models;
using Soulbound.Services;

namespace Soulbound.ViewModels
{
    internal class ScheduleViewModel : ViewModelBase
    {
        private readonly AppService appService;

        public ObservableCollection<Goal> Goals { get; } = new();

        public ScheduleViewModel()
        {
            appService = AppService.GetInstance();
            _ = RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            await appService.EnsureGameDataLoadedAsync();
            Goals.Clear();
            foreach (Goal goal in appService.GetActiveGoals())
            {
                Goals.Add(goal);
            }
        }
    }
}
